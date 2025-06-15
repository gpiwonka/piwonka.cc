    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    namespace Piwonka.CC.ViewComponents
    {
        public class SearchBoxViewComponent : ViewComponent
        {
            public async Task<IViewComponentResult> InvokeAsync()
            {
                return View();
            }
        }
    }
