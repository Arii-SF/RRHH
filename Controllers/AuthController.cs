// ============================================================
//  Controllers/AuthController.cs
// ============================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;
using System.Security.Claims;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _svc;
        public AuthController(IAuthService svc) => _svc = svc;

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _svc.LoginAsync(req);
            if (result == null)
                return Unauthorized(new { success = false, message = "Credenciales incorrectas." });

            return Ok(new { success = true, message = "Login exitoso.", data = result });
        }

        // GET /api/auth/me
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var empId = User.FindFirst("empleadoId")?.Value;

            return Ok(new { success = true, data = new { id, email, rol, empleadoId = empId } });
        }

        // POST /api/auth/usuarios
        [HttpPost("usuarios")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequest req)
        {
            try
            {
                var u = await _svc.CrearUsuarioAsync(req);
                return Ok(new { success = true, message = "Usuario creado.", data = u });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/auth/usuarios
        [HttpGet("usuarios")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ListarUsuarios()
        {
            var lista = await _svc.ListarUsuariosAsync();
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // PUT /api/auth/password
        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest req)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!uint.TryParse(idStr, out var id))
                return Unauthorized();

            var ok = await _svc.CambiarPasswordAsync(id, req);
            if (!ok) return BadRequest(new { success = false, message = "Password actual incorrecto." });
            return Ok(new { success = true, message = "Password actualizado." });
        }

        // DELETE /api/auth/usuarios/5
        [HttpDelete("usuarios/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DesactivarUsuario(uint id)
        {
            var ok = await _svc.DesactivarUsuarioAsync(id);
            if (!ok) return NotFound(new { success = false, message = "Usuario no encontrado." });
            return Ok(new { success = true, message = "Usuario desactivado." });
        }
    }
}