namespace ModuloGestionHumana.DTOs
{
    // ── Vacantes ───────────────────────────────────────────

    public record CrearVacanteRequest(
        string Titulo,
        string DepartamentoArea,
        string Puesto,
        string Descripcion,
        string? Requisitos,
        decimal? SalarioMinimo,
        decimal? SalarioMaximo,
        string TipoContrato,
        string Jornada,
        string Modalidad,
        int VacantesDisponibles,
        DateOnly? FechaCierre,
        bool CanalInterno,
        bool CanalLinkedin,
        bool CanalComputrabajo,
        bool CanalIndeed,
        string? CanalOtro
    );

    public record VacanteResumenResponse(
        int Id,
        string CodigoVacante,
        string Titulo,
        string DepartamentoArea,
        string Puesto,
        string Estado,
        string Modalidad,
        string Jornada,
        decimal? SalarioMinimo,
        decimal? SalarioMaximo,
        DateOnly? FechaPublicacion,
        DateOnly? FechaCierre,
        int VacantesDisponibles,
        int TotalCandidatos,
        int CandidatosActivos,
        List<string> Canales
    );

    public record VacanteDetalleResponse(
    int Id,
    string CodigoVacante,
    string Titulo,
    string DepartamentoArea,
    string Puesto,
    string Descripcion,
    string? Requisitos,
    string Estado,
    string TipoContrato,
    string Jornada,
    string Modalidad,
    decimal? SalarioMinimo,
    decimal? SalarioMaximo,
    DateOnly? FechaPublicacion,
    DateOnly? FechaCierre,
    int VacantesDisponibles,
    bool CanalInterno,
    bool CanalLinkedin,
    bool CanalComputrabajo,
    bool CanalIndeed,
    string? CanalOtro,
    List<string> Canales,
    List<CandidatoResumenResponse> Candidatos,
    PipelineStats Pipeline
);

    public record PipelineStats(
        int TotalCandidatos,
        int Aplicado,
        int Revision,
        int EntrevistaRh,
        int EntrevistaTecnica,
        int Prueba,
        int Oferta,
        int Contratado,
        int Descartado
    );

    // ── Candidatos ─────────────────────────────────────────

    public record CrearCandidatoRequest(
        int VacanteId,
        string Nombres,
        string Apellidos,
        string Email,
        string? Telefono,
        string? Dpi,
        DateOnly? FechaNacimiento,
        string? Direccion,
        string? LinkedinUrl,
        string? CvUrl,
        string? CartaPresentacion,
        string FuenteAplicacion
    );

    public record CandidatoResumenResponse(
        int Id,
        int VacanteId,
        string NombreCompleto,
        string Email,
        string? Telefono,
        string Etapa,
        int? Puntaje,
        string FuenteAplicacion,
        DateTime FechaAplicacion,
        DateTime? FechaUltimaEtapa,
        bool Contratado
    );

    public record CandidatoDetalleResponse(
        int Id,
        int VacanteId,
        string VacanteTitulo,
        string Nombres,
        string Apellidos,
        string NombreCompleto,
        string Email,
        string? Telefono,
        string? Dpi,
        DateOnly? FechaNacimiento,
        string? Direccion,
        string? LinkedinUrl,
        string? CvUrl,
        string? CartaPresentacion,
        string FuenteAplicacion,
        string Etapa,
        int? Puntaje,
        string? NotasReclutador,
        DateTime FechaAplicacion,
        DateTime? FechaUltimaEtapa,
        int? EmpleadoId,
        List<HistorialEtapaResponse> Historial,
        List<ComunicacionResponse> Comunicaciones
    );

    public record CambiarEtapaRequest(
        string EtapaNueva,
        string? Comentario,
        string RealizadoPor
    );

    public record ActualizarCandidatoRequest(
        int? Puntaje,
        string? NotasReclutador,
        string? LinkedinUrl
    );

    public record ContratarCandidatoRequest(
        // Datos laborales para crear el empleado
        string Puesto,
        string DepartamentoArea,
        DateOnly FechaIngreso,
        string TipoEmpleado,
        string TipoContrato,
        decimal SalarioBase,
        decimal BonificacionDecreto,
        decimal OtrasBonificaciones,
        string Jornada,
        decimal HorasSemana,
        string? LugarTrabajo,
        DateOnly? FechaFinContrato,
        string? Nit,
        string? NoIgss
    );

    public record HistorialEtapaResponse(
        int Id,
        string? EtapaAnterior,
        string EtapaNueva,
        string? Comentario,
        string RealizadoPor,
        DateTime FechaMovimiento
    );

    public record ComunicacionResponse(
        int Id,
        string Etapa,
        string Asunto,
        string Cuerpo,
        bool Enviado,
        DateTime? FechaEnvio
    );
}