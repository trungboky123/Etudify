using Ganss.Xss;

namespace back_end.Components;

public class HtmlService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlService()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Add("img");
        _sanitizer.AllowedAttributes.Add("src");
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("style");
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        return _sanitizer.Sanitize(html);
    }
}