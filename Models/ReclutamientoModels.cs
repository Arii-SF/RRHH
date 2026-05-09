using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("vacantes")]
    public class Vacante
    {
        [Key] public uint Id { get; set; }
        [Column("codigo_vacante")] public string CodigoVacante { get; set; } = "";
        [Column("titulo")] public string Titulo { get; set; } = "";
        [Column("departamento_area")] public string DepartamentoArea { get; set; } = "";
        [Column("puesto")] public string Puesto { get; set; } = "";
        [Column("descripcion")] public string Descripcion { get; set; } = "";
        [Column("requisitos")] public string? Requisitos { get; set; }
        [Column("salario_minimo")] public decimal? SalarioMinimo { get; set; }
        [Column("salario_maximo")] public decimal? SalarioMaximo { get; set; }
        [Column("tipo_contrato")] public string TipoContrato { get; set; } = "indefinido";
        [Column("jornada")] public string Jornada { get; set; } = "diurna";
        [Column("modalidad")] public string Modalidad { get; set; } = "presencial";
        [Column("estado")] public string Estado { get; set; } = "borrador";
        [Column("fecha_publicacion")] public DateOnly? FechaPublicacion { get; set; }
        [Column("fecha_cierre")] public DateOnly? FechaCierre { get; set; }
        [Column("vacantes_disponibles")] public int VacantesDisponibles { get; set; } = 1;
        [Column("canal_interno")] public bool CanalInterno { get; set; } = true;
        [Column("canal_linkedin")] public bool CanalLinkedin { get; set; } = false;
        [Column("canal_computrabajo")] public bool CanalComputrabajo { get; set; } = false;
        [Column("canal_indeed")] public bool CanalIndeed { get; set; } = false;
        [Column("canal_otro")] public string? CanalOtro { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<Candidato> Candidatos { get; set; } = new List<Candidato>();
    }

    [Table("candidatos")]
    public class Candidato
    {
        [Key] public uint Id { get; set; }
        [Column("vacante_id")] public uint VacanteId { get; set; }
        [Column("nombres")] public string Nombres { get; set; } = "";
        [Column("apellidos")] public string Apellidos { get; set; } = "";
        [Column("email")] public string Email { get; set; } = "";
        [Column("telefono")] public string? Telefono { get; set; }
        [Column("dpi")] public string? Dpi { get; set; }
        [Column("fecha_nacimiento")] public DateOnly? FechaNacimiento { get; set; }
        [Column("direccion")] public string? Direccion { get; set; }
        [Column("linkedin_url")] public string? LinkedinUrl { get; set; }
        [Column("cv_url")] public string? CvUrl { get; set; }
        [Column("carta_presentacion")] public string? CartaPresentacion { get; set; }
        [Column("fuente_aplicacion")] public string FuenteAplicacion { get; set; } = "otro";
        [Column("etapa")] public string Etapa { get; set; } = "aplicado";
        [Column("puntaje")] public int? Puntaje { get; set; }
        [Column("notas_reclutador")] public string? NotasReclutador { get; set; }
        [Column("fecha_aplicacion")] public DateTime FechaAplicacion { get; set; } = DateTime.Now;
        [Column("fecha_ultima_etapa")] public DateTime? FechaUltimaEtapa { get; set; }
        [Column("empleado_id")] public uint? EmpleadoId { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("VacanteId")]
        public Vacante? Vacante { get; set; }

        public ICollection<CandidatoHistorialEtapa> Historial { get; set; } = new List<CandidatoHistorialEtapa>();
        public ICollection<ReclutamientoComunicacion> Comunicaciones { get; set; } = new List<ReclutamientoComunicacion>();
    }

    [Table("candidato_historial_etapas")]
    public class CandidatoHistorialEtapa
    {
        [Key] public uint Id { get; set; }
        [Column("candidato_id")] public uint CandidatoId { get; set; }
        [Column("etapa_anterior")] public string? EtapaAnterior { get; set; }
        [Column("etapa_nueva")] public string EtapaNueva { get; set; } = "";
        [Column("comentario")] public string? Comentario { get; set; }
        [Column("realizado_por")] public string RealizadoPor { get; set; } = "sistema";
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CandidatoId")]
        public Candidato? Candidato { get; set; }
    }

    [Table("reclutamiento_comunicaciones")]
    public class ReclutamientoComunicacion
    {
        [Key] public uint Id { get; set; }
        [Column("candidato_id")] public uint CandidatoId { get; set; }
        [Column("etapa")] public string Etapa { get; set; } = "";
        [Column("asunto")] public string Asunto { get; set; } = "";
        [Column("cuerpo")] public string Cuerpo { get; set; } = "";
        [Column("enviado")] public bool Enviado { get; set; } = false;
        [Column("fecha_envio")] public DateTime? FechaEnvio { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CandidatoId")]
        public Candidato? Candidato { get; set; }
    }

    [Table("reclutamiento_plantillas_email")]
    public class ReclutamientoPlantillaEmail
    {
        [Key] public uint Id { get; set; }
        [Column("etapa")] public string Etapa { get; set; } = "";
        [Column("asunto")] public string Asunto { get; set; } = "";
        [Column("cuerpo")] public string Cuerpo { get; set; } = "";
        [Column("activa")] public bool Activa { get; set; } = true;
    }
}