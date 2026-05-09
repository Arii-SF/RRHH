// ============================================================
//  DTOs — HU-004 Asistencia
// ============================================================
namespace ModuloGestionHumana.DTOs
{
    public record HorarioResponse(
        int Id, string Nombre, string? Descripcion, string Jornada,
        string HoraEntrada, string HoraSalida, decimal HorasDia,
        string DiasSemana, int ToleranciaMin, bool Activo
    );

    public record RegistrarAsistenciaRequest(
        int EmpleadoId,
        string Tipo,           // "entrada" | "salida"
        string Metodo,         // "biometrico" | "app" | "manual" | "qr"
        string? Ubicacion,
        string? RegistradoPor,
        string? Observaciones
    );

    public record RegistroManualRequest(
        int EmpleadoId,
        string Fecha,
        string? HoraEntrada,
        string? HoraSalida,
        string Estado,
        string? Observaciones,
        string RegistradoPor
    );

    public record AsistenciaResponse(
        int Id, int EmpleadoId, string NombreEmpleado, string CodigoEmpleado,
        string? DepartamentoArea, string Puesto,
        string Fecha, string? HoraEntrada, string? HoraSalida,
        string MetodoEntrada, string? MetodoSalida,
        decimal? HorasTrabajadas, decimal HorasExtras,
        string Estado, int TardanzaMinutos, string? Observaciones
    );

    public record DashboardAsistenciaResponse(
        string Fecha,
        int TotalEmpleados,
        int Presentes,
        int Ausentes,
        int Tardanzas,
        int Permisos,
        int SinRegistro,
        decimal PorcentajeAsistencia,
        List<AsistenciaResponse> Registros
    );

    public record HorasExtrasResponse(
        int Id, int EmpleadoId, string NombreEmpleado,
        string Fecha, decimal HorasSolicitadas,
        string Motivo, string Estado,
        string? AprobadoPor, DateTime? FechaResolucion
    );

    public record AprobarHorasExtrasRequest(
        string AprobadoPor,
        string? Observaciones
    );

    public record AusenciaResponse(
        int Id, int EmpleadoId, string NombreEmpleado,
        string Tipo, string FechaInicio, string FechaFin,
        int DiasHabiles, string? Motivo, string Estado,
        string? AprobadoPor
    );

    public record CrearAusenciaRequest(
        int EmpleadoId,
        string Tipo,
        string FechaInicio,
        string FechaFin,
        string? Motivo
    );

    public record ReporteAsistenciaResponse(
        string Periodo,
        string? DepartamentoArea,
        int TotalEmpleados,
        int TotalDiasHabiles,
        int TotalPresencias,
        int TotalAusencias,
        int TotalTardanzas,
        decimal PorcentajeAsistencia,
        decimal TotalHorasExtras,
        List<ResumenEmpleadoAsistencia> Empleados
    );

    public record ResumenEmpleadoAsistencia(
        int EmpleadoId, string NombreEmpleado, string CodigoEmpleado,
        string? DepartamentoArea, int DiasPresente, int DiasAusente,
        int DiasTardanza, int DiasPermiso, decimal HorasExtras,
        decimal PorcentajeAsistencia
    );

    public record AsignarHorarioRequest(
    int EmpleadoId,
    int HorarioId,
    string FechaInicio
);

    public record EmpleadoHorarioResponse(
        int Id,
        int EmpleadoId,
        string NombreEmpleado,
        string CodigoEmpleado,
        int HorarioId,
        string NombreHorario,
        string HoraEntrada,
        string HoraSalida,
        string Jornada,
        string DiasSemana,
        int ToleranciaMin,
        string FechaInicio,
        string? FechaFin,
        bool Activo
    );
}