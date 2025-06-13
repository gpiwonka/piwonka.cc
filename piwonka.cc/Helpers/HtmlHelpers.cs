// Helpers/HtmlHelpers.cs (neue Datei erstellen)
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Piwonka.CC.Helpers
{
    public static class HtmlHelpers
    {
        public static IHtmlContent FormatCode(this IHtmlHelper htmlHelper, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new HtmlString(string.Empty);
            }

            // Prüfen, ob bereits Prism-Code-Blöcke vorhanden sind
            if (content.Contains("<pre class=\"language-") || content.Contains("<code class=\"language-"))
            {
                // Wenn ja, nichts tun
                return new HtmlString(content);
            }

            // Regex für einfache Code-Blöcke
            var codeBlockRegex = new Regex(@"<pre><code>(.*?)</code></pre>", RegexOptions.Singleline);
            content = codeBlockRegex.Replace(content, match =>
            {
                // Versuchen, die Language zu erkennen
                var code = match.Groups[1].Value;
                var language = "markup"; // Standard ist HTML/Markup

                if (code.Contains("class=") || code.Contains("<div") || code.Contains("<span"))
                {
                    language = "markup";
                }
                else if (code.Contains("function") || code.Contains("var ") || code.Contains("const ") || code.Contains("let "))
                {
                    language = "javascript";
                }
                else if (code.Contains("namespace") || code.Contains("using System") || code.Contains("public class"))
                {
                    language = "csharp";
                }

                return $"<pre class=\"line-numbers\"><code class=\"language-{language}\">{code}</code></pre>";
            });

            return new HtmlString(content);
        }
    }
}