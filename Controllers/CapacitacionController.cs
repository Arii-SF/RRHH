using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CapacitacionController : ControllerBase
    {
        private readonly ICapacitacionService _svc;
        public CapacitacionController(ICapacitacionService svc) => _svc = svc;

        // GET /api/capacitacion/planes?activo=true&categoria=Tecnico
        [HttpGet("planes")]
        public async Task<IActionResult> ListarPlanes(
            [FromQuery] bool? activo,
            [FromQuery] string? categoria)
        {
            var lista = await _svc.ListarPlanesAsync(activo, categoria);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/capacitacion/planes/5
        [HttpGet("planes/{id}")]
        public async Task<IActionResult> ObtenerPlan(uint id)
        {
            var p = await _svc.ObtenerPlanAsync(id);
            if (p == null) return NotFound(new { success = false, message = "Plan no encontrado." });
            return Ok(new { success = true, message = "OK", data = p });
        }

        // POST /api/capacitacion/planes
        [HttpPost("planes")]
        public async Task<IActionResult> CrearPlan([FromBody] CrearPlanRequest req)
        {
            try
            {
                var p = await _svc.CrearPlanAsync(req);
                return Ok(new { success = true, message = "Plan creado.", data = p });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // DELETE /api/capacitacion/planes/5
        [HttpDelete("planes/{id}")]
        public async Task<IActionResult> DesactivarPlan(uint id)
        {
            var ok = await _svc.DesactivarPlanAsync(id);
            if (!ok) return NotFound(new { success = false, message = "Plan no encontrado." });
            return Ok(new { success = true, message = "Plan desactivado." });
        }

        // POST /api/capacitacion/asignaciones
        [HttpPost("asignaciones")]
        public async Task<IActionResult> AsignarPlan([FromBody] AsignarPlanRequest req)
        {
            try
            {
                var lista = await _svc.AsignarPlanAsync(req);
                return Ok(new { success = true, message = $"{lista.Count} asignacion(es) realizadas.", data = lista });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/capacitacion/empleado/5
        [HttpGet("empleado/{empleadoId}")]
        public async Task<IActionResult> ListarAsignacionesEmpleado(uint empleadoId)
        {
            var lista = await _svc.ListarAsignacionesEmpleadoAsync(empleadoId);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // PUT /api/capacitacion/asignaciones/5/avance
        [HttpPut("asignaciones/{id}/avance")]
        public async Task<IActionResult> ActualizarAvance(uint id, [FromBody] ActualizarAvanceRequest req)
        {
            var a = await _svc.ActualizarAvanceAsync(id, req);
            if (a == null) return NotFound(new { success = false, message = "Asignacion no encontrada." });
            return Ok(new { success = true, message = "Avance actualizado.", data = a });
        }

        // POST /api/capacitacion/evidencias
        [HttpPost("evidencias")]
        public async Task<IActionResult> SubirEvidencia([FromBody] SubirEvidenciaRequest req)
        {
            try
            {
                var e = await _svc.SubirEvidenciaAsync(req);
                return Ok(new { success = true, message = "Evidencia subida correctamente.", data = e });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/capacitacion/evidencias/5
        [HttpGet("evidencias/{id}")]
        public async Task<IActionResult> ObtenerEvidencia(uint id)
        {
            var e = await _svc.ObtenerEvidenciaAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Evidencia no encontrada." });
            return Ok(new { success = true, message = "OK", data = e });
        }

        // GET /api/capacitacion/asignaciones/5/evidencias
        [HttpGet("asignaciones/{asignacionId}/evidencias")]
        public async Task<IActionResult> ListarEvidencias(uint asignacionId)
        {
            var lista = await _svc.ListarEvidenciasAsync(asignacionId);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/capacitacion/alertas?enviadas=false
        [HttpGet("alertas")]
        public async Task<IActionResult> ListarAlertas([FromQuery] bool? enviadas)
        {
            var lista = await _svc.ListarAlertasAsync(enviadas);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // POST /api/capacitacion/alertas/procesar
        [HttpPost("alertas/procesar")]
        public async Task<IActionResult> ProcesarAlertas()
        {
            var creadas = await _svc.ProcesarAlertasAsync();
            return Ok(new { success = true, message = $"{creadas} alerta(s) procesada(s).", data = creadas });
        }

        // GET /api/capacitacion/reporte?departamentoArea=Tecnologia
        [HttpGet("reporte")]
        public async Task<IActionResult> Reporte([FromQuery] string? departamentoArea)
        {
            var r = await _svc.GenerarReporteAsync(departamentoArea);
            return Ok(new { success = true, message = "OK", data = r });
        }
    }
}