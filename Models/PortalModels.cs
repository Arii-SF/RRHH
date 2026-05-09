using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("portal_solicitudes")]
    public class PortalSolicitud
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("tipo")] public string Tipo { get; set; } = "";
        [Column("subtipo")] public string? Subtipo { get; set; }
        [Column("fecha_inicio")] public DateOnly? FechaInicio { get; set; }
        [Column("fecha_fin")] public DateOnly? FechaFin { get; set; }
        [Column("dias_solicitados")] public decimal? DiasSolicitados { get; set; }
        [Column("motivo")] public string? Motivo { get; set; }
        [Column("estado")] public string Estado { get; set; } = "pendiente";
        [Column("aprobado_por")] public string? AprobadoPor { get; set; }
        [Column("fecha_resolucion")] public DateTime? FechaResolucion { get; set; }
        [Column("comentario_rrhh")] public string? ComentarioRrhh { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")]
        public Empleado? Empleado { get; set; }
    }

    [Table("portal_actualizaciones")]
    public class PortalActualizacion
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("campo")] public string Campo { get; set; } = "";
        [Column("valor_actual")] public string? ValorActual { get; set; }
        [Column("valor_nuevo")] public string ValorNuevo { get; set; } = "";
        [Column("estado")] public string Estado { get; set; } = "pendiente";
        [Column("revisado_por")] public string? RevisadoPor { get; set; }
        [Column("fecha_revision")] public DateTime? FechaRevision { get; set; }
        [Column("comentario")] public string? Comentario { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")]
        public Empleado? Empleado { get; set; }
    }

    [Table("portal_notificaciones")]
    public class PortalNotificacion
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("titulo")] public string Titulo { get; set; } = "";
        [Column("mensaje")] public string Mensaje { get; set; } = "";
        [Column("tipo")] public string Tipo { get; set; } = "general";
        [Column("leida")] public bool Leida { get; set; } = false;
        [Column("referencia_id")] public uint? ReferenciaId { get; set; }
        [Column("referencia_tipo")] public string? ReferenciaTipo { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")]
        public Empleado? Empleado { get; set; }
    }
}