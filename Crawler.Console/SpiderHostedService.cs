using Crawler.Console.Abstraction;
using Microsoft.Extensions.Hosting;

namespace Crawler.Console;

public class SpiderHostedService : BackgroundService
{
    private readonly IRozetkaApiClient _rozetkaApiClient;
    private readonly ICrawlerService _crawler;

    public SpiderHostedService(IRozetkaApiClient rozetkaApiClient, ICrawlerService crawler)
    {
        _rozetkaApiClient = rozetkaApiClient;
        _crawler = crawler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var categoryUrl = await _rozetkaApiClient.GetRandomCategoryUrlAsync();
        var goodsIds = await _crawler.GetGoodsIdsFromCategory(categoryUrl, stoppingToken);
        var filterGoods = await _rozetkaApiClient.FilterGoods(goodsIds);
    }
}