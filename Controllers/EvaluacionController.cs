using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluacionController : ControllerBase
    {
        private readonly IEvaluacionService _svc;
        public EvaluacionController(IEvaluacionService svc) => _svc = svc;

        // GET /api/evaluacion/ciclos?estado=activo
        [HttpGet("ciclos")]
        public async Task<IActionResult> ListarCiclos([FromQuery] string? estado)
        {
            var lista = await _svc.ListarCiclosAsync(estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/evaluacion/ciclos/5
        [HttpGet("ciclos/{id}")]
        public async Task<IActionResult> ObtenerCiclo(uint id)
        {
            var c = await _svc.ObtenerCicloAsync(id);
            if (c == null) return NotFound(new { success = false, message = "Ciclo no encontrado" });
            return Ok(new { success = true, message = "OK", data = c });
        }

        // POST /api/evaluacion/ciclos
        [HttpPost("ciclos")]
        public async Task<IActionResult> CrearCiclo([FromBody] CrearCicloRequest req)
        {
            try
            {
                var c = await _svc.CrearCicloAsync(req);
                return CreatedAtAction(nameof(ObtenerCiclo), new { id = c.Id },
                    new { success = true, message = $"Ciclo {c.Codigo} creado.", data = c });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST /api/evaluacion/ciclos/5/activar
        [HttpPost("ciclos/{id}/activar")]
        public async Task<IActionResult> ActivarCiclo(uint id)
        {
            var c = await _svc.ActivarCicloAsync(id);
            if (c == null) return NotFound(new { success = false, message = "Ciclo no encontrado" });
            return Ok(new { success = true, message = "Ciclo activado.", data = c });
        }

        // POST /api/evaluacion/ciclos/5/cerrar
        [HttpPost("ciclos/{id}/cerrar")]
        public async Task<IActionResult> CerrarCiclo(uint id)
        {
            var c = await _svc.CerrarCicloAsync(id);
            if (c == null) return NotFound(new { success = false, message = "Ciclo no encontrado" });
            return Ok(new { success = true, message = "Ciclo cerrado.", data = c });
        }

        // POST /api/evaluacion/participantes
        [HttpPost("participantes")]
        public async Task<IActionResult> AgregarParticipante([FromBody] AgregarParticipanteRequest req)
        {
            try
            {
                var p = await _svc.AgregarParticipanteAsync(req);
                return Ok(new { success = true, message = "Participante agregado.", data = p });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/evaluacion/participantes/5
        [HttpGet("participantes/{id}")]
        public async Task<IActionResult> ObtenerEvaluacion(uint id)
        {
            var e = await _svc.ObtenerEvaluacionAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Evaluacion no encontrada" });
            return Ok(new { success = true, message = "OK", data = e });
        }

        // POST /api/evaluacion/calificaciones
        [HttpPost("calificaciones")]
        public async Task<IActionResult> GuardarCalificaciones([FromBody] GuardarCalificacionesRequest req)
        {
            try
            {
                var e = await _svc.GuardarCalificacionesAsync(req);
                if (e == null) return NotFound(new { success = false, message = "Participante no encontrado" });
                return Ok(new { success = true, message = "Calificaciones guardadas.", data = e });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/evaluacion/reporte?periodo=Q1-2025&departamentoArea=Tecnologia
        [HttpGet("reporte")]
        public async Task<IActionResult> Reporte(
            [FromQuery] string? periodo,
            [FromQuery] string? departamentoArea)
        {
            var r = await _svc.GenerarReporteAsync(periodo, departamentoArea);
            return Ok(new { success = true, message = "OK", data = r });
        }

        // POST /api/evaluacion/recordatorios/enviar
        [HttpPost("recordatorios/enviar")]
        public async Task<IActionResult> EnviarRecordatorios()
        {
            var enviados = await _svc.EnviarRecordatoriosAsync();
            return Ok(new { success = true, message = $"{enviados} recordatorios procesados.", data = enviados });
        }
    }
}