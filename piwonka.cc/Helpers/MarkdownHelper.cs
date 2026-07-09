using System.Text.RegularExpressions;

namespace Piwonka.CC.Helpers
{
    public static class MarkdownHelper
    {
        public static string ToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return string.Empty;

            var html = System.Web.HttpUtility.HtmlEncode(markdown);

            // Code blocks (``` ... ```)
            html = Regex.Replace(html,
                @"```(\w*)\r?\n([\s\S]*?)```",
                m => $"<pre><code>{m.Groups[2].Value.Trim()}</code></pre>");

            // Inline code
            html = Regex.Replace(html, @"`([^`]+)`", "<code>$1</code>");

            // Bold
            html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");

            // Italic
            html = Regex.Replace(html, @"\*(.+?)\*", "<em>$1</em>");

            // Headers
            html = Regex.Replace(html, @"^### (.+)$", "<h3>$1</h3>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^## (.+)$", "<h2>$1</h2>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^# (.+)$", "<h1>$1</h1>", RegexOptions.Multiline);

            // Unordered lists
            html = Regex.Replace(html, @"^- (.+)$", "<li>$1</li>", RegexOptions.Multiline);

            // Ordered lists
            html = Regex.Replace(html, @"^\d+\. (.+)$", "<li>$1</li>", RegexOptions.Multiline);

            // Wrap consecutive <li> in <ul>
            html = Regex.Replace(html, @"((?:<li>.*?</li>\s*)+)", "<ul>$1</ul>");

            // Horizontal rules
            html = Regex.Replace(html, @"^---+$", "<hr>", RegexOptions.Multiline);

            // Paragraphs (double newlines)
            html = Regex.Replace(html, @"\n\n+", "</p><p>");
            html = "<p>" + html + "</p>";

            // Clean up empty paragraphs and paragraphs wrapping block elements
            html = Regex.Replace(html, @"<p>\s*</p>", "");
            html = Regex.Replace(html, @"<p>\s*(<h[1-3]>)", "$1");
            html = Regex.Replace(html, @"(</h[1-3]>)\s*</p>", "$1");
            html = Regex.Replace(html, @"<p>\s*(<ul>)", "$1");
            html = Regex.Replace(html, @"(</ul>)\s*</p>", "$1");
            html = Regex.Replace(html, @"<p>\s*(<pre>)", "$1");
            html = Regex.Replace(html, @"(</pre>)\s*</p>", "$1");
            html = Regex.Replace(html, @"<p>\s*(<hr>)", "$1");

            // Single newlines to <br> inside paragraphs
            html = html.Replace("\n", "<br>");

            return html;
        }
    }
}
