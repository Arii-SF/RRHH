using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface IPortalService
    {
        Task<PortalResumenResponse?> ObtenerResumenAsync(uint empleadoId);
        Task<SolicitudResponse> CrearSolicitudAsync(uint empleadoId, CrearSolicitudRequest req);
        Task<List<SolicitudResponse>> ListarSolicitudesAsync(uint? empleadoId, string? tipo, string? estado);
        Task<SolicitudResponse?> ResolverSolicitudAsync(uint id, ResolverSolicitudRequest req);
        Task<ActualizacionResponse> SolicitarActualizacionAsync(uint empleadoId, SolicitarActualizacionRequest req);
        Task<List<ActualizacionResponse>> ListarActualizacionesAsync(uint? empleadoId, string? estado);
        Task<ActualizacionResponse?> RevisarActualizacionAsync(uint id, RevisarActualizacionRequest req);
        Task<List<NotificacionResponse>> ListarNotificacionesAsync(uint empleadoId);
        Task<bool> MarcarNotificacionLeidaAsync(uint id, uint empleadoId);
        Task<int> MarcarTodasLeidasAsync(uint empleadoId);
        Task CrearNotificacionAsync(uint empleadoId, string titulo, string mensaje, string tipo, uint? referenciaId, string? referenciaTipo);
    }

    public class PortalService : IPortalService
    {
        private readonly AppDbContext _db;
        public PortalService(AppDbContext db) => _db = db;

        // ── Resumen del portal ─────────────────────────────
        public async Task<PortalResumenResponse?> ObtenerResumenAsync(uint empleadoId)
        {
            var emp = await _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .FirstOrDefaultAsync(e => e.Id == empleadoId);

            if (emp == null) return null;

            var solicitudesPendientes = await _db.PortalSolicitudes.CountAsync(s => s.EmpleadoId == empleadoId && s.Estado == "pendiente");
            var notificacionesNoLeidas = await _db.PortalNotificaciones.CountAsync(n => n.EmpleadoId == empleadoId && !n.Leida);
            var contrato = emp.Contratos.FirstOrDefault();

            // Calcular dias de vacaciones disponibles (15 dias por año trabajado en Guatemala)
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var anios = (hoy.Year - emp.FechaIngreso.Year);
            var diasVacaciones = Math.Min(anios * 15, 30);

            return new PortalResumenResponse(
                (int)emp.Id,
                $"{emp.Nombres} {emp.Apellidos}",
                emp.Puesto,
                emp.DepartamentoArea,
                emp.FechaIngreso.ToString("yyyy-MM-dd"),
                emp.TipoEmpleado,
                contrato?.SalarioBase ?? 0,
                diasVacaciones,
                solicitudesPendientes,
                notificacionesNoLeidas,
                emp.HoraEntradaEsperada?.ToString("HH:mm"),
                emp.HoraSalidaEsperada?.ToString("HH:mm")
            );
        }

        // ── Solicitudes ────────────────────────────────────
        public async Task<SolicitudResponse> CrearSolicitudAsync(uint empleadoId, CrearSolicitudRequest req)
        {
            var s = new PortalSolicitud
            {
                EmpleadoId = empleadoId,
                Tipo = req.Tipo,
                Subtipo = req.Subtipo,
                FechaInicio = req.FechaInicio != null ? DateOnly.Parse(req.FechaInicio) : null,
                FechaFin = req.FechaFin != null ? DateOnly.Parse(req.FechaFin) : null,
                DiasSolicitados = req.DiasSolicitados,
                Motivo = req.Motivo,
                Estado = "pendiente"
            };
            _db.PortalSolicitudes.Add(s);
            await _db.SaveChangesAsync();

            // Notificar al empleado que su solicitud fue recibida
            await CrearNotificacionAsync(empleadoId,
                "Solicitud recibida",
                $"Tu solicitud de {req.Tipo} fue registrada y esta pendiente de revision.",
                "solicitud", s.Id, "solicitud");

            var result = await _db.PortalSolicitudes
                .Include(x => x.Empleado)
                .FirstAsync(x => x.Id == s.Id);

            return MapSolicitud(result);
        }

        public async Task<List<SolicitudResponse>> ListarSolicitudesAsync(uint? empleadoId, string? tipo, string? estado)
        {
            var q = _db.PortalSolicitudes
                .Include(s => s.Empleado)
                .AsQueryable();

            if (empleadoId.HasValue) q = q.Where(s => s.EmpleadoId == empleadoId.Value);
            if (!string.IsNullOrEmpty(tipo)) q = q.Where(s => s.Tipo == tipo);
            if (!string.IsNullOrEmpty(estado)) q = q.Where(s => s.Estado == estado);

            var lista = await q.OrderByDescending(s => s.CreatedAt).ToListAsync();
            return lista.Select(MapSolicitud).ToList();
        }

        public async Task<SolicitudResponse?> ResolverSolicitudAsync(uint id, ResolverSolicitudRequest req)
        {
            var s = await _db.PortalSolicitudes
                .Include(x => x.Empleado)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) return null;

            s.Estado = req.Estado;
            s.AprobadoPor = req.RevisadoPor;
            s.FechaResolucion = DateTime.Now;
            s.ComentarioRrhh = req.Comentario;
            s.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            // CA-04: Notificar al empleado del cambio de estado
            var estadoLabel = req.Estado == "aprobada" ? "aprobada" : "rechazada";
            await CrearNotificacionAsync(s.EmpleadoId,
                $"Solicitud {estadoLabel}",
                $"Tu solicitud de {s.Tipo} del {s.FechaInicio?.ToString("dd/MM/yyyy") ?? s.CreatedAt.ToString("dd/MM/yyyy")} fue {estadoLabel}." +
                (req.Comentario != null ? $" Comentario: {req.Comentario}" : ""),
                "solicitud", s.Id, "solicitud");

            return MapSolicitud(s);
        }

        // ── Actualizaciones de datos personales ────────────
        public async Task<ActualizacionResponse> SolicitarActualizacionAsync(uint empleadoId, SolicitarActualizacionRequest req)
        {
            // Obtener valor actual del campo
            var emp = await _db.Empleados.FindAsync(empleadoId);
            var valorActual = emp != null ? ObtenerValorCampo(emp, req.Campo) : null;

            var a = new PortalActualizacion
            {
                EmpleadoId = empleadoId,
                Campo = req.Campo,
                ValorActual = valorActual,
                ValorNuevo = req.ValorNuevo,
                Estado = "pendiente"
            };
            _db.PortalActualizaciones.Add(a);
            await _db.SaveChangesAsync();

            await CrearNotificacionAsync(empleadoId,
                "Solicitud de actualizacion enviada",
                $"Tu solicitud para actualizar '{req.Campo}' esta pendiente de revision por RR.HH.",
                "general", a.Id, "actualizacion");

            return MapActualizacion(a);
        }

        public async Task<List<ActualizacionResponse>> ListarActualizacionesAsync(uint? empleadoId, string? estado)
        {
            var q = _db.PortalActualizaciones.AsQueryable();
            if (empleadoId.HasValue) q = q.Where(a => a.EmpleadoId == empleadoId.Value);
            if (!string.IsNullOrEmpty(estado)) q = q.Where(a => a.Estado == estado);

            var lista = await q.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return lista.Select(MapActualizacion).ToList();
        }

        public async Task<ActualizacionResponse?> RevisarActualizacionAsync(uint id, RevisarActualizacionRequest req)
        {
            var a = await _db.PortalActualizaciones
                .Include(x => x.Empleado)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return null;

            a.Estado = req.Estado;
            a.RevisadoPor = req.RevisadoPor;
            a.FechaRevision = DateTime.Now;
            a.Comentario = req.Comentario;

            // Si se aprueba, aplicar el cambio al empleado
            if (req.Estado == "aprobada" && a.Empleado != null)
            {
                AplicarCampoEmpleado(a.Empleado, a.Campo, a.ValorNuevo);
                a.Empleado.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            var estadoLabel = req.Estado == "aprobada" ? "aprobada" : "rechazada";
            await CrearNotificacionAsync(a.EmpleadoId,
                $"Actualizacion de datos {estadoLabel}",
                $"Tu solicitud para actualizar '{a.Campo}' fue {estadoLabel}." +
                (req.Comentario != null ? $" Comentario: {req.Comentario}" : ""),
                "general", a.Id, "actualizacion");

            return MapActualizacion(a);
        }

        // ── Notificaciones ─────────────────────────────────
        public async Task<List<NotificacionResponse>> ListarNotificacionesAsync(uint empleadoId)
        {
            var lista = await _db.PortalNotificaciones
                .Where(n => n.EmpleadoId == empleadoId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return lista.Select(MapNotificacion).ToList();
        }

        public async Task<bool> MarcarNotificacionLeidaAsync(uint id, uint empleadoId)
        {
            var n = await _db.PortalNotificaciones
                .FirstOrDefaultAsync(x => x.Id == id && x.EmpleadoId == empleadoId);

            if (n == null) return false;
            n.Leida = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarcarTodasLeidasAsync(uint empleadoId)
        {
            var notificaciones = await _db.PortalNotificaciones
                .Where(n => n.EmpleadoId == empleadoId && !n.Leida)
                .ToListAsync();

            foreach (var n in notificaciones) n.Leida = true;
            await _db.SaveChangesAsync();
            return notificaciones.Count;
        }

        public async Task CrearNotificacionAsync(uint empleadoId, string titulo, string mensaje,
            string tipo, uint? referenciaId, string? referenciaTipo)
        {
            _db.PortalNotificaciones.Add(new PortalNotificacion
            {
                EmpleadoId = empleadoId,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                ReferenciaId = referenciaId,
                ReferenciaTipo = referenciaTipo,
            });
            await _db.SaveChangesAsync();
        }

        // ── Helpers ────────────────────────────────────────
        private static string? ObtenerValorCampo(Empleado emp, string campo) => campo switch
        {
            "telefono" => emp.Telefono,
            "direccion" => emp.Direccion,
            "municipio" => emp.Municipio,
            "email" => emp.Email,
            "estadoCivil" => emp.EstadoCivil,
            "nacionalidad" => emp.Nacionalidad,
            _ => null
        };

        private static void AplicarCampoEmpleado(Empleado emp, string campo, string valor)
        {
            switch (campo)
            {
                case "telefono": emp.Telefono = valor; break;
                case "direccion": emp.Direccion = valor; break;
                case "municipio": emp.Municipio = valor; break;
                case "estadoCivil": emp.EstadoCivil = valor; break;
                case "nacionalidad": emp.Nacionalidad = valor; break;
            }
        }

        private static SolicitudResponse MapSolicitud(PortalSolicitud s) =>
            new((int)s.Id, (int)s.EmpleadoId,
                s.Empleado != null ? $"{s.Empleado.Nombres} {s.Empleado.Apellidos}" : "",
                s.Tipo, s.Subtipo,
                s.FechaInicio?.ToString("yyyy-MM-dd"),
                s.FechaFin?.ToString("yyyy-MM-dd"),
                s.DiasSolicitados, s.Motivo, s.Estado,
                s.AprobadoPor, s.FechaResolucion, s.ComentarioRrhh, s.CreatedAt);

        private static ActualizacionResponse MapActualizacion(PortalActualizacion a) =>
            new((int)a.Id, (int)a.EmpleadoId, a.Campo, a.ValorActual, a.ValorNuevo,
                a.Estado, a.RevisadoPor, a.FechaRevision, a.Comentario, a.CreatedAt);

        private static NotificacionResponse MapNotificacion(PortalNotificacion n) =>
            new((int)n.Id, (int)n.EmpleadoId, n.Titulo, n.Mensaje, n.Tipo, n.Leida,
                n.ReferenciaId.HasValue ? (int)n.ReferenciaId.Value : null,
                n.ReferenciaTipo, n.CreatedAt);
    }
}