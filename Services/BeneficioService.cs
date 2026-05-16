using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface IBeneficioService
    {
        Task<List<TipoBeneficioResponse>> ListarTiposAsync();
        Task<List<PaqueteResumenResponse>> ListarPaquetesAsync(bool? activo);
        Task<PaqueteDetalleResponse?> ObtenerPaqueteAsync(uint id);
        Task<PaqueteDetalleResponse> CrearPaqueteAsync(CrearPaqueteRequest req);
        Task<AsignacionResponse> AsignarPaqueteAsync(AsignarPaqueteRequest req);
        Task<bool> DesasignarPaqueteAsync(uint asignacionId);
        Task<List<AsignacionResponse>> ListarAsignacionesEmpleadoAsync(uint empleadoId);
        Task<List<AlertaBeneficioResponse>> ListarAlertasAsync(bool? enviadas);
        Task<int> ProcesarAlertasAsync();
        Task<int> AsignarAutomaticoAsync();
        Task<ReporteBeneficiosResponse> GenerarReporteAsync(string? departamentoArea);
    }

    public class BeneficioService : IBeneficioService
    {
        private readonly AppDbContext _db;
        public BeneficioService(AppDbContext db) => _db = db;

        // ── Tipos ──────────────────────────────────────────
        public async Task<List<TipoBeneficioResponse>> ListarTiposAsync()
        {
            return await _db.CatTiposBeneficio
                .Where(t => t.Activo)
                .Select(t => new TipoBeneficioResponse(
                    (int)t.Id, t.Nombre, t.Descripcion, t.Activo))
                .ToListAsync();
        }

        // ── Paquetes ───────────────────────────────────────
        public async Task<List<PaqueteResumenResponse>> ListarPaquetesAsync(bool? activo)
        {
            var q = _db.BeneficioPaquetes
                .Include(p => p.Items)
                .Include(p => p.Asignaciones.Where(a => a.Activo))
                .AsQueryable();

            if (activo.HasValue) q = q.Where(p => p.Activo == activo.Value);

            var lista = await q.OrderBy(p => p.Nombre).ToListAsync();
            return lista.Select(MapPaqueteResumen).ToList();
        }

        public async Task<PaqueteDetalleResponse?> ObtenerPaqueteAsync(uint id)
        {
            var p = await _db.BeneficioPaquetes
                .Include(p => p.Items).ThenInclude(i => i.Tipo)
                .Include(p => p.Asignaciones.Where(a => a.Activo))
                    .ThenInclude(a => a.Empleado)
                .FirstOrDefaultAsync(p => p.Id == id);

            return p == null ? null : MapPaqueteDetalle(p);
        }

        public async Task<PaqueteDetalleResponse> CrearPaqueteAsync(CrearPaqueteRequest req)
        {
            var paquete = new BeneficioPaquete
            {
                Nombre = req.Nombre,
                Descripcion = req.Descripcion,
                CriterioTipo = req.CriterioTipo,
                CriterioValor = req.CriterioValor,
                AntiguedadMinima = req.AntiguedadMinima,
                Activo = true
            };
            _db.BeneficioPaquetes.Add(paquete);
            await _db.SaveChangesAsync();

            foreach (var item in req.Items)
            {
                var bi = new BeneficioItem
                {
                    PaqueteId = paquete.Id,
                    TipoId = (uint)item.TipoId,
                    Nombre = item.Nombre,
                    Descripcion = item.Descripcion,
                    ValorMonetario = item.ValorMonetario,
                    Periodicidad = item.Periodicidad,
                    FechaInicio = DateOnly.Parse(item.FechaInicio),
                    FechaFin = item.FechaFin != null ? DateOnly.Parse(item.FechaFin) : null,
                    Activo = true
                };
                _db.BeneficioItems.Add(bi);
            }
            await _db.SaveChangesAsync();

            return (await ObtenerPaqueteAsync(paquete.Id))!;
        }

        // ── Asignaciones ───────────────────────────────────
        public async Task<AsignacionResponse> AsignarPaqueteAsync(AsignarPaqueteRequest req)
        {
            var asig = new BeneficioAsignacion
            {
                EmpleadoId = (uint)req.EmpleadoId,
                PaqueteId = (uint)req.PaqueteId,
                FechaInicio = DateOnly.Parse(req.FechaInicio),
                FechaFin = req.FechaFin != null ? DateOnly.Parse(req.FechaFin) : null,
                AsignadoPor = req.AsignadoPor,
                Activo = true
            };
            _db.BeneficioAsignaciones.Add(asig);
            await _db.SaveChangesAsync();

            // Crear alerta de nueva asignacion en portal del empleado
            var paquete = await _db.BeneficioPaquetes
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == asig.PaqueteId);

            if (paquete != null)
            {
                foreach (var item in paquete.Items.Where(i => i.Activo && i.FechaFin.HasValue))
                {
                    _db.BeneficioAlertas.Add(new BeneficioAlerta
                    {
                        ItemId = item.Id,
                        EmpleadoId = asig.EmpleadoId,
                        TipoAlerta = "nueva_asignacion",
                        Mensaje = $"Se te ha asignado el beneficio '{item.Nombre}' del paquete '{paquete.Nombre}'.",
                        DiasAnticipacion = 0,
                        Enviada = false
                    });
                }
                await _db.SaveChangesAsync();
            }

            var result = await _db.BeneficioAsignaciones
                .Include(a => a.Empleado)
                .Include(a => a.Paquete)
                .FirstAsync(a => a.Id == asig.Id);

            return MapAsignacion(result);
        }

        public async Task<bool> DesasignarPaqueteAsync(uint asignacionId)
        {
            var asig = await _db.BeneficioAsignaciones.FindAsync(asignacionId);
            if (asig == null) return false;
            asig.Activo = false;
            asig.FechaFin = DateOnly.FromDateTime(DateTime.Now);
            asig.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<AsignacionResponse>> ListarAsignacionesEmpleadoAsync(uint empleadoId)
        {
            var lista = await _db.BeneficioAsignaciones
                .Include(a => a.Empleado)
                .Include(a => a.Paquete).ThenInclude(p => p!.Items).ThenInclude(i => i.Tipo)
                .Where(a => a.EmpleadoId == empleadoId && a.Activo)
                .ToListAsync();

            return lista.Select(MapAsignacion).ToList();
        }

        // ── Alertas CA-03 ──────────────────────────────────
        public async Task<List<AlertaBeneficioResponse>> ListarAlertasAsync(bool? enviadas)
        {
            var q = _db.BeneficioAlertas
                .Include(a => a.Item).ThenInclude(i => i!.Paquete)
                .AsQueryable();

            if (enviadas.HasValue) q = q.Where(a => a.Enviada == enviadas.Value);

            var lista = await q.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return lista.Select(MapAlerta).ToList();
        }

        // CA-03: Procesar alertas de vencimiento 30 dias antes
        public async Task<int> ProcesarAlertasAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoy.AddDays(30);
            var enviados = 0;

            var itemsProximos = await _db.BeneficioItems
                .Include(i => i.Paquete).ThenInclude(p => p!.Asignaciones.Where(a => a.Activo))
                .Where(i => i.Activo && i.FechaFin.HasValue && i.FechaFin <= limite && i.FechaFin >= hoy)
                .ToListAsync();

            foreach (var item in itemsProximos)
            {
                var diasRestantes = item.FechaFin!.Value.DayNumber - hoy.DayNumber;

                // Alerta global para RR.HH.
                var existeGlobal = await _db.BeneficioAlertas.AnyAsync(a =>
                    a.ItemId == item.Id && a.TipoAlerta == "vencimiento" && !a.Enviada && a.EmpleadoId == null);

                if (!existeGlobal)
                {
                    _db.BeneficioAlertas.Add(new BeneficioAlerta
                    {
                        ItemId = item.Id,
                        EmpleadoId = null,
                        TipoAlerta = "vencimiento",
                        Mensaje = $"El beneficio '{item.Nombre}' vence en {diasRestantes} dias ({item.FechaFin:dd/MM/yyyy}).",
                        DiasAnticipacion = diasRestantes,
                        Enviada = true,
                        FechaEnvio = DateTime.Now
                    });
                    enviados++;
                }

                // Alerta por empleado
                if (item.Paquete?.Asignaciones != null)
                {
                    foreach (var asig in item.Paquete.Asignaciones)
                    {
                        var existeEmp = await _db.BeneficioAlertas.AnyAsync(a =>
                            a.ItemId == item.Id && a.EmpleadoId == asig.EmpleadoId &&
                            a.TipoAlerta == "vencimiento" && !a.Enviada);

                        if (!existeEmp)
                        {
                            _db.BeneficioAlertas.Add(new BeneficioAlerta
                            {
                                ItemId = item.Id,
                                EmpleadoId = asig.EmpleadoId,
                                TipoAlerta = "vencimiento",
                                Mensaje = $"Tu beneficio '{item.Nombre}' vence en {diasRestantes} dias.",
                                DiasAnticipacion = diasRestantes,
                                Enviada = true,
                                FechaEnvio = DateTime.Now
                            });
                            enviados++;
                        }
                    }
                }
            }

            if (enviados > 0) await _db.SaveChangesAsync();
            return enviados;
        }

        // CA-01: Asignacion automatica segun criterios
        public async Task<int> AsignarAutomaticoAsync()
        {
            var paquetes = await _db.BeneficioPaquetes
                .Include(p => p.Asignaciones.Where(a => a.Activo))
                .Where(p => p.Activo)
                .ToListAsync();

            var empleados = await _db.Empleados
                .Include(e => e.Contratos.Where(c => c.Vigente))
                .Where(e => e.Estado == "activo")
                .ToListAsync();

            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var asignados = 0;

            foreach (var paquete in paquetes)
            {
                foreach (var emp in empleados)
                {
                    var yaAsignado = paquete.Asignaciones.Any(a => a.EmpleadoId == emp.Id);
                    if (yaAsignado) continue;

                    var meses = (hoy.Year - emp.FechaIngreso.Year) * 12 + hoy.Month - emp.FechaIngreso.Month;
                    var aplica = paquete.CriterioTipo switch
                    {
                        "todos" => true,
                        "puesto" => emp.Puesto == paquete.CriterioValor,
                        "area" => emp.DepartamentoArea == paquete.CriterioValor,
                        "antiguedad" => meses >= (paquete.AntiguedadMinima ?? 0),
                        _ => false
                    };

                    if (aplica)
                    {
                        _db.BeneficioAsignaciones.Add(new BeneficioAsignacion
                        {
                            EmpleadoId = emp.Id,
                            PaqueteId = paquete.Id,
                            FechaInicio = hoy,
                            AsignadoPor = "sistema",
                            Activo = true
                        });
                        asignados++;
                    }
                }
            }

            if (asignados > 0) await _db.SaveChangesAsync();
            return asignados;
        }

        // CA-04: Reporte de costos
        public async Task<ReporteBeneficiosResponse> GenerarReporteAsync(string? departamentoArea)
        {
            var q = _db.BeneficioAsignaciones
                .Include(a => a.Empleado)
                .Include(a => a.Paquete).ThenInclude(p => p!.Items).ThenInclude(i => i.Tipo)
                .Where(a => a.Activo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(departamentoArea))
                q = q.Where(a => a.Empleado!.DepartamentoArea == departamentoArea);

            var asignaciones = await q.ToListAsync();

            var totalEmpleados = asignaciones.Select(a => a.EmpleadoId).Distinct().Count();

            // Calcular costo mensual total
            decimal costoMensual = 0;
            foreach (var asig in asignaciones)
            {
                foreach (var item in asig.Paquete?.Items.Where(i => i.Activo) ?? Enumerable.Empty<BeneficioItem>())
                {
                    costoMensual += item.ValorMonetario.HasValue ? item.Periodicidad switch
                    {
                        "mensual" => item.ValorMonetario.Value,
                        "trimestral" => item.ValorMonetario.Value / 3,
                        "semestral" => item.ValorMonetario.Value / 6,
                        "anual" => item.ValorMonetario.Value / 12,
                        _ => 0
                    } : 0;
                }
            }

            // Desglose por tipo
            var desglose = asignaciones
                .SelectMany(a => a.Paquete?.Items.Where(i => i.Activo) ?? Enumerable.Empty<BeneficioItem>())
                .GroupBy(i => i.Tipo?.Nombre ?? "Sin tipo")
                .Select(g =>
                {
                    var costoMen = g.Sum(i => i.ValorMonetario.HasValue ? i.Periodicidad switch
                    {
                        "mensual" => i.ValorMonetario.Value,
                        "trimestral" => i.ValorMonetario.Value / 3,
                        "semestral" => i.ValorMonetario.Value / 6,
                        "anual" => i.ValorMonetario.Value / 12,
                        _ => 0
                    } : 0);
                    return new DesgloseBeneficioResponse(g.Key, g.Count(), costoMen, costoMen * 12);
                }).ToList();

            // Costo por area
            var porArea = asignaciones
                .GroupBy(a => a.Empleado?.DepartamentoArea ?? "Sin area")
                .Select(g =>
                {
                    var costoArea = g.Sum(a => a.Paquete?.Items.Where(i => i.Activo).Sum(i =>
                        i.ValorMonetario.HasValue ? i.Periodicidad switch
                        {
                            "mensual" => i.ValorMonetario.Value,
                            "trimestral" => i.ValorMonetario.Value / 3,
                            "semestral" => i.ValorMonetario.Value / 6,
                            "anual" => i.ValorMonetario.Value / 12,
                            _ => 0
                        } : 0) ?? 0);
                    return new CostoPorAreaResponse(
                        g.Key,
                        g.Select(a => a.EmpleadoId).Distinct().Count(),
                        costoArea);
                }).ToList();

            return new ReporteBeneficiosResponse(
                DateTime.Now.ToString("yyyy-MM"),
                departamentoArea,
                totalEmpleados,
                costoMensual,
                costoMensual * 12,
                desglose,
                porArea
            );
        }

        // ── Helpers ────────────────────────────────────────
        private static PaqueteResumenResponse MapPaqueteResumen(BeneficioPaquete p)
        {
            var costoMensual = p.Items.Where(i => i.Activo).Sum(i =>
                i.ValorMonetario.HasValue ? i.Periodicidad switch
                {
                    "mensual" => i.ValorMonetario.Value,
                    "trimestral" => i.ValorMonetario.Value / 3,
                    "semestral" => i.ValorMonetario.Value / 6,
                    "anual" => i.ValorMonetario.Value / 12,
                    _ => 0
                } : 0);

            return new PaqueteResumenResponse(
                (int)p.Id, p.Nombre, p.Descripcion, p.CriterioTipo,
                p.CriterioValor, p.AntiguedadMinima, p.Activo,
                p.Items.Count(i => i.Activo),
                costoMensual,
                p.Asignaciones.Count);
        }

        private static PaqueteDetalleResponse MapPaqueteDetalle(BeneficioPaquete p) =>
            new((int)p.Id, p.Nombre, p.Descripcion, p.CriterioTipo,
                p.CriterioValor, p.AntiguedadMinima, p.Activo,
                p.Items.Select(i => new ItemResponse(
                    (int)i.Id, (int)i.PaqueteId, (int)i.TipoId,
                    i.Tipo?.Nombre ?? "", i.Nombre, i.Descripcion,
                    i.ValorMonetario, i.Periodicidad,
                    i.FechaInicio.ToString("yyyy-MM-dd"),
                    i.FechaFin?.ToString("yyyy-MM-dd"),
                    i.Activo,
                    i.FechaFin.HasValue && i.FechaFin.Value <= DateOnly.FromDateTime(DateTime.Now).AddDays(30)
                )).ToList(),
                p.Asignaciones.Select(MapAsignacion).ToList()
            );

        private static AsignacionResponse MapAsignacion(BeneficioAsignacion a) =>
            new((int)a.Id, (int)a.EmpleadoId,
                a.Empleado != null ? $"{a.Empleado.Nombres} {a.Empleado.Apellidos}" : "",
                (int)a.PaqueteId, a.Paquete?.Nombre ?? "",
                a.FechaInicio.ToString("yyyy-MM-dd"),
                a.FechaFin?.ToString("yyyy-MM-dd"),
                a.Activo);

        private static AlertaBeneficioResponse MapAlerta(BeneficioAlerta a) =>
            new((int)a.Id, (int)a.ItemId,
                a.Item?.Nombre ?? "",
                a.Item?.Paquete?.Nombre ?? "",
                a.EmpleadoId.HasValue ? (int)a.EmpleadoId.Value : null,
                a.TipoAlerta, a.Mensaje, a.DiasAnticipacion,
                a.Enviada, a.FechaEnvio, a.CreatedAt);
    }
}