using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;
using System.Security.Claims;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PortalController : ControllerBase
    {
        private readonly IPortalService _svc;
        public PortalController(IPortalService svc) => _svc = svc;

        private uint GetEmpleadoId()
        {
            var val = User.FindFirst("empleadoId")?.Value;
            return uint.TryParse(val, out var id) ? id : 0;
        }

        // GET /api/portal/resumen
        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            var r = await _svc.ObtenerResumenAsync(empleadoId);
            if (r == null) return NotFound(new { success = false, message = "Empleado no encontrado." });
            return Ok(new { success = true, message = "OK", data = r });
        }

        // POST /api/portal/solicitudes
        [HttpPost("solicitudes")]
        public async Task<IActionResult> CrearSolicitud([FromBody] CrearSolicitudRequest req)
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            try
            {
                var s = await _svc.CrearSolicitudAsync(empleadoId, req);
                return Ok(new { success = true, message = "Solicitud registrada.", data = s });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/portal/solicitudes
        [HttpGet("solicitudes")]
        public async Task<IActionResult> ListarSolicitudes([FromQuery] string? tipo, [FromQuery] string? estado)
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            var lista = await _svc.ListarSolicitudesAsync(empleadoId, tipo, estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/portal/solicitudes/todas  (solo admin/supervisor)
        [HttpGet("solicitudes/todas")]
        [Authorize(Roles = "admin,supervisor")]
        public async Task<IActionResult> ListarTodasSolicitudes([FromQuery] string? tipo, [FromQuery] string? estado)
        {
            var lista = await _svc.ListarSolicitudesAsync(null, tipo, estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // PUT /api/portal/solicitudes/5/resolver
        [HttpPut("solicitudes/{id}/resolver")]
        [Authorize(Roles = "admin,supervisor")]
        public async Task<IActionResult> ResolverSolicitud(uint id, [FromBody] ResolverSolicitudRequest req)
        {
            var s = await _svc.ResolverSolicitudAsync(id, req);
            if (s == null) return NotFound(new { success = false, message = "Solicitud no encontrada." });
            return Ok(new { success = true, message = $"Solicitud {req.Estado}.", data = s });
        }

        // POST /api/portal/actualizaciones
        [HttpPost("actualizaciones")]
        public async Task<IActionResult> SolicitarActualizacion([FromBody] SolicitarActualizacionRequest req)
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            var a = await _svc.SolicitarActualizacionAsync(empleadoId, req);
            return Ok(new { success = true, message = "Solicitud de actualizacion enviada.", data = a });
        }

        // GET /api/portal/actualizaciones
        [HttpGet("actualizaciones")]
        public async Task<IActionResult> ListarActualizaciones([FromQuery] string? estado)
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            var lista = await _svc.ListarActualizacionesAsync(empleadoId, estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/portal/actualizaciones/todas  (solo admin)
        [HttpGet("actualizaciones/todas")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ListarTodasActualizaciones([FromQuery] string? estado)
        {
            var lista = await _svc.ListarActualizacionesAsync(null, estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // PUT /api/portal/actualizaciones/5/revisar
        [HttpPut("actualizaciones/{id}/revisar")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RevisarActualizacion(uint id, [FromBody] RevisarActualizacionRequest req)
        {
            var a = await _svc.RevisarActualizacionAsync(id, req);
            if (a == null) return NotFound(new { success = false, message = "Solicitud no encontrada." });
            return Ok(new { success = true, message = $"Actualizacion {req.Estado}.", data = a });
        }

        // GET /api/portal/notificaciones
        [HttpGet("notificaciones")]
        public async Task<IActionResult> ListarNotificaciones()
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            var lista = await _svc.ListarNotificacionesAsync(empleadoId);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // PUT /api/portal/notificaciones/5/leer
        [HttpPut("notificaciones/{id}/leer")]
        public async Task<IActionResult> MarcarLeida(uint id)
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            var ok = await _svc.MarcarNotificacionLeidaAsync(id, empleadoId);
            if (!ok) return NotFound(new { success = false, message = "Notificacion no encontrada." });
            return Ok(new { success = true, message = "Notificacion marcada como leida." });
        }

        // PUT /api/portal/notificaciones/leer-todas
        [HttpPut("notificaciones/leer-todas")]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var empleadoId = GetEmpleadoId();
            if (empleadoId == 0) return Forbid();

            var count = await _svc.MarcarTodasLeidasAsync(empleadoId);
            return Ok(new { success = true, message = $"{count} notificaciones marcadas como leidas.", data = count });
        }
    }
}