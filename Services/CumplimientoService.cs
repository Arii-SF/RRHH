using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface ICumplimientoService
    {
        Task<CumplimientoDashboardResponse> ObtenerDashboardAsync();
        Task<List<NormativaResponse>> ListarNormativasAsync(string? categoria);
        Task<NormativaResponse?> ObtenerNormativaAsync(uint id);
        Task<NormativaResponse?> ActualizarNormativaAsync(uint id, ActualizarNormativaRequest req);
        Task<List<AlertaResponse>> ListarAlertasAsync(bool? resuelta, string? severidad);
        Task<AlertaResponse?> ResolverAlertaAsync(uint id, ResolverAlertaRequest req);
        Task<int> VerificarCumplimientoEmpleadosAsync();
        Task<ReporteResumenResponse> GenerarReporteAsync(GenerarReporteRequest req);
        Task<List<ReporteResumenResponse>> ListarReportesAsync();
    }

    public class CumplimientoService : ICumplimientoService
    {
        private readonly AppDbContext _db;
        public CumplimientoService(AppDbContext db) => _db = db;

        // ── Dashboard ──────────────────────────────────────
        public async Task<CumplimientoDashboardResponse> ObtenerDashboardAsync()
        {
            var totalNorm = await _db.CatNormativas.CountAsync(n => n.Activa);
            var alertas = await _db.CumplimientoAlertas.Include(a => a.Normativa).Include(a => a.Empleado).ToListAsync();
            var activas = alertas.Where(a => !a.Resuelta).ToList();
            var criticas = activas.Count(a => a.Severidad == "critica");
            var advertencias = activas.Count(a => a.Severidad == "advertencia");
            var resueltas = alertas.Count(a => a.Resuelta);
            var totalEmp = await _db.Empleados.CountAsync(e => e.Estado == "activo");
            var pct = totalEmp > 0 ? Math.Round(100m - ((decimal)activas.Count / Math.Max(1, totalEmp) * 100), 2) : 100m;
            pct = Math.Max(0, Math.Min(100, pct));

            var recientes = alertas.OrderByDescending(a => a.CreatedAt).Take(5).Select(MapAlerta).ToList();

            var normActualizadas = await _db.NormativaHistorialCambios
                .Include(h => h.Normativa)
                .OrderByDescending(h => h.CreatedAt)
                .Take(5)
                .Select(h => h.Normativa!)
                .Distinct()
                .Select(n => MapNormativa(n))
                .ToListAsync();

            return new CumplimientoDashboardResponse(
                totalNorm, activas.Count, criticas, advertencias, resueltas,
                pct, recientes, normActualizadas
            );
        }

        // ── Normativas ─────────────────────────────────────
        public async Task<List<NormativaResponse>> ListarNormativasAsync(string? categoria)
        {
            var q = _db.CatNormativas.AsQueryable();
            if (!string.IsNullOrEmpty(categoria))
                q = q.Where(n => n.Categoria == categoria);
            var lista = await q.OrderBy(n => n.Categoria).ThenBy(n => n.Nombre).ToListAsync();
            return lista.Select(MapNormativa).ToList();
        }

        public async Task<NormativaResponse?> ObtenerNormativaAsync(uint id)
        {
            var n = await _db.CatNormativas.FindAsync(id);
            return n == null ? null : MapNormativa(n);
        }

        // CA-03: Actualizar normativa y registrar cambio con notificación
        public async Task<NormativaResponse?> ActualizarNormativaAsync(uint id, ActualizarNormativaRequest req)
        {
            var n = await _db.CatNormativas.FindAsync(id);
            if (n == null) return null;

            var valorAnterior = n.ValorMinimo?.ToString() ?? n.ValorReferencia ?? "";

            n.Nombre = req.Nombre;
            n.Descripcion = req.Descripcion;
            n.ValorMinimo = req.ValorMinimo;
            n.ValorReferencia = req.ValorReferencia;
            n.VigenteHasta = req.VigenteHasta;
            n.Activa = req.Activa;
            n.UpdatedAt = DateTime.Now;

            _db.NormativaHistorialCambios.Add(new NormativaHistorialCambio
            {
                NormativaId = n.Id,
                TipoCambio = "actualizacion",
                DescripcionCambio = req.DescripcionCambio,
                ValorAnterior = valorAnterior,
                ValorNuevo = req.ValorMinimo?.ToString() ?? req.ValorReferencia ?? "",
                ReferenciaLegal = n.ReferenciaLegal,
                Notificado = true,
                FechaVigencia = req.FechaVigencia,
                RealizadoPor = "administrador"
            });

            await _db.SaveChangesAsync();
            return MapNormativa(n);
        }

        // ── Alertas ────────────────────────────────────────
        public async Task<List<AlertaResponse>> ListarAlertasAsync(bool? resuelta, string? severidad)
        {
            var q = _db.CumplimientoAlertas
                .Include(a => a.Normativa)
                .Include(a => a.Empleado)
                .AsQueryable();

            if (resuelta.HasValue) q = q.Where(a => a.Resuelta == resuelta.Value);
            if (!string.IsNullOrEmpty(severidad)) q = q.Where(a => a.Severidad == severidad);

            var lista = await q.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return lista.Select(MapAlerta).ToList();
        }

        public async Task<AlertaResponse?> ResolverAlertaAsync(uint id, ResolverAlertaRequest req)
        {
            var a = await _db.CumplimientoAlertas
                .Include(a => a.Normativa)
                .Include(a => a.Empleado)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (a == null) return null;

            a.Resuelta = true;
            a.ResueltaPor = req.ResueltaPor;
            a.FechaResolucion = DateTime.Now;
            a.NotasResolucion = req.NotasResolucion;

            await _db.SaveChangesAsync();
            return MapAlerta(a);
        }

        // CA-01 + CA-04: Verificar cumplimiento de todos los empleados activos
        public async Task<int> VerificarCumplimientoEmpleadosAsync()
        {
            var empleados = await _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .Where(e => e.Estado == "activo")
                .ToListAsync();

            var normativas = await _db.CatNormativas
                .Where(n => n.Activa)
                .ToListAsync();

            var alertasExistentes = await _db.CumplimientoAlertas
                .Where(a => !a.Resuelta)
                .Select(a => new { a.EmpleadoId, a.TipoAlerta, a.NormativaId })
                .ToListAsync();

            int nuevasAlertas = 0;

            foreach (var emp in empleados)
            {
                var contrato = emp.Contratos.FirstOrDefault();

                // Verificar salario mínimo
                var normSalario = normativas.FirstOrDefault(n => n.Codigo == "SAL-MIN-NO-AGR");
                if (normSalario != null && contrato != null)
                {
                    if (contrato.SalarioBase < normSalario.ValorMinimo)
                    {
                        var yaExiste = alertasExistentes.Any(a =>
                            a.EmpleadoId == emp.Id &&
                            a.NormativaId == normSalario.Id &&
                            a.TipoAlerta == "salario");

                        if (!yaExiste)
                        {
                            _db.CumplimientoAlertas.Add(new CumplimientoAlerta
                            {
                                NormativaId = normSalario.Id,
                                EmpleadoId = emp.Id,
                                ContratoId = contrato.Id,
                                TipoAlerta = "salario",
                                Severidad = "critica",
                                Titulo = $"Salario por debajo del mínimo — {emp.Nombres} {emp.Apellidos}",
                                Descripcion = $"El salario base ({contrato.SalarioBase:C}) es menor al salario mínimo vigente ({normSalario.ValorMinimo:C}).",
                                ReferenciaLegal = normSalario.ReferenciaLegal
                            });
                            nuevasAlertas++;
                        }
                    }
                }

                // Verificar bonificación mínima
                var normBon = normativas.FirstOrDefault(n => n.Codigo == "BON-DEC-37");
                if (normBon != null && contrato != null)
                {
                    if (contrato.BonificacionDecreto < normBon.ValorMinimo)
                    {
                        var yaExiste = alertasExistentes.Any(a =>
                            a.EmpleadoId == emp.Id &&
                            a.NormativaId == normBon.Id &&
                            a.TipoAlerta == "beneficio");

                        if (!yaExiste)
                        {
                            _db.CumplimientoAlertas.Add(new CumplimientoAlerta
                            {
                                NormativaId = normBon.Id,
                                EmpleadoId = emp.Id,
                                ContratoId = contrato.Id,
                                TipoAlerta = "beneficio",
                                Severidad = "advertencia",
                                Titulo = $"Bonificación Decreto por debajo del mínimo — {emp.Nombres} {emp.Apellidos}",
                                Descripcion = $"La bonificación decreto ({contrato.BonificacionDecreto:C}) es menor al mínimo legal de {normBon.ValorMinimo:C}.",
                                ReferenciaLegal = normBon.ReferenciaLegal
                            });
                            nuevasAlertas++;
                        }
                    }
                }

                // Verificar jornada máxima
                var jornada = contrato?.Jornada ?? "diurna";
                var codigoJornada = jornada switch
                {
                    "nocturna" => "JOR-NOCTURNA",
                    "mixta" => "JOR-MIXTA",
                    _ => "JOR-DIURNA"
                };
                var normJornada = normativas.FirstOrDefault(n => n.Codigo == codigoJornada);
                if (normJornada != null && contrato != null)
                {
                    if (contrato.HorasSemana > normJornada.ValorMinimo)
                    {
                        var yaExiste = alertasExistentes.Any(a =>
                            a.EmpleadoId == emp.Id &&
                            a.NormativaId == normJornada.Id &&
                            a.TipoAlerta == "jornada");

                        if (!yaExiste)
                        {
                            _db.CumplimientoAlertas.Add(new CumplimientoAlerta
                            {
                                NormativaId = normJornada.Id,
                                EmpleadoId = emp.Id,
                                ContratoId = contrato.Id,
                                TipoAlerta = "jornada",
                                Severidad = "critica",
                                Titulo = $"Jornada excede el máximo legal — {emp.Nombres} {emp.Apellidos}",
                                Descripcion = $"Las horas semanales ({contrato.HorasSemana}h) exceden el máximo permitido para jornada {jornada} ({normJornada.ValorMinimo}h).",
                                ReferenciaLegal = normJornada.ReferenciaLegal
                            });
                            nuevasAlertas++;
                        }
                    }
                }

                // CA-04: Verificar que el contrato tenga campos requeridos
                var normContrato = normativas.FirstOrDefault(n => n.Codigo == "CONT-ESCRITO");
                if (normContrato != null && contrato == null)
                {
                    var yaExiste = alertasExistentes.Any(a =>
                        a.EmpleadoId == emp.Id &&
                        a.NormativaId == normContrato.Id &&
                        a.TipoAlerta == "contrato");

                    if (!yaExiste)
                    {
                        _db.CumplimientoAlertas.Add(new CumplimientoAlerta
                        {
                            NormativaId = normContrato.Id,
                            EmpleadoId = emp.Id,
                            TipoAlerta = "contrato",
                            Severidad = "critica",
                            Titulo = $"Empleado sin contrato vigente — {emp.Nombres} {emp.Apellidos}",
                            Descripcion = $"El empleado {emp.CodigoEmpleado} no tiene un contrato vigente registrado en el sistema.",
                            ReferenciaLegal = normContrato.ReferenciaLegal
                        });
                        nuevasAlertas++;
                    }
                }
            }

            if (nuevasAlertas > 0)
                await _db.SaveChangesAsync();

            return nuevasAlertas;
        }

        // CA-02: Generar reporte de cumplimiento
        public async Task<ReporteResumenResponse> GenerarReporteAsync(GenerarReporteRequest req)
        {
            await VerificarCumplimientoEmpleadosAsync();

            var alertas = await _db.CumplimientoAlertas
                .Include(a => a.Normativa)
                .Include(a => a.Empleado)
                .Where(a => a.CreatedAt >= req.PeriodoInicio.ToDateTime(TimeOnly.MinValue) &&
                            a.CreatedAt <= req.PeriodoFin.ToDateTime(TimeOnly.MaxValue))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var totalEmp = await _db.Empleados.CountAsync(e => e.Estado == "activo");
            var criticas = alertas.Count(a => a.Severidad == "critica");
            var resueltas = alertas.Count(a => a.Resuelta);
            var pct = alertas.Count == 0 ? 100m :
                            Math.Round(100m - ((decimal)alertas.Count(a => !a.Resuelta) / Math.Max(1, totalEmp) * 100), 2);
            pct = Math.Max(0, Math.Min(100, pct));

            var count = await _db.CumplimientoReportes.CountAsync();
            var codigo = $"RPT-{DateTime.Now.Year}-{(count + 1):D4}";

            var reporte = new CumplimientoReporte
            {
                CodigoReporte = codigo,
                Titulo = $"Reporte de Cumplimiento — {req.PeriodoInicio:dd/MM/yyyy} al {req.PeriodoFin:dd/MM/yyyy}",
                PeriodoInicio = req.PeriodoInicio,
                PeriodoFin = req.PeriodoFin,
                GeneradoPor = req.GeneradoPor,
                TotalEmpleados = totalEmp,
                TotalAlertas = alertas.Count,
                AlertasCriticas = criticas,
                AlertasResueltas = resueltas,
                PorcentajeCumplimiento = pct
            };

            _db.CumplimientoReportes.Add(reporte);
            await _db.SaveChangesAsync();

            return new ReporteResumenResponse(
                (int)reporte.Id, codigo, reporte.Titulo,
                req.PeriodoInicio, req.PeriodoFin, req.GeneradoPor,
                totalEmp, alertas.Count, criticas, resueltas, pct,
                reporte.CreatedAt,
                alertas.Select(MapAlerta).ToList()
            );
        }

        public async Task<List<ReporteResumenResponse>> ListarReportesAsync()
        {
            var reportes = await _db.CumplimientoReportes
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reportes.Select(r => new ReporteResumenResponse(
                (int)r.Id, r.CodigoReporte, r.Titulo,
                r.PeriodoInicio, r.PeriodoFin, r.GeneradoPor,
                r.TotalEmpleados, r.TotalAlertas, r.AlertasCriticas,
                r.AlertasResueltas, r.PorcentajeCumplimiento,
                r.CreatedAt, new List<AlertaResponse>()
            )).ToList();
        }

        // ── Helpers ────────────────────────────────────────
        private static NormativaResponse MapNormativa(CatNormativa n) =>
            new((int)n.Id, n.Codigo, n.Nombre, n.Descripcion, n.Categoria,
                n.ReferenciaLegal, n.DecretoLey, n.VigenteDesde, n.VigenteHasta,
                n.Activa, n.ValorMinimo, n.ValorReferencia);

        private static AlertaResponse MapAlerta(CumplimientoAlerta a) =>
            new((int)a.Id, (int)a.NormativaId,
                a.Normativa?.Codigo ?? "", a.Normativa?.Nombre ?? "",
                a.ReferenciaLegal,
                a.EmpleadoId.HasValue ? (int)a.EmpleadoId.Value : null,
                a.Empleado != null ? $"{a.Empleado.Nombres} {a.Empleado.Apellidos}" : null,
                a.ContratoId.HasValue ? (int)a.ContratoId.Value : null,
                a.TipoAlerta, a.Severidad, a.Titulo, a.Descripcion,
                a.Resuelta, a.ResueltaPor, a.FechaResolucion,
                a.NotasResolucion, a.CreatedAt);
    }
}