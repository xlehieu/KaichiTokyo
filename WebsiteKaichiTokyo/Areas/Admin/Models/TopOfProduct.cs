using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Models
{
    public class TopOfProduct
    {
        public Product Product { get; set; }
        public int? Amount { get; set; }
        public long? Total { get; set;}
    }
}
