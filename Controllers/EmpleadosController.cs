using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoService _svc;
        public EmpleadosController(IEmpleadoService svc) => _svc = svc;

        // GET /api/empleados?estado=activo&busqueda=carlos
        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? estado,
            [FromQuery] string? busqueda)
        {
            var lista = await _svc.ListarAsync(estado, busqueda);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/empleados/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(uint id)
        {
            var emp = await _svc.ObtenerAsync(id);
            if (emp == null)
                return NotFound(new { success = false, message = "Empleado no encontrado" });
            return Ok(new { success = true, message = "OK", data = emp });
        }

        // GET /api/empleados/total-activos
        [HttpGet("total-activos")]
        public async Task<IActionResult> TotalActivos()
        {
            var total = await _svc.TotalActivosAsync();
            return Ok(new { success = true, message = "OK", data = total });
        }

        // POST /api/empleados
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearEmpleadoRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });
            try
            {
                var emp = await _svc.CrearAsync(req);
                return CreatedAtAction(nameof(Obtener), new { id = emp.Id },
                    new { success = true, message = $"Empleado {emp.CodigoEmpleado} registrado correctamente.", data = emp });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // PUT /api/empleados/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(uint id, [FromBody] ActualizarEmpleadoRequest req)
        {
            var emp = await _svc.ActualizarAsync(id, req);
            if (emp == null)
                return NotFound(new { success = false, message = "Empleado no encontrado" });
            return Ok(new { success = true, message = "Empleado actualizado correctamente.", data = emp });
        }

        // DELETE /api/empleados/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Desactivar(uint id)
        {
            var ok = await _svc.DesactivarAsync(id);
            if (!ok)
                return NotFound(new { success = false, message = "Empleado no encontrado" });
            return Ok(new { success = true, message = "Empleado desactivado correctamente." });
        }

        // POST /api/empleados/5/onboarding/3/completar
        [HttpPost("{empleadoId}/onboarding/{tareaId}/completar")]
        public async Task<IActionResult> CompletarTarea(uint empleadoId, uint tareaId)
        {
            var ok = await _svc.CompletarTareaOnboardingAsync(empleadoId, tareaId);
            if (!ok)
                return NotFound(new { success = false, message = "Tarea no encontrada" });
            return Ok(new { success = true, message = "Tarea marcada como completada." });
        }

        // GET /api/empleados/catalogos/departamentos
        [HttpGet("catalogos/departamentos")]
        public async Task<IActionResult> Departamentos()
        {
            var lista = await _svc.ListarDepartamentosAsync();
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // GET /api/empleados/catalogos/puestos?departamentoId=1
        [HttpGet("catalogos/puestos")]
        public async Task<IActionResult> Puestos([FromQuery] int? departamentoId)
        {
            var lista = await _svc.ListarPuestosAsync(departamentoId);
            return Ok(new { success = true, message = "OK", data = lista });
        }

        // PUT /api/empleados/5/contrato
        [HttpPut("{id}/contrato")]
        public async Task<IActionResult> ActualizarContrato(uint id, [FromBody] ActualizarContratoRequest req)
        {
            var c = await _svc.ActualizarContratoAsync(id, req);
            if (c == null) return NotFound(new { success = false, message = "Contrato no encontrado" });
            return Ok(new { success = true, message = "Contrato actualizado correctamente.", data = c });
        }

        // PUT /api/empleados/5/horario
        [HttpPut("{id}/horario")]
        public async Task<IActionResult> ActualizarHorario(uint id, [FromBody] ActualizarHorarioRequest req)
        {
            var r = await _svc.ActualizarHorarioAsync(id, req);
            if (r == null) return NotFound(new { success = false, message = "Empleado no encontrado" });
            return Ok(new { success = true, message = "Horario actualizado.", data = r });
        }
    }
}