using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace MPM_Betting.Services.Data;

public static class FootballApi
{
    record LeagueEntry(string Name, string Country, int Id);
    
    public static WebApplication MapFootballEndpoints(this WebApplication app)
    {
        app.MapGet("/football/leagues", async (IMemoryCache cache) =>
            {
                const string cacheKey = "footballLeagues";
                if (cache.TryGetValue(cacheKey, out List<LeagueEntry>? allLeagues)) 
                    return allLeagues;

                allLeagues ??= [];
                
                var client = new HttpClient();
                var response = await client.GetAsync("https://www.fotmob.com/api/allLeagues");

                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                
                var international = (JArray)jObject["international"]!;
                var internationalLeagues = international[0]["leagues"]!;
                
                foreach (var league in internationalLeagues)
                {
                    var leagueName = league["name"]!.Value<string>()!;
                    var leagueId = league["id"]!.Value<int>();
                    allLeagues.Add(new LeagueEntry(leagueName, "international", leagueId));
                }
                
                var countries = (JArray)jObject["countries"]!;
                
                foreach (var jToken in countries)
                {
                    var countryName = jToken["name"]!.Value<string>()!;
                    var leagues = (JArray)jToken["leagues"]!;
                    foreach (var league in leagues)
                    {
                        var leagueName = league["name"]!.Value<string>()!;
                        var leagueId = league["id"]!.Value<int>();
                        allLeagues.Add(new LeagueEntry(leagueName, countryName, leagueId));
                    }
                }
                
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(1));

                cache.Set(cacheKey, allLeagues, cacheEntryOptions);
                
                return allLeagues;
            })
        .WithName("GetAllFootballLeagues")
        .WithOpenApi();
        
        return app;
    }
}