using Polly;

namespace Crawler.Console;

public static class CustomRetryPolicy
{
    private const int MaxApiCallRetries = 5;

    public static IAsyncPolicy<HttpResponseMessage> GetDefault() =>
        Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(MaxApiCallRetries, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
}