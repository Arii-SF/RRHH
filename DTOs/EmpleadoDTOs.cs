namespace ModuloGestionHumana.DTOs
{
    // ── Request: Crear Empleado ────────────────────────────
    public record CrearEmpleadoRequest(
        // Datos personales
        string Nombres,
        string Apellidos,
        string Dpi,
        string Nit,
        string Email,
        string? Telefono,
        DateOnly? FechaNacimiento,
        string? Genero,
        string? EstadoCivil,
        string Nacionalidad,
        string? Direccion,
        string? Municipio,
        string? DepartamentoGeo,
        string? FotoUrl,

        // Datos laborales
        string Puesto,
        string? DepartamentoArea,
        DateOnly FechaIngreso,
        string TipoEmpleado,

        // Contrato
        string TipoContrato,
        decimal SalarioBase,
        decimal BonificacionDecreto,
        decimal OtrasBonificaciones,
        string Jornada,
        decimal HorasSemana,
        string? LugarTrabajo,
        DateOnly? FechaFinContrato,
        string? ObservacionesContrato,

        // Contacto de emergencia (opcional)
        string? ContactoNombre,
        string? ContactoParentesco,
        string? ContactoTelefono
    );

    // ── Request: Actualizar Empleado ───────────────────────
    public record ActualizarEmpleadoRequest(
        string Nombres,
        string Apellidos,
        string Nit,
        string? NoIgss,
        string Email,
        string? Telefono,
        DateOnly? FechaNacimiento,
        string? Genero,
        string? EstadoCivil,
        string Nacionalidad,
        string? Direccion,
        string? Municipio,
        string? DepartamentoGeo,
        string? FotoUrl,
        string Puesto,
        string? DepartamentoArea,
        string TipoEmpleado,
        string Estado
    );

    // ── Response: Empleado Resumen (para lista) ────────────
    public record EmpleadoResumenResponse(
    int Id,
    string CodigoEmpleado,
    string NombreCompleto,
    string Puesto,
    string? DepartamentoArea,
    string Email,
    string? Telefono,
    DateOnly FechaIngreso,
    string TipoEmpleado,
    string Estado,
    string? FotoUrl,
    int MesesAntiguedad,
    decimal SalarioBase,
    bool TieneContrato,
    string? HoraEntradaEsperada,
    string? HoraSalidaEsperada,
    int ToleranciaMinutos
);

    // ── Response: Empleado Detalle completo ────────────────
    public record EmpleadoDetalleResponse(
        int Id,
        string CodigoEmpleado,
        string Nombres,
        string Apellidos,
        string NombreCompleto,
        string Dpi,
        string Nit,
        string? NoIgss,
        string Email,
        string? Telefono,
        DateOnly? FechaNacimiento,
        string? Genero,
        string? EstadoCivil,
        string Nacionalidad,
        string? Direccion,
        string? Municipio,
        string? DepartamentoGeo,
        string Puesto,
        string? DepartamentoArea,
        DateOnly FechaIngreso,
        string TipoEmpleado,
        string Estado,
        string? FotoUrl,
        int MesesAntiguedad,
        ContratoResponse? ContratoVigente,
        List<ContactoEmergenciaResponse> ContactosEmergencia,
        List<OnboardingTareaResponse> OnboardingTareas
    );

    // ── Response: Contrato ─────────────────────────────────
    public record ContratoResponse(
        int Id,
        string CodigoContrato,
        string TipoContrato,
        decimal SalarioBase,
        decimal BonificacionDecreto,
        decimal OtrasBonificaciones,
        string Jornada,
        decimal HorasSemana,
        string? LugarTrabajo,
        DateOnly FechaInicio,
        DateOnly? FechaFin,
        bool Vigente,
        string? Observaciones
    );

    // ── Response: Contacto de Emergencia ──────────────────
    public record ContactoEmergenciaResponse(
        int Id,
        string NombreCompleto,
        string Parentesco,
        string Telefono,
        string? TelefonoAlt,
        bool EsPrincipal
    );

    // ── Response: Onboarding Tarea ─────────────────────────
    public record OnboardingTareaResponse(
        int Id,
        string Tarea,
        string? Descripcion,
        string? Responsable,
        DateOnly? FechaLimite,
        bool Completada,
        DateTime? FechaCompletada,
        int Orden
    );

    // ── Response: Catálogo Departamento ───────────────────
    public record DepartamentoResponse(
        int Id,
        string Nombre,
        string Codigo
    );

    // ── Response: Catálogo Puesto ─────────────────────────
    public record PuestoResponse(
        int Id,
        int DepartamentoId,
        string NombreDepartamento,
        string Nombre,
        string Codigo,
        string? NivelSalarial,
        decimal? SalarioMinimo,
        decimal? SalarioMaximo
    );

    public record ActualizarContratoRequest(
    string TipoContrato,
    decimal SalarioBase,
    string Jornada,
    decimal HorasSemana,
    string? LugarTrabajo
);

    public record ActualizarHorarioRequest(
    string? HoraEntradaEsperada,
    string? HoraSalidaEsperada,
    int ToleranciaMinutos
);
}