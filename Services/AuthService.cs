// ============================================================
//  Services/AuthService.cs
// ============================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest req);
        Task<UsuarioResponse> CrearUsuarioAsync(CrearUsuarioRequest req);
        Task<List<UsuarioResponse>> ListarUsuariosAsync();
        Task<bool> CambiarPasswordAsync(uint usuarioId, CambiarPasswordRequest req);
        Task<bool> DesactivarUsuarioAsync(uint id);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest req)
        {
            var usuario = await _db.Usuarios
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.Email == req.Email && u.Activo);

            if (usuario == null) return null;
            if (usuario.PasswordHash != req.Password) return null;

            usuario.UltimoAcceso = DateTime.Now;
            usuario.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            var token = GenerarToken(usuario);
            var expiracion = DateTime.Now.AddHours(8);

            var nombre = usuario.Empleado != null
                ? $"{usuario.Empleado.Nombres} {usuario.Empleado.Apellidos}"
                : usuario.Email;

            return new LoginResponse(
                token, usuario.Rol, (int)usuario.Id,
                usuario.EmpleadoId.HasValue ? (int)usuario.EmpleadoId.Value : null,
                nombre, usuario.Email, expiracion
            );
        }

        public async Task<UsuarioResponse> CrearUsuarioAsync(CrearUsuarioRequest req)
        {
            var u = new Usuario
            {
                Email = req.Email,
                PasswordHash = req.Password,
                Rol = req.Rol,
                EmpleadoId = req.EmpleadoId.HasValue ? (uint?)req.EmpleadoId.Value : null,
                Activo = true
            };
            _db.Usuarios.Add(u);
            await _db.SaveChangesAsync();

            var result = await _db.Usuarios.Include(x => x.Empleado).FirstAsync(x => x.Id == u.Id);
            return MapUsuario(result);
        }

        public async Task<List<UsuarioResponse>> ListarUsuariosAsync()
        {
            var lista = await _db.Usuarios
                .Include(u => u.Empleado)
                .OrderBy(u => u.Rol)
                .ThenBy(u => u.Email)
                .ToListAsync();
            return lista.Select(MapUsuario).ToList();
        }

        public async Task<bool> CambiarPasswordAsync(uint usuarioId, CambiarPasswordRequest req)
        {
            var u = await _db.Usuarios.FindAsync(usuarioId);
            if (u == null) return false;
            if (u.PasswordHash != req.PasswordActual) return false;

            u.PasswordHash = req.PasswordNuevo;
            u.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DesactivarUsuarioAsync(uint id)
        {
            var u = await _db.Usuarios.FindAsync(id);
            if (u == null) return false;
            u.Activo = false;
            u.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Helpers ────────────────────────────────────────
        private string GenerarToken(Usuario usuario)
        {
            var jwtKey = _config["Jwt:Key"] ?? "ERP_RRHH_GT_SuperSecretKey_2025!";
            var jwtIssuer = _config["Jwt:Issuer"] ?? "ModuloGestionHumana";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email,          usuario.Email),
                new Claim(ClaimTypes.Role,           usuario.Rol),
                new Claim("empleadoId",              usuario.EmpleadoId?.ToString() ?? ""),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UsuarioResponse MapUsuario(Usuario u) =>
            new((int)u.Id, u.Email, u.Rol,
                u.EmpleadoId.HasValue ? (int)u.EmpleadoId.Value : null,
                u.Empleado != null ? $"{u.Empleado.Nombres} {u.Empleado.Apellidos}" : null,
                u.Activo, u.UltimoAcceso);
    }
}