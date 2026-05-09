// ============================================================
//  NominasController — API REST Módulo Nómina
// ============================================================
using Microsoft.AspNetCore.Mvc;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Services;

namespace ModuloGestionHumana.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NominasController(INominaService svc, ILogger<NominasController> logger) : ControllerBase
    {
        private readonly INominaService _svc = svc;
        private readonly ILogger<NominasController> _logger = logger;

        // GET /api/nominas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _svc.ListarNominasAsync();
            return Ok(new ApiResponse<List<NominaResumenDto>>(true, "OK", data));
        }

        // GET /api/nominas/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(uint id)
        {
            var data = await _svc.ObtenerNominaConDetallesAsync(id);
            if (data == null) return NotFound(new ApiResponse<object>(false, "Nómina no encontrada", null));
            return Ok(new ApiResponse<NominaDetalleDto>(true, "OK", data));
        }

        // POST /api/nominas
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearNominaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(false, "Datos inválidos", null,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            try
            {
                uint userId = 1;
                var result = await _svc.CrearNominaAsync(request, userId);
                return CreatedAtAction(nameof(GetById), new { id = result!.Id },
                    new ApiResponse<NominaResumenDto>(true, "Nómina creada correctamente", result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
            }
        }

        // POST /api/nominas/{id}/procesar
        [HttpPost("{id:int}/procesar")]
        public async Task<IActionResult> Procesar(uint id)
        {
            try
            {
                var result = await _svc.ProcesarNominaAsync(id);
                return Ok(new ApiResponse<ProcesarNominaResponse>(true, result.Mensaje, result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message, null));
            }
        }

        // POST /api/nominas/{id}/aprobar
        [HttpPost("{id:int}/aprobar")]
        public async Task<IActionResult> Aprobar(uint id)
        {
            uint userId = 1;
            var ok = await _svc.AprobarNominaAsync(id, userId);
            if (!ok) return BadRequest(new ApiResponse<object>(false,
                "No se puede aprobar. La nómina debe estar en estado 'calculada'.", null));
            return Ok(new ApiResponse<object>(true, "Nómina aprobada correctamente.", null));
        }

        // POST /api/nominas/{id}/anular
        [HttpPost("{id:int}/anular")]
        public async Task<IActionResult> Anular(uint id)
        {
            var ok = await _svc.AnularNominaAsync(id);
            if (!ok) return BadRequest(new ApiResponse<object>(false,
                "No se puede anular una nómina pagada.", null));
            return Ok(new ApiResponse<object>(true, "Nómina anulada.", null));
        }

        // GET /api/nominas/{id}/recibo/{empleadoId}
        [HttpGet("{id:int}/recibo/{empleadoId:int}")]
        public async Task<IActionResult> GetRecibo(uint id, uint empleadoId)
        {
            var recibo = await _svc.GenerarReciboAsync(id, empleadoId);
            if (recibo == null) return NotFound(new ApiResponse<object>(false,
                "Recibo no encontrado.", null));
            return Ok(new ApiResponse<ReciboNominaDto>(true, "OK", recibo));
        }

        // GET /api/nominas/empleados-activos
        [HttpGet("empleados-activos")]
        public async Task<IActionResult> EmpleadosActivos()
        {
            var data = await _svc.ObtenerEmpleadosParaNominaAsync();
            return Ok(new ApiResponse<List<EmpleadoResumenDto>>(true, "OK", data));
        }
    }
}