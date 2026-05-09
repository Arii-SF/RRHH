// ============================================================
//  Models — Módulo Nómina
//  ERP RRHH Guatemala · .NET 8 · Entity Framework Core
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("cat_tipos_retencion")]
    public class CatTipoRetencion
    {
        [Key] public uint Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string AplicaA { get; set; } = "ambos";
        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<CatRetencionFiscal> Retenciones { get; set; } = [];
    }

    [Table("cat_retenciones_fiscales")]
    public class CatRetencionFiscal
    {
        [Key] public uint Id { get; set; }
        public uint TipoRetencionId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string MetodoCalculo { get; set; } = string.Empty;
        [Column(TypeName = "decimal(7,4)")] public decimal? TasaPorcentaje { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal? MontoFijo { get; set; }
        public string BaseCalculo { get; set; } = "salario_base";
        public string? ReferenciaLegal { get; set; }
        public DateOnly VigenteDe { get; set; }
        public DateOnly? VigenteHasta { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(TipoRetencionId))]
        public CatTipoRetencion? TipoRetencion { get; set; }
    }

    [Table("cat_isr_tramos")]
    public class CatIsrTramo
    {
        [Key] public uint Id { get; set; }
        [Column(TypeName = "decimal(14,2)")] public decimal RangoDesdeGtq { get; set; }
        [Column(TypeName = "decimal(14,2)")] public decimal? RangoHastaGtq { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal TasaPorcentaje { get; set; }
        [Column(TypeName = "decimal(14,2)")] public decimal CuotaFijaGtq { get; set; }
        public DateOnly VigenteDe { get; set; }
        public DateOnly? VigenteHasta { get; set; }
        public string? ReferenciaLegal { get; set; }
    }

    [Table("empleados")]
    public class Empleado
    {
        [Key] public uint Id { get; set; }
        public string CodigoEmpleado { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Dpi { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string? NoIgss { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string Nacionalidad { get; set; } = "Guatemalteca";
        public string? Direccion { get; set; }
        public string? Municipio { get; set; }
        public string? Departamento { get; set; }
        public string Puesto { get; set; } = string.Empty;
        public string? DepartamentoArea { get; set; }
        public DateOnly FechaIngreso { get; set; }
        public string TipoEmpleado { get; set; } = "planilla";
        public string Estado { get; set; } = "activo";
        [Column(TypeName = "mediumtext")] public string? FotoUrl { get; set; }

        // ── Horario de trabajo ─────────────────────────────
        [Column("hora_entrada_esperada")] public TimeOnly? HoraEntradaEsperada { get; set; }
        [Column("hora_salida_esperada")] public TimeOnly? HoraSalidaEsperada { get; set; }
        [Column("tolerancia_minutos")] public int ToleranciaMinutos { get; set; } = 10;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [NotMapped]
        public string NombreCompleto => $"{Nombres} {Apellidos}";

        public ICollection<Contrato> Contratos { get; set; } = [];
        public ICollection<EmpleadoContactoEmergencia> ContactosEmergencia { get; set; } = new List<EmpleadoContactoEmergencia>();
        public ICollection<EmpleadoHistorial> Historial { get; set; } = new List<EmpleadoHistorial>();
        public ICollection<OnboardingChecklist> OnboardingTareas { get; set; } = new List<OnboardingChecklist>();
    }

    [Table("contratos")]
    public class Contrato
    {
        [Key] public uint Id { get; set; }
        public uint EmpleadoId { get; set; }
        public string CodigoContrato { get; set; } = string.Empty;
        public string TipoContrato { get; set; } = "indefinido";
        [Column(TypeName = "decimal(12,2)")] public decimal SalarioBase { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal BonificacionDecreto { get; set; } = 250m;
        [Column(TypeName = "decimal(12,2)")] public decimal OtrasBonificaciones { get; set; } = 0m;
        public string Jornada { get; set; } = "diurna";
        [Column(TypeName = "decimal(5,2)")] public decimal HorasSemana { get; set; } = 44m;
        public string? LugarTrabajo { get; set; }
        public string Moneda { get; set; } = "GTQ";
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public bool Vigente { get; set; } = true;
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(EmpleadoId))]
        public Empleado? Empleado { get; set; }
    }

    [Table("nominas")]
    public class Nomina
    {
        [Key] public uint Id { get; set; }
        public string CodigoNomina { get; set; } = string.Empty;
        public DateOnly PeriodoInicio { get; set; }
        public DateOnly PeriodoFin { get; set; }
        public string TipoPeriodo { get; set; } = "mensual";
        public DateOnly FechaPago { get; set; }
        public string Estado { get; set; } = "borrador";
        public int TotalEmpleados { get; set; } = 0;
        [Column(TypeName = "decimal(14,2)")] public decimal TotalBrutoGtq { get; set; }
        [Column(TypeName = "decimal(14,2)")] public decimal TotalDeduccionesGtq { get; set; }
        [Column(TypeName = "decimal(14,2)")] public decimal TotalNetoGtq { get; set; }
        [Column(TypeName = "decimal(14,2)")] public decimal TotalCuotaPatronal { get; set; }
        public string? Observaciones { get; set; }
        public uint? AprobadoPor { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public uint? CreatedBy { get; set; }

        public ICollection<NominaDetalleEmpleado> Detalles { get; set; } = [];
        public ICollection<NominaAlerta> Alertas { get; set; } = [];
    }

    [Table("nomina_detalle_empleado")]
    public class NominaDetalleEmpleado
    {
        [Key] public uint Id { get; set; }
        public uint NominaId { get; set; }
        public uint EmpleadoId { get; set; }
        public uint ContratoId { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal SalarioBase { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal BonificacionDecreto { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal OtrasBonificaciones { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal HorasExtrasMonto { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal OtrosIngresos { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal TotalIngresosBruto { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal TotalDeduccionesEmpleado { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal TotalCuotaPatronal { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal TotalNeto { get; set; }
        public string Estado { get; set; } = "calculado";
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(NominaId))] public Nomina? Nomina { get; set; }
        [ForeignKey(nameof(EmpleadoId))] public Empleado? Empleado { get; set; }
        [ForeignKey(nameof(ContratoId))] public Contrato? Contrato { get; set; }
        public ICollection<NominaRetencionAplicada> Retenciones { get; set; } = [];
    }

    [Table("nomina_retenciones_aplicadas")]
    public class NominaRetencionAplicada
    {
        [Key] public uint Id { get; set; }
        public uint NominaDetalleId { get; set; }
        public uint RetencionId { get; set; }
        public string CodigoRetencion { get; set; } = string.Empty;
        public string NombreRetencion { get; set; } = string.Empty;
        public string? ReferenciaLegal { get; set; }
        public string MetodoCalculo { get; set; } = string.Empty;
        [Column(TypeName = "decimal(7,4)")] public decimal? TasaAplicada { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal BaseCalculoMonto { get; set; }
        [Column(TypeName = "decimal(12,2)")] public decimal MontoRetenido { get; set; }
        public bool EsDeduccionEmpleado { get; set; } = true;
        public bool EsCuotaPatronal { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(NominaDetalleId))] public NominaDetalleEmpleado? Detalle { get; set; }
        [ForeignKey(nameof(RetencionId))] public CatRetencionFiscal? Retencion { get; set; }
    }

    [Table("nomina_alertas")]
    public class NominaAlerta
    {
        [Key] public uint Id { get; set; }
        public uint NominaId { get; set; }
        public uint? EmpleadoId { get; set; }
        public string TipoAlerta { get; set; } = "otro";
        public string Descripcion { get; set; } = string.Empty;
        public bool Resuelta { get; set; } = false;
        public uint? ResueltaPor { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(NominaId))] public Nomina? Nomina { get; set; }
    }
}