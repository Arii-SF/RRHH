using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface IEmpleadoService
    {
        Task<List<EmpleadoResumenResponse>> ListarAsync(string? estado, string? busqueda);
        Task<EmpleadoDetalleResponse?> ObtenerAsync(uint id);
        Task<EmpleadoDetalleResponse> CrearAsync(CrearEmpleadoRequest req);
        Task<EmpleadoDetalleResponse?> ActualizarAsync(uint id, ActualizarEmpleadoRequest req);
        Task<bool> DesactivarAsync(uint id);
        Task<List<DepartamentoResponse>> ListarDepartamentosAsync();
        Task<List<PuestoResponse>> ListarPuestosAsync(int? departamentoId);
        Task<bool> CompletarTareaOnboardingAsync(uint empleadoId, uint tareaId);
        Task<int> TotalActivosAsync();
        Task<ContratoResponse?> ActualizarContratoAsync(uint empleadoId, ActualizarContratoRequest req);
        Task<EmpleadoResumenResponse?> ActualizarHorarioAsync(uint id, ActualizarHorarioRequest req);

    }

    public class EmpleadoService : IEmpleadoService
    {
        private readonly AppDbContext _db;
        public EmpleadoService(AppDbContext db) => _db = db;

        // ── Listar empleados ───────────────────────────────
        public async Task<List<EmpleadoResumenResponse>> ListarAsync(string? estado, string? busqueda)
        {
            var q = _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                q = q.Where(e => e.Estado == estado);

            if (!string.IsNullOrEmpty(busqueda))
            {
                var b = busqueda.ToLower();
                q = q.Where(e =>
                    e.Nombres.ToLower().Contains(b) ||
                    e.Apellidos.ToLower().Contains(b) ||
                    e.CodigoEmpleado.ToLower().Contains(b) ||
                    e.Dpi.Contains(b) ||
                    e.Email.ToLower().Contains(b));
            }

            var empleados = await q.OrderBy(e => e.CodigoEmpleado).ToListAsync();

            return empleados.Select(e =>
            {
                var contrato = e.Contratos.FirstOrDefault();
                var meses = CalcularMeses(e.FechaIngreso);
            return new EmpleadoResumenResponse(
(int)e.Id,
e.CodigoEmpleado,
$"{e.Nombres} {e.Apellidos}",
e.Puesto,
e.DepartamentoArea,
e.Email,
e.Telefono,
e.FechaIngreso,
e.TipoEmpleado,
e.Estado,
e.FotoUrl,
meses,
contrato?.SalarioBase ?? 0,
contrato != null,
e.HoraEntradaEsperada?.ToString("HH:mm"),
e.HoraSalidaEsperada?.ToString("HH:mm"),
e.ToleranciaMinutos

                );
            }).ToList();
        }

        // ── Obtener detalle ────────────────────────────────
        public async Task<EmpleadoDetalleResponse?> ObtenerAsync(uint id)
        {
            var e = await _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .Include(e => e.ContactosEmergencia)
                .Include(e => e.OnboardingTareas.OrderBy(t => t.Orden))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (e == null) return null;
            return MapDetalle(e);
        }

        // ── Crear empleado ─────────────────────────────────
        public async Task<EmpleadoDetalleResponse> CrearAsync(CrearEmpleadoRequest req)
        {
            var ultimo = await _db.Empleados
                .OrderByDescending(e => e.Id)
                .Select(e => e.CodigoEmpleado)
                .FirstOrDefaultAsync();

            var numero = 1;
            if (ultimo != null && ultimo.StartsWith("EMP-"))
                int.TryParse(ultimo[4..], out numero);
            numero++;
            var codigo = $"EMP-{numero:D4}";

            var empleado = new Empleado
            {
                CodigoEmpleado = codigo,
                Nombres = req.Nombres,
                Apellidos = req.Apellidos,
                Dpi = req.Dpi,
                Nit = req.Nit,
                Email = req.Email,
                Telefono = req.Telefono,
                FechaNacimiento = req.FechaNacimiento,
                Genero = req.Genero,
                EstadoCivil = req.EstadoCivil,
                Nacionalidad = req.Nacionalidad,
                Direccion = req.Direccion,
                Municipio = req.Municipio,
                Departamento = req.DepartamentoGeo,
                Puesto = req.Puesto,
                DepartamentoArea = req.DepartamentoArea,
                FechaIngreso = req.FechaIngreso,
                TipoEmpleado = req.TipoEmpleado,
                FotoUrl = req.FotoUrl,
                Estado = "activo"
            };

            _db.Empleados.Add(empleado);
            await _db.SaveChangesAsync();

            var codigoContrato = $"CONT-{DateTime.Now.Year}-{empleado.Id:D4}";
            var contrato = new Contrato
            {
                EmpleadoId = empleado.Id,
                CodigoContrato = codigoContrato,
                TipoContrato = req.TipoContrato,
                SalarioBase = req.SalarioBase,
                BonificacionDecreto = req.BonificacionDecreto,
                OtrasBonificaciones = req.OtrasBonificaciones,
                Jornada = req.Jornada,
                HorasSemana = req.HorasSemana,
                LugarTrabajo = req.LugarTrabajo,
                FechaInicio = req.FechaIngreso,
                FechaFin = req.FechaFinContrato,
                Observaciones = req.ObservacionesContrato,
                Vigente = true
            };
            _db.Contratos.Add(contrato);

            if (!string.IsNullOrEmpty(req.ContactoNombre))
            {
                _db.ContactosEmergencia.Add(new EmpleadoContactoEmergencia
                {
                    EmpleadoId = empleado.Id,
                    NombreCompleto = req.ContactoNombre,
                    Parentesco = req.ContactoParentesco ?? "",
                    Telefono = req.ContactoTelefono ?? "",
                    EsPrincipal = true
                });
            }

            var plantilla = await _db.OnboardingPlantilla
                .Where(p => p.Activa)
                .OrderBy(p => p.Orden)
                .ToListAsync();

            foreach (var p in plantilla)
            {
                _db.OnboardingChecklist.Add(new OnboardingChecklist
                {
                    EmpleadoId = empleado.Id,
                    Tarea = p.Tarea,
                    Descripcion = p.Descripcion,
                    Responsable = p.Responsable,
                    FechaLimite = DateOnly.FromDateTime(DateTime.Now.AddDays(p.DiasLimite)),
                    Orden = p.Orden
                });
            }

            _db.EmpleadoHistorial.Add(new EmpleadoHistorial
            {
                EmpleadoId = empleado.Id,
                TipoCambio = "contratacion",
                Descripcion = $"Empleado {codigo} registrado en el sistema",
                ValorNuevo = $"Puesto: {req.Puesto} | Salario: {req.SalarioBase:C}",
                RealizadoPor = "sistema"
            });

            await _db.SaveChangesAsync();

            var creado = await _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .Include(e => e.ContactosEmergencia)
                .Include(e => e.OnboardingTareas.OrderBy(t => t.Orden))
                .FirstAsync(e => e.Id == empleado.Id);

            return MapDetalle(creado);
        }

        // ── Actualizar empleado ────────────────────────────
        public async Task<EmpleadoDetalleResponse?> ActualizarAsync(uint id, ActualizarEmpleadoRequest req)
        {
            var e = await _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .Include(e => e.ContactosEmergencia)
                .Include(e => e.OnboardingTareas.OrderBy(t => t.Orden))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (e == null) return null;

            var cambios = new List<string>();
            if (e.Puesto != req.Puesto) cambios.Add($"Puesto: {e.Puesto} → {req.Puesto}");
            if (e.Estado != req.Estado) cambios.Add($"Estado: {e.Estado} → {req.Estado}");

            e.Nombres = req.Nombres;
            e.Apellidos = req.Apellidos;
            e.Nit = req.Nit;
            e.NoIgss = req.NoIgss;
            e.Email = req.Email;
            e.Telefono = req.Telefono;
            e.FechaNacimiento = req.FechaNacimiento;
            e.Genero = req.Genero;
            e.EstadoCivil = req.EstadoCivil;
            e.Nacionalidad = req.Nacionalidad;
            e.Direccion = req.Direccion;
            e.Municipio = req.Municipio;
            e.Departamento = req.DepartamentoGeo;
            e.Puesto = req.Puesto;
            e.DepartamentoArea = req.DepartamentoArea;
            e.TipoEmpleado = req.TipoEmpleado;
            e.Estado = req.Estado;
            e.FotoUrl = req.FotoUrl;
            e.UpdatedAt = DateTime.Now;

            if (cambios.Any())
            {
                _db.EmpleadoHistorial.Add(new EmpleadoHistorial
                {
                    EmpleadoId = e.Id,
                    TipoCambio = "actualizacion",
                    Descripcion = string.Join(", ", cambios),
                    RealizadoPor = "sistema"
                });
            }

            await _db.SaveChangesAsync();
            return MapDetalle(e);
        }

        // ── Desactivar empleado ────────────────────────────
        public async Task<bool> DesactivarAsync(uint id)
        {
            var e = await _db.Empleados.FindAsync(id);
            if (e == null) return false;

            e.Estado = "inactivo";
            e.UpdatedAt = DateTime.Now;

            var contratos = await _db.Contratos
                .Where(c => c.EmpleadoId == id && c.Vigente)
                .ToListAsync();

            foreach (var c in contratos)
            {
                c.Vigente = false;
                c.FechaFin = DateOnly.FromDateTime(DateTime.Now);
                c.UpdatedAt = DateTime.Now;
            }

            _db.EmpleadoHistorial.Add(new EmpleadoHistorial
            {
                EmpleadoId = id,
                TipoCambio = "baja",
                Descripcion = "Empleado dado de baja del sistema",
                RealizadoPor = "sistema"
            });

            await _db.SaveChangesAsync();
            return true;
        }

        // ── Catálogos ──────────────────────────────────────
        public async Task<List<DepartamentoResponse>> ListarDepartamentosAsync()
        {
            return await _db.CatDepartamentos
                .Where(d => d.Activo)
                .OrderBy(d => d.Nombre)
                .Select(d => new DepartamentoResponse(d.Id, d.Nombre, d.Codigo))
                .ToListAsync();
        }

        public async Task<List<PuestoResponse>> ListarPuestosAsync(int? departamentoId)
        {
            var q = _db.CatPuestos
                .Include(p => p.Departamento)
                .Where(p => p.Activo)
                .AsQueryable();

            if (departamentoId.HasValue)
                q = q.Where(p => p.DepartamentoId == departamentoId.Value);

            return await q.OrderBy(p => p.Nombre).Select(p => new PuestoResponse(
                p.Id,
                p.DepartamentoId,
                p.Departamento!.Nombre,
                p.Nombre,
                p.Codigo,
                p.NivelSalarial,
                p.SalarioMinimo,
                p.SalarioMaximo
            )).ToListAsync();
        }

        // ── Completar tarea onboarding ─────────────────────
        public async Task<bool> CompletarTareaOnboardingAsync(uint empleadoId, uint tareaId)
        {
            var tarea = await _db.OnboardingChecklist
                .FirstOrDefaultAsync(t => t.Id == tareaId && t.EmpleadoId == empleadoId);

            if (tarea == null) return false;

            tarea.Completada = true;
            tarea.FechaCompletada = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Total activos ──────────────────────────────────
        public async Task<int> TotalActivosAsync()
        {
            return await _db.Empleados.CountAsync(e => e.Estado == "activo");
        }

        // ── Helpers ────────────────────────────────────────
        private static int CalcularMeses(DateOnly fechaIngreso)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var meses = (hoy.Year - fechaIngreso.Year) * 12 + hoy.Month - fechaIngreso.Month;
            return Math.Max(0, meses);
        }

        private static EmpleadoDetalleResponse MapDetalle(Empleado e)
        {
            var contrato = e.Contratos.FirstOrDefault();
            return new EmpleadoDetalleResponse(
                (int)e.Id,
                e.CodigoEmpleado,
                e.Nombres,
                e.Apellidos,
                $"{e.Nombres} {e.Apellidos}",
                e.Dpi,
                e.Nit,
                e.NoIgss,
                e.Email,
                e.Telefono,
                e.FechaNacimiento,
                e.Genero,
                e.EstadoCivil,
                e.Nacionalidad,
                e.Direccion,
                e.Municipio,
                e.Departamento,
                e.Puesto,
                e.DepartamentoArea,
                e.FechaIngreso,
                e.TipoEmpleado,
                e.Estado,
                e.FotoUrl,
                CalcularMeses(e.FechaIngreso),
                contrato == null ? null : new ContratoResponse(
                    (int)contrato.Id,
                    contrato.CodigoContrato,
                    contrato.TipoContrato,
                    contrato.SalarioBase,
                    contrato.BonificacionDecreto,
                    contrato.OtrasBonificaciones,
                    contrato.Jornada,
                    contrato.HorasSemana,
                    contrato.LugarTrabajo,
                    contrato.FechaInicio,
                    contrato.FechaFin,
                    contrato.Vigente,
                    contrato.Observaciones
                ),
                e.ContactosEmergencia.Select(c => new ContactoEmergenciaResponse(
                    (int)c.Id, c.NombreCompleto, c.Parentesco, c.Telefono, c.TelefonoAlt, c.EsPrincipal
                )).ToList(),
                e.OnboardingTareas.Select(t => new OnboardingTareaResponse(
                    (int)t.Id, t.Tarea, t.Descripcion, t.Responsable,
                    t.FechaLimite, t.Completada, t.FechaCompletada, t.Orden
                )).ToList()
            );
        }

        // Implementación
        public async Task<EmpleadoResumenResponse?> ActualizarHorarioAsync(uint id, ActualizarHorarioRequest req)
        {
            var emp = await _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emp == null) return null;

            emp.HoraEntradaEsperada = req.HoraEntradaEsperada != null
                ? TimeOnly.Parse(req.HoraEntradaEsperada) : null;
            emp.HoraSalidaEsperada = req.HoraSalidaEsperada != null
                ? TimeOnly.Parse(req.HoraSalidaEsperada) : null;
            emp.ToleranciaMinutos = req.ToleranciaMinutos;
            emp.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            var contrato = emp.Contratos.FirstOrDefault();
            return new EmpleadoResumenResponse(
     (int)emp.Id,
     emp.CodigoEmpleado,
     $"{emp.Nombres} {emp.Apellidos}",
     emp.Puesto,
     emp.DepartamentoArea,
     emp.Email,
     emp.Telefono,
     emp.FechaIngreso,
     emp.TipoEmpleado,
     emp.Estado,
     emp.FotoUrl,
     CalcularMeses(emp.FechaIngreso),
     contrato?.SalarioBase ?? 0,
     contrato != null,
     emp.HoraEntradaEsperada?.ToString("HH:mm"),
     emp.HoraSalidaEsperada?.ToString("HH:mm"),
     emp.ToleranciaMinutos
 );
        }

        public async Task<ContratoResponse?> ActualizarContratoAsync(uint empleadoId, ActualizarContratoRequest req)
        {
            var contrato = await _db.Contratos
                .FirstOrDefaultAsync(c => c.EmpleadoId == empleadoId && c.Vigente);

            if (contrato == null) return null;

            var anterior = $"Salario: {contrato.SalarioBase} | Jornada: {contrato.Jornada}";

            contrato.TipoContrato = req.TipoContrato;
            contrato.SalarioBase = req.SalarioBase;
            contrato.Jornada = req.Jornada;
            contrato.HorasSemana = req.HorasSemana;
            contrato.LugarTrabajo = req.LugarTrabajo;
            contrato.UpdatedAt = DateTime.Now;

            _db.EmpleadoHistorial.Add(new EmpleadoHistorial
            {
                EmpleadoId = empleadoId,
                TipoCambio = "cambio_contrato",
                Descripcion = "Actualización de contrato vigente",
                ValorAnterior = anterior,
                ValorNuevo = $"Salario: {req.SalarioBase} | Jornada: {req.Jornada}",
                RealizadoPor = "sistema"
            });

            await _db.SaveChangesAsync();

            return new ContratoResponse(
                (int)contrato.Id, contrato.CodigoContrato, contrato.TipoContrato,
                contrato.SalarioBase, contrato.BonificacionDecreto, contrato.OtrasBonificaciones,
                contrato.Jornada, contrato.HorasSemana, contrato.LugarTrabajo,
                contrato.FechaInicio, contrato.FechaFin, contrato.Vigente, contrato.Observaciones
            );
        }
    }
}