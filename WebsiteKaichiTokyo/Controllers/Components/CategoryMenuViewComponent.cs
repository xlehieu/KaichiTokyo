using Microsoft.AspNetCore.Mvc;
using WebsiteKaichiTokyo.Repository;

namespace WebsiteKaichiTokyo.Controllers.Components
{
    public class CategoryMenuViewComponent:ViewComponent
    {
        private readonly CategoryRepositoryInterface _CategoryRepositoryInterface;
        public CategoryMenuViewComponent(CategoryRepositoryInterface categoryRepositoryInterface)
        {
            _CategoryRepositoryInterface = categoryRepositoryInterface;
        }
        public IViewComponentResult Invoke()
        {
            return View(_CategoryRepositoryInterface.GetAllCategories());
        }
    }
}
