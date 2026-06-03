using System.ComponentModel.DataAnnotations;

namespace LanchesMac.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Usuário")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; }
    }
}