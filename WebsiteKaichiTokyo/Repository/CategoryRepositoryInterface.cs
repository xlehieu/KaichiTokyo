using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Repository
{
    public interface CategoryRepositoryInterface
    {
        IEnumerable<Category> GetAllCategories();
    }
}
