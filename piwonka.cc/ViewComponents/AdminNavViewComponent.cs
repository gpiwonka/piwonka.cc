using Microsoft.AspNetCore.Mvc;
using Piwonka.CC.ViewModels;

namespace Piwonka.CC.ViewComponents
{
    public class AdminNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var viewModel = new AdminNavViewModel();

            // Aktuelle Seite aus RouteData ermitteln
            var currentPage = ViewContext.RouteData.Values["page"]?.ToString() ?? string.Empty;
            viewModel.CurrentPage = currentPage;

            return View(viewModel);
        }
    }
}