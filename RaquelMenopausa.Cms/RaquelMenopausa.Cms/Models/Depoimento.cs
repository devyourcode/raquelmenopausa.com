using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("DEPOIMENTO")]
    public class Depoimento
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

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

        [Column("IMAGEM")]
        [StringLength(255)]
        public string? Imagem { get; set; }

        [Column("USERINC")]
        public int? UserInc { get; set; }

        [Column("USERALT")]
        public int? UserAlt { get; set; }

        [Required]
        [Column("NOME")]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column("DESCRICAO", TypeName = "text")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Column("ORDEM")]
        public int Ordem { get; set; }

        [ForeignKey("UserInc")]
        public virtual Usuario? UsuarioInc { get; set; }

        [ForeignKey("UserAlt")]
        public virtual Usuario? UsuarioAlt { get; set; }
    }
}
