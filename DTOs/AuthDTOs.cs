// ============================================================
//  DTOs/AuthDTOs.cs
// ============================================================
namespace ModuloGestionHumana.DTOs
{
    public record LoginRequest(
        string Email,
        string Password
    );

    public record LoginResponse(
        string Token,
        string Rol,
        int UsuarioId,
        int? EmpleadoId,
        string NombreCompleto,
        string Email,
        DateTime Expiracion
    );

    public record CrearUsuarioRequest(
        string Email,
        string Password,
        string Rol,
        int? EmpleadoId
    );

    public record UsuarioResponse(
        int Id,
        string Email,
        string Rol,
        int? EmpleadoId,
        string? NombreEmpleado,
        bool Activo,
        DateTime? UltimoAcceso
    );

    public record CambiarPasswordRequest(
        string PasswordActual,
        string PasswordNuevo
    );
}