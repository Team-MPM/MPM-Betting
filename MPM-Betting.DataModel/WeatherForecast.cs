namespace MPM_Betting.DataModel;

/**
 * Represents a weather forecast for a specific date.
 */
public record WeatherForecast(
    /**
     * The date of the forecast.
     */
    DateOnly Date,

    /**
     * The temperature in Celsius.
     */
    int TemperatureC,

    /**
     * A brief summary of the weather forecast.
     */
    string? Summary)
{
    /**
     * The temperature in Fahrenheit, calculated from the Celsius temperature.
     */
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
