using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("cat_normativas")]
    public class CatNormativa
    {
        [Key] public uint Id { get; set; }
        [Column("codigo")] public string Codigo { get; set; } = "";
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("categoria")] public string Categoria { get; set; } = "otro";
        [Column("referencia_legal")] public string ReferenciaLegal { get; set; } = "";
        [Column("decreto_ley")] public string? DecretoLey { get; set; }
        [Column("vigente_desde")] public DateOnly VigenteDesde { get; set; }
        [Column("vigente_hasta")] public DateOnly? VigenteHasta { get; set; }
        [Column("activa")] public bool Activa { get; set; } = true;
        [Column("valor_minimo")] public decimal? ValorMinimo { get; set; }
        [Column("valor_referencia")] public string? ValorReferencia { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<CumplimientoAlerta> Alertas { get; set; } = new List<CumplimientoAlerta>();
        public ICollection<NormativaHistorialCambio> Historial { get; set; } = new List<NormativaHistorialCambio>();
    }

    [Table("cumplimiento_alertas")]
    public class CumplimientoAlerta
    {
        [Key] public uint Id { get; set; }
        [Column("normativa_id")] public uint NormativaId { get; set; }
        [Column("empleado_id")] public uint? EmpleadoId { get; set; }
        [Column("contrato_id")] public uint? ContratoId { get; set; }
        [Column("tipo_alerta")] public string TipoAlerta { get; set; } = "otro";
        [Column("severidad")] public string Severidad { get; set; } = "advertencia";
        [Column("titulo")] public string Titulo { get; set; } = "";
        [Column("descripcion")] public string Descripcion { get; set; } = "";
        [Column("referencia_legal")] public string ReferenciaLegal { get; set; } = "";
        [Column("resuelta")] public bool Resuelta { get; set; } = false;
        [Column("resuelta_por")] public string? ResueltaPor { get; set; }
        [Column("fecha_resolucion")] public DateTime? FechaResolucion { get; set; }
        [Column("notas_resolucion")] public string? NotasResolucion { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("NormativaId")] public CatNormativa? Normativa { get; set; }
        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }
        [ForeignKey("ContratoId")] public Contrato? Contrato { get; set; }
    }

    [Table("cumplimiento_reportes")]
    public class CumplimientoReporte
    {
        [Key] public uint Id { get; set; }
        [Column("codigo_reporte")] public string CodigoReporte { get; set; } = "";
        [Column("titulo")] public string Titulo { get; set; } = "";
        [Column("periodo_inicio")] public DateOnly PeriodoInicio { get; set; }
        [Column("periodo_fin")] public DateOnly PeriodoFin { get; set; }
        [Column("generado_por")] public string GeneradoPor { get; set; } = "sistema";
        [Column("total_empleados")] public int TotalEmpleados { get; set; }
        [Column("total_alertas")] public int TotalAlertas { get; set; }
        [Column("alertas_criticas")] public int AlertasCriticas { get; set; }
        [Column("alertas_resueltas")] public int AlertasResueltas { get; set; }
        [Column("porcentaje_cumplimiento")] public decimal PorcentajeCumplimiento { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("normativa_historial_cambios")]
    public class NormativaHistorialCambio
    {
        [Key] public uint Id { get; set; }
        [Column("normativa_id")] public uint NormativaId { get; set; }
        [Column("tipo_cambio")] public string TipoCambio { get; set; } = "";
        [Column("descripcion_cambio")] public string DescripcionCambio { get; set; } = "";
        [Column("valor_anterior")] public string? ValorAnterior { get; set; }
        [Column("valor_nuevo")] public string? ValorNuevo { get; set; }
        [Column("referencia_legal")] public string? ReferenciaLegal { get; set; }
        [Column("notificado")] public bool Notificado { get; set; } = false;
        [Column("fecha_vigencia")] public DateOnly FechaVigencia { get; set; }
        [Column("realizado_por")] public string RealizadoPor { get; set; } = "sistema";
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("NormativaId")]
        public CatNormativa? Normativa { get; set; }
    }
}