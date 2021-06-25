using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
    public class UserForAuthenticationDto
    {
        [Required(ErrorMessage = "UserName field is required")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "Password field is required")]
        public string Password { get; set; }
    }
}