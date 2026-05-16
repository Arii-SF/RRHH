using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BeneficioController : ControllerBase
    {
        private readonly IBeneficioService _svc;
        public BeneficioController(IBeneficioService svc) => _svc = svc;

        // GET /api/beneficio/tipos
        [HttpGet("tipos")]
        public async Task<IActionResult> ListarTipos()
        {
            var lista = await _svc.ListarTiposAsync();
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/beneficio/paquetes?activo=true
        [HttpGet("paquetes")]
        public async Task<IActionResult> ListarPaquetes([FromQuery] bool? activo)
        {
            var lista = await _svc.ListarPaquetesAsync(activo);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/beneficio/paquetes/5
        [HttpGet("paquetes/{id}")]
        public async Task<IActionResult> ObtenerPaquete(uint id)
        {
            var p = await _svc.ObtenerPaqueteAsync(id);
            if (p == null) return NotFound(new { success = false, message = "Paquete no encontrado." });
            return Ok(new { success = true, message = "OK", data = p });
        }

        // POST /api/beneficio/paquetes
        [HttpPost("paquetes")]
        public async Task<IActionResult> CrearPaquete([FromBody] CrearPaqueteRequest req)
        {
            try
            {
                var p = await _svc.CrearPaqueteAsync(req);
                return Ok(new { success = true, message = "Paquete creado.", data = p });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST /api/beneficio/asignaciones
        [HttpPost("asignaciones")]
        public async Task<IActionResult> AsignarPaquete([FromBody] AsignarPaqueteRequest req)
        {
            try
            {
                var a = await _svc.AsignarPaqueteAsync(req);
                return Ok(new { success = true, message = "Paquete asignado.", data = a });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // DELETE /api/beneficio/asignaciones/5
        [HttpDelete("asignaciones/{id}")]
        public async Task<IActionResult> DesasignarPaquete(uint id)
        {
            var ok = await _svc.DesasignarPaqueteAsync(id);
            if (!ok) return NotFound(new { success = false, message = "Asignacion no encontrada." });
            return Ok(new { success = true, message = "Paquete desasignado." });
        }

        // GET /api/beneficio/empleado/5
        [HttpGet("empleado/{empleadoId}")]
        public async Task<IActionResult> ListarAsignacionesEmpleado(uint empleadoId)
        {
            var lista = await _svc.ListarAsignacionesEmpleadoAsync(empleadoId);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/beneficio/alertas?enviadas=false
        [HttpGet("alertas")]
        public async Task<IActionResult> ListarAlertas([FromQuery] bool? enviadas)
        {
            var lista = await _svc.ListarAlertasAsync(enviadas);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // POST /api/beneficio/alertas/procesar
        [HttpPost("alertas/procesar")]
        public async Task<IActionResult> ProcesarAlertas()
        {
            var enviadas = await _svc.ProcesarAlertasAsync();
            return Ok(new { success = true, message = $"{enviadas} alerta(s) procesada(s).", data = enviadas });
        }

        // POST /api/beneficio/asignaciones/automatico
        [HttpPost("asignaciones/automatico")]
        public async Task<IActionResult> AsignarAutomatico()
        {
            var asignados = await _svc.AsignarAutomaticoAsync();
            return Ok(new { success = true, message = $"{asignados} asignacion(es) automatica(s) realizadas.", data = asignados });
        }

        // GET /api/beneficio/reporte?departamentoArea=Tecnologia
        [HttpGet("reporte")]
        public async Task<IActionResult> Reporte([FromQuery] string? departamentoArea)
        {
            var r = await _svc.GenerarReporteAsync(departamentoArea);
            return Ok(new { success = true, message = "OK", data = r });
        }
    }
}