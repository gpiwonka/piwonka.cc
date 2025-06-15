// Filters/AdminAuthFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Piwonka.CC.Filters
{
    public class AdminAuthFilter : IPageFilter
    {
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            // Keine Aktion erforderlich
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var isAuthenticated = context.HttpContext.Session.GetString("IsAuthenticated");

            // Wenn nicht authentifiziert und nicht bereits auf der Login-Seite
            if (isAuthenticated != "true" && !context.ActionDescriptor.DisplayName.Contains("Login"))
            {
                context.Result = new RedirectToPageResult("/Admin/Login");
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            // Keine Aktion erforderlich
        }
    }
}