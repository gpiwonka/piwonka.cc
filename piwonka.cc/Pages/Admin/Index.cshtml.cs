using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piwonka.CC.Filters;

namespace Piwonka.CC.Pages.Admin
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
