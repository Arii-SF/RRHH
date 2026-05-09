using Microsoft.EntityFrameworkCore;
using ModuloGestionHumana.Data;
using ModuloGestionHumana.DTOs;
using ModuloGestionHumana.Models;

namespace ModuloGestionHumana.Services
{
    public interface IAsistenciaService
    {
        Task<List<HorarioResponse>> ListarHorariosAsync();
        Task<DashboardAsistenciaResponse> ObtenerDashboardAsync(DateOnly? fecha);
        Task<AsistenciaResponse> RegistrarAsistenciaAsync(RegistrarAsistenciaRequest req);
        Task<AsistenciaResponse> RegistroManualAsync(RegistroManualRequest req);
        Task<List<AsistenciaResponse>> ListarAsistenciaAsync(DateOnly? fecha, int? empleadoId, string? estado);
        Task<List<HorasExtrasResponse>> ListarHorasExtrasAsync(string? estado);
        Task<HorasExtrasResponse?> AprobarHorasExtrasAsync(uint id, AprobarHorasExtrasRequest req);
        Task<HorasExtrasResponse?> RechazarHorasExtrasAsync(uint id, AprobarHorasExtrasRequest req);
        Task<AusenciaResponse> CrearAusenciaAsync(CrearAusenciaRequest req);
        Task<List<AusenciaResponse>> ListarAusenciasAsync(int? empleadoId, string? estado);
        Task<AusenciaResponse?> AprobarAusenciaAsync(uint id, string aprobadoPor);
        Task<ReporteAsistenciaResponse> GenerarReporteAsync(string periodoInicio, string periodoFin, string? departamentoArea);
    }

    public class AsistenciaService : IAsistenciaService
    {
        private readonly AppDbContext _db;
        public AsistenciaService(AppDbContext db) => _db = db;

        private static DateTime AhoraGuatemala()
        {
            try
            {
                // Windows
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch { }
            try
            {
                // Linux/Mac
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch { }
            return DateTime.Now;
        }

        public async Task<List<HorarioResponse>> ListarHorariosAsync()
        {
            var empleados = await _db.Empleados
                .Where(e => e.Estado == "activo" && e.HoraEntradaEsperada != null)
                .ToListAsync();

            return empleados
                .GroupBy(e => new { e.HoraEntradaEsperada, e.HoraSalidaEsperada, e.ToleranciaMinutos })
                .Select((g, i) => new HorarioResponse(
                    i + 1,
                    $"{g.Key.HoraEntradaEsperada:HH\\:mm} - {g.Key.HoraSalidaEsperada:HH\\:mm}",
                    null, "diurna",
                    g.Key.HoraEntradaEsperada?.ToString("HH:mm") ?? "",
                    g.Key.HoraSalidaEsperada?.ToString("HH:mm") ?? "",
                    8m, "L,M,X,J,V",
                    g.Key.ToleranciaMinutos,
                    true
                )).ToList();
        }

        // CA-02: Dashboard en tiempo real
        public async Task<DashboardAsistenciaResponse> ObtenerDashboardAsync(DateOnly? fecha)
        {
            var ahora = AhoraGuatemala();
            var hoy = fecha ?? DateOnly.FromDateTime(ahora);
            var empleados = await _db.Empleados.Where(e => e.Estado == "activo").ToListAsync();
            var registros = await _db.AsistenciaRegistros
                .Include(r => r.Empleado)
                .Where(r => r.Fecha == hoy)
                .ToListAsync();

            var presentes = registros.Count(r => r.Estado == "presente");
            var tardanzas = registros.Count(r => r.Estado == "tardanza");
            var permisos = registros.Count(r => r.Estado == "permiso" || r.Estado == "vacacion");
            var ausentes = registros.Count(r => r.Estado == "ausente");
            var sinReg = empleados.Count - registros.Count;
            var pct = empleados.Count > 0
                ? Math.Round((decimal)(presentes + tardanzas) / empleados.Count * 100, 2) : 0;

            return new DashboardAsistenciaResponse(
                hoy.ToString("yyyy-MM-dd"),
                empleados.Count, presentes, ausentes, tardanzas, permisos,
                Math.Max(0, sinReg), pct,
                registros.Select(MapAsistencia).ToList()
            );
        }

        // CA-01: Registrar entrada o salida
        public async Task<AsistenciaResponse> RegistrarAsistenciaAsync(RegistrarAsistenciaRequest req)
        {
            var ahora = AhoraGuatemala();
            var hoy = DateOnly.FromDateTime(ahora);
            var empleadoId = (uint)req.EmpleadoId;

            // Cargar empleado — tiene el horario directamente en sus columnas
            var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId);
            if (empleado == null)
                throw new InvalidOperationException("Empleado no encontrado.");

            var registro = await _db.AsistenciaRegistros
                .Include(r => r.Empleado)
                .FirstOrDefaultAsync(r => r.EmpleadoId == empleadoId && r.Fecha == hoy);

            if (req.Tipo == "entrada")
            {
                if (registro != null)
                    throw new InvalidOperationException("Ya existe un registro de entrada para hoy.");

                int tardanza = 0;
                string estado = "presente";

                if (empleado.HoraEntradaEsperada.HasValue)
                {
                    var horaEsperada = empleado.HoraEntradaEsperada.Value.ToTimeSpan();
                    var horaActual = ahora.TimeOfDay;
                    var limiteEntrada = horaEsperada.Add(TimeSpan.FromMinutes(empleado.ToleranciaMinutos));

                    if (horaActual > limiteEntrada)
                    {
                        tardanza = (int)Math.Round((horaActual - horaEsperada).TotalMinutes);
                        estado = "tardanza";
                    }
                }

                registro = new AsistenciaRegistro
                {
                    EmpleadoId = empleadoId,
                    Fecha = hoy,
                    HoraEntrada = ahora,
                    MetodoEntrada = req.Metodo,
                    UbicacionEntrada = req.Ubicacion,
                    Estado = estado,
                    TardanzaMinutos = tardanza,
                    Observaciones = req.Observaciones,
                    RegistradoPor = req.RegistradoPor
                };

                _db.AsistenciaRegistros.Add(registro);
            }
            else // salida
            {
                if (registro == null)
                    throw new InvalidOperationException("No existe registro de entrada para hoy.");

                registro.HoraSalida = ahora;
                registro.MetodoSalida = req.Metodo;
                registro.UbicacionSalida = req.Ubicacion;
                registro.UpdatedAt = ahora;

                if (registro.HoraEntrada.HasValue)
                {
                    var trabajadas = (decimal)(ahora - registro.HoraEntrada.Value).TotalHours;
                    registro.HorasTrabajadas = Math.Round(trabajadas, 2);

                    // CA-03: Calcular horas extras usando horario del empleado
                    if (empleado.HoraEntradaEsperada.HasValue && empleado.HoraSalidaEsperada.HasValue)
                    {
                        var entrada = empleado.HoraEntradaEsperada.Value.ToTimeSpan();
                        var salida = empleado.HoraSalidaEsperada.Value.ToTimeSpan();
                        var horasDia = (decimal)(salida - entrada).TotalHours;

                        if (trabajadas > horasDia)
                        {
                            var extras = Math.Round(trabajadas - horasDia, 2);
                            registro.HorasExtras = extras;

                            _db.HorasExtrasSolicitudes.Add(new HorasExtrasSolicitud
                            {
                                EmpleadoId = empleadoId,
                                AsistenciaId = registro.Id,
                                Fecha = hoy,
                                HorasSolicitadas = extras,
                                Motivo = $"Horas extras generadas automaticamente el {hoy}",
                                Estado = "pendiente"
                            });
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();

            var result = await _db.AsistenciaRegistros
                .Include(r => r.Empleado)
                .FirstAsync(r => r.Id == registro.Id);

            return MapAsistencia(result);
        }

        // Registro manual (supervisor)
        public async Task<AsistenciaResponse> RegistroManualAsync(RegistroManualRequest req)
        {
            var empleadoId = (uint)req.EmpleadoId;
            var fecha = DateOnly.Parse(req.Fecha);

            var existente = await _db.AsistenciaRegistros
                .Include(r => r.Empleado)
                .FirstOrDefaultAsync(r => r.EmpleadoId == empleadoId && r.Fecha == fecha);

            if (existente != null)
            {
                existente.HoraEntrada = req.HoraEntrada != null ? DateTime.Parse(req.HoraEntrada) : existente.HoraEntrada;
                existente.HoraSalida = req.HoraSalida != null ? DateTime.Parse(req.HoraSalida) : existente.HoraSalida;
                existente.Observaciones = req.Observaciones;
                existente.RegistradoPor = req.RegistradoPor;
                existente.UpdatedAt = AhoraGuatemala();

                // Recalcular tardanza si se modificó la hora de entrada
                if (req.HoraEntrada != null && req.Estado == "presente")
                {
                    var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId);
                    if (empleado?.HoraEntradaEsperada != null)
                    {
                        var horaEsperada = empleado.HoraEntradaEsperada.Value.ToTimeSpan();
                        var horaEntrada = DateTime.Parse(req.HoraEntrada).TimeOfDay;
                        var limiteEntrada = horaEsperada.Add(TimeSpan.FromMinutes(empleado.ToleranciaMinutos));

                        if (horaEntrada > limiteEntrada)
                        {
                            existente.TardanzaMinutos = (int)Math.Round((horaEntrada - horaEsperada).TotalMinutes);
                            existente.Estado = "tardanza";
                        }
                        else
                        {
                            existente.TardanzaMinutos = 0;
                            existente.Estado = "presente";
                        }
                    }
                    else
                    {
                        existente.Estado = req.Estado;
                    }
                }
                else
                {
                    existente.Estado = req.Estado;
                }

                if (existente.HoraEntrada.HasValue && existente.HoraSalida.HasValue)
                    existente.HorasTrabajadas = Math.Round(
                        (decimal)(existente.HoraSalida.Value - existente.HoraEntrada.Value).TotalHours, 2);
            }
            else
            {
                // Calcular tardanza si hay hora de entrada y el empleado tiene horario
                int tardanza = 0;
                string estado = req.Estado;

                if (req.HoraEntrada != null && req.Estado == "presente")
                {
                    var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId);
                    if (empleado?.HoraEntradaEsperada != null)
                    {
                        var horaEsperada = empleado.HoraEntradaEsperada.Value.ToTimeSpan();
                        var horaEntrada = DateTime.Parse(req.HoraEntrada).TimeOfDay;
                        var limiteEntrada = horaEsperada.Add(TimeSpan.FromMinutes(empleado.ToleranciaMinutos));

                        if (horaEntrada > limiteEntrada)
                        {
                            tardanza = (int)Math.Round((horaEntrada - horaEsperada).TotalMinutes);
                            estado = "tardanza";
                        }
                    }
                }

                existente = new AsistenciaRegistro
                {
                    EmpleadoId = empleadoId,
                    Fecha = fecha,
                    HoraEntrada = req.HoraEntrada != null ? DateTime.Parse(req.HoraEntrada) : null,
                    HoraSalida = req.HoraSalida != null ? DateTime.Parse(req.HoraSalida) : null,
                    MetodoEntrada = "manual",
                    Estado = estado,           // usa el estado calculado
                    TardanzaMinutos = tardanza,         // guarda los minutos de tardanza
                    Observaciones = req.Observaciones,
                    RegistradoPor = req.RegistradoPor
                };

                if (existente.HoraEntrada.HasValue && existente.HoraSalida.HasValue)
                    existente.HorasTrabajadas = Math.Round(
                        (decimal)(existente.HoraSalida.Value - existente.HoraEntrada.Value).TotalHours, 2);

                _db.AsistenciaRegistros.Add(existente);
            }

            await _db.SaveChangesAsync();

            var result = await _db.AsistenciaRegistros
                .Include(r => r.Empleado)
                .FirstAsync(r => r.Id == existente.Id);
            return MapAsistencia(result);
        }

        public async Task<List<AsistenciaResponse>> ListarAsistenciaAsync(DateOnly? fecha, int? empleadoId, string? estado)
        {
            var q = _db.AsistenciaRegistros.Include(r => r.Empleado).AsQueryable();
            if (fecha.HasValue) q = q.Where(r => r.Fecha == fecha.Value);
            if (empleadoId.HasValue) q = q.Where(r => r.EmpleadoId == (uint)empleadoId.Value);
            if (!string.IsNullOrEmpty(estado)) q = q.Where(r => r.Estado == estado);
            var lista = await q.OrderByDescending(r => r.Fecha).ThenBy(r => r.Empleado!.Apellidos).ToListAsync();
            return lista.Select(MapAsistencia).ToList();
        }

        public async Task<List<HorasExtrasResponse>> ListarHorasExtrasAsync(string? estado)
        {
            var q = _db.HorasExtrasSolicitudes.Include(h => h.Empleado).AsQueryable();
            if (!string.IsNullOrEmpty(estado)) q = q.Where(h => h.Estado == estado);
            var lista = await q.OrderByDescending(h => h.CreatedAt).ToListAsync();
            return lista.Select(MapHorasExtras).ToList();
        }

        public async Task<HorasExtrasResponse?> AprobarHorasExtrasAsync(uint id, AprobarHorasExtrasRequest req)
        {
            var h = await _db.HorasExtrasSolicitudes.Include(x => x.Empleado).FirstOrDefaultAsync(x => x.Id == id);
            if (h == null) return null;
            h.Estado = "aprobada"; h.AprobadoPor = req.AprobadoPor;
            h.FechaResolucion = AhoraGuatemala(); h.Observaciones = req.Observaciones;
            await _db.SaveChangesAsync();
            return MapHorasExtras(h);
        }

        public async Task<HorasExtrasResponse?> RechazarHorasExtrasAsync(uint id, AprobarHorasExtrasRequest req)
        {
            var h = await _db.HorasExtrasSolicitudes.Include(x => x.Empleado).FirstOrDefaultAsync(x => x.Id == id);
            if (h == null) return null;
            h.Estado = "rechazada"; h.AprobadoPor = req.AprobadoPor;
            h.FechaResolucion = AhoraGuatemala(); h.Observaciones = req.Observaciones;
            await _db.SaveChangesAsync();
            return MapHorasExtras(h);
        }

        public async Task<AusenciaResponse> CrearAusenciaAsync(CrearAusenciaRequest req)
        {
            var inicio = DateOnly.Parse(req.FechaInicio);
            var fin = DateOnly.Parse(req.FechaFin);
            var dias = (int)(fin.ToDateTime(TimeOnly.MinValue) - inicio.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

            var ausencia = new Ausencia
            {
                EmpleadoId = (uint)req.EmpleadoId,
                Tipo = req.Tipo,
                FechaInicio = inicio,
                FechaFin = fin,
                DiasHabiles = dias,
                Motivo = req.Motivo,
                Estado = "pendiente"
            };
            _db.Ausencias.Add(ausencia);
            await _db.SaveChangesAsync();

            var result = await _db.Ausencias.Include(a => a.Empleado).FirstAsync(a => a.Id == ausencia.Id);
            return MapAusencia(result);
        }

        public async Task<List<AusenciaResponse>> ListarAusenciasAsync(int? empleadoId, string? estado)
        {
            var q = _db.Ausencias.Include(a => a.Empleado).AsQueryable();
            if (empleadoId.HasValue) q = q.Where(a => a.EmpleadoId == (uint)empleadoId.Value);
            if (!string.IsNullOrEmpty(estado)) q = q.Where(a => a.Estado == estado);
            var lista = await q.OrderByDescending(a => a.FechaInicio).ToListAsync();
            return lista.Select(MapAusencia).ToList();
        }

        public async Task<AusenciaResponse?> AprobarAusenciaAsync(uint id, string aprobadoPor)
        {
            var a = await _db.Ausencias.Include(x => x.Empleado).FirstOrDefaultAsync(x => x.Id == id);
            if (a == null) return null;
            a.Estado = "aprobada"; a.AprobadoPor = aprobadoPor;
            a.FechaAprobacion = AhoraGuatemala();
            await _db.SaveChangesAsync();
            return MapAusencia(a);
        }

        public async Task<ReporteAsistenciaResponse> GenerarReporteAsync(string periodoInicio, string periodoFin, string? departamentoArea)
        {
            var inicio = DateOnly.Parse(periodoInicio);
            var fin = DateOnly.Parse(periodoFin);
            var diasHabiles = (int)(fin.ToDateTime(TimeOnly.MinValue) - inicio.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

            var q = _db.Empleados.Where(e => e.Estado == "activo");
            if (!string.IsNullOrEmpty(departamentoArea))
                q = q.Where(e => e.DepartamentoArea == departamentoArea);
            var empleados = await q.ToListAsync();

            var registros = await _db.AsistenciaRegistros
                .Where(r => r.Fecha >= inicio && r.Fecha <= fin &&
                            empleados.Select(e => e.Id).Contains(r.EmpleadoId))
                .ToListAsync();

            var resumen = empleados.Select(emp =>
            {
                var regs = registros.Where(r => r.EmpleadoId == emp.Id).ToList();
                var diasPresente = regs.Count(r => r.Estado == "presente" || r.Estado == "tardanza");
                var diasAusente = regs.Count(r => r.Estado == "ausente");
                var diasTardanza = regs.Count(r => r.Estado == "tardanza");
                var diasPermiso = regs.Count(r => r.Estado == "permiso" || r.Estado == "vacacion");
                var horasExtras = regs.Sum(r => r.HorasExtras);
                var pct = diasHabiles > 0 ? Math.Round((decimal)diasPresente / diasHabiles * 100, 2) : 0;
                return new ResumenEmpleadoAsistencia(
                    (int)emp.Id, $"{emp.Nombres} {emp.Apellidos}", emp.CodigoEmpleado,
                    emp.DepartamentoArea, diasPresente, diasAusente, diasTardanza, diasPermiso, horasExtras, pct
                );
            }).ToList();

            var pctGeneral = empleados.Count > 0 && diasHabiles > 0
                ? Math.Round((decimal)resumen.Sum(r => r.DiasPresente) / (empleados.Count * diasHabiles) * 100, 2) : 0;

            return new ReporteAsistenciaResponse(
                $"{periodoInicio} al {periodoFin}", departamentoArea,
                empleados.Count, diasHabiles,
                resumen.Sum(r => r.DiasPresente), resumen.Sum(r => r.DiasAusente),
                resumen.Sum(r => r.DiasTardanza), pctGeneral,
                resumen.Sum(r => r.HorasExtras), resumen
            );
        }

        // ── Mappers ────────────────────────────────────────
        private static AsistenciaResponse MapAsistencia(AsistenciaRegistro r) =>
            new((int)r.Id, (int)r.EmpleadoId,
                r.Empleado != null ? $"{r.Empleado.Nombres} {r.Empleado.Apellidos}" : "",
                r.Empleado?.CodigoEmpleado ?? "",
                r.Empleado?.DepartamentoArea,
                r.Empleado?.Puesto ?? "",
                r.Fecha.ToString("yyyy-MM-dd"),
                r.HoraEntrada?.ToString("yyyy-MM-dd HH:mm:ss"),
                r.HoraSalida?.ToString("yyyy-MM-dd HH:mm:ss"),
                r.MetodoEntrada, r.MetodoSalida,
                r.HorasTrabajadas, r.HorasExtras,
                r.Estado, r.TardanzaMinutos, r.Observaciones);

        private static HorasExtrasResponse MapHorasExtras(HorasExtrasSolicitud h) =>
            new((int)h.Id, (int)h.EmpleadoId,
                h.Empleado != null ? $"{h.Empleado.Nombres} {h.Empleado.Apellidos}" : "",
                h.Fecha.ToString("yyyy-MM-dd"), h.HorasSolicitadas,
                h.Motivo, h.Estado, h.AprobadoPor, h.FechaResolucion);

        private static AusenciaResponse MapAusencia(Ausencia a) =>
            new((int)a.Id, (int)a.EmpleadoId,
                a.Empleado != null ? $"{a.Empleado.Nombres} {a.Empleado.Apellidos}" : "",
                a.Tipo, a.FechaInicio.ToString("yyyy-MM-dd"), a.FechaFin.ToString("yyyy-MM-dd"),
                a.DiasHabiles, a.Motivo, a.Estado, a.AprobadoPor);
    }
}