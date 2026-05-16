namespace ModuloGestionHumana.DTOs
{
    // ── Tipos ──────────────────────────────────────────────
    public record TipoBeneficioResponse(
        int Id, string Nombre, string? Descripcion, bool Activo
    );

    // ── Paquetes ───────────────────────────────────────────
    public record CrearPaqueteRequest(
        string Nombre,
        string? Descripcion,
        string CriterioTipo,
        string? CriterioValor,
        int? AntiguedadMinima,
        List<CrearItemRequest> Items
    );

    public record CrearItemRequest(
        int TipoId,
        string Nombre,
        string? Descripcion,
        decimal? ValorMonetario,
        string Periodicidad,
        string FechaInicio,
        string? FechaFin
    );

    public record PaqueteResumenResponse(
        int Id,
        string Nombre,
        string? Descripcion,
        string CriterioTipo,
        string? CriterioValor,
        int? AntiguedadMinima,
        bool Activo,
        int TotalItems,
        decimal ValorTotalMensual,
        int TotalAsignados
    );

    public record PaqueteDetalleResponse(
        int Id,
        string Nombre,
        string? Descripcion,
        string CriterioTipo,
        string? CriterioValor,
        int? AntiguedadMinima,
        bool Activo,
        List<ItemResponse> Items,
        List<AsignacionResponse> Asignaciones
    );

    public record ItemResponse(
        int Id,
        int PaqueteId,
        int TipoId,
        string NombreTipo,
        string Nombre,
        string? Descripcion,
        decimal? ValorMonetario,
        string Periodicidad,
        string FechaInicio,
        string? FechaFin,
        bool Activo,
        bool ProximoVencer
    );

    // ── Asignaciones ───────────────────────────────────────
    public record AsignarPaqueteRequest(
        int EmpleadoId,
        int PaqueteId,
        string FechaInicio,
        string? FechaFin,
        string AsignadoPor
    );

    public record AsignacionResponse(
        int Id,
        int EmpleadoId,
        string NombreEmpleado,
        int PaqueteId,
        string NombrePaquete,
        string FechaInicio,
        string? FechaFin,
        bool Activo
    );

    // ── Alertas ────────────────────────────────────────────
    public record AlertaBeneficioResponse(
        int Id,
        int ItemId,
        string NombreItem,
        string NombrePaquete,
        int? EmpleadoId,
        string TipoAlerta,
        string Mensaje,
        int DiasAnticipacion,
        bool Enviada,
        DateTime? FechaEnvio,
        DateTime CreatedAt
    );

    // ── Reporte ────────────────────────────────────────────
    public record ReporteBeneficiosResponse(
        string Periodo,
        string? DepartamentoArea,
        int TotalEmpleados,
        decimal CostoTotalMensual,
        decimal CostoTotalAnual,
        List<DesgloseBeneficioResponse> Desglose,
        List<CostoPorAreaResponse> PorArea
    );

    public record DesgloseBeneficioResponse(
        string TipoBeneficio,
        int TotalEmpleados,
        decimal CostoMensual,
        decimal CostoAnual
    );

    public record CostoPorAreaResponse(
        string Area,
        int TotalEmpleados,
        decimal CostoMensual
    );
}