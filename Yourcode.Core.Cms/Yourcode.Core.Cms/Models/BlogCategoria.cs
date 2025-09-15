using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yourcode.Core.Cms.Models
{
    [Table("BLOG_CATEGORIA")]
    public class BlogCategoria
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TITULO")]
        [StringLength(255)]
        public string? Titulo { get; set; }

        [Column("SLUG")]
        [StringLength(255)]
        public string? Slug { get; set; }

        [Required]
        [Column("ATIVO")]
        public bool Ativo { get; set; }

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        [Required]
        [Column("DATAINC")]
        public DateTime DataInc { get; set; }

        [Column("DATAALT")]
        public DateTime? DataAlt { get; set; }

        [Column("USERINC")]
        public int? UserInc { get; set; }

        [Column("USERALT")]
        public int? UserAlt { get; set; }

        [ForeignKey("UserInc")]
        public virtual Usuario? UserInclusao { get; set; }

        [ForeignKey("UserAlt")]
        public virtual Usuario? UserAlteracao { get; set; }

        public virtual ICollection<Blog>? Blogs { get; set; }
    }
}
