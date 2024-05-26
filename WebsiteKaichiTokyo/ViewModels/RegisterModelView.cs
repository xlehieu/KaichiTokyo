using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebsiteKaichiTokyo.ViewModels
{
    public class RegisterModelView
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; }
        [MaxLength(150)]
        [Display(Name ="Email")]
        [Required(ErrorMessage ="Vui lòng nhập Email")]
        [DataType(DataType.EmailAddress)]
        [Remote(action:"ValidateEmail",controller:"Accounts")] 
        public string EmailAddress { get; set;}
        [MaxLength(11)]
        [Required(ErrorMessage ="Vui lòng nhập số điện thoại")]
        [Display(Name ="Số điện thoại")]
        [DataType(DataType.PhoneNumber)]
        [Remote(action:"ValidatePhone",controller:"Accounts")]
        public string PhoneNumber { get; set;}
        [Display(Name ="Mật khẩu")]
        [Required(ErrorMessage ="Vui lòng nhập mật khẩu")]
        [MinLength(6,ErrorMessage ="Mật khẩu tối thiểu là 6 ký tự")]
        public string Password { get; set;}
        [Display(Name ="Nhập lại mật khẩu")]
        [Required(ErrorMessage ="Vui lòng nhập lại mật khẩu")]
        [MinLength(6,ErrorMessage ="Vui lòng nhập lại mật khẩu")]
        [Compare("Password",ErrorMessage ="Vui lòng nhập mật khẩu giống nhau")]
        public string ConfirmPassword { get; set;}
    }
}
