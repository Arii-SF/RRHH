using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("cat_tipos_beneficio")]
    public class CatTipoBeneficio
    {
        [Key] public uint Id { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
    }

    [Table("beneficio_paquetes")]
    public class BeneficioPaquete
    {
        [Key] public uint Id { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("criterio_tipo")] public string CriterioTipo { get; set; } = "todos";
        [Column("criterio_valor")] public string? CriterioValor { get; set; }
        [Column("antiguedad_minima")] public int? AntiguedadMinima { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<BeneficioItem> Items { get; set; } = new List<BeneficioItem>();
        public ICollection<BeneficioAsignacion> Asignaciones { get; set; } = new List<BeneficioAsignacion>();
    }

    [Table("beneficio_items")]
    public class BeneficioItem
    {
        [Key] public uint Id { get; set; }
        [Column("paquete_id")] public uint PaqueteId { get; set; }
        [Column("tipo_id")] public uint TipoId { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("valor_monetario")] public decimal? ValorMonetario { get; set; }
        [Column("periodicidad")] public string Periodicidad { get; set; } = "mensual";
        [Column("fecha_inicio")] public DateOnly FechaInicio { get; set; }
        [Column("fecha_fin")] public DateOnly? FechaFin { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("PaqueteId")] public BeneficioPaquete? Paquete { get; set; }
        [ForeignKey("TipoId")] public CatTipoBeneficio? Tipo { get; set; }
        public ICollection<BeneficioAlerta> Alertas { get; set; } = new List<BeneficioAlerta>();
    }

    [Table("beneficio_asignaciones")]
    public class BeneficioAsignacion
    {
        [Key] public uint Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("paquete_id")] public uint PaqueteId { get; set; }
        [Column("fecha_inicio")] public DateOnly FechaInicio { get; set; }
        [Column("fecha_fin")] public DateOnly? FechaFin { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("asignado_por")] public string? AsignadoPor { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }
        [ForeignKey("PaqueteId")] public BeneficioPaquete? Paquete { get; set; }
    }

    [Table("beneficio_alertas")]
    public class BeneficioAlerta
    {
        [Key] public uint Id { get; set; }
        [Column("item_id")] public uint ItemId { get; set; }
        [Column("empleado_id")] public uint? EmpleadoId { get; set; }
        [Column("tipo_alerta")] public string TipoAlerta { get; set; } = "vencimiento";
        [Column("mensaje")] public string Mensaje { get; set; } = "";
        [Column("dias_anticipacion")] public int DiasAnticipacion { get; set; } = 30;
        [Column("enviada")] public bool Enviada { get; set; } = false;
        [Column("fecha_envio")] public DateTime? FechaEnvio { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ItemId")] public BeneficioItem? Item { get; set; }
    }
}