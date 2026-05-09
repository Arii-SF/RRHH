using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("horarios")]
    public class Horario
    {
        [Key] public uint Id { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("jornada")] public string Jornada { get; set; } = "diurna";
        [Column("hora_entrada")] public TimeOnly HoraEntrada { get; set; }
        [Column("hora_salida")] public TimeOnly HoraSalida { get; set; }
        [Column("horas_dia")] public decimal HorasDia { get; set; } = 8;
        [Column("dias_semana")] public string DiasSemana { get; set; } = "L,M,X,J,V";
        [Column("tolerancia_min")] public int ToleranciaMin { get; set; } = 10;
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<EmpleadoHorario> EmpleadoHorarios { get; set; } = new List<EmpleadoHorario>();
    }

    [Table("empleado_horarios")]
    public class EmpleadoHorario
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("horario_id")] public uint HorarioId { get; set; }
        [Column("fecha_inicio")] public DateOnly FechaInicio { get; set; }
        [Column("fecha_fin")] public DateOnly? FechaFin { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }
        [ForeignKey("HorarioId")] public Horario? Horario { get; set; }
    }

    [Table("asistencia_registros")]
    public class AsistenciaRegistro
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("fecha")] public DateOnly Fecha { get; set; }
        [Column("hora_entrada")] public DateTime? HoraEntrada { get; set; }
        [Column("hora_salida")] public DateTime? HoraSalida { get; set; }
        [Column("metodo_entrada")] public string MetodoEntrada { get; set; } = "manual";
        [Column("metodo_salida")] public string? MetodoSalida { get; set; }
        [Column("ubicacion_entrada")] public string? UbicacionEntrada { get; set; }
        [Column("ubicacion_salida")] public string? UbicacionSalida { get; set; }
        [Column("horas_trabajadas")] public decimal? HorasTrabajadas { get; set; }
        [Column("horas_extras")] public decimal HorasExtras { get; set; } = 0;
        [Column("estado")] public string Estado { get; set; } = "presente";
        [Column("tardanza_minutos")] public int TardanzaMinutos { get; set; } = 0;
        [Column("observaciones")] public string? Observaciones { get; set; }
        [Column("registrado_por")] public string? RegistradoPor { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }
        public ICollection<HorasExtrasSolicitud> HorasExtrasSolicitudes { get; set; } = new List<HorasExtrasSolicitud>();
    }

    [Table("horas_extras_solicitudes")]
    public class HorasExtrasSolicitud
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("asistencia_id")] public uint AsistenciaId { get; set; }
        [Column("fecha")] public DateOnly Fecha { get; set; }
        [Column("horas_solicitadas")] public decimal HorasSolicitadas { get; set; }
        [Column("motivo")] public string Motivo { get; set; } = "";
        [Column("estado")] public string Estado { get; set; } = "pendiente";
        [Column("aprobado_por")] public string? AprobadoPor { get; set; }
        [Column("fecha_resolucion")] public DateTime? FechaResolucion { get; set; }
        [Column("observaciones")] public string? Observaciones { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }
        [ForeignKey("AsistenciaId")] public AsistenciaRegistro? Asistencia { get; set; }
    }

    [Table("ausencias")]
    public class Ausencia
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("tipo")] public string Tipo { get; set; } = "permiso";
        [Column("fecha_inicio")] public DateOnly FechaInicio { get; set; }
        [Column("fecha_fin")] public DateOnly FechaFin { get; set; }
        [Column("dias_habiles")] public int DiasHabiles { get; set; } = 1;
        [Column("motivo")] public string? Motivo { get; set; }
        [Column("estado")] public string Estado { get; set; } = "pendiente";
        [Column("aprobado_por")] public string? AprobadoPor { get; set; }
        [Column("fecha_aprobacion")] public DateTime? FechaAprobacion { get; set; }
        [Column("documento_url")] public string? DocumentoUrl { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }
    }
}