using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("LOG")]
    public class Log
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("DATA", TypeName = "timestamp")]
        public DateTime Data { get; set; } 

        [Required]
        [MaxLength(150)]
        [Column("ACAO")]
        public string Acao { get; set; }

        [Required]
        [Column("MENSAGEM", TypeName = "text")]
        public string Mensagem { get; set; }

        [Required]
        [Column("URL", TypeName = "text")]
        public string Url { get; set; }

        [Required]
        [Column("MAQUINA", TypeName = "text")]
        public string Maquina { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("IP")]
        public string Ip { get; set; } = string.Empty;

        [Column("USUARIO_ID")]
        public int? UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario? Usuario { get; set; }
    }
}
