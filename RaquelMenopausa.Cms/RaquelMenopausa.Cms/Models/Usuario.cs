using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models
{
    [Table("USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("NOME")]
        [MaxLength(150)]
        public string Nome { get; set; }

        [Required]
        [Column("CARGO")]
        [MaxLength(150)]
        public string Cargo { get; set; }

        [Required]
        [Column("SENHA")]
        [MaxLength(100)]
        public string Senha { get; set; }

        [Column("RECUPERAR_SENHA")]
        [MaxLength(100)]
        public string? RecuperarSenha { get; set; }

        [Required]
        [Column("EMAIL")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Column("ATIVO")]
        public bool? Ativo { get; set; }

        [Required]
        [Column("DATAINC")]
        public DateTime DataInc { get; set; }

        [Required]
        [Column("PERMISSAO_ID")]
        public int PermissaoId { get; set; }

        [Required]
        [Column("SITUACAO")]
        public bool Situacao { get; set; }

        [Column("USERALT")]
        public int? UserAlt { get; set; }

        [Column("DATAALT")]
        public DateTime? DataAlt { get; set; }

        // Relacionamentos

        [ForeignKey("PermissaoId")]
        public virtual Permissao Permissao { get; set; }

        [ForeignKey("UserAlt")]
        public virtual Usuario UsuarioAlteracao { get; set; }
        public virtual ICollection<UsuarioModuloPermissao> UsuarioModuloPermissoes { get; set; }

    }
}
