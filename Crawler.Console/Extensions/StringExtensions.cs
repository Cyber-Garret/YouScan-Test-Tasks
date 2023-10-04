namespace Crawler.Console.Extensions;

public static class StringExtensions
{
    public static Uri GetAbsoluteUriFromString(this string url, string baseUrl)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
            uri = new Uri(new Uri(baseUrl), uri);
        return uri;
    }
}