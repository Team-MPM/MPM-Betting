using MPM_Betting.DataModel; // DataModel namespace is imported

var builder = WebApplication.CreateBuilder(args); // Creates a new web application builder instance

builder.AddServiceDefaults(); // Adds default services

builder.Services.AddEndpointsApiExplorer(); // Adds Endpoints API Explorer service
builder.Services.AddSwaggerGen(); // Adds Swagger Generator service

var app = builder.Build(); // Builds the web application

app.MapDefaultEndpoints(); // Maps default endpoints

if (app.Environment.IsDevelopment()) // If the application is running in development environment
{
    app.UseSwagger(); // Enables Swagger documentation
    app.UseSwaggerUI(); // Enables Swagger UI
}

app.UseHttpsRedirection(); // Redirects HTTP requests to HTTPS

var summaries = new[] // Defines an array of weather summary descriptions
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () => // Defines a GET request handler for the "/weatherforecast" endpoint
{
    Thread.Sleep(2000); // Simulates a 2-second delay for demonstration purposes
    var forecast = Enumerable.Range(1, 5).Select(index => // Generates a 5-day weather forecast
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)), // Sets the date for each day in the forecast
                Random.Shared.Next(-20, 55), // Generates a random temperature value
                summaries[Random.Shared.Next(summaries.Length)] // Selects a random weather summary description
            ))
        .ToArray();
    return forecast; // Returns the generated weather forecast
})
.WithName("GetWe
