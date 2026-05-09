using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("evaluacion_ciclos")]
    public class EvaluacionCiclo
    {
        [Key] public uint Id { get; set; }
        [Column("codigo")] public string Codigo { get; set; } = "";
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("periodo")] public string Periodo { get; set; } = "";
        [Column("fecha_inicio")] public DateOnly FechaInicio { get; set; }
        [Column("fecha_fin")] public DateOnly FechaFin { get; set; }
        [Column("fecha_cierre")] public DateOnly FechaCierre { get; set; }
        [Column("estado")] public string Estado { get; set; } = "borrador";
        [Column("incluye_autoevaluacion")] public bool IncluyeAutoevaluacion { get; set; } = true;
        [Column("escala_minima")] public int EscalaMinima { get; set; } = 1;
        [Column("escala_maxima")] public int EscalaMaxima { get; set; } = 5;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<EvaluacionCriterio> Criterios { get; set; } = new List<EvaluacionCriterio>();
        public ICollection<EvaluacionParticipante> Participantes { get; set; } = new List<EvaluacionParticipante>();
        public ICollection<EvaluacionRecordatorio> Recordatorios { get; set; } = new List<EvaluacionRecordatorio>();
    }

    [Table("evaluacion_criterios")]
    public class EvaluacionCriterio
    {
        [Key] public uint Id { get; set; }
        [Column("ciclo_id")] public uint CicloId { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("descripcion")] public string? Descripcion { get; set; }
        [Column("peso")] public decimal Peso { get; set; } = 20;
        [Column("orden")] public int Orden { get; set; } = 0;

        [ForeignKey("CicloId")]
        public EvaluacionCiclo? Ciclo { get; set; }
        public ICollection<EvaluacionCalificacion> Calificaciones { get; set; } = new List<EvaluacionCalificacion>();
    }

    [Table("evaluacion_participantes")]
    public class EvaluacionParticipante
    {
        [Key] public uint Id { get; set; }
        [Column("ciclo_id")] public uint CicloId { get; set; }
        [Column("empleado_id")] public uint EmpleadoId { get; set; }
        [Column("evaluador_id")] public uint EvaluadorId { get; set; }
        [Column("estado")] public string Estado { get; set; } = "pendiente";
        [Column("fecha_completada")] public DateTime? FechaCompletada { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CicloId")] public EvaluacionCiclo? Ciclo { get; set; }
        [ForeignKey("EmpleadoId")] public Empleado? Empleado { get; set; }
        [ForeignKey("EvaluadorId")] public Empleado? Evaluador { get; set; }

        public ICollection<EvaluacionCalificacion> Calificaciones { get; set; } = new List<EvaluacionCalificacion>();
        public EvaluacionResultado? Resultado { get; set; }
    }

    [Table("evaluacion_calificaciones")]
    public class EvaluacionCalificacion
    {
        [Key] public uint Id { get; set; }
        [Column("participante_id")] public uint ParticipanteId { get; set; }
        [Column("criterio_id")] public uint CriterioId { get; set; }
        [Column("tipo")] public string Tipo { get; set; } = "evaluador";
        [Column("calificacion")] public decimal Calificacion { get; set; }
        [Column("comentario")] public string? Comentario { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ParticipanteId")] public EvaluacionParticipante? Participante { get; set; }
        [ForeignKey("CriterioId")] public EvaluacionCriterio? Criterio { get; set; }
    }

    [Table("evaluacion_resultados")]
    public class EvaluacionResultado
    {
        [Key] public uint Id { get; set; }
        [Column("participante_id")] public uint ParticipanteId { get; set; }
        [Column("puntaje_evaluador")] public decimal? PuntajeEvaluador { get; set; }
        [Column("puntaje_autoevaluacion")] public decimal? PuntajeAutoevaluacion { get; set; }
        [Column("puntaje_final")] public decimal? PuntajeFinal { get; set; }
        [Column("nivel")] public string? Nivel { get; set; }
        [Column("comentarios_generales")] public string? ComentariosGenerales { get; set; }
        [Column("plan_mejora")] public string? PlanMejora { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ParticipanteId")]
        public EvaluacionParticipante? Participante { get; set; }
    }

    [Table("evaluacion_recordatorios")]
    public class EvaluacionRecordatorio
    {
        [Key] public uint Id { get; set; }
        [Column("ciclo_id")] public uint CicloId { get; set; }
        [Column("dias_antes")] public int DiasAntes { get; set; }
        [Column("enviado")] public bool Enviado { get; set; } = false;
        [Column("fecha_envio")] public DateTime? FechaEnvio { get; set; }
        [Column("destinatarios")] public int Destinatarios { get; set; } = 0;
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CicloId")]
        public EvaluacionCiclo? Ciclo { get; set; }
    }
}