using System.Collections.Generic;

namespace Piwonka.CC.ViewModels
{
    public class AdminNavViewModel
    {
        public string CurrentPage { get; set; } = string.Empty;
        public List<AdminNavItem> NavItems { get; set; } = new();

        public AdminNavViewModel()
        {
            NavItems = new List<AdminNavItem>
            {
                new AdminNavItem
                {
                    Title = "Dashboard",
                    PagePath = "/Admin/Index",
                    IsExactMatch = true
                },
                new AdminNavItem
                {
                    Title = "Posts",
                    PagePath = "/Admin/Posts/Index",
                    SectionPath = "/Admin/Posts",
                    IsExactMatch = false
                },
                new AdminNavItem
                {
                    Title = "Seiten",
                    PagePath = "/Admin/Seiten/Index",
                    SectionPath = "/Admin/Seiten",
                    IsExactMatch = false
                },
                new AdminNavItem
                {
                    Title = "Kategorien",
                    PagePath = "/Admin/Kategorien/Index",
                    SectionPath = "/Admin/Kategorien",
                    IsExactMatch = false
                },
                new AdminNavItem
                {
                    Title = "WordPress-Import",
                    PagePath = "/Admin/Import/Wordpress",
                    IsExactMatch = true
                }
            };
        }

        public bool IsActive(AdminNavItem item)
        {
            if (item.IsExactMatch)
            {
                return CurrentPage.Equals(item.PagePath, System.StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return !string.IsNullOrEmpty(item.SectionPath) &&
                       CurrentPage.StartsWith(item.SectionPath, System.StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    public class AdminNavItem
    {
        public string Title { get; set; } = string.Empty;
        public string PagePath { get; set; } = string.Empty;
        public string? SectionPath { get; set; }
        public bool IsExactMatch { get; set; } = true;
    }
}