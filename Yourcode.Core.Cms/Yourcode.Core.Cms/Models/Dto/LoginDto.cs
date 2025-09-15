using System.ComponentModel.DataAnnotations;

namespace RaquelMenopausa.Cms.Models.Dto
{
    public class LoginDto
    {
        [Required]
        [Display(Name = "User name")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Senha { get; set; }

        [Display(Name = "Remember me?")]
        public bool Lembrar { get; set; }
    }
}
