using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Models.ViewModel
{
    public class AdminHomeView
    {
        public int? TotalOrder { get; set; }
        public int? ThisMonth { get; set; }
        public long? SaleThisMonth { get; set; }
        public int? TotalProduct { get; set; }
        public List<TopOfProduct>? TopOfProducts { get; set; }
        public List<Order>? OrderList { get; set; }
        public List<Contact>? ContactList { get; set; }

    }
}
