namespace Crawler.Console.Abstraction;

public interface IRozetkaApiClient
{
    Task<string> GetRandomCategoryUrlAsync();
    Task<string[]> FilterGoods(int[] ids);
}