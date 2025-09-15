using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("CURRICULO")]
    public class Curriculo
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
        [Column("DATAINCALT")]
        public DateTime DataIncAlt { get; set; }

        [Required]
        [StringLength(250)]
        [Column("ARQUIVO", TypeName = "varchar(250)")]
        public string Arquivo { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("NOME", TypeName = "varchar(255)")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column("VISUALIZADO")]
        public bool Visualizado { get; set; }
    }
}
