using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface IReclutamientoService
    {
        // Vacantes
        Task<List<VacanteResumenResponse>> ListarVacantesAsync(string? estado);
        Task<VacanteDetalleResponse?> ObtenerVacanteAsync(uint id);
        Task<VacanteDetalleResponse> CrearVacanteAsync(CrearVacanteRequest req);
        Task<VacanteDetalleResponse?> PublicarVacanteAsync(uint id);
        Task<VacanteDetalleResponse?> CerrarVacanteAsync(uint id);
        // Candidatos
        Task<CandidatoDetalleResponse> RegistrarCandidatoAsync(CrearCandidatoRequest req);
        Task<CandidatoDetalleResponse?> ObtenerCandidatoAsync(uint id);
        Task<CandidatoDetalleResponse?> CambiarEtapaAsync(uint candidatoId, CambiarEtapaRequest req);
        Task<CandidatoDetalleResponse?> ActualizarCandidatoAsync(uint id, ActualizarCandidatoRequest req);
        Task<EmpleadoDetalleResponse?> ContratarCandidatoAsync(uint candidatoId, ContratarCandidatoRequest req);
    }

    public class ReclutamientoService : IReclutamientoService
    {
        private readonly AppDbContext _db;
        private readonly IEmpleadoService _empSvc;

        public ReclutamientoService(AppDbContext db, IEmpleadoService empSvc)
        {
            _db = db;
            _empSvc = empSvc;
        }

        // ══════════════════════════════════════════════════
        //  VACANTES
        // ══════════════════════════════════════════════════

        public async Task<List<VacanteResumenResponse>> ListarVacantesAsync(string? estado)
        {
            var q = _db.Vacantes
                .Include(v => v.Candidatos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                q = q.Where(v => v.Estado == estado);

            var lista = await q.OrderByDescending(v => v.CreatedAt).ToListAsync();
            return lista.Select(MapVacanteResumen).ToList();
        }

        public async Task<VacanteDetalleResponse?> ObtenerVacanteAsync(uint id)
        {
            var v = await _db.Vacantes
                .Include(v => v.Candidatos)
                .FirstOrDefaultAsync(v => v.Id == id);
            return v == null ? null : MapVacanteDetalle(v);
        }

        public async Task<VacanteDetalleResponse> CrearVacanteAsync(CrearVacanteRequest req)
        {
            var count = await _db.Vacantes.CountAsync();
            var codigo = $"VAC-{DateTime.Now.Year}-{(count + 1):D4}";

            var vacante = new Vacante
            {
                CodigoVacante = codigo,
                Titulo = req.Titulo,
                DepartamentoArea = req.DepartamentoArea,
                Puesto = req.Puesto,
                Descripcion = req.Descripcion,
                Requisitos = req.Requisitos,
                SalarioMinimo = req.SalarioMinimo,
                SalarioMaximo = req.SalarioMaximo,
                TipoContrato = req.TipoContrato,
                Jornada = req.Jornada,
                Modalidad = req.Modalidad,
                Estado = "borrador",
                FechaCierre = req.FechaCierre,
                VacantesDisponibles = req.VacantesDisponibles,
                CanalInterno = req.CanalInterno,
                CanalLinkedin = req.CanalLinkedin,
                CanalComputrabajo = req.CanalComputrabajo,
                CanalIndeed = req.CanalIndeed,
                CanalOtro = req.CanalOtro,
            };

            _db.Vacantes.Add(vacante);
            await _db.SaveChangesAsync();

            var creada = await _db.Vacantes.Include(v => v.Candidatos).FirstAsync(v => v.Id == vacante.Id);
            return MapVacanteDetalle(creada);
        }

        public async Task<VacanteDetalleResponse?> PublicarVacanteAsync(uint id)
        {
            var v = await _db.Vacantes.Include(v => v.Candidatos).FirstOrDefaultAsync(v => v.Id == id);
            if (v == null) return null;

            v.Estado = "publicada";
            v.FechaPublicacion = DateOnly.FromDateTime(DateTime.Now);
            v.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return MapVacanteDetalle(v);
        }

        public async Task<VacanteDetalleResponse?> CerrarVacanteAsync(uint id)
        {
            var v = await _db.Vacantes.Include(v => v.Candidatos).FirstOrDefaultAsync(v => v.Id == id);
            if (v == null) return null;

            v.Estado = "cerrada";
            v.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return MapVacanteDetalle(v);
        }

        // ══════════════════════════════════════════════════
        //  CANDIDATOS
        // ══════════════════════════════════════════════════

        public async Task<CandidatoDetalleResponse> RegistrarCandidatoAsync(CrearCandidatoRequest req)
        {
            var candidato = new Candidato
            {
                VacanteId = (uint)req.VacanteId,
                Nombres = req.Nombres,
                Apellidos = req.Apellidos,
                Email = req.Email,
                Telefono = req.Telefono,
                Dpi = req.Dpi,
                FechaNacimiento = req.FechaNacimiento,
                Direccion = req.Direccion,
                LinkedinUrl = req.LinkedinUrl,
                CvUrl = req.CvUrl,
                CartaPresentacion = req.CartaPresentacion,
                FuenteAplicacion = req.FuenteAplicacion,
                Etapa = "aplicado",
                FechaAplicacion = DateTime.Now,
            };

            _db.Candidatos.Add(candidato);
            await _db.SaveChangesAsync();

            // Historial inicial
            _db.CandidatoHistorialEtapas.Add(new CandidatoHistorialEtapa
            {
                CandidatoId = candidato.Id,
                EtapaAnterior = null,
                EtapaNueva = "aplicado",
                Comentario = "Candidato registrado en el sistema",
                RealizadoPor = "sistema"
            });

            // Comunicación automática
            await GenerarComunicacionAsync(candidato, "aplicado");
            await _db.SaveChangesAsync();

            return (await ObtenerCandidatoAsync(candidato.Id))!;
        }

        public async Task<CandidatoDetalleResponse?> ObtenerCandidatoAsync(uint id)
        {
            var c = await _db.Candidatos
                .Include(c => c.Vacante)
                .Include(c => c.Historial.OrderByDescending(h => h.CreatedAt))
                .Include(c => c.Comunicaciones.OrderByDescending(x => x.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id);
            return c == null ? null : MapCandidatoDetalle(c);
        }

        public async Task<CandidatoDetalleResponse?> CambiarEtapaAsync(uint candidatoId, CambiarEtapaRequest req)
        {
            var c = await _db.Candidatos
                .Include(c => c.Vacante)
                .Include(c => c.Historial)
                .Include(c => c.Comunicaciones)
                .FirstOrDefaultAsync(c => c.Id == candidatoId);

            if (c == null) return null;

            var etapaAnterior = c.Etapa;
            c.Etapa = req.EtapaNueva;
            c.FechaUltimaEtapa = DateTime.Now;
            c.UpdatedAt = DateTime.Now;

            _db.CandidatoHistorialEtapas.Add(new CandidatoHistorialEtapa
            {
                CandidatoId = c.Id,
                EtapaAnterior = etapaAnterior,
                EtapaNueva = req.EtapaNueva,
                Comentario = req.Comentario,
                RealizadoPor = req.RealizadoPor
            });

            // Comunicación automática por cambio de etapa (CA-03)
            await GenerarComunicacionAsync(c, req.EtapaNueva);

            await _db.SaveChangesAsync();
            return MapCandidatoDetalle(c);
        }

        public async Task<CandidatoDetalleResponse?> ActualizarCandidatoAsync(uint id, ActualizarCandidatoRequest req)
        {
            var c = await _db.Candidatos
                .Include(c => c.Vacante)
                .Include(c => c.Historial)
                .Include(c => c.Comunicaciones)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return null;

            if (req.Puntaje.HasValue) c.Puntaje = req.Puntaje;
            if (req.NotasReclutador != null) c.NotasReclutador = req.NotasReclutador;
            if (req.LinkedinUrl != null) c.LinkedinUrl = req.LinkedinUrl;
            c.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return MapCandidatoDetalle(c);
        }

        // CA-04: Contratar candidato → crear empleado automáticamente
        public async Task<EmpleadoDetalleResponse?> ContratarCandidatoAsync(uint candidatoId, ContratarCandidatoRequest req)
        {
            var c = await _db.Candidatos
                .Include(c => c.Vacante)
                .Include(c => c.Historial)
                .Include(c => c.Comunicaciones)
                .FirstOrDefaultAsync(c => c.Id == candidatoId);

            if (c == null) return null;

            // Crear empleado desde datos del candidato
            var empleado = await _empSvc.CrearAsync(new CrearEmpleadoRequest(
                Nombres: c.Nombres,
                Apellidos: c.Apellidos,
                Dpi: c.Dpi ?? "",
                Nit: req.Nit ?? "CF",
                Email: c.Email,
                Telefono: c.Telefono,
                FechaNacimiento: c.FechaNacimiento,
                Genero: null,
                EstadoCivil: null,
                Nacionalidad: "Guatemalteca",
                Direccion: c.Direccion,
                Municipio: null,
                DepartamentoGeo: null,
                FotoUrl: null,
                Puesto: req.Puesto,
                DepartamentoArea: req.DepartamentoArea,
                FechaIngreso: req.FechaIngreso,
                TipoEmpleado: req.TipoEmpleado,
                TipoContrato: req.TipoContrato,
                SalarioBase: req.SalarioBase,
                BonificacionDecreto: req.BonificacionDecreto,
                OtrasBonificaciones: req.OtrasBonificaciones,
                Jornada: req.Jornada,
                HorasSemana: req.HorasSemana,
                LugarTrabajo: req.LugarTrabajo,
                FechaFinContrato: req.FechaFinContrato,
                ObservacionesContrato: $"Contratado desde proceso de reclutamiento {c.Vacante?.CodigoVacante}",
                ContactoNombre: null,
                ContactoParentesco: null,
                ContactoTelefono: null
            ));

            // Vincular candidato con el empleado creado
            c.EmpleadoId = (uint)empleado.Id;
            c.Etapa = "contratado";
            c.FechaUltimaEtapa = DateTime.Now;
            c.UpdatedAt = DateTime.Now;

            _db.CandidatoHistorialEtapas.Add(new CandidatoHistorialEtapa
            {
                CandidatoId = c.Id,
                EtapaAnterior = c.Etapa,
                EtapaNueva = "contratado",
                Comentario = $"Contratado. Empleado creado: {empleado.CodigoEmpleado}",
                RealizadoPor = "sistema"
            });

            await GenerarComunicacionAsync(c, "contratado");
            await _db.SaveChangesAsync();

            return empleado;
        }

        // ── Helpers ────────────────────────────────────────

        private async Task GenerarComunicacionAsync(Candidato candidato, string etapa)
        {
            var plantilla = await _db.PlantillasEmail
                .FirstOrDefaultAsync(p => p.Etapa == etapa && p.Activa);

            if (plantilla == null) return;

            var vacanteTitulo = candidato.Vacante?.Titulo ?? "la vacante";
            var cuerpo = plantilla.Cuerpo
                .Replace("{nombre}", $"{candidato.Nombres} {candidato.Apellidos}")
                .Replace("{vacante}", vacanteTitulo)
                .Replace("{empresa}", "ERP RRHH GT");

            _db.ReclutamientoComunicaciones.Add(new ReclutamientoComunicacion
            {
                CandidatoId = candidato.Id,
                Etapa = etapa,
                Asunto = plantilla.Asunto.Replace("{vacante}", vacanteTitulo),
                Cuerpo = cuerpo,
                Enviado = true,
                FechaEnvio = DateTime.Now,
            });
        }

        private static VacanteResumenResponse MapVacanteResumen(Vacante v)
        {
            var canales = new List<string>();
            if (v.CanalInterno) canales.Add("Interno");
            if (v.CanalLinkedin) canales.Add("LinkedIn");
            if (v.CanalComputrabajo) canales.Add("Computrabajo");
            if (v.CanalIndeed) canales.Add("Indeed");
            if (!string.IsNullOrEmpty(v.CanalOtro)) canales.Add(v.CanalOtro);

            var activos = v.Candidatos.Count(c => c.Etapa != "descartado");
            return new VacanteResumenResponse(
                (int)v.Id, v.CodigoVacante, v.Titulo, v.DepartamentoArea, v.Puesto,
                v.Estado, v.Modalidad, v.Jornada, v.SalarioMinimo, v.SalarioMaximo,
                v.FechaPublicacion, v.FechaCierre, v.VacantesDisponibles,
                v.Candidatos.Count, activos, canales
            );
        }

        private static VacanteDetalleResponse MapVacanteDetalle(Vacante v)
        {
            var canales = new List<string>();
            if (v.CanalInterno) canales.Add("Interno");
            if (v.CanalLinkedin) canales.Add("LinkedIn");
            if (v.CanalComputrabajo) canales.Add("Computrabajo");
            if (v.CanalIndeed) canales.Add("Indeed");
            if (!string.IsNullOrEmpty(v.CanalOtro)) canales.Add(v.CanalOtro);

            var pipeline = new PipelineStats(
                v.Candidatos.Count,
                v.Candidatos.Count(c => c.Etapa == "aplicado"),
                v.Candidatos.Count(c => c.Etapa == "revision"),
                v.Candidatos.Count(c => c.Etapa == "entrevista_rh"),
                v.Candidatos.Count(c => c.Etapa == "entrevista_tecnica"),
                v.Candidatos.Count(c => c.Etapa == "prueba"),
                v.Candidatos.Count(c => c.Etapa == "oferta"),
                v.Candidatos.Count(c => c.Etapa == "contratado"),
                v.Candidatos.Count(c => c.Etapa == "descartado")
            );

            return new VacanteDetalleResponse(
                (int)v.Id, v.CodigoVacante, v.Titulo, v.DepartamentoArea, v.Puesto,
                v.Descripcion, v.Requisitos, v.Estado, v.TipoContrato, v.Jornada,
                v.Modalidad, v.SalarioMinimo, v.SalarioMaximo,
                v.FechaPublicacion, v.FechaCierre, v.VacantesDisponibles,
                v.CanalInterno, v.CanalLinkedin, v.CanalComputrabajo, v.CanalIndeed,
                v.CanalOtro, canales,
                v.Candidatos.Select(c => new CandidatoResumenResponse(
                    (int)c.Id, (int)c.VacanteId,
                    $"{c.Nombres} {c.Apellidos}", c.Email, c.Telefono,
                    c.Etapa, c.Puntaje, c.FuenteAplicacion,
                    c.FechaAplicacion, c.FechaUltimaEtapa, c.EmpleadoId.HasValue
                )).ToList(),
                pipeline
            );
        }

        private static CandidatoDetalleResponse MapCandidatoDetalle(Candidato c) =>
            new(
                (int)c.Id, (int)c.VacanteId,
                c.Vacante?.Titulo ?? "",
                c.Nombres, c.Apellidos,
                $"{c.Nombres} {c.Apellidos}",
                c.Email, c.Telefono, c.Dpi,
                c.FechaNacimiento, c.Direccion,
                c.LinkedinUrl, c.CvUrl, c.CartaPresentacion,
                c.FuenteAplicacion, c.Etapa, c.Puntaje,
                c.NotasReclutador, c.FechaAplicacion, c.FechaUltimaEtapa,
                c.EmpleadoId.HasValue ? (int)c.EmpleadoId.Value : null,
                c.Historial.Select(h => new HistorialEtapaResponse(
                    (int)h.Id, h.EtapaAnterior, h.EtapaNueva,
                    h.Comentario, h.RealizadoPor, h.CreatedAt
                )).ToList(),
                c.Comunicaciones.Select(x => new ComunicacionResponse(
                    (int)x.Id, x.Etapa, x.Asunto, x.Cuerpo, x.Enviado, x.FechaEnvio
                )).ToList()
            );
    }
}