using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaquelMenopausa.Cms.Models
{
    [Table("BLOG")]
    public class Blog
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("TITULO")]
        [StringLength(255)]
        public string? Titulo { get; set; }

        [Column("IMAGEM_PRINCIPAL")]
        [StringLength(255)]
        public string? ImagemPrincipal { get; set; }

        [Required]
        [Column("DESCRICAO")]
        public string? Descricao { get; set; }

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        [Required]
        [Column("ATIVO")]
        public bool Ativo { get; set; }

        [Required]
        [Column("DATAINC")]
        public DateTime DataInc { get; set; }

        [Column("DATAALT")]
        public DateTime? DataAlt { get; set; }

        [Column("USERINC")]
        public int? UserInc { get; set; }

        [Column("USERALT")]
        public int? UserAlt { get; set; }

        [Column("NR_ACESSOS")]
        public int? NrAcessos { get; set; }

        [Column("SLUG")]
        [StringLength(255)]
        public string? Slug { get; set; }

        [Column("CATEGORIA_ID")]
        public int? CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual BlogCategoria? Categoria { get; set; }

        [ForeignKey("UserInc")]
        public virtual Usuario? UserInclusao { get; set; }

        [ForeignKey("UserAlt")]
        public virtual Usuario? UserAlteracao { get; set; }
        public virtual ICollection<BlogImagem> BlogImagens { get; set; } = new List<BlogImagem>();
    }
}
