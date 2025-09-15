using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yourcode.Core.Cms.Models
{
    [Table("BLOG_IMAGEM")]
    public class BlogImagem
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        [Required]
        [Column("DATAINC")]
        public DateTime DataInc { get; set; }

        [Column("USERINC")]
        public int UserInc { get; set; }

        [Required]
        [Column("IMAGEM")]
        [StringLength(255)]
        public string? Imagem { get; set; }

        [Required]
        [Column("BLOG_ID")]
        public int BlogId { get; set; }

        [ForeignKey("UserInc")]
        public virtual Usuario? UserInclusao { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog? Blog { get; set; }
    }
}
