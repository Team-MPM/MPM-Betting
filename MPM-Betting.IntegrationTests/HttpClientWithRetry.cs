using Polly;
using Polly.Retry;

namespace MPM_Betting.IntegrationTests;

public class HttpClientWithRetry(HttpClient httpClient)
{
    private readonly AsyncRetryPolicy m_RetryPolicy = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(1));

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await m_RetryPolicy.ExecuteAsync(async () =>
        {
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }

            return response;
        });
    }
}