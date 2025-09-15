using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("BANNER")]
    public class Banner
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TITULO")]
        [MaxLength(255)]
        public string? Titulo { get; set; }

        [Column("SUBTITULO")]
        [MaxLength(255)]
        public string? Subtitulo { get; set; }

        [Column("LINK_BOTAO")]
        [MaxLength(255)]
        public string? LinkBotao { get; set; }

        [Column("NOME_BOTAO")]
        [MaxLength(255)]
        public string? NomeBotao { get; set; }

        [Column("IMAGEM")]
        [MaxLength(255)]
        public string? Imagem { get; set; }

        [Column("IMAGEM_MOBILE")]
        [MaxLength(255)]
        public string? ImagemMobile { get; set; }

        [Required]
        [Column("ATIVO")]
        public bool Ativo { get; set; }

        [Required]
        [Column("DATAINC")]
        public DateTime DataInc { get; set; }

        [Column("DATAALT")]
        public DateTime? DataAlt { get; set; }

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        [Column("USERINC")]
        public int? UserInc { get; set; }

        [Column("USERALT")]
        public int? UserAlt { get; set; }

        [Required]
        [Column("ORDEM")]
        public int Ordem { get; set; }

        [ForeignKey("UserInc")]
        public virtual Usuario? UsuarioInclusao { get; set; }

        [ForeignKey("UserAlt")]
        public virtual Usuario? UsuarioAlteracao { get; set; }
    }
}
