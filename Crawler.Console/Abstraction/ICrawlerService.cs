namespace Crawler.Console.Abstraction;

public interface ICrawlerService
{
    Task<int[]> GetGoodsIdsFromCategory(string categoryUrl, CancellationToken cancellationToken);
}