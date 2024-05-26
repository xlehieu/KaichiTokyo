using System.ComponentModel.DataAnnotations;

namespace WebsiteKaichiTokyo.ViewModels
{
    public class OrderViewVM
    {
        [Required(ErrorMessage ="Họ và tên không được để trống")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage ="Số điện thoại không được để trống")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage ="Địa chỉ không được để trống")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress]
        public string Email { get; set; }
        public int TypePayment { get; set; }
        public int TypePaymentVN { get; set; }
        public int? ProductId { get; set; }
        public int? SoLuong { get; set; }
    }
}
