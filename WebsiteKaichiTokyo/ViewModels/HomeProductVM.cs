using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.ViewModels
{
    public class HomeProductVM
    {
        public Category Category { get; set; }
        public List<Product> Products { get; set; }
    }
}
