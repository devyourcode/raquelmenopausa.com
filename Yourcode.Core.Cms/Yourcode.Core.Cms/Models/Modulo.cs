using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaquelMenopausa.Cms.Models
{
    [Table("MODULO")]
    public class Modulo
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [MaxLength(45)]
        [Column("NOME")]
        public string Nome { get; set; }

        [Required]
        [MaxLength(45)]
        [Column("VALOR")]
        public string Valor { get; set; }

        [Required]
        [Column("ATIVO")]
        public bool Ativo { get; set; }

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        public virtual ICollection<ModuloPermissao> ModuloPermissoes { get; set; }
    }
}
