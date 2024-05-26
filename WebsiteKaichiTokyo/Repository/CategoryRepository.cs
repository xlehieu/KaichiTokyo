using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Repository
{
    public class CategoryRepository : CategoryRepositoryInterface
    {
        private readonly CuaHangNhatBanContext _context;
        public CategoryRepository(CuaHangNhatBanContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _context.Categories;
        }
    }
}
