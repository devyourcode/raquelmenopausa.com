using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Yourcode.Core.Cms.Models
{
    [Table("CONFIG")]
    public class Config
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("NOME")]
        [MaxLength(255)]
        public string? Nome { get; set; }

        [Required]
        [Column("CHAVE")]
        [MaxLength(255)]
        public string Chave { get; set; }

        [Column("VALOR")]
        public string? Valor { get; set; }

        [Column("OBS")]
        [MaxLength(255)]
        public string? Obs { get; set; }

        [Required]
        [Column("VISIVEL")]
        public bool Visivel { get; set; }

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        [Column("USERINC")]
        public int? UserIncId { get; set; }

        [Column("USERALT")]
        public int? UserAltId { get; set; }

        [Required]
        [Column("DATAINC")]
        public DateTime DataInc { get; set; }

        [Column("DATAALT")]
        public DateTime? DataAlt { get; set; }

        [ForeignKey("UserIncId")]
        public virtual Usuario? UsuarioInclusao { get; set; }

        [ForeignKey("UserAltId")]
        public virtual Usuario? UsuarioAlteracao { get; set; }
    }
}
