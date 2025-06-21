using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piwonka.CC.Services
{
    public interface IIndexNowService
    {
        Task NotifyUrlAsync(string url);
        Task NotifyUrlsAsync(IEnumerable<string> urls);
        Task NotifyPostCreatedAsync(string slug);
        Task NotifyPostUpdatedAsync(string slug);
        Task NotifySeiteCreatedAsync(string slug);
        Task NotifySeiteUpdatedAsync(string slug);
        Task NotifySitemapUpdatedAsync();
    }
}