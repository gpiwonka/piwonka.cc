using System.Collections.Generic;
using System.Linq;

namespace Piwonka.CC.ViewModels
{
    public class MenuViewModel
    {
        public List<MenuGroup> MenuGroups { get; set; } = new();
        public List<MenuItemViewModel> TopLevelItems { get; set; } = new();
    }

    public class MenuGroup
    {
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; }
        public List<MenuItemViewModel> Items { get; set; } = new();
        public bool HasUrl => !string.IsNullOrEmpty(Url);
    }

    public class MenuItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Reihenfolge { get; set; }
        public List<MenuItemViewModel> Children { get; set; } = new();
        public bool HasChildren => Children.Any();
    }
}
