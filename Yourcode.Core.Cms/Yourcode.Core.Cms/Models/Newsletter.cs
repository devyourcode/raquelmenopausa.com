using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("NEWSLETTER")]
    public class Newsletter
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("NOME", TypeName = "varchar(255)")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("EMAIL", TypeName = "varchar(255)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        [Required]
        [Column("DATAINC")]
        public DateTime DataInc { get; set; }
    }
}
