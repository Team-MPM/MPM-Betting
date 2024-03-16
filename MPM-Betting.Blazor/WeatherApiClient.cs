using MPM_Betting.DataModel;

namespace MPM_Betting.Blazor;

// The WeatherApiClient class is a client for interacting with a weather API.
// It uses the HttpClient class to send HTTP requests and receive responses.
public class WeatherApiClient
{
    // The constructor for the WeatherApiClient class takes an HttpClient object as a parameter.
    // This HttpClient object is used to send HTTP requests to the weather API.
    public WeatherApiClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    // The HttpClient object that is used to send HTTP requests to the weather API.
    private HttpClient HttpClient { get; }

    // The GetWeatherAsync method sends an HTTP GET request to the weather API's /weatherforecast endpoint.
    // It returns an array of WeatherForecast objects that contain the forecast data.
    // If the API request fails, the method returns an empty array.
    public async Task<WeatherForecast[]> GetWeatherAsync()
    {
        return await HttpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast") ?? Array.Empty<WeatherForecast>();
    }
}

