namespace ModuloGestionHumana.DTOs
{
    // ── Normativas ─────────────────────────────────────────
    public record NormativaResponse(
        int Id,
        string Codigo,
        string Nombre,
        string? Descripcion,
        string Categoria,
        string ReferenciaLegal,
        string? DecretoLey,
        DateOnly VigenteDesde,
        DateOnly? VigenteHasta,
        bool Activa,
        decimal? ValorMinimo,
        string? ValorReferencia
    );

    public record ActualizarNormativaRequest(
        string Nombre,
        string? Descripcion,
        decimal? ValorMinimo,
        string? ValorReferencia,
        DateOnly? VigenteHasta,
        bool Activa,
        string DescripcionCambio,
        DateOnly FechaVigencia
    );

    // ── Alertas ────────────────────────────────────────────
    public record AlertaResponse(
        int Id,
        int NormativaId,
        string CodigoNormativa,
        string NombreNormativa,
        string ReferenciaLegal,
        int? EmpleadoId,
        string? NombreEmpleado,
        int? ContratoId,
        string TipoAlerta,
        string Severidad,
        string Titulo,
        string Descripcion,
        bool Resuelta,
        string? ResueltaPor,
        DateTime? FechaResolucion,
        string? NotasResolucion,
        DateTime FechaCreacion
    );

    public record ResolverAlertaRequest(
        string ResueltaPor,
        string? NotasResolucion
    );

    // ── Reporte ────────────────────────────────────────────
    public record ReporteResumenResponse(
        int Id,
        string CodigoReporte,
        string Titulo,
        DateOnly PeriodoInicio,
        DateOnly PeriodoFin,
        string GeneradoPor,
        int TotalEmpleados,
        int TotalAlertas,
        int AlertasCriticas,
        int AlertasResueltas,
        decimal PorcentajeCumplimiento,
        DateTime FechaGeneracion,
        List<AlertaResponse> Alertas
    );

    public record GenerarReporteRequest(
        DateOnly PeriodoInicio,
        DateOnly PeriodoFin,
        string GeneradoPor
    );

    // ── Dashboard ──────────────────────────────────────────
    public record CumplimientoDashboardResponse(
        int TotalNormativas,
        int TotalAlertasActivas,
        int AlertasCriticas,
        int AlertasAdvertencia,
        int AlertasResueltas,
        decimal PorcentajeCumplimiento,
        List<AlertaResponse> AlertasRecientes,
        List<NormativaResponse> NormativasActualizadasReciente
    );
}