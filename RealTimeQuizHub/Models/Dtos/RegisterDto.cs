using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizHub.Models.Dtos
{
    public class RegisterDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(4)]
        public string Password { get; set; }

        public bool IsAdmin { get; set; } = false;
    }
}
