using System.ComponentModel.DataAnnotations;

namespace Truyen.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; } = "";

        public string? ReturnUrl { get; set; }
    }
}