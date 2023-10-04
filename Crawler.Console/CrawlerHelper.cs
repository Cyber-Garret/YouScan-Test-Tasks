namespace Crawler.Console;

public static class CrawlerHelper
{
    public static async Task SimulateUserDelayAsync(CancellationToken cancellationToken = default) =>
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 5)), cancellationToken);
}