using HtmlAgilityPack;

namespace Crawler.Console.Extensions;

public static class HtmlAgilityPackExtensions
{
    public static List<T?> GetUniqueValues<T>(this HtmlDocument document, string xPath, string attributeName)
    {
        return document.DocumentNode.SelectNodes(xPath)
            .Select(x => x.GetAttributeValue(attributeName, default(T)))
            .Where(x => !EqualityComparer<T>.Default.Equals(x, default)).Distinct().ToList();
    }
}