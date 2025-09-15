using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RaquelMenopausa.Cms.Models
{
    [Table("MODULO_PERMISSAO")]
    public class ModuloPermissao
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("PERMITIR")]
        public bool? Permitir { get; set; }

        [Required]
        [Column("MODULO_ID")]
        public int ModuloId { get; set; }

        [Required]
        [Column("PERMISSAO_ID")]
        public int PermissaoId { get; set; }

        [ForeignKey("ModuloId")]
        public virtual Modulo Modulo { get; set; }

        [ForeignKey("PermissaoId")]
        public virtual Permissao Permissao { get; set; }
    }
}