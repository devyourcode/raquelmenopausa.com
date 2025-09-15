using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("USUARIO_LOG")]
    public class UsuarioLog
    {
        [Key]
        public int Id { get; set; }

        public string Log { get; set; }

        public DateTime? DataInc { get; set; }

        [ForeignKey("Usuario")]
        [Column("USUARIO_ID")]
        public int UsuarioId { get; set; }

        [Column("USUARIO")]
        public virtual Usuario Usuario { get; set; }
    }
}
