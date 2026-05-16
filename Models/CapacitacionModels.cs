using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("capacitacion_planes")]
    public class CapacitacionPlan
    {
        [Key] public uint Id { get; set; }
        [Column("codigo")] public string Codigo { get; set; } = "";
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("tipo")] public string Tipo { get; set; } = "interno";
        [Column("categoria")] public string? Categoria { get; set; }
        [Column("horas_requeridas")] public decimal HorasRequeridas { get; set; }
        [Column("presupuesto")] public decimal? Presupuesto { get; set; }
        [Column("fecha_inicio")] public DateOnly FechaInicio { get; set; }
        [Column("fecha_fin")] public DateOnly FechaFin { get; set; }
        [Column("fecha_limite")] public DateOnly FechaLimite { get; set; }
        [Column("instructor")] public string? Instructor { get; set; }
        [Column("lugar")] public string? Lugar { get; set; }
        [Column("es_requerido")] public bool EsRequerido { get; set; } = true;
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<CapacitacionAsignacion> Asignaciones { get; set; } = new List<CapacitacionAsignacion>();
    }

    [Table("capacitacion_asignaciones")]
    public class CapacitacionAsignacion
    {
        [Key] public uint Id { get; set; }
        [Column("plan_id")] public uint PlanId { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("estado")] public string Estado { get; set; } = "pendiente";
        [Column("horas_completadas")] public decimal HorasCompletadas { get; set; } = 0;
        [Column("porcentaje_avance")] public decimal PorcentajeAvance { get; set; } = 0;
        [Column("fecha_completada")] public DateTime? FechaCompletada { get; set; }
        [Column("asignado_por")] public string? AsignadoPor { get; set; }
        [Column("observaciones")] public string? Observaciones { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("PlanId")] public CapacitacionPlan? Plan { get; set; }
        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }

        public ICollection<CapacitacionEvidencia> Evidencias { get; set; } = new List<CapacitacionEvidencia>();
        public ICollection<CapacitacionAlerta> Alertas { get; set; } = new List<CapacitacionAlerta>();
    }

    [Table("capacitacion_evidencias")]
    public class CapacitacionEvidencia
    {
        [Key] public uint Id { get; set; }
        [Column("asignacion_id")] public uint AsignacionId { get; set; }
        [Column("nombre_archivo")] public string NombreArchivo { get; set; } = "";
        [Column("tipo_archivo")] public string TipoArchivo { get; set; } = "application/pdf";
        [Column("archivo_base64")] public byte[] ArchivoBase64 { get; set; } = Array.Empty<byte>();
        [Column("tamano_bytes")] public int? TamanoBytes { get; set; }
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("subido_por")] public string? SubidoPor { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("AsignacionId")]
        public CapacitacionAsignacion? Asignacion { get; set; }
    }

    [Table("capacitacion_alertas")]
    public class CapacitacionAlerta
    {
        [Key] public uint Id { get; set; }
        [Column("asignacion_id")] public uint AsignacionId { get; set; }
        [Column("tipo_alerta")] public string TipoAlerta { get; set; } = "";
        [Column("mensaje")] public string Mensaje { get; set; } = "";
        [Column("enviada")] public bool Enviada { get; set; } = false;
        [Column("fecha_envio")] public DateTime? FechaEnvio { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("AsignacionId")]
        public CapacitacionAsignacion? Asignacion { get; set; }
    }
}