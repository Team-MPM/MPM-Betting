using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace MPM_Betting.Services.Data;

public class FootballApi(ILogger<FootballApi> logger, MpmCache cache)
{
    public record League(string Name, string Country, int Id);
    public record Team(int Id, string Name);
    public record Player(string Name, string Position, int Id);
    public record LeagueTable(League League, List<LeagueTableEntry> Table, string Season, List<string> Seasons); //TODO: add ongoing, last 5 and promotion/relegation
    public record LeagueTableEntry(Team Team, int Rank, int Played, int Won, int Drawn, int Lost, int GoalsFor, int GoalsAgainst, int Points);
    public record struct ScoreEntry(Team HomeTeam, Team AwayTeam, int HomeScore, int AwayScore, GameState State);
    public record GameEntry(int Id, ScoreEntry Score, DateTime StartTime, string? Time);

    public enum GameState
    {
        None = 0,
        Cancelled,
        FirstHalf,
        HalfTimeBreak,
        SecondHalf,
        EndedAfterSecondHalf,
        BreakAfterSecondHalf,
        FirstOvertime,
        OvertimeBreak,
        SecondOvertime,
        EndedAfterOverTime,
        PenaltyShootout,
        EndedAfterPenaltyShootout
    }
    
    

    public async Task<List<GameEntry>> GetGameEntries(int leagueId, DateOnly? date)
    {
        return await cache.GetViaCache(TimeSpan.FromSeconds(1), $"game-entries-{leagueId}-{date ?? DateOnly.MaxValue}", async () =>
        {
            var client = new HttpClient();
            var url = $"https://www.fotmob.com/api/leagues?id={leagueId}";
            var response = await cache.GetViaCache(TimeSpan.FromSeconds(1), url,
                async () => await client.GetAsync(url));
            var json = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(json);

            var matches = jObject["matches"]!;
            var allMatches = (JArray)matches["allMatches"]!;
            
            List<GameEntry> gameEntries = [];
            
            foreach (var match in allMatches)
            {
                var round = match["round"]!.Value<int>();
                var matchId = match["id"]!.Value<int>();
                var home = new Team(match["home"]!["id"]!.Value<int>(), match["home"]!["name"]!.Value<string>()!);
                var away = new Team(match["away"]!["id"]!.Value<int>(), match["away"]!["name"]!.Value<string>()!);
                var status = match["status"]!;
                var utcTime = status["utcTime"]!.Value<DateTime>();
                var finished = status["finished"]!.Value<bool>();
                var started = status["started"]!.Value<bool>();
                var cancelled = status["cancelled"]!.Value<bool>();
                var scoreStr = status["scoreStr"]?.Value<string>();
                var scores = scoreStr is not null ? scoreStr.Split('-').Select(s => int.Parse(s.Trim())).ToArray() : [0, 0];
                var homeScore = scores[0];
                var awayScore = scores[1];
                var reason = status["reason"]?["short"]?.Value<string>();
                var live = status["liveTime"];
                var maxTime = live?["maxTime"]?.Value<int>() ?? 0;
                var addedTime = live?["addedTime"]?.Value<int>() ?? 0;
                var currentTime = live?["long"]?.Value<string>();
                var currentTimeShort = live?["short"]?.Value<string>();
                var state = GameState.None;
                state = maxTime switch
                {
                    45 => GameState.FirstHalf,
                    90 => GameState.SecondHalf,
                    105 => GameState.FirstOvertime,
                    120 => GameState.SecondOvertime,
                    _ => state
                };
                state = currentTimeShort switch
                {
                    "HT" => GameState.HalfTimeBreak,
                    _ => state
                };
                state = reason switch
                {
                    "FT" => GameState.EndedAfterSecondHalf,
                    _ => state
                };
                state = cancelled switch
                {
                    true => GameState.Cancelled,
                    false => state
                };
                
                gameEntries.Add(new GameEntry(matchId, new ScoreEntry(home, away, homeScore, awayScore, state), utcTime, currentTime));
            }
            
            
            //finished
            // {
            //     "round": "1",
            //     "roundName": 1,
            //     "pageUrl": "/matches/burnley-vs-manchester-city/2ai7j8#4193450",
            //     "id": "4193450",
            //     "home": {
            //         "name": "Burnley",
            //         "shortName": "Burnley",
            //         "id": "8191"
            //     },
            //     "away": {
            //         "name": "Manchester City",
            //         "shortName": "Man City",
            //         "id": "8456"
            //     },
            //     "status": {
            //         "utcTime": "2023-08-11T19:00:00Z",
            //         "finished": true,
            //         "started": true,
            //         "cancelled": false,
            //         "scoreStr": "0 - 3",
            //         "reason": {
            //             "short": "FT",
            //             "shortKey": "fulltime_short",
            //             "long": "Full-Time",
            //             "longKey": "finished"
            //         }
            //     }
            // },
            
            //running
            // {
            //     "round": "33",
            //     "roundName": 33,
            //     "pageUrl": "/matches/persita-vs-persis-solo/3klkl3z6#4184143",
            //     "id": "4184143",
            //     "home": {
            //         "name": "Persis Solo",
            //         "shortName": "Persis Solo",
            //         "id": "583034"
            //     },
            //     "away": {
            //         "name": "Persita",
            //         "shortName": "Persita",
            //         "id": "165206"
            //     },
            //     "status": {
            //         "utcTime": "2024-04-26T08:00:00.000Z",
            //         "finished": false,
            //         "started": true,
            //         "cancelled": false,
            //         "ongoing": true,
            //         "scoreStr": "0 - 1",
            //         "liveTime": {
            //             "short": "28’",
            //             "shortKey": "",
            //             "long": "27:02",
            //             "longKey": "",
            //             "maxTime": 45,
            //             "addedTime": 0
            //         }
            //     }
            // },
            
            //not started
            // {
            //     "round": "33",
            //     "roundName": 33,
            //     "pageUrl": "/matches/persija-jakarta-vs-rans-nusantara/a9fy1lqf#4184152",
            //     "id": "4184152",
            //     "home": {
            //         "name": "RANS Nusantara",
            //         "shortName": "RANS Nusantara",
            //         "id": "1103033"
            //     },
            //     "away": {
            //         "name": "Persija Jakarta",
            //         "shortName": "Persija Jakarta",
            //         "id": "165191"
            //     },
            //     "status": {
            //         "utcTime": "2024-04-26T12:00:00Z",
            //         "started": false,
            //         "cancelled": false,
            //         "finished": false
            //     }
            // },
            
            
            // half time
            
            // "liveTime": {
            //     "short": "HT",
            //     "shortKey": "halftime_short",
            //     "long": "Half-Time",
            //     "longKey": "pause_match",
            //     "maxTime": 45,
            //     "addedTime": 0
            // }
            
            return gameEntries;
        });
    }
    
    public async Task<LeagueTable> GetLeagueTable(int leagueId, string? season)
    {
        return await cache.GetViaCache(TimeSpan.FromSeconds(1), $"leagueTable-{leagueId}-{season ?? "latest"}", async () =>
        {
            var client = new HttpClient();
            var url = $"https://www.fotmob.com/api/leagues?id={leagueId}";
            var response = await cache.GetViaCache(TimeSpan.FromMinutes(1), url,
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
                response = await cache.GetViaCache(TimeSpan.FromMinutes(1), url,
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
    
    public async Task<List<Player> > GetAllFootballPlayers()
    {
        return await cache.GetViaCache(TimeSpan.FromDays(1), $"footballPlayers", async () =>
        {
            var allTeams = await GetAllFootballTeams();
            var allPlayers = new List<Player>();

            var tasks = allTeams.Select(async team =>
            {
                var players = await GetPlayersFromTeam(team.Id);
                allPlayers.AddRange(players);
            });

            await Task.WhenAll(tasks);

            return allPlayers;
        });
    }
    
    public async Task<List<Player>> GetPlayersFromTeam(int teamId)
    {
        return await cache.GetViaCache(TimeSpan.FromDays(1), $"footballPlayers-{teamId}", async () =>
        {
            var allPlayers = new List<Player>();
            try
            {
                var client = new HttpClient();
                var url = $"https://www.fotmob.com/api/teams?id={teamId}";
                var response = await cache.GetViaCache(TimeSpan.FromMinutes(1), url,
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
    
    public async Task<List<Team>> GetTeamsFromLeague(int leagueId)
    {
        return await cache.GetViaCache(TimeSpan.FromDays(1), $"footballTeams-{leagueId}", async () =>
        {
            var allTeams = new List<Team>();
            try
            {
                var client = new HttpClient();
                var url = $"https://www.fotmob.com/api/leagues?id={leagueId}";
                var response = await cache.GetViaCache(TimeSpan.FromMinutes(1), url,
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
    
    public async Task<List<Team>> GetAllFootballTeams()
    {
        return await cache.GetViaCache(TimeSpan.FromDays(1), "footballTeams", async () =>
        {
            var allTeams = new List<Team>();
            var allLeagues = await GetAllFootballLeagues();

            var tasks = allLeagues.Select(async leagueEntry =>
            {
                if (leagueEntry.Country == "international" || leagueEntry.Name.Contains("Cup"))
                {
                    return;
                }

                allTeams.AddRange(await GetTeamsFromLeague(leagueEntry.Id));
            });
            
            await Task.WhenAll(tasks);

            return allTeams;
        });
    }

    public async Task<List<League>> GetAllFootballLeagues()
    {
        return await cache.GetViaCache(TimeSpan.FromDays(1), "footballLeagues", async () =>
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