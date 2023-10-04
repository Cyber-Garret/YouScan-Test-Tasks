using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Crawler.Console.Abstraction;
using Crawler.Console.Extensions;
using Flurl;
using HtmlAgilityPack;

namespace Crawler.Console.Services;

public partial class CrawlerService : ICrawlerService
{
    private const int MaxParallelRequests = 5;
    private readonly HttpClient _httpClient;

    public CrawlerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int[]> GetGoodsIdsFromCategory(string categoryUrl, CancellationToken cancellationToken)
    {
        var responseMessage = await _httpClient.GetAsync(categoryUrl, cancellationToken);
        var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
        var doc = new HtmlDocument();
        doc.Load(contentStream);

        var goodsIdsList = doc.GetUniqueValues<int>("//div[@class='goods-tile__inner']", "data-goods-id");
        var paginator = doc.GetUniqueValues<string>("//a[contains(@class, 'pagination__link')]", "href");
        var maxPageNum = GetMaxPageNum(paginator);

        var urls = BuildUrlsForCategory(categoryUrl, maxPageNum);


        var semaphoreSlim = new SemaphoreSlim(MaxParallelRequests, MaxParallelRequests);

        var ints = await Task.WhenAll(urls
            .Select(t => MakeRequestWithSemaphoreSlimAsync(t, semaphoreSlim, cancellationToken)).ToArray());

        var result = ints.SelectMany(x => x).ToArray();
        goodsIdsList.AddRange(result);
        return goodsIdsList.ToArray();
    }

    private static IEnumerable<Url> BuildUrlsForCategory(string categoryUrl, int maxPage)
    {
        var urls = new List<Url>();
        for (var i = 2; i <= maxPage; i++)
            urls.Add(new Url($"{categoryUrl}page={i}/"));
        return urls;
    }

    private static int GetMaxPageNum(IEnumerable<string?> paginator)
    {
        var maxPageNumber = paginator
            .Select(url => PageNumberRegex().Match(url ?? string.Empty))
            .Select(match => int.Parse(match.Groups[1].Value))
            .Max();
        return maxPageNumber;
    }

    private async Task<int[]> MakeRequestWithSemaphoreSlimAsync(Url url,
        SemaphoreSlim semaphoreSlim,
        CancellationToken cancellationToken)
    {
        try
        {
            await semaphoreSlim.WaitAsync(cancellationToken);
            await CrawlerHelper.SimulateUserDelayAsync(cancellationToken);

            var responseMessage = await _httpClient.GetAsync(url, cancellationToken);
            var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

            var doc = new HtmlDocument();
            doc.Load(contentStream);
            var nodeCollection = doc.GetUniqueValues<int>("//div[@class='goods-tile__inner']", "data-goods-id");
            return nodeCollection.Count == 0 ? Array.Empty<int>() : nodeCollection.ToArray();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    [GeneratedRegex("page=(\\d*)", RegexOptions.Compiled)]
    private static partial Regex PageNumberRegex();
}