﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace MPM_Betting.Services.Data;

public static class FootballApi
{
    record LeagueEntry(string Name, string Country, int Id);
    record TeamEntry(int Id, string Name);
    record PlayerEntry(string Name, string Position, int Id);
    
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
        
        return app;
    }
    
    private static async Task<List<PlayerEntry> > GetAllFootballPlayers(IDistributedCache cache)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), $"footballPlayers", async () =>
        {
            var allTeams = await GetAllFootballTeams(cache);
            var allPlayers = new List<PlayerEntry>();

            var tasks = allTeams.Select(async team =>
            {
                var players = await GetPlayersFromTeam(cache, team.Id);
                allPlayers.AddRange(players);
            });

            await Task.WhenAll(tasks);

            return allPlayers;
        });
    }
    
    private static async Task<List<PlayerEntry>> GetPlayersFromTeam(IDistributedCache cache, int teamId)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), $"footballPlayers-{teamId}", async () =>
        {
            var allPlayers = new List<PlayerEntry>();
            try
            {
                var client = new HttpClient();
                var url = $"https://www.fotmob.com/api/teams?id={teamId}";
                var response = await Utils.GetViaCache(cache, TimeSpan.FromMinutes(1), url,
                    async () => await client.GetAsync(url));
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);

                var squad = (JArray)jObject["squad"]!;

                foreach (var position in squad)
                {
                    var title = position["title"]!.Value<string>()!;
                    var members = (JArray)position["members"]!;
                    foreach (var member in members)
                    {
                        var id = member["id"]!.Value<int>();
                        var name = member["name"]!.Value<string>()!;
                        allPlayers.Add(new PlayerEntry(name, title, id));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load football team {teamId}");
            }

            return allPlayers;
        });
    } 
    
    private static async Task<List<TeamEntry>> GetTeamsFromLeague(IDistributedCache cache, int leagueId)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), $"footballTeams-{leagueId}", async () =>
        {
            var allTeams = new List<TeamEntry>();
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

                foreach (var team in all)
                {
                    var name = team["name"]!.Value<string>()!;
                    var id = team["id"]!.Value<int>();
                    allTeams.Add(new TeamEntry(id, name));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load football league {leagueId}");
            }

            return allTeams;
        });
    }
    
    private static async Task<List<TeamEntry>> GetAllFootballTeams(IDistributedCache cache)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), "footballTeams", async () =>
        {
            var allTeams = new List<TeamEntry>();
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

    private static async Task<List<LeagueEntry>> GetAllFootballLeagues(IDistributedCache cache)
    {
        return await Utils.GetViaCache(cache, TimeSpan.FromDays(1), "footballLeagues", async () =>
        {
            var allLeagues = new List<LeagueEntry>();

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

            return allLeagues;
        });
        
    }
}