using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AUDITORIA")]
public class Auditoria
{
    [Key]
    [Column("AUDITORIA_ID")]
    [StringLength(255)]
    public string AuditoriaId { get; set; }

    [Required]
    [Column("USUARIO_ACESSO")]
    [StringLength(255)]
    public string UsuarioAcesso { get; set; }

    [Column("IP")]
    [StringLength(255)]
    public string? Ip { get; set; }

    [Column("AREA_ACESSO")]
    [StringLength(255)]
    public string? AreaAcesso { get; set; }

    [Column("DATA")]
    public DateTime? Data { get; set; }
}
