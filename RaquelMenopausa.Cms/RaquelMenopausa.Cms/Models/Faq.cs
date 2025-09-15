using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("FAQ")]
    public class FAQ
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("PERGUNTA", TypeName = "text")]
        public string Pergunta { get; set; } = string.Empty;

        [Required]
        [Column("RESPOSTA", TypeName = "text")]
        public string Resposta { get; set; } = string.Empty;

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
        public int UserIncId { get; set; }

        [Column("USERALT")]
        public int? UserAltId { get; set; }

        [Required]
        [Column("ORDEM")]
        public int Ordem { get; set; }

        [ForeignKey("UserIncId")]
        public virtual Usuario? UserInc { get; set; }

        [ForeignKey("UserAltId")]
        public virtual Usuario? UserAlt { get; set; }
    }
}
