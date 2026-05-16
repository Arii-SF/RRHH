using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface ICapacitacionService
    {
        Task<List<PlanResumenResponse>> ListarPlanesAsync(bool? activo, string? categoria);
        Task<PlanDetalleResponse?> ObtenerPlanAsync(uint id);
        Task<PlanDetalleResponse> CrearPlanAsync(CrearPlanRequest req);
        Task<bool> DesactivarPlanAsync(uint id);
        Task<List<AsignacionCapacitacionResponse>> AsignarPlanAsync(AsignarPlanRequest req);
        Task<List<AsignacionCapacitacionResponse>> ListarAsignacionesEmpleadoAsync(uint empleadoId);
        Task<AsignacionCapacitacionResponse?> ActualizarAvanceAsync(uint asignacionId, ActualizarAvanceRequest req);
        Task<EvidenciaResponse> SubirEvidenciaAsync(SubirEvidenciaRequest req);
        Task<EvidenciaResponse?> ObtenerEvidenciaAsync(uint evidenciaId);
        Task<List<EvidenciaResponse>> ListarEvidenciasAsync(uint asignacionId);
        Task<List<AlertaCapacitacionResponse>> ListarAlertasAsync(bool? enviadas);
        Task<int> ProcesarAlertasAsync();
        Task<ReporteCapacitacionResponse> GenerarReporteAsync(string? departamentoArea);
    }

    public class CapacitacionService : ICapacitacionService
    {
        private readonly AppDbContext _db;
        public CapacitacionService(AppDbContext db) => _db = db;

        // ── Planes ─────────────────────────────────────────
        public async Task<List<PlanResumenResponse>> ListarPlanesAsync(bool? activo, string? categoria)
        {
            var q = _db.CapacitacionPlanes
                .Include(p => p.Asignaciones)
                .AsQueryable();

            if (activo.HasValue) q = q.Where(p => p.Activo == activo.Value);
            if (!string.IsNullOrEmpty(categoria)) q = q.Where(p => p.Categoria == categoria);

            var lista = await q.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return lista.Select(MapPlanResumen).ToList();
        }

        public async Task<PlanDetalleResponse?> ObtenerPlanAsync(uint id)
        {
            var p = await _db.CapacitacionPlanes
                .Include(p => p.Asignaciones).ThenInclude(a => a.Empleado)
                .Include(p => p.Asignaciones).ThenInclude(a => a.Evidencias)
                .FirstOrDefaultAsync(p => p.Id == id);

            return p == null ? null : MapPlanDetalle(p);
        }

        public async Task<PlanDetalleResponse> CrearPlanAsync(CrearPlanRequest req)
        {
            var count = await _db.CapacitacionPlanes.CountAsync();
            var codigo = $"CAP-{DateTime.Now.Year}-{(count + 1):D4}";

            var plan = new CapacitacionPlan
            {
                Codigo = codigo,
                Nombre = req.Nombre,
                Descripcion = req.Descripcion,
                Tipo = req.Tipo,
                Categoria = req.Categoria,
                HorasRequeridas = req.HorasRequeridas,
                Presupuesto = req.Presupuesto,
                FechaInicio = DateOnly.Parse(req.FechaInicio),
                FechaFin = DateOnly.Parse(req.FechaFin),
                FechaLimite = DateOnly.Parse(req.FechaLimite),
                Instructor = req.Instructor,
                Lugar = req.Lugar,
                EsRequerido = req.EsRequerido,
                Activo = true
            };
            _db.CapacitacionPlanes.Add(plan);
            await _db.SaveChangesAsync();
            return (await ObtenerPlanAsync(plan.Id))!;
        }

        public async Task<bool> DesactivarPlanAsync(uint id)
        {
            var p = await _db.CapacitacionPlanes.FindAsync(id);
            if (p == null) return false;
            p.Activo = false;
            p.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Asignaciones ───────────────────────────────────
        public async Task<List<AsignacionCapacitacionResponse>> AsignarPlanAsync(AsignarPlanRequest req)
        {
            var resultados = new List<AsignacionCapacitacionResponse>();
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            foreach (var empId in req.EmpleadoIds)
            {
                var existe = await _db.CapacitacionAsignaciones.AnyAsync(a =>
                    a.PlanId == (uint)req.PlanId && a.EmpleadoId == (uint)empId);

                if (existe) continue;

                var asig = new CapacitacionAsignacion
                {
                    PlanId = (uint)req.PlanId,
                    EmpleadoId = (uint)empId,
                    Estado = "pendiente",
                    AsignadoPor = req.AsignadoPor
                };
                _db.CapacitacionAsignaciones.Add(asig);
                await _db.SaveChangesAsync();

                var result = await _db.CapacitacionAsignaciones
                    .Include(a => a.Empleado)
                    .Include(a => a.Plan)
                    .Include(a => a.Evidencias)
                    .FirstAsync(a => a.Id == asig.Id);

                resultados.Add(MapAsignacion(result));
            }

            return resultados;
        }

        public async Task<List<AsignacionCapacitacionResponse>> ListarAsignacionesEmpleadoAsync(uint empleadoId)
        {
            var lista = await _db.CapacitacionAsignaciones
                .Include(a => a.Plan)
                .Include(a => a.Empleado)
                .Include(a => a.Evidencias)
                .Where(a => a.EmpleadoId == empleadoId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return lista.Select(MapAsignacion).ToList();
        }

        public async Task<AsignacionCapacitacionResponse?> ActualizarAvanceAsync(uint asignacionId, ActualizarAvanceRequest req)
        {
            var a = await _db.CapacitacionAsignaciones
                .Include(x => x.Plan)
                .Include(x => x.Empleado)
                .Include(x => x.Evidencias)
                .FirstOrDefaultAsync(x => x.Id == asignacionId);

            if (a == null) return null;

            a.HorasCompletadas = req.HorasCompletadas;
            a.PorcentajeAvance = req.PorcentajeAvance;
            a.Observaciones = req.Observaciones;
            a.UpdatedAt = DateTime.Now;

            if (req.PorcentajeAvance >= 100 || req.HorasCompletadas >= (a.Plan?.HorasRequeridas ?? 0))
            {
                a.Estado = "completada";
                a.FechaCompletada = DateTime.Now;
            }
            else if (req.HorasCompletadas > 0)
            {
                a.Estado = "en_progreso";
            }

            await _db.SaveChangesAsync();
            return MapAsignacion(a);
        }

        // ── Evidencias ─────────────────────────────────────
        public async Task<EvidenciaResponse> SubirEvidenciaAsync(SubirEvidenciaRequest req)
        {
            // Remover prefijo data:application/pdf;base64, si existe
            var base64 = req.ArchivoBase64;
            var commaIdx = base64.IndexOf(',');
            if (commaIdx >= 0) base64 = base64[(commaIdx + 1)..];

            var bytes = Convert.FromBase64String(base64);

            var evidencia = new CapacitacionEvidencia
            {
                AsignacionId = (uint)req.AsignacionId,
                NombreArchivo = req.NombreArchivo,
                TipoArchivo = req.TipoArchivo,
                ArchivoBase64 = bytes,
                TamanoBytes = bytes.Length,
                Descripcion = req.Descripcion,
                SubidoPor = req.SubidoPor
            };
            _db.CapacitacionEvidencias.Add(evidencia);

            // Marcar asignacion como en progreso si estaba pendiente
            var asig = await _db.CapacitacionAsignaciones.FindAsync((uint)req.AsignacionId);
            if (asig?.Estado == "pendiente")
            {
                asig.Estado = "en_progreso";
                asig.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return MapEvidencia(evidencia);
        }

        public async Task<EvidenciaResponse?> ObtenerEvidenciaAsync(uint evidenciaId)
        {
            var e = await _db.CapacitacionEvidencias.FindAsync(evidenciaId);
            return e == null ? null : MapEvidencia(e);
        }

        public async Task<List<EvidenciaResponse>> ListarEvidenciasAsync(uint asignacionId)
        {
            var lista = await _db.CapacitacionEvidencias
                .Where(e => e.AsignacionId == asignacionId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return lista.Select(MapEvidencia).ToList();
        }

        // ── Alertas CA-04 ──────────────────────────────────
        public async Task<List<AlertaCapacitacionResponse>> ListarAlertasAsync(bool? enviadas)
        {
            var q = _db.CapacitacionAlertas
                .Include(a => a.Asignacion).ThenInclude(x => x!.Empleado)
                .Include(a => a.Asignacion).ThenInclude(x => x!.Plan)
                .AsQueryable();

            if (enviadas.HasValue) q = q.Where(a => a.Enviada == enviadas.Value);

            var lista = await q.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return lista.Select(MapAlerta).ToList();
        }

        public async Task<int> ProcesarAlertasAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var en7dias = hoy.AddDays(7);
            var creadas = 0;

            var asignaciones = await _db.CapacitacionAsignaciones
                .Include(a => a.Plan)
                .Include(a => a.Empleado)
                .Include(a => a.Alertas)
                .Where(a => a.Estado != "completada" && a.Plan!.EsRequerido && a.Plan.Activo)
                .ToListAsync();

            foreach (var asig in asignaciones)
            {
                var limite = asig.Plan!.FechaLimite;

                // Alerta 7 dias antes
                if (limite <= en7dias && limite > hoy)
                {
                    var tieneAlerta7 = asig.Alertas.Any(al => al.TipoAlerta == "vencimiento_7dias");
                    if (!tieneAlerta7)
                    {
                        _db.CapacitacionAlertas.Add(new CapacitacionAlerta
                        {
                            AsignacionId = asig.Id,
                            TipoAlerta = "vencimiento_7dias",
                            Mensaje = $"La capacitacion '{asig.Plan.Nombre}' vence en {(limite.DayNumber - hoy.DayNumber)} dias ({limite:dd/MM/yyyy}). Aun no la has completado.",
                            Enviada = true,
                            FechaEnvio = DateTime.Now
                        });
                        creadas++;
                    }
                }

                // Alerta el dia de vencimiento
                if (limite == hoy)
                {
                    var tieneAlertaHoy = asig.Alertas.Any(al => al.TipoAlerta == "vencimiento_hoy");
                    if (!tieneAlertaHoy)
                    {
                        _db.CapacitacionAlertas.Add(new CapacitacionAlerta
                        {
                            AsignacionId = asig.Id,
                            TipoAlerta = "vencimiento_hoy",
                            Mensaje = $"HOY es el ultimo dia para completar '{asig.Plan.Nombre}'. Sube tu evidencia antes de que venza.",
                            Enviada = true,
                            FechaEnvio = DateTime.Now
                        });
                        creadas++;
                    }
                }

                // Alerta capacitacion no completada despues del vencimiento
                if (limite < hoy)
                {
                    var tieneAlertaNoCom = asig.Alertas.Any(al => al.TipoAlerta == "no_completada");
                    if (!tieneAlertaNoCom)
                    {
                        _db.CapacitacionAlertas.Add(new CapacitacionAlerta
                        {
                            AsignacionId = asig.Id,
                            TipoAlerta = "no_completada",
                            Mensaje = $"El empleado {asig.Empleado?.Nombres} {asig.Empleado?.Apellidos} no completo '{asig.Plan.Nombre}' (vencio el {limite:dd/MM/yyyy}).",
                            Enviada = true,
                            FechaEnvio = DateTime.Now
                        });
                        asig.Estado = "no_completada";
                        asig.UpdatedAt = DateTime.Now;
                        creadas++;
                    }
                }
            }

            if (creadas > 0) await _db.SaveChangesAsync();
            return creadas;
        }

        // ── Reporte CA-03 ──────────────────────────────────
        public async Task<ReporteCapacitacionResponse> GenerarReporteAsync(string? departamentoArea)
        {
            var q = _db.CapacitacionAsignaciones
                .Include(a => a.Empleado)
                .Include(a => a.Plan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(departamentoArea))
                q = q.Where(a => a.Empleado!.DepartamentoArea == departamentoArea);

            var lista = await q.ToListAsync();

            var totalEmpleados = lista.Select(a => a.EmpleadoId).Distinct().Count();
            var totalCapacitaciones = lista.Count;
            var completadas = lista.Count(a => a.Estado == "completada");
            var pendientes = lista.Count(a => a.Estado != "completada");
            var pctCumplimiento = totalCapacitaciones > 0
                ? Math.Round((decimal)completadas / totalCapacitaciones * 100, 2) : 0;
            var totalHoras = lista.Sum(a => a.HorasCompletadas);

            var empleados = lista
                .GroupBy(a => a.EmpleadoId)
                .Select(g =>
                {
                    var completadasEmp = g.Count(a => a.Estado == "completada");
                    var totalEmp = g.Count();
                    return new ResumenEmpleadoCapacitacion(
                        (int)g.Key,
                        g.First().Empleado != null ? $"{g.First().Empleado!.Nombres} {g.First().Empleado.Apellidos}" : "",
                        g.First().Empleado?.DepartamentoArea,
                        totalEmp,
                        completadasEmp,
                        totalEmp - completadasEmp,
                        g.Sum(a => a.HorasCompletadas),
                        totalEmp > 0 ? Math.Round((decimal)completadasEmp / totalEmp * 100, 2) : 0
                    );
                }).OrderByDescending(e => e.PorcentajeCumplimiento).ToList();

            return new ReporteCapacitacionResponse(
                departamentoArea, totalEmpleados, totalCapacitaciones,
                completadas, pendientes, pctCumplimiento, totalHoras, empleados
            );
        }

        // ── Helpers ────────────────────────────────────────
        private static PlanResumenResponse MapPlanResumen(CapacitacionPlan p)
        {
            var completados = p.Asignaciones.Count(a => a.Estado == "completada");
            var total = p.Asignaciones.Count;
            var pct = total > 0
                ? Math.Round(p.Asignaciones.Average(a => a.PorcentajeAvance), 2) : 0;

            return new PlanResumenResponse(
                (int)p.Id, p.Codigo, p.Nombre, p.Descripcion,
                p.Tipo, p.Categoria, p.HorasRequeridas, p.Presupuesto,
                p.FechaInicio.ToString("yyyy-MM-dd"),
                p.FechaFin.ToString("yyyy-MM-dd"),
                p.FechaLimite.ToString("yyyy-MM-dd"),
                p.Instructor, p.Lugar, p.EsRequerido, p.Activo,
                total, completados, total - completados, pct
            );
        }

        private static PlanDetalleResponse MapPlanDetalle(CapacitacionPlan p) =>
            new((int)p.Id, p.Codigo, p.Nombre, p.Descripcion,
                p.Tipo, p.Categoria, p.HorasRequeridas, p.Presupuesto,
                p.FechaInicio.ToString("yyyy-MM-dd"),
                p.FechaFin.ToString("yyyy-MM-dd"),
                p.FechaLimite.ToString("yyyy-MM-dd"),
                p.Instructor, p.Lugar, p.EsRequerido, p.Activo,
                p.Asignaciones.Select(MapAsignacion).ToList()
            );

        private static AsignacionCapacitacionResponse MapAsignacion(CapacitacionAsignacion a)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var limite = a.Plan?.FechaLimite ?? DateOnly.MaxValue;
            var proxVenc = a.Estado != "completada" && limite <= hoy.AddDays(7);

            return new AsignacionCapacitacionResponse(
                (int)a.Id, (int)a.PlanId,
                a.Plan?.Nombre ?? "",
                a.Plan?.Categoria,
                a.Plan?.HorasRequeridas ?? 0,
                limite.ToString("yyyy-MM-dd"),
                a.Plan?.EsRequerido ?? false,
                (int)a.EmpleadoId,
                a.Empleado != null ? $"{a.Empleado.Nombres} {a.Empleado.Apellidos}" : "",
                a.Empleado?.DepartamentoArea,
                a.Estado, a.HorasCompletadas, a.PorcentajeAvance,
                a.FechaCompletada,
                a.Evidencias.Count,
                proxVenc
            );
        }

        private static EvidenciaResponse MapEvidencia(CapacitacionEvidencia e) =>
            new((int)e.Id, (int)e.AsignacionId,
                e.NombreArchivo, e.TipoArchivo,
                Convert.ToBase64String(e.ArchivoBase64),
                e.TamanoBytes, e.Descripcion, e.SubidoPor, e.CreatedAt);

        private static AlertaCapacitacionResponse MapAlerta(CapacitacionAlerta a) =>
            new((int)a.Id, (int)a.AsignacionId,
                a.Asignacion?.Empleado != null
                    ? $"{a.Asignacion.Empleado.Nombres} {a.Asignacion.Empleado.Apellidos}" : "",
                a.Asignacion?.Plan?.Nombre ?? "",
                a.TipoAlerta, a.Mensaje, a.Enviada, a.FechaEnvio, a.CreatedAt);
    }
}