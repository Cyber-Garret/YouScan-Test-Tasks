using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Crawler.Console.Abstraction;
using Flurl;
using HtmlAgilityPack;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;

namespace Crawler.Console.Services;

public partial class RozetkaApiClient : IRozetkaApiClient
{
    private readonly HttpClient _httpClient;

    public RozetkaApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetRandomCategoryUrlAsync()
    {
        var responseMessage = await _httpClient.GetAsync(
            "https://common-api.rozetka.com.ua/v2/fat-menu/full?front-type=xl&country=UA&lang=ua&r=0.2272461346749406");

        var content = await responseMessage.Content.ReadAsStringAsync();


        var jObject = JObject.Parse(content);
        var arrayOfUrls = jObject.DescendantsAndSelf().OfType<JProperty>()
            .Where(p => p.Name == "manual_url")
            .Select(p => p.Value.Value<string>())
            .ToArray();
        while (true)
        {
            var randomIndex = Random.Shared.Next(arrayOfUrls.Length);
            var url = CategoryUrlRegex().Match(arrayOfUrls[randomIndex] ?? string.Empty).Value;

            if (!string.IsNullOrEmpty(url))
                return url;
        }
    }


    public async Task<string[]> FilterGoods(int[] ids)
    {
        var ints = new List<string>();
        foreach (var batch in ids.Batch(60))
        {
            var idsString = string.Join(',', batch);
            var responseMessage = await _httpClient.GetAsync(
                $"https://xl-catalog-api.rozetka.com.ua/v4/goods/getDetails?front-type=xl&country=UA&lang=ua&product_ids={idsString}");

            var content = await responseMessage.Content.ReadFromJsonAsync<GoodsResponse>();

            var filteredGoods = content?.GoodsCollection
                .Where(x => x.CommentsAmount > 100)
                .ToArray();

            if (filteredGoods?.Length > 0)
                ints.AddRange(filteredGoods.Select(x => x.Href));
        }

        return ints.ToArray();
    }

    [GeneratedRegex(@"http(?s).*?/c\d+/$", RegexOptions.Compiled)]
    private static partial Regex CategoryUrlRegex();
}

public class GoodsResponse
{
    [JsonPropertyName("data")]
    public Goods[] GoodsCollection { get; set; } = Array.Empty<Goods>();
}

public class Goods
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    [JsonPropertyName("comments_amount")]
    public long CommentsAmount { get; set; }

    [JsonPropertyName("comments_mark")]
    public double CommentsMark { get; set; }
}