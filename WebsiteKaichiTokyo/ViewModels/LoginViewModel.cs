using System.ComponentModel.DataAnnotations;

namespace WebsiteKaichiTokyo.ViewModels
{
    public class LoginViewModel
    {
        [Key]
        [MaxLength(100)]
        [Required(ErrorMessage ="Vui lòng nhập Email")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name ="Email")]
        public string UserName { get; set; }
        [MinLength(6,ErrorMessage ="Mật khẩu tối thiểu là 6 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

    }
}
