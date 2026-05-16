namespace ModuloGestionHumana.DTOs
{
    // ── Planes ─────────────────────────────────────────────
    public record CrearPlanRequest(
        string Nombre,
        string? Descripcion,
        string Tipo,
        string? Categoria,
        decimal HorasRequeridas,
        decimal? Presupuesto,
        string FechaInicio,
        string FechaFin,
        string FechaLimite,
        string? Instructor,
        string? Lugar,
        bool EsRequerido
    );

    public record PlanResumenResponse(
        int Id,
        string Codigo,
        string Nombre,
        string? Descripcion,
        string Tipo,
        string? Categoria,
        decimal HorasRequeridas,
        decimal? Presupuesto,
        string FechaInicio,
        string FechaFin,
        string FechaLimite,
        string? Instructor,
        string? Lugar,
        bool EsRequerido,
        bool Activo,
        int TotalAsignados,
        int Completados,
        int Pendientes,
        decimal PorcentajeAvancePromedio
    );

    public record PlanDetalleResponse(
        int Id,
        string Codigo,
        string Nombre,
        string? Descripcion,
        string Tipo,
        string? Categoria,
        decimal HorasRequeridas,
        decimal? Presupuesto,
        string FechaInicio,
        string FechaFin,
        string FechaLimite,
        string? Instructor,
        string? Lugar,
        bool EsRequerido,
        bool Activo,
        List<AsignacionCapacitacionResponse> Asignaciones
    );

    // ── Asignaciones ───────────────────────────────────────
    public record AsignarPlanRequest(
        int PlanId,
        List<int> EmpleadoIds,
        string AsignadoPor
    );

    public record AsignacionCapacitacionResponse(
        int Id,
        int PlanId,
        string NombrePlan,
        string? CategoriaPlan,
        decimal HorasRequeridas,
        string FechaLimite,
        bool EsRequerido,
        int EmpleadoId,
        string NombreEmpleado,
        string? AreaEmpleado,
        string Estado,
        decimal HorasCompletadas,
        decimal PorcentajeAvance,
        DateTime? FechaCompletada,
        int TotalEvidencias,
        bool ProximoVencer
    );

    public record ActualizarAvanceRequest(
        decimal HorasCompletadas,
        decimal PorcentajeAvance,
        string? Observaciones
    );

    // ── Evidencias ─────────────────────────────────────────
    public record SubirEvidenciaRequest(
        int AsignacionId,
        string NombreArchivo,
        string TipoArchivo,
        string ArchivoBase64,
        string? Descripcion,
        string SubidoPor
    );

    public record EvidenciaResponse(
        int Id,
        int AsignacionId,
        string NombreArchivo,
        string TipoArchivo,
        string ArchivoBase64,
        int? TamanoBytes,
        string? Descripcion,
        string? SubidoPor,
        DateTime CreatedAt
    );

    // ── Alertas ────────────────────────────────────────────
    public record AlertaCapacitacionResponse(
        int Id,
        int AsignacionId,
        string NombreEmpleado,
        string NombrePlan,
        string TipoAlerta,
        string Mensaje,
        bool Enviada,
        DateTime? FechaEnvio,
        DateTime CreatedAt
    );

    // ── Reporte CA-03 ──────────────────────────────────────
    public record ReporteCapacitacionResponse(
        string? DepartamentoArea,
        int TotalEmpleados,
        int TotalCapacitaciones,
        int Completadas,
        int Pendientes,
        decimal PorcentajeCumplimiento,
        decimal TotalHorasAcumuladas,
        List<ResumenEmpleadoCapacitacion> Empleados
    );

    public record ResumenEmpleadoCapacitacion(
        int EmpleadoId,
        string NombreEmpleado,
        string? DepartamentoArea,
        int TotalAsignadas,
        int Completadas,
        int Pendientes,
        decimal HorasAcumuladas,
        decimal PorcentajeCumplimiento
    );
}