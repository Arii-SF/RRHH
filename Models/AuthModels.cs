// ============================================================
//  Models/AuthModels.cs
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloGestionHumana.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key] public uint Id { get; set; }
        [Column("email")] public string Email { get; set; } = "";
        [Column("password_hash")] public string PasswordHash { get; set; } = "";
        [Column("rol")] public string Rol { get; set; } = "empleado";
        [Column("empleado_id")] public uint? EmpleadoId { get; set; }
        [Column("activo")] public bool Activo { get; set; } = true;
        [Column("ultimo_acceso")] public DateTime? UltimoAcceso { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("EmpleadoId")]
        public Empleado? Empleado { get; set; }
    }
}