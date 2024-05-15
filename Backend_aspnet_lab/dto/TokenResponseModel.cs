using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto
{
    public class TokenResponseModel
    {
        public TokenResponseModel(string token) { Token = token; }

        [Required(ErrorMessage = "Поле 'token' является обязательным.")]
        [MinLength(1, ErrorMessage = "Поле 'token' должно содержать минимум 1 символ.")]
        public string Token { get; set; }
    }
}
