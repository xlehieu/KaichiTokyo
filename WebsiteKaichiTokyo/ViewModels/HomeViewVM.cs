using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.ViewModels
{
    public class HomeViewVM
    {
        public List<TinTuc>? TinTucs { get; set; }
        public List<HomeProductVM>? LsHomeProductVM { get; set; }

        public List<Product>? LastestProduct { get;set; }
        public List<Product>? TopRateProduct { get; set; }
        public List<Product>? AllProduct { get; set; }
        public List<Slider>? Sliders { get; set; }

    }
}
