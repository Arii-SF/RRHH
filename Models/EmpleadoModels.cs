using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    // ── Catálogo de Departamentos ──────────────────────────
    [Table("cat_departamentos")]
    public class CatDepartamento
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("codigo")] public string Codigo { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CatPuesto> Puestos { get; set; } = new List<CatPuesto>();
    }

    // ── Catálogo de Puestos ────────────────────────────────
    [Table("cat_puestos")]
    public class CatPuesto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("departamento_id")] public int DepartamentoId { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("codigo")] public string Codigo { get; set; } = "";
        [Column("nivel_salarial")] public string? NivelSalarial { get; set; }
        [Column("salario_minimo")] public decimal? SalarioMinimo { get; set; }
        [Column("salario_maximo")] public decimal? SalarioMaximo { get; set; }
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("DepartamentoId")]
        public CatDepartamento? Departamento { get; set; }
    }

    // ── Documentos del Empleado ────────────────────────────
    [Table("empleado_documentos")]
    public class EmpleadoDocumento
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("tipo_documento")] public string TipoDocumento { get; set; } = "";
        [Column("nombre_archivo")] public string NombreArchivo { get; set; } = "";
        [Column("url_archivo")] public string UrlArchivo { get; set; } = "";
        [Column("fecha_vence")] public DateOnly? FechaVence { get; set; }
        [Column("verificado")] public bool Verificado { get; set; } = false;
        [Column("verificado_por")] public string? VerificadoPor { get; set; }
        [Column("notas")] public string? Notas { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ── Contactos de Emergencia ────────────────────────────
    [Table("empleado_contactos_emergencia")]
    public class EmpleadoContactoEmergencia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("nombre_completo")] public string NombreCompleto { get; set; } = "";
        [Column("parentesco")] public string Parentesco { get; set; } = "";
        [Column("telefono")] public string Telefono { get; set; } = "";
        [Column("telefono_alt")] public string? TelefonoAlt { get; set; }
        [Column("es_principal")] public bool EsPrincipal { get; set; } = false;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ── Historial de Cambios ───────────────────────────────
    [Table("empleado_historial")]
    public class EmpleadoHistorial
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("tipo_cambio")] public string TipoCambio { get; set; } = "";
        [Column("descripcion")] public string Descripcion { get; set; } = "";
        [Column("valor_anterior")] public string? ValorAnterior { get; set; }
        [Column("valor_nuevo")] public string? ValorNuevo { get; set; }
        [Column("realizado_por")] public string RealizadoPor { get; set; } = "sistema";
        [Column("fecha_cambio")] public DateTime FechaCambio { get; set; } = DateTime.Now;
    }

    // ── Onboarding Checklist ───────────────────────────────
    [Table("onboarding_checklist")]
    public class OnboardingChecklist
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("tarea")] public string Tarea { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("responsable")] public string? Responsable { get; set; }
        [Column("fecha_limite")] public DateOnly? FechaLimite { get; set; }
        [Column("completada")] public bool Completada { get; set; } = false;
        [Column("fecha_completada")] public DateTime? FechaCompletada { get; set; }
        [Column("orden")] public int Orden { get; set; } = 0;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ── Onboarding Plantilla ───────────────────────────────
    [Table("onboarding_plantilla")]
    public class OnboardingPlantilla
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("tarea")] public string Tarea { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("responsable")] public string? Responsable { get; set; }
        [Column("dias_limite")] public int DiasLimite { get; set; } = 3;
        [Column("orden")] public int Orden { get; set; } = 0;
        [Column("activa")] public bool Activa { get; set; } = true;

    }
}