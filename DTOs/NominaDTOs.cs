// ============================================================
//  DTOs — Módulo Nómina
// ============================================================
using System.ComponentModel.DataAnnotations;

namespace ModuloGestionHumana.DTOs
{
    public record CrearNominaRequest(
        [Required] DateOnly PeriodoInicio,
        [Required] DateOnly PeriodoFin,
        [Required] DateOnly FechaPago,
        string TipoPeriodo = "mensual",
        string? Observaciones = null
    );

    public record NominaResumenDto(
        uint Id,
        string CodigoNomina,
        DateOnly PeriodoInicio,
        DateOnly PeriodoFin,
        DateOnly FechaPago,
        string Estado,
        int TotalEmpleados,
        decimal TotalBrutoGtq,
        decimal TotalDeduccionesGtq,
        decimal TotalNetoGtq,
        decimal TotalCuotaPatronal,
        DateTime CreatedAt
    );

    public record NominaDetalleDto(
        uint NominaId,
        string CodigoNomina,
        DateOnly PeriodoInicio,
        DateOnly PeriodoFin,
        DateOnly FechaPago,
        string Estado,
        int TotalEmpleados,
        decimal TotalBrutoGtq,
        decimal TotalDeduccionesGtq,
        decimal TotalNetoGtq,
        decimal TotalCuotaPatronal,
        List<DetalleEmpleadoDto> Detalles,
        List<AlertaDto> Alertas
    );

    public record DetalleEmpleadoDto(
        uint Id,
        uint EmpleadoId,
        string CodigoEmpleado,
        string NombreEmpleado,
        string Puesto,
        string? DepartamentoArea,
        decimal SalarioBase,
        decimal BonificacionDecreto,
        decimal OtrasBonificaciones,
        decimal HorasExtrasMonto,
        decimal OtrosIngresos,
        decimal TotalIngresosBruto,
        decimal TotalDeduccionesEmpleado,
        decimal TotalCuotaPatronal,
        decimal TotalNeto,
        string Estado,
        List<RetencionAplicadaDto> Retenciones
    );

    public record RetencionAplicadaDto(
        string CodigoRetencion,
        string NombreRetencion,
        string? ReferenciaLegal,
        string MetodoCalculo,
        decimal? TasaAplicada,
        decimal BaseCalculoMonto,
        decimal MontoRetenido,
        bool EsDeduccionEmpleado,
        bool EsCuotaPatronal
    );

    public record ReciboNominaDto(
        string CodigoNomina,
        DateOnly PeriodoInicio,
        DateOnly PeriodoFin,
        DateOnly FechaPago,
        string CodigoEmpleado,
        string NombreEmpleado,
        string Puesto,
        string? DepartamentoArea,
        decimal SalarioBase,
        decimal BonificacionDecreto,
        decimal OtrasBonificaciones,
        decimal HorasExtrasMonto,
        decimal OtrosIngresos,
        decimal TotalIngresosBruto,
        decimal IgssEm,
        decimal IgssIvs,
        decimal IsrMensual,
        decimal OtrasDeducciones,
        decimal TotalDeduccionesEmpleado,
        decimal TotalNeto,
        decimal IgssPatEm,
        decimal IgssPatIvs,
        decimal IntecapPat,
        decimal IrtraPat,
        decimal TotalCuotaPatronal
    );

    public record AlertaDto(
        uint Id,
        string TipoAlerta,
        string Descripcion,
        bool Resuelta,
        uint? EmpleadoId,
        DateTime CreatedAt
    );

    public record EmpleadoResumenDto(
        uint Id,
        string CodigoEmpleado,
        string NombreCompleto,
        string Puesto,
        string? DepartamentoArea,
        string Estado,
        decimal SalarioBase,
        decimal BonificacionDecreto
    );

    public record ApiResponse<T>(
        bool Success,
        string Message,
        T? Data,
        List<string>? Errors = null
    );

    public record ProcesarNominaResponse(
        uint NominaId,
        string CodigoNomina,
        int TotalEmpleados,
        decimal TotalBrutoGtq,
        decimal TotalNetoGtq,
        int AlertasGeneradas,
        string Mensaje
    );
}