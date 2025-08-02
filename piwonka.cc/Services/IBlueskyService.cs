namespace Piwonka.CC.Services
{
    public interface IBlueskyService
    {
        Task<bool> PostToBlueskyAsync(string text, string? url = null);
        Task<bool> IsConfiguredAsync();
        Task<BlueskyPostPreviewModel> CreatePostPreviewAsync(string title, string excerpt, string url);
    }
}
