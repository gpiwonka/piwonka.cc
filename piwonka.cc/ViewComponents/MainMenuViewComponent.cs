using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.Services;
using Piwonka.CC.ViewModels;
using System.Threading.Tasks;

namespace Piwonka.CC.ViewComponents
{
    public class MainMenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;

        public MainMenuViewComponent(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuViewModel = await _menuService.GetMenuAsync();
            return View(menuViewModel);
        }
    }
}