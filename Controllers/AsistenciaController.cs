using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciaController : ControllerBase
    {
        private readonly IAsistenciaService _svc;
        public AsistenciaController(IAsistenciaService svc) => _svc = svc;

        // GET /api/asistencia/horarios
        [HttpGet("horarios")]
        public async Task<IActionResult> Horarios()
        {
            var lista = await _svc.ListarHorariosAsync();
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/asistencia/dashboard?fecha=2025-03-01
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard([FromQuery] string? fecha)
        {
            var f = fecha != null ? DateOnly.Parse(fecha) : (DateOnly?)null;
            var data = await _svc.ObtenerDashboardAsync(f);
            return Ok(new { success = true, message = "OK", data });
        }

        // POST /api/asistencia/registrar
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAsistenciaRequest req)
        {
            try
            {
                var r = await _svc.RegistrarAsistenciaAsync(req);
                return Ok(new { success = true, message = $"{req.Tipo} registrada correctamente.", data = r });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST /api/asistencia/manual
        [HttpPost("manual")]
        public async Task<IActionResult> RegistroManual([FromBody] RegistroManualRequest req)
        {
            var r = await _svc.RegistroManualAsync(req);
            return Ok(new { success = true, message = "Registro manual guardado.", data = r });
        }

        // GET /api/asistencia?fecha=2025-03-01&estado=tardanza
        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? fecha,
            [FromQuery] int? empleadoId,
            [FromQuery] string? estado)
        {
            var f = fecha != null ? DateOnly.Parse(fecha) : (DateOnly?)null;
            var lista = await _svc.ListarAsistenciaAsync(f, empleadoId, estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/asistencia/horas-extras?estado=pendiente
        [HttpGet("horas-extras")]
        public async Task<IActionResult> HorasExtras([FromQuery] string? estado)
        {
            var lista = await _svc.ListarHorasExtrasAsync(estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // POST /api/asistencia/horas-extras/5/aprobar
        [HttpPost("horas-extras/{id}/aprobar")]
        public async Task<IActionResult> AprobarHorasExtras(uint id, [FromBody] AprobarHorasExtrasRequest req)
        {
            var r = await _svc.AprobarHorasExtrasAsync(id, req);
            if (r == null) return NotFound(new { success = false, message = "Solicitud no encontrada" });
            return Ok(new { success = true, message = "Horas extras aprobadas.", data = r });
        }

        // POST /api/asistencia/horas-extras/5/rechazar
        [HttpPost("horas-extras/{id}/rechazar")]
        public async Task<IActionResult> RechazarHorasExtras(uint id, [FromBody] AprobarHorasExtrasRequest req)
        {
            var r = await _svc.RechazarHorasExtrasAsync(id, req);
            if (r == null) return NotFound(new { success = false, message = "Solicitud no encontrada" });
            return Ok(new { success = true, message = "Horas extras rechazadas.", data = r });
        }

        // POST /api/asistencia/ausencias
        [HttpPost("ausencias")]
        public async Task<IActionResult> CrearAusencia([FromBody] CrearAusenciaRequest req)
        {
            var a = await _svc.CrearAusenciaAsync(req);
            return Ok(new { success = true, message = "Ausencia registrada.", data = a });
        }

        // GET /api/asistencia/ausencias?estado=pendiente
        [HttpGet("ausencias")]
        public async Task<IActionResult> ListarAusencias([FromQuery] int? empleadoId, [FromQuery] string? estado)
        {
            var lista = await _svc.ListarAusenciasAsync(empleadoId, estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // POST /api/asistencia/ausencias/5/aprobar
        [HttpPost("ausencias/{id}/aprobar")]
        public async Task<IActionResult> AprobarAusencia(uint id, [FromQuery] string aprobadoPor = "supervisor")
        {
            var a = await _svc.AprobarAusenciaAsync(id, aprobadoPor);
            if (a == null) return NotFound(new { success = false, message = "Ausencia no encontrada" });
            return Ok(new { success = true, message = "Ausencia aprobada.", data = a });
        }

        // GET /api/asistencia/reporte?periodoInicio=2025-01-01&periodoFin=2025-01-31
        [HttpGet("reporte")]
        public async Task<IActionResult> Reporte(
            [FromQuery] string periodoInicio,
            [FromQuery] string periodoFin,
            [FromQuery] string? departamentoArea)
        {
            var r = await _svc.GenerarReporteAsync(periodoInicio, periodoFin, departamentoArea);
            return Ok(new { success = true, message = "OK", data = r });
        }

      
    }
}