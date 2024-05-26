using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.ViewModels
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Amount { get; set; }
        public int Discount => Product.Discount.HasValue?Product.Discount.Value:0;
        public double TotalMoney =>(Product.Price.Value *(100-Discount)/100)*Amount;
    }
}
