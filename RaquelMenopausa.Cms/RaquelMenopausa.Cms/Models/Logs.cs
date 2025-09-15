using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("logs")]
    public class Logs
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("Level")]
        [MaxLength(50)]
        public string Level { get; set; }

        [Column("Message")]
        public string Message { get; set; }

        [Column("Exception")]
        public string Exception { get; set; }

        [Column("Properties")]
        public string Properties { get; set; }
    }
}
