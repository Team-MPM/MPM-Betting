using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;

namespace MPM_Betting.Services.Data;

public static class FootballApi
{
    record League(string Name, string Country, int Id);
    record Team(int Id, string Name);
    record Player(string Name, string Position, int Id);
    record LeagueTable(League League, List<LeagueTableEntry> Table, string Season, List<string> Seasons); //TODO: add ongoing, last 5 and promotion/relegation
    record LeagueTableEntry(Team Team, int Rank, int Played, int Won, int Drawn, int Lost, int GoalsFor, int GoalsAgainst, int Points);
    
    public static WebApplication MapFootballEndpoints(this WebApplication app)
    {
        // TODO: separate leagues and cups
        app.MapGet("/football/leagues", async (IDistributedCache cache) => await GetAllFootballLeagues(cache))
            .WithName("GetAllFootballLeagues")
            .WithOpenApi();
        
        app.MapGet("/football/teams", async (IDistributedCache cache, [FromQuery] int? league) => league is null ? await GetAllFootballTeams(cache) : await GetTeamsFromLeague(cache, league.Value))
            .WithName("GetAllFootballTeams")
            .WithOpenApi();
        
        app.MapGet("/football/players", async (IDistributedCache cache, [FromQuery] int? team) => team is null ? await GetAllFootballPlayers(cache) : await GetPlayersFromTeam(cache, team.Value))
            .WithName("GetAllFootballPlayers")
            .WithOpenApi();
        
        app.MapGet("/football/table", async (IDistributedCache cache, [FromQuery] int league, [FromQuery] string? season) => await GetLeagueTable(cache, league, season))
            .WithName("GetTable")
            .WithOpenApi();
        
        return app;
    }
    
    private static async Task<LeagueTable> GetLeagueTable(IDistributedCache cache, int leagueId, string? season)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromSeconds(1), $"leagueTable-{leagueId}-{season ?? "latest"}", async () =>
        {
            var client = new HttpClient();
            var url = $"https://www.fotmob.com/api/leagues?id={leagueId}";
            var response = await Utils.GetViaCache(cache, TimeSpan.FromMinutes(1), url,
                async () => await client.GetAsync(url));
            var json = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(json);

            var details = jObject["details"]!;
            var leagueName = details["name"]!.Value<string>()!;
            var country = details["country"]!.Value<string>()!;
            var selectedSeason = details["selectedSeason"]!.Value<string>()!;
            var allAvailableSeasons = (JArray)jObject["allAvailableSeasons"]!;
            var seasons = allAvailableSeasons.Select(availableSeason => availableSeason.Value<string>()!).ToList();
            
            if (season is null || !seasons.Contains(season))
            {
                season = seasons[0];
            }

            var data = jObject["table"]![0]!["data"]!;

            if (season != selectedSeason)
            {
                url = $"https://www.fotmob.com/api/table?url=https%3A%2F%2Fdata.fotmob.com%2Ftables.ext.{leagueId}.fot&selectedSeason={UrlEncoder.Default.Encode(season)}";
                response = await Utils.GetViaCache(cache, TimeSpan.FromMinutes(1), url,
                    async () => await client.GetAsync(url));
                json = await response.Content.ReadAsStringAsync();
                jObject = JObject.Parse(json);
                data = jObject;
            }

            var table = (JArray)data["table"]!["all"]!;
            var entries = (
                from entry in table
                let name = entry["name"]!.Value<string>()!
                let id = entry["id"]!.Value<int>()
                let rank = entry["idx"]!.Value<int>()
                let played = entry["played"]!.Value<int>()
                let wins = entry["wins"]!.Value<int>()
                let draws = entry["draws"]!.Value<int>()
                let losses = entry["losses"]!.Value<int>()
                let goalsFor = entry["scoresStr"]!.Value<string>()!.Split('-')[0]
                let goalsAgainst = entry["scoresStr"]!.Value<string>()!.Split('-')[1]
                let points = entry["pts"]!.Value<int>()
                select new LeagueTableEntry(new Team(id, name), rank, played, wins, draws, losses, int.Parse(goalsFor), int.Parse(goalsAgainst), points)).ToList();


            return new LeagueTable(new League(leagueName, country, leagueId), entries, season, seasons);
        });
    }
    
    private static async Task<List<Player> > GetAllFootballPlayers(IDistributedCache cache)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), $"footballPlayers", async () =>
        {
            var allTeams = await GetAllFootballTeams(cache);
            var allPlayers = new List<Player>();

            var tasks = allTeams.Select(async team =>
            {
                var players = await GetPlayersFromTeam(cache, team.Id);
                allPlayers.AddRange(players);
            });

            await Task.WhenAll(tasks);

            return allPlayers;
        });
    }
    
    private static async Task<List<Player>> GetPlayersFromTeam(IDistributedCache cache, int teamId)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), $"footballPlayers-{teamId}", async () =>
        {
            var allPlayers = new List<Player>();
            try
            {
                var client = new HttpClient();
                var url = $"https://www.fotmob.com/api/teams?id={teamId}";
                var response = await Utils.GetViaCache(cache, TimeSpan.FromMinutes(1), url,
                    async () => await client.GetAsync(url));
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);

                var squad = (JArray)jObject["squad"]!;

                allPlayers.AddRange(
                    from position in squad
                    let title = position["title"]!.Value<string>()!
                    let members = (JArray)position["members"]!
                    from member in members
                    let id = member["id"]!.Value<int>()
                    let name = member["name"]!.Value<string>()!
                    select new Player(name, title, id));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load football team {teamId}");
            }

            return allPlayers;
        });
    } 
    
    private static async Task<List<Team>> GetTeamsFromLeague(IDistributedCache cache, int leagueId)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), $"footballTeams-{leagueId}", async () =>
        {
            var allTeams = new List<Team>();
            try
            {
                var client = new HttpClient();
                var url = $"https://www.fotmob.com/api/leagues?id={leagueId}";
                var response = await Utils.GetViaCache(cache, TimeSpan.FromMinutes(1), url,
                    async () => await client.GetAsync(url));
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);

                var table = ((JArray)jObject["table"]!)[0]["data"]!["table"];
                table ??= ((JArray)jObject["table"]!)[0]["data"]!["tables"]![0]!["table"]!;
                var all = (JArray)table["all"]!;

                allTeams.AddRange(
                    from team in all
                    let name = team["name"]!.Value<string>()!
                    let id = team["id"]!.Value<int>()
                    select new Team(id, name));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load football league {leagueId}");
            }

            return allTeams;
        });
    }
    
    private static async Task<List<Team>> GetAllFootballTeams(IDistributedCache cache)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), "footballTeams", async () =>
        {
            var allTeams = new List<Team>();
            var allLeagues = await GetAllFootballLeagues(cache);

            var tasks = allLeagues.Select(async leagueEntry =>
            {
                if (leagueEntry.Country == "international" || leagueEntry.Name.Contains("Cup"))
                {
                    return;
                }

                allTeams.AddRange(await GetTeamsFromLeague(cache, leagueEntry.Id));
            });
            
            await Task.WhenAll(tasks);

            return allTeams;
        });
    }

    private static async Task<List<League>> GetAllFootballLeagues(IDistributedCache cache)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), "footballLeagues", async () =>
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://www.fotmob.com/api/allLeagues");

            var json = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(json);

            var international = (JArray)jObject["international"]!;
            var internationalLeagues = international[0]["leagues"]!;

            var allLeagues = (
                from league in internationalLeagues
                let leagueName = league["name"]!.Value<string>()!
                let leagueId = league["id"]!.Value<int>()
                select new League(leagueName, "international", leagueId)).ToList();

            var countries = (JArray)jObject["countries"]!;

            allLeagues.AddRange(
                from jToken in countries
                let countryName = jToken["name"]!.Value<string>()!
                let leagues = (JArray)jToken["leagues"]!
                from league in leagues
                let leagueName = league["name"]!.Value<string>()!
                let leagueId = league["id"]!.Value<int>()
                select new League(leagueName, countryName, leagueId));

            return allLeagues;
        });
        
    }
}