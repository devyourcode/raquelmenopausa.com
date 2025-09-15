using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("POPUP")]
    public class Popup
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

        [Column("TITULO")]
        [StringLength(255)]
        public string? Titulo { get; set; }

        [Required]
        [Column("DATA_CADASTRO")]
        public DateTime DataCadastro { get; set; }

        [Column("IMAGEM")]
        [StringLength(255)]
        public string? Imagem { get; set; }
    }
}
