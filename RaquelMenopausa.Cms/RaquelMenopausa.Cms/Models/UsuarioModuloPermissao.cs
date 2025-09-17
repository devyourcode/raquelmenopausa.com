using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("USUARIO_MODULO_PERMISSAO")]
    public class UsuarioModuloPermissao
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("USUARIO_ID")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("MODULO_ID")]
        public int ModuloId { get; set; }

        [Required]
        [Column("PERMITIR")]
        public bool Permitir { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("ModuloId")]
        public virtual Modulo Modulo { get; set; }
    }
}
