using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface IEvaluacionService
    {
        Task<List<CicloResumenResponse>> ListarCiclosAsync(string? estado);
        Task<CicloDetalleResponse?> ObtenerCicloAsync(uint id);
        Task<CicloDetalleResponse> CrearCicloAsync(CrearCicloRequest req);
        Task<CicloDetalleResponse?> ActivarCicloAsync(uint id);
        Task<CicloDetalleResponse?> CerrarCicloAsync(uint id);
        Task<ParticipanteResponse> AgregarParticipanteAsync(AgregarParticipanteRequest req);
        Task<EvaluacionDetalleResponse?> ObtenerEvaluacionAsync(uint participanteId);
        Task<EvaluacionDetalleResponse?> GuardarCalificacionesAsync(GuardarCalificacionesRequest req);
        Task<ReporteDesempenoResponse> GenerarReporteAsync(string? periodo, string? departamentoArea);
        Task<int> EnviarRecordatoriosAsync();
    }

    public class EvaluacionService : IEvaluacionService
    {
        private readonly AppDbContext _db;
        public EvaluacionService(AppDbContext db) => _db = db;

        // ── Ciclos ─────────────────────────────────────────
        public async Task<List<CicloResumenResponse>> ListarCiclosAsync(string? estado)
        {
            var q = _db.EvaluacionCiclos
                .Include(c => c.Participantes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                q = q.Where(c => c.Estado == estado);

            var lista = await q.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return lista.Select(MapCicloResumen).ToList();
        }

        public async Task<CicloDetalleResponse?> ObtenerCicloAsync(uint id)
        {
            var c = await _db.EvaluacionCiclos
                .Include(c => c.Criterios.OrderBy(x => x.Orden))
                .Include(c => c.Participantes)
                    .ThenInclude(p => p.Empleado)
                .Include(c => c.Participantes)
                    .ThenInclude(p => p.Evaluador)
                .Include(c => c.Participantes)
                    .ThenInclude(p => p.Resultado)
                .Include(c => c.Recordatorios)
                .FirstOrDefaultAsync(c => c.Id == id);

            return c == null ? null : MapCicloDetalle(c);
        }

        public async Task<CicloDetalleResponse> CrearCicloAsync(CrearCicloRequest req)
        {
            var count = await _db.EvaluacionCiclos.CountAsync();
            var codigo = $"EVAL-{DateTime.Now.Year}-{(count + 1):D4}";

            var ciclo = new EvaluacionCiclo
            {
                Codigo = codigo,
                Nombre = req.Nombre,
                Descripcion = req.Descripcion,
                Periodo = req.Periodo,
                FechaInicio = DateOnly.Parse(req.FechaInicio),
                FechaFin = DateOnly.Parse(req.FechaFin),
                FechaCierre = DateOnly.Parse(req.FechaCierre),
                Estado = "borrador",
                IncluyeAutoevaluacion = req.IncluyeAutoevaluacion,
                EscalaMinima = req.EscalaMinima,
                EscalaMaxima = req.EscalaMaxima,
            };
            _db.EvaluacionCiclos.Add(ciclo);
            await _db.SaveChangesAsync();

            // Crear criterios
            foreach (var cr in req.Criterios)
            {
                _db.EvaluacionCriterios.Add(new EvaluacionCriterio
                {
                    CicloId = ciclo.Id,
                    Nombre = cr.Nombre,
                    Descripcion = cr.Descripcion,
                    Peso = cr.Peso,
                    Orden = cr.Orden,
                });
            }

            // Crear recordatorios para 7, 3 y 1 dia antes
            foreach (var dias in new[] { 7, 3, 1 })
            {
                _db.EvaluacionRecordatorios.Add(new EvaluacionRecordatorio
                {
                    CicloId = ciclo.Id,
                    DiasAntes = dias,
                    Enviado = false
                });
            }

            await _db.SaveChangesAsync();
            return (await ObtenerCicloAsync(ciclo.Id))!;
        }

        public async Task<CicloDetalleResponse?> ActivarCicloAsync(uint id)
        {
            var c = await _db.EvaluacionCiclos.FindAsync(id);
            if (c == null) return null;
            c.Estado = "activo";
            c.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return await ObtenerCicloAsync(id);
        }

        public async Task<CicloDetalleResponse?> CerrarCicloAsync(uint id)
        {
            var c = await _db.EvaluacionCiclos.FindAsync(id);
            if (c == null) return null;
            c.Estado = "cerrado";
            c.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return await ObtenerCicloAsync(id);
        }

        // ── Participantes ──────────────────────────────────
        public async Task<ParticipanteResponse> AgregarParticipanteAsync(AgregarParticipanteRequest req)
        {
            var p = new EvaluacionParticipante
            {
                CicloId = (uint)req.CicloId,
                EmpleadoId = (uint)req.EmpleadoId,
                EvaluadorId = (uint)req.EvaluadorId,
                Estado = "pendiente"
            };
            _db.EvaluacionParticipantes.Add(p);
            await _db.SaveChangesAsync();

            var result = await _db.EvaluacionParticipantes
                .Include(x => x.Empleado)
                .Include(x => x.Evaluador)
                .Include(x => x.Resultado)
                .FirstAsync(x => x.Id == p.Id);

            return MapParticipante(result);
        }

        // ── Evaluaciones ───────────────────────────────────
        public async Task<EvaluacionDetalleResponse?> ObtenerEvaluacionAsync(uint participanteId)
        {
            var p = await _db.EvaluacionParticipantes
                .Include(x => x.Empleado)
                .Include(x => x.Ciclo)
                    .ThenInclude(c => c!.Criterios.OrderBy(cr => cr.Orden))
                .Include(x => x.Calificaciones)
                    .ThenInclude(cal => cal.Criterio)
                .Include(x => x.Resultado)
                .FirstOrDefaultAsync(x => x.Id == participanteId);

            return p == null ? null : MapEvaluacionDetalle(p);
        }

        // CA-02: Guardar calificaciones (evaluador o autoevaluacion)
        public async Task<EvaluacionDetalleResponse?> GuardarCalificacionesAsync(GuardarCalificacionesRequest req)
        {
            var p = await _db.EvaluacionParticipantes
                .Include(x => x.Empleado)
                .Include(x => x.Ciclo)
                    .ThenInclude(c => c!.Criterios.OrderBy(cr => cr.Orden))
                .Include(x => x.Calificaciones)
                    .ThenInclude(cal => cal.Criterio)
                .Include(x => x.Resultado)
                .FirstOrDefaultAsync(x => x.Id == (uint)req.ParticipanteId);

            if (p == null) return null;

            foreach (var item in req.Calificaciones)
            {
                var existente = p.Calificaciones.FirstOrDefault(c =>
                    c.CriterioId == (uint)item.CriterioId && c.Tipo == req.Tipo);

                if (existente != null)
                {
                    existente.Calificacion = item.Calificacion;
                    existente.Comentario = item.Comentario;
                }
                else
                {
                    _db.EvaluacionCalificaciones.Add(new EvaluacionCalificacion
                    {
                        ParticipanteId = p.Id,
                        CriterioId = (uint)item.CriterioId,
                        Tipo = req.Tipo,
                        Calificacion = item.Calificacion,
                        Comentario = item.Comentario,
                    });
                }
            }

            // Calcular puntaje ponderado
            await _db.SaveChangesAsync();

            // Recargar calificaciones actualizadas
            await _db.Entry(p).Collection(x => x.Calificaciones).LoadAsync();

            var calEval = p.Calificaciones.Where(c => c.Tipo == "evaluador").ToList();
            var calAuto = p.Calificaciones.Where(c => c.Tipo == "autoevaluacion").ToList();

            decimal? puntajeEval = calEval.Any()
                ? calEval.Sum(c => c.Calificacion * (c.Criterio?.Peso ?? 20) / 100) : null;

            decimal? puntajeAuto = calAuto.Any()
                ? calAuto.Sum(c => c.Calificacion * (c.Criterio?.Peso ?? 20) / 100) : null;

            decimal? puntajeFinal = puntajeEval.HasValue
                ? (puntajeAuto.HasValue ? Math.Round((puntajeEval.Value + puntajeAuto.Value) / 2, 2) : puntajeEval)
                : puntajeAuto;

            string? nivel = puntajeFinal.HasValue ? CalcularNivel(puntajeFinal.Value, p.Ciclo!.EscalaMaxima) : null;

            // Actualizar o crear resultado
            if (p.Resultado == null)
            {
                _db.EvaluacionResultados.Add(new EvaluacionResultado
                {
                    ParticipanteId = p.Id,
                    PuntajeEvaluador = puntajeEval,
                    PuntajeAutoevaluacion = puntajeAuto,
                    PuntajeFinal = puntajeFinal,
                    Nivel = nivel,
                    ComentariosGenerales = req.ComentariosGenerales,
                    PlanMejora = req.PlanMejora,
                });
            }
            else
            {
                p.Resultado.PuntajeEvaluador = puntajeEval;
                p.Resultado.PuntajeAutoevaluacion = puntajeAuto;
                p.Resultado.PuntajeFinal = puntajeFinal;
                p.Resultado.Nivel = nivel;
                if (req.ComentariosGenerales != null) p.Resultado.ComentariosGenerales = req.ComentariosGenerales;
                if (req.PlanMejora != null) p.Resultado.PlanMejora = req.PlanMejora;
            }

            // Marcar como completada si tiene calificaciones de evaluador
            if (calEval.Count >= (p.Ciclo?.Criterios.Count ?? 1))
            {
                p.Estado = "completada";
                p.FechaCompletada = DateTime.Now;
            }
            else
            {
                p.Estado = "en_progreso";
            }

            await _db.SaveChangesAsync();
            return await ObtenerEvaluacionAsync(p.Id);
        }

        // CA-03: Reporte comparativo
        public async Task<ReporteDesempenoResponse> GenerarReporteAsync(string? periodo, string? departamentoArea)
        {
            var q = _db.EvaluacionParticipantes
                .Include(p => p.Empleado)
                .Include(p => p.Ciclo)
                .Include(p => p.Resultado)
                .Where(p => p.Estado == "completada" && p.Resultado != null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(periodo))
                q = q.Where(p => p.Ciclo!.Periodo == periodo);

            if (!string.IsNullOrEmpty(departamentoArea))
                q = q.Where(p => p.Empleado!.DepartamentoArea == departamentoArea);

            var lista = await q.ToListAsync();

            var empleados = lista.Select(p => new ResumenEmpleadoDesempeno(
                (int)p.EmpleadoId,
                p.Empleado != null ? $"{p.Empleado.Nombres} {p.Empleado.Apellidos}" : "",
                p.Empleado?.DepartamentoArea,
                p.Empleado?.Puesto ?? "",
                p.Resultado?.PuntajeFinal,
                p.Resultado?.Nivel,
                p.Ciclo?.Nombre ?? "",
                p.Ciclo?.Periodo ?? ""
            )).ToList();

            var promedio = empleados.Any(e => e.PuntajeFinal.HasValue)
                ? Math.Round(empleados.Where(e => e.PuntajeFinal.HasValue).Average(e => e.PuntajeFinal!.Value), 2) : 0;
            var excelentes = empleados.Count(e => e.Nivel == "Excelente");
            var buenos = empleados.Count(e => e.Nivel == "Bueno");
            var regulares = empleados.Count(e => e.Nivel == "Regular");
            var deficientes = empleados.Count(e => e.Nivel == "Deficiente");

            return new ReporteDesempenoResponse(
                periodo ?? "Todos los periodos",
                departamentoArea,
                empleados.Count,
                promedio,
                excelentes, buenos, regulares, deficientes,
                empleados.OrderByDescending(e => e.PuntajeFinal).ToList()
            );
        }

        // CA-04: Enviar recordatorios
        public async Task<int> EnviarRecordatoriosAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var recordatorios = await _db.EvaluacionRecordatorios
                .Include(r => r.Ciclo)
                    .ThenInclude(c => c!.Participantes)
                .Where(r => !r.Enviado && r.Ciclo!.Estado == "activo")
                .ToListAsync();

            int enviados = 0;
            foreach (var rec in recordatorios)
            {
                var fechaEnvio = rec.Ciclo!.FechaCierre.AddDays(-rec.DiasAntes);
                if (hoy >= fechaEnvio)
                {
                    var pendientes = rec.Ciclo.Participantes.Count(p => p.Estado != "completada");
                    rec.Enviado = true;
                    rec.FechaEnvio = DateTime.Now;
                    rec.Destinatarios = pendientes;
                    enviados++;
                }
            }

            if (enviados > 0) await _db.SaveChangesAsync();
            return enviados;
        }

        // ── Helpers ────────────────────────────────────────
        private static string CalcularNivel(decimal puntaje, int escalaMax)
        {
            var pct = puntaje / escalaMax * 100;
            return pct switch
            {
                >= 90 => "Excelente",
                >= 75 => "Bueno",
                >= 60 => "Regular",
                _ => "Deficiente"
            };
        }

        private static CicloResumenResponse MapCicloResumen(EvaluacionCiclo c)
        {
            var completadas = c.Participantes.Count(p => p.Estado == "completada");
            var total = c.Participantes.Count;
            var pct = total > 0 ? Math.Round((decimal)completadas / total * 100, 2) : 0;
            return new CicloResumenResponse(
                (int)c.Id, c.Codigo, c.Nombre, c.Periodo,
                c.FechaInicio.ToString("yyyy-MM-dd"),
                c.FechaFin.ToString("yyyy-MM-dd"),
                c.FechaCierre.ToString("yyyy-MM-dd"),
                c.Estado, c.IncluyeAutoevaluacion, c.EscalaMinima, c.EscalaMaxima,
                total, completadas, total - completadas, pct
            );
        }

        private static CicloDetalleResponse MapCicloDetalle(EvaluacionCiclo c) =>
            new((int)c.Id, c.Codigo, c.Nombre, c.Descripcion, c.Periodo,
                c.FechaInicio.ToString("yyyy-MM-dd"),
                c.FechaFin.ToString("yyyy-MM-dd"),
                c.FechaCierre.ToString("yyyy-MM-dd"),
                c.Estado, c.IncluyeAutoevaluacion, c.EscalaMinima, c.EscalaMaxima,
                c.Criterios.Select(cr => new CriterioResponse(
                    (int)cr.Id, (int)cr.CicloId, cr.Nombre, cr.Descripcion, cr.Peso, cr.Orden
                )).ToList(),
                c.Participantes.Select(MapParticipante).ToList(),
                c.Recordatorios.Select(r => new RecordatorioResponse(
                    (int)r.Id, (int)r.CicloId, r.DiasAntes, r.Enviado, r.FechaEnvio, r.Destinatarios
                )).ToList()
            );

        private static ParticipanteResponse MapParticipante(EvaluacionParticipante p) =>
            new((int)p.Id, (int)p.CicloId, (int)p.EmpleadoId,
                p.Empleado != null ? $"{p.Empleado.Nombres} {p.Empleado.Apellidos}" : "",
                p.Empleado?.Puesto ?? "",
                p.Empleado?.DepartamentoArea,
                (int)p.EvaluadorId,
                p.Evaluador != null ? $"{p.Evaluador.Nombres} {p.Evaluador.Apellidos}" : "",
                p.Estado, p.FechaCompletada,
                p.Resultado?.PuntajeFinal,
                p.Resultado?.Nivel
            );

        private static EvaluacionDetalleResponse MapEvaluacionDetalle(EvaluacionParticipante p)
        {
            var calEval = p.Calificaciones.Where(c => c.Tipo == "evaluador").ToList();
            var calAuto = p.Calificaciones.Where(c => c.Tipo == "autoevaluacion").ToList();
            return new(
                (int)p.Id,
                p.Empleado != null ? $"{p.Empleado.Nombres} {p.Empleado.Apellidos}" : "",
                p.Ciclo?.Nombre ?? "",
                p.Estado,
                p.Ciclo?.Criterios.Select(cr => new CriterioResponse(
                    (int)cr.Id, (int)cr.CicloId, cr.Nombre, cr.Descripcion, cr.Peso, cr.Orden
                )).ToList() ?? new(),
                calEval.Select(c => new CalificacionResponse(
                    (int)c.Id, (int)c.ParticipanteId, (int)c.CriterioId,
                    c.Criterio?.Nombre ?? "", c.Criterio?.Peso ?? 0, c.Tipo, c.Calificacion, c.Comentario
                )).ToList(),
                calAuto.Select(c => new CalificacionResponse(
                    (int)c.Id, (int)c.ParticipanteId, (int)c.CriterioId,
                    c.Criterio?.Nombre ?? "", c.Criterio?.Peso ?? 0, c.Tipo, c.Calificacion, c.Comentario
                )).ToList(),
                p.Resultado?.PuntajeEvaluador,
                p.Resultado?.PuntajeAutoevaluacion,
                p.Resultado?.PuntajeFinal,
                p.Resultado?.Nivel,
                p.Resultado?.ComentariosGenerales,
                p.Resultado?.PlanMejora
            );
        }
    }
}