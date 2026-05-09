namespace ModuloGestionHumana.DTOs
{
    // ── Ciclos ─────────────────────────────────────────────
    public record CrearCicloRequest(
        string Nombre,
        string? Descripcion,
        string Periodo,
        string FechaInicio,
        string FechaFin,
        string FechaCierre,
        bool IncluyeAutoevaluacion,
        int EscalaMinima,
        int EscalaMaxima,
        List<CrearCriterioRequest> Criterios
    );

    public record CrearCriterioRequest(
        string Nombre,
        string? Descripcion,
        decimal Peso,
        int Orden
    );

    public record CicloResumenResponse(
        int Id,
        string Codigo,
        string Nombre,
        string Periodo,
        string FechaInicio,
        string FechaFin,
        string FechaCierre,
        string Estado,
        bool IncluyeAutoevaluacion,
        int EscalaMinima,
        int EscalaMaxima,
        int TotalParticipantes,
        int EvaluacionesCompletadas,
        int EvaluacionesPendientes,
        decimal PorcentajeAvance
    );

    public record CicloDetalleResponse(
        int Id,
        string Codigo,
        string Nombre,
        string? Descripcion,
        string Periodo,
        string FechaInicio,
        string FechaFin,
        string FechaCierre,
        string Estado,
        bool IncluyeAutoevaluacion,
        int EscalaMinima,
        int EscalaMaxima,
        List<CriterioResponse> Criterios,
        List<ParticipanteResponse> Participantes,
        List<RecordatorioResponse> Recordatorios
    );

    public record CriterioResponse(
        int Id,
        int CicloId,
        string Nombre,
        string? Descripcion,
        decimal Peso,
        int Orden
    );

    // ── Participantes ──────────────────────────────────────
    public record AgregarParticipanteRequest(
        int CicloId,
        int EmpleadoId,
        int EvaluadorId
    );

    public record ParticipanteResponse(
        int Id,
        int CicloId,
        int EmpleadoId,
        string NombreEmpleado,
        string PuestoEmpleado,
        string? AreaEmpleado,
        int EvaluadorId,
        string NombreEvaluador,
        string Estado,
        DateTime? FechaCompletada,
        decimal? PuntajeFinal,
        string? Nivel
    );

    // ── Calificaciones ─────────────────────────────────────
    public record GuardarCalificacionesRequest(
        int ParticipanteId,
        string Tipo,
        List<CalificacionItemRequest> Calificaciones,
        string? ComentariosGenerales,
        string? PlanMejora
    );

    public record CalificacionItemRequest(
        int CriterioId,
        decimal Calificacion,
        string? Comentario
    );

    public record CalificacionResponse(
        int Id,
        int ParticipanteId,
        int CriterioId,
        string NombreCriterio,
        decimal Peso,
        string Tipo,
        decimal Calificacion,
        string? Comentario
    );

    public record EvaluacionDetalleResponse(
        int ParticipanteId,
        string NombreEmpleado,
        string NombreCiclo,
        string Estado,
        List<CriterioResponse> Criterios,
        List<CalificacionResponse> CalificacionesEvaluador,
        List<CalificacionResponse> CalificacionesAuto,
        decimal? PuntajeEvaluador,
        decimal? PuntajeAutoevaluacion,
        decimal? PuntajeFinal,
        string? Nivel,
        string? ComentariosGenerales,
        string? PlanMejora
    );

    // ── Recordatorios ──────────────────────────────────────
    public record RecordatorioResponse(
        int Id,
        int CicloId,
        int DiasAntes,
        bool Enviado,
        DateTime? FechaEnvio,
        int Destinatarios
    );

    // ── Reporte ────────────────────────────────────────────
    public record ReporteDesempenoResponse(
        string Periodo,
        string? DepartamentoArea,
        int TotalEvaluados,
        decimal PromedioGeneral,
        int Excelentes,
        int Buenos,
        int Regulares,
        int Deficientes,
        List<ResumenEmpleadoDesempeno> Empleados
    );

    public record ResumenEmpleadoDesempeno(
        int EmpleadoId,
        string NombreEmpleado,
        string? DepartamentoArea,
        string Puesto,
        decimal? PuntajeFinal,
        string? Nivel,
        string Ciclo,
        string Periodo
    );
}