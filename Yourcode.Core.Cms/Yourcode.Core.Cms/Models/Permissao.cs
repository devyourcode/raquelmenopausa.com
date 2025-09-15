using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Yourcode.Core.Cms.Models
{
    [Table("PERMISSAO")]
    public class Permissao
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("TIPO")]
        public string Tipo { get; set; }

        public virtual ICollection<ModuloPermissao> ModuloPermissoes { get; set; }
    }

}
