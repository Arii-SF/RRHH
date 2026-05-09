using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReclutamientoController : ControllerBase
    {
        private readonly IReclutamientoService _svc;
        public ReclutamientoController(IReclutamientoService svc) => _svc = svc;

        // ── VACANTES ───────────────────────────────────────

        // GET /api/reclutamiento/vacantes?estado=publicada
        [HttpGet("vacantes")]
        public async Task<IActionResult> ListarVacantes([FromQuery] string? estado)
        {
            var lista = await _svc.ListarVacantesAsync(estado);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/reclutamiento/vacantes/5
        [HttpGet("vacantes/{id}")]
        public async Task<IActionResult> ObtenerVacante(uint id)
        {
            var v = await _svc.ObtenerVacanteAsync(id);
            if (v == null) return NotFound(new { success = false, message = "Vacante no encontrada" });
            return Ok(new { success = true, message = "OK", data = v });
        }

        // POST /api/reclutamiento/vacantes
        [HttpPost("vacantes")]
        public async Task<IActionResult> CrearVacante([FromBody] CrearVacanteRequest req)
        {
            try
            {
                var v = await _svc.CrearVacanteAsync(req);
                return CreatedAtAction(nameof(ObtenerVacante), new { id = v.Id },
                    new { success = true, message = $"Vacante {v.CodigoVacante} creada.", data = v });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST /api/reclutamiento/vacantes/5/publicar
        [HttpPost("vacantes/{id}/publicar")]
        public async Task<IActionResult> PublicarVacante(uint id)
        {
            var v = await _svc.PublicarVacanteAsync(id);
            if (v == null) return NotFound(new { success = false, message = "Vacante no encontrada" });
            return Ok(new { success = true, message = "Vacante publicada en los canales seleccionados.", data = v });
        }

        // POST /api/reclutamiento/vacantes/5/cerrar
        [HttpPost("vacantes/{id}/cerrar")]
        public async Task<IActionResult> CerrarVacante(uint id)
        {
            var v = await _svc.CerrarVacanteAsync(id);
            if (v == null) return NotFound(new { success = false, message = "Vacante no encontrada" });
            return Ok(new { success = true, message = "Vacante cerrada.", data = v });
        }

        // ── CANDIDATOS ─────────────────────────────────────

        // POST /api/reclutamiento/candidatos
        [HttpPost("candidatos")]
        public async Task<IActionResult> RegistrarCandidato([FromBody] CrearCandidatoRequest req)
        {
            try
            {
                var c = await _svc.RegistrarCandidatoAsync(req);
                return CreatedAtAction(nameof(ObtenerCandidato), new { id = c.Id },
                    new { success = true, message = $"Candidato {c.NombreCompleto} registrado.", data = c });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET /api/reclutamiento/candidatos/5
        [HttpGet("candidatos/{id}")]
        public async Task<IActionResult> ObtenerCandidato(uint id)
        {
            var c = await _svc.ObtenerCandidatoAsync(id);
            if (c == null) return NotFound(new { success = false, message = "Candidato no encontrado" });
            return Ok(new { success = true, message = "OK", data = c });
        }

        // POST /api/reclutamiento/candidatos/5/etapa
        [HttpPost("candidatos/{id}/etapa")]
        public async Task<IActionResult> CambiarEtapa(uint id, [FromBody] CambiarEtapaRequest req)
        {
            var c = await _svc.CambiarEtapaAsync(id, req);
            if (c == null) return NotFound(new { success = false, message = "Candidato no encontrado" });
            return Ok(new { success = true, message = $"Candidato movido a etapa: {req.EtapaNueva}.", data = c });
        }

        // PUT /api/reclutamiento/candidatos/5
        [HttpPut("candidatos/{id}")]
        public async Task<IActionResult> ActualizarCandidato(uint id, [FromBody] ActualizarCandidatoRequest req)
        {
            var c = await _svc.ActualizarCandidatoAsync(id, req);
            if (c == null) return NotFound(new { success = false, message = "Candidato no encontrado" });
            return Ok(new { success = true, message = "Candidato actualizado.", data = c });
        }

        // POST /api/reclutamiento/candidatos/5/contratar
        [HttpPost("candidatos/{id}/contratar")]
        public async Task<IActionResult> ContratarCandidato(uint id, [FromBody] ContratarCandidatoRequest req)
        {
            try
            {
                var emp = await _svc.ContratarCandidatoAsync(id, req);
                if (emp == null) return NotFound(new { success = false, message = "Candidato no encontrado" });
                return Ok(new { success = true, message = $"Candidato contratado. Empleado {emp.CodigoEmpleado} creado.", data = emp });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}