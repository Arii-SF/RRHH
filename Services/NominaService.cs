// ============================================================
//  NominaService — Lógica de negocio del módulo Nómina
// ============================================================
using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface INominaService
    {
        Task<NominaResumenDto?> CrearNominaAsync(CrearNominaRequest request, uint createdBy);
        Task<ProcesarNominaResponse> ProcesarNominaAsync(uint nominaId);
        Task<NominaDetalleDto?> ObtenerNominaConDetallesAsync(uint nominaId);
        Task<List<NominaResumenDto>> ListarNominasAsync();
        Task<ReciboNominaDto?> GenerarReciboAsync(uint nominaId, uint empleadoId);
        Task<List<EmpleadoResumenDto>> ObtenerEmpleadosParaNominaAsync();
        Task<bool> AprobarNominaAsync(uint nominaId, uint aprobadoPor);
        Task<bool> AnularNominaAsync(uint nominaId);
    }

    public class NominaService(AppDbContext db, ILogger<NominaService> logger) : INominaService
    {
        private readonly AppDbContext _db = db;
        private readonly ILogger<NominaService> _logger = logger;

        // ── Crear nómina ──────────────────────────────────────
        public async Task<NominaResumenDto?> CrearNominaAsync(CrearNominaRequest req, uint createdBy)
        {
            bool existe = await _db.Nominas.AnyAsync(n =>
                n.PeriodoInicio == req.PeriodoInicio &&
                n.PeriodoFin == req.PeriodoFin &&
                n.Estado != "anulada");

            if (existe)
                throw new InvalidOperationException("Ya existe una nómina para ese período.");

            var codigo = $"NOM-{req.PeriodoInicio:yyyy}-{req.PeriodoInicio.Month:D2}";

            var nomina = new Nomina
            {
                CodigoNomina = codigo,
                PeriodoInicio = req.PeriodoInicio,
                PeriodoFin = req.PeriodoFin,
                TipoPeriodo = req.TipoPeriodo,
                FechaPago = req.FechaPago,
                Estado = "borrador",
                Observaciones = req.Observaciones,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Nominas.Add(nomina);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Nómina {Codigo} creada (id={Id})", nomina.CodigoNomina, nomina.Id);
            return MapToResumen(nomina);
        }

        // ── Procesar masivo ───────────────────────────────────
        public async Task<ProcesarNominaResponse> ProcesarNominaAsync(uint nominaId)
        {
            var nomina = await _db.Nominas.FindAsync(nominaId)
                         ?? throw new KeyNotFoundException($"Nómina {nominaId} no encontrada.");

            if (nomina.Estado is "aprobada" or "pagada")
                throw new InvalidOperationException("No se puede recalcular una nómina aprobada o pagada.");

            nomina.Estado = "procesando";
            await _db.SaveChangesAsync();

            // Limpiar cálculos anteriores
            var detIds = await _db.NominaDetalleEmpleados
                .Where(d => d.NominaId == nominaId)
                .Select(d => d.Id).ToListAsync();

            if (detIds.Count > 0)
            {
                await _db.NominaRetencioneAplicadas
                    .Where(r => detIds.Contains(r.NominaDetalleId))
                    .ExecuteDeleteAsync();
                await _db.NominaDetalleEmpleados
                    .Where(d => d.NominaId == nominaId)
                    .ExecuteDeleteAsync();
            }

            var catalogoRetenciones = await _db.CatRetencionesFiscales
                .Where(r => r.Activo && (r.VigenteHasta == null || r.VigenteHasta >= DateOnly.FromDateTime(DateTime.Today)))
                .ToListAsync();

            var tramosIsr = await _db.CatIsrTramos
                .Where(t => t.VigenteDe <= DateOnly.FromDateTime(DateTime.Today)
                         && (t.VigenteHasta == null || t.VigenteHasta >= DateOnly.FromDateTime(DateTime.Today)))
                .OrderBy(t => t.RangoDesdeGtq)
                .ToListAsync();

            var empleadosConContrato = await _db.Contratos
                .Where(c => c.Vigente && c.Empleado!.Estado == "activo")
                .Include(c => c.Empleado)
                .ToListAsync();

            int alertasCount = 0;
            decimal totalBruto = 0, totalNeto = 0, totalPatronal = 0;
            var detalles = new List<NominaDetalleEmpleado>();

            foreach (var contrato in empleadosConContrato)
            {
                try
                {
                    var (detalle, retenciones) = CalcularDetalleEmpleado(
                        nominaId, contrato, catalogoRetenciones, tramosIsr);

                    _db.NominaDetalleEmpleados.Add(detalle);
                    await _db.SaveChangesAsync();

                    foreach (var ret in retenciones)
                    {
                        ret.NominaDetalleId = detalle.Id;
                        _db.NominaRetencioneAplicadas.Add(ret);
                    }
                    await _db.SaveChangesAsync();

                    detalles.Add(detalle);
                    totalBruto += detalle.TotalIngresosBruto;
                    totalNeto += detalle.TotalNeto;
                    totalPatronal += detalle.TotalCuotaPatronal;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculando empleado {Id}", contrato.EmpleadoId);
                    _db.NominaAlertas.Add(new NominaAlerta
                    {
                        NominaId = nominaId,
                        EmpleadoId = contrato.EmpleadoId,
                        TipoAlerta = "error_calculo",
                        Descripcion = ex.Message,
                        CreatedAt = DateTime.UtcNow
                    });
                    alertasCount++;
                    await _db.SaveChangesAsync();
                }
            }

            nomina.TotalEmpleados = detalles.Count;
            nomina.TotalBrutoGtq = totalBruto;
            nomina.TotalNetoGtq = totalNeto;
            nomina.TotalDeduccionesGtq = totalBruto - totalNeto;
            nomina.TotalCuotaPatronal = totalPatronal;
            nomina.Estado = "calculada";
            nomina.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new ProcesarNominaResponse(
                nomina.Id, nomina.CodigoNomina, detalles.Count,
                totalBruto, totalNeto, alertasCount,
                alertasCount > 0
                    ? $"Nómina calculada con {alertasCount} alerta(s). Revisar antes de aprobar."
                    : "Nómina calculada correctamente. Lista para aprobación."
            );
        }

        // ── Cálculo fiscal por empleado ───────────────────────
        private static (NominaDetalleEmpleado detalle, List<NominaRetencionAplicada> retenciones)
            CalcularDetalleEmpleado(
                uint nominaId,
                Contrato contrato,
                List<CatRetencionFiscal> catalogo,
                List<CatIsrTramo> tramosIsr)
        {
            var salarioBase = contrato.SalarioBase;
            var bonoDecreto = contrato.BonificacionDecreto;
            var totalBruto = salarioBase + bonoDecreto + contrato.OtrasBonificaciones;

            var retenciones = new List<NominaRetencionAplicada>();

            // IGSS empleado EM (2%)
            var igssEmRet = catalogo.First(r => r.Codigo == "IGSS_EMP_EM");
            var igssEmMonto = Math.Round(salarioBase * (igssEmRet.TasaPorcentaje!.Value / 100), 2);
            retenciones.Add(BuildRetencion(igssEmRet, salarioBase, igssEmMonto, true, false));

            // IGSS empleado IVS (1.83%)
            var igssIvsRet = catalogo.First(r => r.Codigo == "IGSS_EMP_IVS");
            var igssIvsMonto = Math.Round(salarioBase * (igssIvsRet.TasaPorcentaje!.Value / 100), 2);
            retenciones.Add(BuildRetencion(igssIvsRet, salarioBase, igssIvsMonto, true, false));

            // ISR mensual (tabla progresiva Decreto 10-2012)
            var igssAnual = (igssEmMonto + igssIvsMonto) * 12;
            var rentaAnual = (salarioBase * 12) - igssAnual - 48_000m;
            var isrAnual = CalcIsrAnual(rentaAnual, tramosIsr);
            var isrMensual = Math.Round(isrAnual / 12, 2);
            var isrRet = catalogo.First(r => r.Codigo == "ISR_DEP");
            retenciones.Add(BuildRetencion(isrRet, salarioBase, isrMensual, true, false));

            // Cuotas patronales
            void AddPatronal(string codigo, decimal tasa)
            {
                var r = catalogo.FirstOrDefault(x => x.Codigo == codigo);
                if (r == null) return;
                var monto = Math.Round(salarioBase * (tasa / 100), 2);
                retenciones.Add(BuildRetencion(r, salarioBase, monto, false, true));
            }
            AddPatronal("IGSS_PAT_EM", 4.83m);
            AddPatronal("IGSS_PAT_IVS", 3.67m);
            AddPatronal("INTECAP_PAT", 1.00m);
            AddPatronal("IRTRA_PAT", 1.00m);

            var totalDedEmp = retenciones.Where(r => r.EsDeduccionEmpleado).Sum(r => r.MontoRetenido);
            var totalPatronal = retenciones.Where(r => r.EsCuotaPatronal).Sum(r => r.MontoRetenido);
            var totalNeto = totalBruto - totalDedEmp;

            var detalle = new NominaDetalleEmpleado
            {
                NominaId = nominaId,
                EmpleadoId = contrato.EmpleadoId,
                ContratoId = contrato.Id,
                SalarioBase = salarioBase,
                BonificacionDecreto = bonoDecreto,
                OtrasBonificaciones = contrato.OtrasBonificaciones,
                TotalIngresosBruto = totalBruto,
                TotalDeduccionesEmpleado = totalDedEmp,
                TotalCuotaPatronal = totalPatronal,
                TotalNeto = totalNeto,
                Estado = "calculado",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return (detalle, retenciones);
        }

        private static decimal CalcIsrAnual(decimal rentaImponible, List<CatIsrTramo> tramos)
        {
            if (rentaImponible <= 0) return 0m;
            foreach (var tramo in tramos.OrderByDescending(t => t.RangoDesdeGtq))
            {
                if (rentaImponible >= tramo.RangoDesdeGtq)
                    return tramo.CuotaFijaGtq + Math.Round(
                        (rentaImponible - tramo.RangoDesdeGtq) * (tramo.TasaPorcentaje / 100), 2);
            }
            return 0m;
        }

        private static NominaRetencionAplicada BuildRetencion(
            CatRetencionFiscal cat, decimal baseCalc, decimal monto,
            bool esEmp, bool esPat) => new()
            {
                RetencionId = cat.Id,
                CodigoRetencion = cat.Codigo,
                NombreRetencion = cat.Nombre,
                ReferenciaLegal = cat.ReferenciaLegal,
                MetodoCalculo = cat.MetodoCalculo,
                TasaAplicada = cat.TasaPorcentaje,
                BaseCalculoMonto = baseCalc,
                MontoRetenido = monto,
                EsDeduccionEmpleado = esEmp,
                EsCuotaPatronal = esPat,
                CreatedAt = DateTime.UtcNow
            };

        // ── Obtener nómina con detalles ───────────────────────
        public async Task<NominaDetalleDto?> ObtenerNominaConDetallesAsync(uint nominaId)
        {
            var nomina = await _db.Nominas
                .Include(n => n.Detalles).ThenInclude(d => d.Empleado)
                .Include(n => n.Detalles).ThenInclude(d => d.Retenciones)
                .Include(n => n.Alertas)
                .FirstOrDefaultAsync(n => n.Id == nominaId);

            if (nomina == null) return null;

            var detallesDto = nomina.Detalles.Select(d => new DetalleEmpleadoDto(
                d.Id, d.EmpleadoId,
                d.Empleado!.CodigoEmpleado,
                $"{d.Empleado.Nombres} {d.Empleado.Apellidos}",
                d.Empleado.Puesto,
                d.Empleado.DepartamentoArea,
                d.SalarioBase, d.BonificacionDecreto,
                d.OtrasBonificaciones, d.HorasExtrasMonto, d.OtrosIngresos,
                d.TotalIngresosBruto, d.TotalDeduccionesEmpleado,
                d.TotalCuotaPatronal, d.TotalNeto, d.Estado,
                d.Retenciones.Select(r => new RetencionAplicadaDto(
                    r.CodigoRetencion, r.NombreRetencion, r.ReferenciaLegal,
                    r.MetodoCalculo, r.TasaAplicada, r.BaseCalculoMonto,
                    r.MontoRetenido, r.EsDeduccionEmpleado, r.EsCuotaPatronal
                )).ToList()
            )).ToList();

            var alertasDto = nomina.Alertas.Select(a => new AlertaDto(
                a.Id, a.TipoAlerta, a.Descripcion, a.Resuelta, a.EmpleadoId, a.CreatedAt
            )).ToList();

            return new NominaDetalleDto(
                nomina.Id, nomina.CodigoNomina,
                nomina.PeriodoInicio, nomina.PeriodoFin, nomina.FechaPago,
                nomina.Estado, nomina.TotalEmpleados,
                nomina.TotalBrutoGtq, nomina.TotalDeduccionesGtq,
                nomina.TotalNetoGtq, nomina.TotalCuotaPatronal,
                detallesDto, alertasDto);
        }

        // ── Listar nóminas ────────────────────────────────────
        public async Task<List<NominaResumenDto>> ListarNominasAsync() =>
            await _db.Nominas
                .OrderByDescending(n => n.PeriodoInicio)
                .Select(n => MapToResumen(n))
                .ToListAsync();

        // ── Recibo de pago ────────────────────────────────────
        public async Task<ReciboNominaDto?> GenerarReciboAsync(uint nominaId, uint empleadoId)
        {
            var detalle = await _db.NominaDetalleEmpleados
                .Include(d => d.Nomina)
                .Include(d => d.Empleado)
                .Include(d => d.Retenciones)
                .FirstOrDefaultAsync(d => d.NominaId == nominaId && d.EmpleadoId == empleadoId);

            if (detalle == null) return null;

            decimal Get(string cod) => detalle.Retenciones
                .Where(r => r.CodigoRetencion == cod && r.EsDeduccionEmpleado)
                .Sum(r => r.MontoRetenido);
            decimal GetPat(string cod) => detalle.Retenciones
                .Where(r => r.CodigoRetencion == cod && r.EsCuotaPatronal)
                .Sum(r => r.MontoRetenido);

            return new ReciboNominaDto(
                detalle.Nomina!.CodigoNomina,
                detalle.Nomina.PeriodoInicio, detalle.Nomina.PeriodoFin, detalle.Nomina.FechaPago,
                detalle.Empleado!.CodigoEmpleado,
                $"{detalle.Empleado.Nombres} {detalle.Empleado.Apellidos}",
                detalle.Empleado.Puesto, detalle.Empleado.DepartamentoArea,
                detalle.SalarioBase, detalle.BonificacionDecreto,
                detalle.OtrasBonificaciones, detalle.HorasExtrasMonto, detalle.OtrosIngresos,
                detalle.TotalIngresosBruto,
                Get("IGSS_EMP_EM"), Get("IGSS_EMP_IVS"), Get("ISR_DEP"),
                detalle.Retenciones.Where(r => r.EsDeduccionEmpleado
                    && r.CodigoRetencion is not ("IGSS_EMP_EM" or "IGSS_EMP_IVS" or "ISR_DEP"))
                    .Sum(r => r.MontoRetenido),
                detalle.TotalDeduccionesEmpleado, detalle.TotalNeto,
                GetPat("IGSS_PAT_EM"), GetPat("IGSS_PAT_IVS"),
                GetPat("INTECAP_PAT"), GetPat("IRTRA_PAT"),
                detalle.TotalCuotaPatronal
            );
        }

        // ── Empleados activos ─────────────────────────────────
        public async Task<List<EmpleadoResumenDto>> ObtenerEmpleadosParaNominaAsync() =>
            await _db.Contratos
                .Where(c => c.Vigente && c.Empleado!.Estado == "activo")
                .Include(c => c.Empleado)
                .Select(c => new EmpleadoResumenDto(
                    c.Empleado!.Id,
                    c.Empleado.CodigoEmpleado,
                    $"{c.Empleado.Nombres} {c.Empleado.Apellidos}",
                    c.Empleado.Puesto,
                    c.Empleado.DepartamentoArea,
                    c.Empleado.Estado,
                    c.SalarioBase,
                    c.BonificacionDecreto))
                .ToListAsync();

        // ── Aprobar nómina ────────────────────────────────────
        public async Task<bool> AprobarNominaAsync(uint nominaId, uint aprobadoPor)
        {
            var nomina = await _db.Nominas.FindAsync(nominaId);
            if (nomina == null || nomina.Estado != "calculada") return false;
            nomina.Estado = "aprobada";
            nomina.AprobadoPor = aprobadoPor;
            nomina.FechaAprobacion = DateTime.UtcNow;
            nomina.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Anular nómina ─────────────────────────────────────
        public async Task<bool> AnularNominaAsync(uint nominaId)
        {
            var nomina = await _db.Nominas.FindAsync(nominaId);
            if (nomina == null || nomina.Estado is "pagada") return false;
            nomina.Estado = "anulada";
            nomina.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static NominaResumenDto MapToResumen(Nomina n) => new(
            n.Id, n.CodigoNomina, n.PeriodoInicio, n.PeriodoFin, n.FechaPago,
            n.Estado, n.TotalEmpleados, n.TotalBrutoGtq, n.TotalDeduccionesGtq,
            n.TotalNetoGtq, n.TotalCuotaPatronal, n.CreatedAt);
    }
}