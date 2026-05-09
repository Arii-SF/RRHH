using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CumplimientoController : ControllerBase
    {
        private readonly ICumplimientoService _svc;
        public CumplimientoController(ICumplimientoService svc) => _svc = svc;

        // GET /api/cumplimiento/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var data = await _svc.ObtenerDashboardAsync();
            return Ok(new { success = true, message = "OK", data });
        }

        // GET /api/cumplimiento/normativas?categoria=salario
        [HttpGet("normativas")]
        public async Task<IActionResult> ListarNormativas([FromQuery] string? categoria)
        {
            var lista = await _svc.ListarNormativasAsync(categoria);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/cumplimiento/normativas/5
        [HttpGet("normativas/{id}")]
        public async Task<IActionResult> ObtenerNormativa(uint id)
        {
            var n = await _svc.ObtenerNormativaAsync(id);
            if (n == null) return NotFound(new { success = false, message = "Normativa no encontrada" });
            return Ok(new { success = true, message = "OK", data = n });
        }

        // PUT /api/cumplimiento/normativas/5
        [HttpPut("normativas/{id}")]
        public async Task<IActionResult> ActualizarNormativa(uint id, [FromBody] ActualizarNormativaRequest req)
        {
            var n = await _svc.ActualizarNormativaAsync(id, req);
            if (n == null) return NotFound(new { success = false, message = "Normativa no encontrada" });
            return Ok(new { success = true, message = "Normativa actualizada. Administrador notificado.", data = n });
        }

        // GET /api/cumplimiento/alertas?resuelta=false&severidad=critica
        [HttpGet("alertas")]
        public async Task<IActionResult> ListarAlertas(
            [FromQuery] bool? resuelta,
            [FromQuery] string? severidad)
        {
            var lista = await _svc.ListarAlertasAsync(resuelta, severidad);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // POST /api/cumplimiento/alertas/5/resolver
        [HttpPost("alertas/{id}/resolver")]
        public async Task<IActionResult> ResolverAlerta(uint id, [FromBody] ResolverAlertaRequest req)
        {
            var a = await _svc.ResolverAlertaAsync(id, req);
            if (a == null) return NotFound(new { success = false, message = "Alerta no encontrada" });
            return Ok(new { success = true, message = "Alerta marcada como resuelta.", data = a });
        }

        // POST /api/cumplimiento/verificar
        [HttpPost("verificar")]
        public async Task<IActionResult> VerificarCumplimiento()
        {
            var nuevas = await _svc.VerificarCumplimientoEmpleadosAsync();
            return Ok(new { success = true, message = $"Verificación completada. {nuevas} nuevas alertas generadas.", data = nuevas });
        }

        // POST /api/cumplimiento/reportes
        [HttpPost("reportes")]
        public async Task<IActionResult> GenerarReporte([FromBody] GenerarReporteRequest req)
        {
            var reporte = await _svc.GenerarReporteAsync(req);
            return Ok(new { success = true, message = $"Reporte {reporte.CodigoReporte} generado.", data = reporte });
        }

        // GET /api/cumplimiento/reportes
        [HttpGet("reportes")]
        public async Task<IActionResult> ListarReportes()
        {
            var lista = await _svc.ListarReportesAsync();
            return Ok(new { success = true, message = "OK", data = lista });
        }
    }
}