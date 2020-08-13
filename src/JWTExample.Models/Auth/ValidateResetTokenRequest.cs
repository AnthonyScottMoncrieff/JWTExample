using System.ComponentModel.DataAnnotations;

namespace JWTExample.Models.Auth
{
    public class ValidateResetTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}