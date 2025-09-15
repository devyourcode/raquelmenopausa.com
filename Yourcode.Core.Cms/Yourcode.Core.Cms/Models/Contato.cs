using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("CONTATO")]
public class Contato
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("NOME")]
    [StringLength(255)]
    public string Nome { get; set; }

    [Required]
    [Column("EMAIL")]
    [StringLength(255)]
    public string Email { get; set; }

    [Column("TELEFONE")]
    [StringLength(255)]
    public string? Telefone { get; set; }

    [Required]
    [Column("MENSAGEM", TypeName = "TEXT")]
    public string Mensagem { get; set; }

    [Required]
    [Column("DATAINC")]
    public DateTime DataInc { get; set; }

    [Required]
    [Column("RESPONDIDO")]
    public bool Respondido { get; set; }

    [Required]
    [Column("SITUACAO")]
    public bool Situacao { get; set; }

    [Column("DATAALT")]
    public DateTime? DataAlt { get; set; }

    [Column("USERALT")]
    public int? UserAlt { get; set; }
}
