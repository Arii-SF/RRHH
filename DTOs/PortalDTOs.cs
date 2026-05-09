namespace ModuloGestionHumana.DTOs
{
    // ── Solicitudes ────────────────────────────────────────
    public record CrearSolicitudRequest(
        string Tipo,
        string? Subtipo,
        string? FechaInicio,
        string? FechaFin,
        decimal? DiasSolicitados,
        string? Motivo
    );

    public record SolicitudResponse(
        int Id,
        int EmpleadoId,
        string NombreEmpleado,
        string Tipo,
        string? Subtipo,
        string? FechaInicio,
        string? FechaFin,
        decimal? DiasSolicitados,
        string? Motivo,
        string Estado,
        string? AprobadoPor,
        DateTime? FechaResolucion,
        string? ComentarioRrhh,
        DateTime CreatedAt
    );

    public record ResolverSolicitudRequest(
        string Estado,
        string? Comentario,
        string RevisadoPor
    );

    // ── Actualizaciones de datos ───────────────────────────
    public record SolicitarActualizacionRequest(
        string Campo,
        string ValorNuevo
    );

    public record ActualizacionResponse(
        int Id,
        int EmpleadoId,
        string Campo,
        string? ValorActual,
        string ValorNuevo,
        string Estado,
        string? RevisadoPor,
        DateTime? FechaRevision,
        string? Comentario,
        DateTime CreatedAt
    );

    public record RevisarActualizacionRequest(
        string Estado,
        string? Comentario,
        string RevisadoPor
    );

    // ── Notificaciones ─────────────────────────────────────
    public record NotificacionResponse(
        int Id,
        int EmpleadoId,
        string Titulo,
        string Mensaje,
        string Tipo,
        bool Leida,
        int? ReferenciaId,
        string? ReferenciaTipo,
        DateTime CreatedAt
    );

    // ── Resumen portal ─────────────────────────────────────
    public record PortalResumenResponse(
        int EmpleadoId,
        string NombreCompleto,
        string Puesto,
        string? DepartamentoArea,
        string FechaIngreso,
        string TipoEmpleado,
        decimal SalarioBase,
        int DiasVacacionesDisponibles,
        int SolicitudesPendientes,
        int NotificacionesNoLeidas,
        string? HoraEntradaEsperada,
        string? HoraSalidaEsperada
    );
}