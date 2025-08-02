using Piwonka.CC.Services;
using System.Text;
using System.Text.Json;

public class BlueskyService : IBlueskyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlueskyService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public BlueskyService(HttpClient httpClient, IConfiguration configuration, ILogger<BlueskyService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> IsConfiguredAsync()
    {
        var handle = _configuration["Bluesky:Handle"];
        var password = _configuration["Bluesky:AppPassword"];

        return !string.IsNullOrEmpty(handle) && !string.IsNullOrEmpty(password);
    }

    public async Task<BlueskyPostPreviewModel> CreatePostPreviewAsync(string title, string excerpt, string url)
    {
        var baseText = $"📝 Neuer Blog-Post: {title}";

        if (!string.IsNullOrEmpty(excerpt))
        {
            var shortExcerpt = excerpt.Length > 100 ? excerpt.Substring(0, 97) + "..." : excerpt;
            baseText += $"\n\n{shortExcerpt}";
        }

        baseText += $"\n\n🔗 {url}";

        // Hashtags hinzufügen
        baseText += "\n\n#Blog #TechBlog #Entwicklung";

        return new BlueskyPostPreviewModel
        {
            Text = baseText,
            CharacterCount = baseText.Length,
            IsValid = baseText.Length <= 300 // Bluesky Limit
        };
    }

    public async Task<bool> PostToBlueskyAsync(string text, string? url = null)
    {
        try
        {
            if (!await IsConfiguredAsync())
            {
                _logger.LogWarning("Bluesky is not properly configured");
                return false;
            }

            // Token holen/erneuern falls nötig
            if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _tokenExpiry)
            {
                if (!await AuthenticateAsync())
                {
                    return false;
                }
            }

            // Post erstellen
            var postData = new
            {
                repo = _configuration["Bluesky:Handle"],
                collection = "app.bsky.feed.post",
                record = new
                {
                    text = text,
                    createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    facets = ExtractFacets(text, url)
                }
            };

            var json = JsonSerializer.Serialize(postData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://bsky.social/xrpc/com.atproto.repo.createRecord")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {_accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully posted to Bluesky");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to post to Bluesky: {response.StatusCode} - {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting to Bluesky");
            return false;
        }
    }

    private async Task<bool> AuthenticateAsync()
    {
        try
        {
            var authData = new
            {
                identifier = _configuration["Bluesky:Handle"],
                password = _configuration["Bluesky:AppPassword"]
            };

            var json = JsonSerializer.Serialize(authData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://bsky.social/xrpc/com.atproto.server.createSession", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<BlueskyAuthResponse>(responseContent);

                _accessToken = authResponse?.AccessJwt;
                _tokenExpiry = DateTime.UtcNow.AddHours(1); // Token läuft nach 1 Stunde ab

                return !string.IsNullOrEmpty(_accessToken);
            }
            else
            {
                _logger.LogError($"Bluesky authentication failed: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with Bluesky");
            return false;
        }
    }

    private object[] ExtractFacets(string text, string? url)
    {
        var facets = new List<object>();

        if (!string.IsNullOrEmpty(url))
        {
            // Finde URL-Position im Text
            var urlIndex = text.IndexOf(url);
            if (urlIndex >= 0)
            {
                facets.Add(new
                {
                    index = new
                    {
                        byteStart = Encoding.UTF8.GetByteCount(text.Substring(0, urlIndex)),
                        byteEnd = Encoding.UTF8.GetByteCount(text.Substring(0, urlIndex + url.Length))
                    },
                    features = new object[]
                    {
                            new
                            {
                                type = "app.bsky.richtext.facet#link",
                                uri = url
                            }
                    }
                });
            }
        }

        // Hashtags finden
        var hashtagPattern = @"#\w+";
        var matches = System.Text.RegularExpressions.Regex.Matches(text, hashtagPattern);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            facets.Add(new
            {
                index = new
                {
                    byteStart = Encoding.UTF8.GetByteCount(text.Substring(0, match.Index)),
                    byteEnd = Encoding.UTF8.GetByteCount(text.Substring(0, match.Index + match.Length))
                },
                features = new object[]
                {
                        new
                        {
                            type = "app.bsky.richtext.facet#tag",
                            tag = match.Value.Substring(1) // Ohne #
                        }
                }
            });
        }

        return facets.ToArray();
    }
}

// Response Models
public class BlueskyAuthResponse
{
    public string? AccessJwt { get; set; }
    public string? RefreshJwt { get; set; }
    public string? Handle { get; set; }
    public string? Did { get; set; }
}

public class BlueskyPostPreviewModel
{
    public string Text { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    public bool IsValid { get; set; }
    public string? ValidationMessage => IsValid ? null : $"Text ist zu lang ({CharacterCount}/300 Zeichen)";
}
