using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace MPM_Betting.Services.Data;

public class FootballApi(ILogger<FootballApi> logger, MpmCache cache)
{
    public record struct League(string Name, string Country, int Id);

    public record struct Team(int Id, string Name);

    public record struct Player(string Name, string Position, int Id);

    public record struct LeagueTable(
        League League,
        List<LeagueTableEntry> Table,
        string Season,
        List<string> Seasons); //TODO: add ongoing, last 5 and promotion/relegation

    public record struct LeagueTableEntry(
        Team Team,
        int Rank,
        int Played,
        int Won,
        int Drawn,
        int Lost,
        int GoalsFor,
        int GoalsAgainst,
        int Points);

    public record struct ScoreEntry(Team HomeTeam, Team AwayTeam, int HomeScore, int AwayScore, GameState State);

    public record struct GameEntry(int Id, ScoreEntry Score, DateTime StartTime, string? Time, League League);

    public record struct GameDetails(GameEntry GameEntry, GameTimeData TimeData); // TODO: add actual details (goals, cards, subs, passing, possession, etc.)

    public record struct GameTimeData(
        DateTime? FirstHalfStarted,
        DateTime? FirstHalfEnded,
        DateTime? SecondHalfStarted,
        DateTime? SecondHalfEnded,
        DateTime? FirstExtraHalfStarted,
        DateTime? FirstExtraHalfEnded,
        DateTime? SecondExtraHalfStarted,
        DateTime? SecondExtraHalfEnded,
        DateTime? GameEnded);

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

    /// <summary>
    /// General exception when something goes wrong in the Football Api
    /// </summary>
    /// <param name="message">What data failed to load</param>
    /// <param name="innerException">The exception that is the cause of the current</param>
    public class FailedToGetApiDataException(string? message, Exception? innerException = null)
        : Exception(message, innerException);

    /// <summary>
    /// Thrown when a game with the given id is not found
    /// </summary>
    /// <param name="gameId">ID of the game that was not found</param>
    /// <param name="innerException">The exception that is the cause of the current</param>
    public class GameNotFoundException(int gameId, Exception? innerException = null)
        : Exception($"Football Game with id {gameId} not found", innerException);

    /// <summary>
    /// Thrown when a league with the given id is not found
    /// </summary>
    /// <param name="leagueId">ID of the league that was not found</param>
    /// <param name="innerException">The exception that is the cause of the current</param>
    public class LeagueNotFoundException(int leagueId, Exception? innerException = null)
        : Exception($"Football League with id {leagueId} not found", innerException);

    /// <summary>
    /// Thrown when a team with the given id is not found
    /// </summary>
    /// <param name="teamId">ID of the team that was not found</param>
    /// <param name="innerException">The exception that is the cause of the current</param>
    public class TeamNotFoundException(int teamId, Exception? innerException = null)
        : Exception($"Football team with id {teamId} not found", innerException);

    
    /// <summary>
    /// Get detailed information about a football game
    /// </summary>
    /// <param name="gameId">Id optained from other Api endpoints</param>
    /// <returns>GameDetails for id</returns>
    /// <exception cref="GameNotFoundException">
    ///     Returned as failure if the game id does not correspond to a valid game entry
    ///     or the game entry could not be loaded
    /// </exception>
    public async Task<MpmResult<GameDetails>> GetGameDetails(int gameId)
    {
        return await cache.Get(TimeSpan.FromSeconds(3), $"game-entry-{gameId}", async () =>
        {
            var uri = new Uri($"https://www.fotmob.com/api/matchDetails?matchId={gameId}");
            var result = await cache.GetByUri(TimeSpan.FromSeconds(1), uri);
            return result.Match(
                json => Utils.Try(() => GetGameDetailsFromJson(json)),
                err => new GameNotFoundException(gameId, err)
            );
        });
    }

    private static GameDetails GetGameDetailsFromJson(string json)
    {
        var match = JObject.Parse(json);

        var general = match["general"]!;
        var matchId = general["matchId"]!.Value<int>();
        var leagueId = general["leagueId"]!.Value<int>();
        var leagueName = general["leagueName"]!.Value<string>()!;
        var countryCode = general["countryCode"]!.Value<string>()!;
        var league = new League(leagueName, countryCode, leagueId);
        var started = general["started"]!.Value<bool>();
        var finished = general["finished"]!.Value<bool>();
        var matchTimeUtcDate = general["matchTimeUTCDate"]!.Value<DateTime>();
        var home = new Team(general["homeTeam"]!["id"]!.Value<int>(),
            general["homeTeam"]!["name"]!.Value<string>()!);
        var away = new Team(general["awayTeam"]!["id"]!.Value<int>(),
            general["awayTeam"]!["name"]!.Value<string>()!);
        var season = general["parentLeagueSeason"]!.Value<string>()!;
        var round = general["round"]?.Value<int?>() ?? 0;

        var header = match["header"]!;
        var status = header["status"]!;
        var scoreStr = status["scoreStr"]?.Value<string>();
        var scores = scoreStr is not null
            ? scoreStr.Split('-').Select(s => int.Parse(s.Trim())).ToArray()
            : [0, 0];
        var homeScore = scores[0];
        var awayScore = scores[1];

        var liveTime = status["liveTime"];
        var currentTime = liveTime?["long"]?.Value<string>() ?? "";

        var cancelled = status["cancelled"]!.Value<bool>();
        var halfs = status["halfs"]!;

        var gameTimes = GameTimeDataFromHalfs(halfs);
        // TODO: idk, maybe fix?
        //var state = ExtractGameStateFromStatus(gameTimes, cancelled);
        var state = GameState.None;
        if (started)
            state = GameState.FirstHalf;
        if (finished)
            state = GameState.EndedAfterSecondHalf;

        var gameEntry = new GameEntry(
            matchId, 
            new ScoreEntry(home, away, homeScore, awayScore, state), 
            matchTimeUtcDate, 
            currentTime,
            league);
        return new GameDetails(gameEntry, gameTimes);
    }

    // ReSharper disable once CognitiveComplexity // its just a frew if blocks xD
    private static GameState ExtractGameStateFromStatus(GameTimeData gameTimes, bool cancelled)
    {
        var state = GameState.None;
        if (gameTimes.FirstHalfStarted is not null)
        {
            state = GameState.FirstHalf;
        }

        if (gameTimes.FirstHalfEnded is not null)
        {
            state = GameState.HalfTimeBreak;
        }

        if (gameTimes.SecondHalfStarted is not null)
        {
            state = GameState.SecondHalf;
        }

        if (gameTimes.SecondHalfEnded is not null)
        {
            state = GameState.BreakAfterSecondHalf;
        }

        if (gameTimes.FirstExtraHalfStarted is not null)
        {
            state = GameState.FirstOvertime;
        }

        if (gameTimes.FirstExtraHalfEnded is not null)
        {
            state = GameState.OvertimeBreak;
        }

        if (gameTimes.SecondExtraHalfStarted is not null)
        {
            state = GameState.SecondOvertime;
        }

        if (gameTimes.SecondExtraHalfEnded is not null)
        {
            state = GameState.EndedAfterOverTime;
        }

        if (gameTimes.GameEnded is null && gameTimes.SecondExtraHalfEnded is not null)
        {
            state = GameState.PenaltyShootout;
        }

        if (cancelled)
        {
            state = GameState.Cancelled;
        }

        if (gameTimes.GameEnded is not null && state is GameState.BreakAfterSecondHalf)
        {
            state = GameState.EndedAfterSecondHalf;
        }

        if (gameTimes.GameEnded is not null && state is GameState.PenaltyShootout)
        {
            state = GameState.EndedAfterPenaltyShootout;
        }

        return state;
    }

    private static GameTimeData GameTimeDataFromHalfs(JToken halfs)
    {
        const string format = "dd.MM.yyyy HH:mm:ss";
        return new GameTimeData
        {
            FirstHalfStarted = halfs.TryGetDateTime("firstHalfStarted", format),
            FirstHalfEnded = halfs.TryGetDateTime("firstHalfEnded", format),
            SecondHalfStarted = halfs.TryGetDateTime("secondHalfStarted", format),
            SecondHalfEnded = halfs.TryGetDateTime("secondHalfEnded", format),
            FirstExtraHalfStarted = halfs.TryGetDateTime("firstExtraHalfStarted", format),
            FirstExtraHalfEnded = halfs.TryGetDateTime("firstExtraHalfEnded", format),
            SecondExtraHalfStarted = halfs.TryGetDateTime("secondExtraHalfStarted", format),
            SecondExtraHalfEnded = halfs.TryGetDateTime("secondExtraHalfEnded", format),
            GameEnded = halfs.TryGetDateTime("gameEnded", format)
        }; 
    }

    /// <summary>
    /// Get a list of games for a league
    /// </summary>
    /// <param name="leagueId">Id optained from </param>
    /// <param name="date">NOT IMPLEMENTED (TODO)</param>
    /// <returns>GameDetails for id</returns>
    /// <exception cref="LeagueNotFoundException">
    ///     Returned as failure if the league id does not correspond to a valid league entry
    ///     or the league entry could not be loaded
    /// </exception>
    public async Task<MpmResult<List<GameEntry>>> GetGameEntries(int leagueId, DateOnly? date)
    {
        return await cache.Get(TimeSpan.FromSeconds(5), 
            $"game-entries-l:{leagueId}-d:{date ?? DateOnly.MaxValue}", async () =>
            {
                var uri = new Uri($"https://www.fotmob.com/api/leagues?id={leagueId}");
                var result = await cache.GetByUri(TimeSpan.FromSeconds(1), uri);
                return result.Match(
                    json => Utils.Try(() => GameEntriesFromJson(json)),
                    err => new LeagueNotFoundException(leagueId, err)
                );
            });
    }

    private static List<GameEntry> GameEntriesFromJson(string json)
    {
        var jObject = JObject.Parse(json);

        var matches = jObject["matches"]!;
        var allMatches = (JArray)matches["allMatches"]!;
        
        var details = jObject["details"]!;
        var leagueId = details["id"]!.Value<int>();
        var leagueName = details["name"]!.Value<string>()!;
        var country = details["country"]!.Value<string>()!;
        var league = new League(leagueName, country, leagueId);

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
            var scores = scoreStr is not null
                ? scoreStr.Split('-').Select(s => int.Parse(s.Trim())).ToArray()
                : [0, 0];
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
                "FT" => GameState.EndedAfterSecondHalf,
                _ => state
            };
            state = reason switch
            {
                "FT" => GameState.EndedAfterSecondHalf,
                _ => state
            };

            state = cancelled ? GameState.Cancelled : state;

            gameEntries.Add(
                new GameEntry(
                    matchId, 
                    new ScoreEntry(home, away, homeScore, awayScore, state),
                    utcTime, 
                    currentTime,
                    league));
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


        // half-time

        // "liveTime": {
        //     "short": "HT",
        //     "shortKey": "halftime_short",
        //     "long": "Half-Time",
        //     "longKey": "pause_match",
        //     "maxTime": 45,
        //     "addedTime": 0
        // }
        return gameEntries;
    }

    /// <summary>
    /// Get an overview of the league table, including rankings, points, goals, ga, etc.
    /// </summary>
    /// <param name="leagueId">The league id to query the data for</param>
    /// <param name="season">Optional: the season you want to query the data from. Default: latest</param>
    /// <returns>Result of LeagueTable</returns>
    /// /// <exception cref="LeagueNotFoundException">
    ///     Returned as failure if the league id does not correspond to a valid league entry
    ///     or the game league could not be loaded
    /// </exception>
    public async Task<MpmResult<LeagueTable>> GetLeagueTable(int leagueId, string? season)
    {
        return await cache.Get(TimeSpan.FromSeconds(1), $"leagueTable-{leagueId}-{season ?? "latest"}", async () =>
        {
            var uri = new Uri($"https://www.fotmob.com/api/leagues?id={leagueId}");
            var result = await cache.GetByUri(TimeSpan.FromMinutes(1), uri);
            return await result.Match<Task<MpmResult<LeagueTable>>>(
                async json => await GetLeagueTableFromJson(json, season),
                err => Task.FromResult<MpmResult<LeagueTable>>(new LeagueNotFoundException(leagueId, err))
            );
        });
    }

    private async Task<MpmResult<LeagueTable>> GetLeagueTableFromJson(string json, string? season)
    {
        var jObject = JObject.Parse(json);

        var details = jObject["details"]!;
        var leagueId = details["id"]!.Value<int>();
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
            var uri = new Uri(
                $"https://www.fotmob.com/api/table?url=https%3A%2F%2Fdata.fotmob.com%2Ftables.ext.{leagueId}.fot&selectedSeason={UrlEncoder.Default.Encode(season)}");
            var result = await cache.GetByUri(TimeSpan.FromMinutes(1), uri);
            if (result.IsFaulted) return new LeagueNotFoundException(leagueId, result.Exception);
            jObject = JObject.Parse(result.Value);
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
            select new LeagueTableEntry(new Team(id, name), rank, played, wins, draws, losses, int.Parse(goalsFor),
                int.Parse(goalsAgainst), points)).ToList();
        
        return new LeagueTable(new League(leagueName, country, leagueId), entries, season, seasons);
    }
    
    /// <summary>
    /// Get a list of string representing the available seasons for a league
    /// </summary>
    /// <param name="leagueId">The league id to query the data for</param>
    /// <returns>Result of a string list</returns>
    public async Task<MpmResult<List<string>>> GetSeasonsForLeague(int leagueId)
    {
        return await cache.Get(TimeSpan.FromSeconds(1), $"league-seasons-{leagueId}", async () =>
        {
            var uri = new Uri($"https://www.fotmob.com/api/leagues?id={leagueId}");
            var result = await cache.GetByUri(TimeSpan.FromMinutes(1), uri);
            return result.Match(GetSeasonsForLeagueFromJson,
                err => new LeagueNotFoundException(leagueId, err)
            );
        });
    }

    private static MpmResult<List<string>> GetSeasonsForLeagueFromJson(string? json)
    {
        if (json is null or "null") return new ArgumentNullException(nameof(json));
        var jObject = JObject.Parse(json);
        var allAvailableSeasons = (JArray)jObject["allAvailableSeasons"]!;
        var seasons = allAvailableSeasons.Select(availableSeason => availableSeason.Value<string>()!).ToList();
        
        return seasons;
    }

    /// <summary>
    /// Get all football players from all teams from all leagues
    /// </summary>
    /// <returns>A List of Player Entries</returns>
    /// <exception cref="TeamNotFoundException">
    ///     Returned as failure if the team id does not correspond to a valid team entry
    ///     or the team entry could not be loaded
    /// </exception>
    /// <exception cref="FailedToGetApiDataException"></exception>
    public async Task<MpmResult<List<Player>>> GetAllFootballPlayers()
    {
        return await cache.Get<Task<MpmResult<List<Player>>>>(TimeSpan.FromDays(1), "footballPlayers", async () =>
        {
            var allPlayers = new List<Player>();
            var allTeams = await GetAllFootballTeams();
            if (allTeams.IsFaulted) return allTeams.Exception;
            
            var tasks = allTeams.Value.Select(async team =>
            {
                var players = await GetPlayersFromTeam(team.Id);
                if (players.IsFaulted) throw new TeamNotFoundException(team.Id);
                allPlayers.AddRange(players.Value);
            });

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                return new FailedToGetApiDataException("Unexpected Error occured while trying to load team data", e);
            }

            return allPlayers;
        });
    }

    /// <summary>
    ///     Get all football players from a specified team
    /// </summary>
    /// <returns>A List of Player Entries</returns>
    /// <param name="teamId">The id of the team you want to get all players from</param>
    /// <exception cref="TeamNotFoundException">
    ///     Returned as failure if the team id does not correspond to a valid team entry
    ///     or the team entry could not be loaded
    /// </exception>
    /// <exception cref="FailedToGetApiDataException"></exception>
    public async Task<MpmResult<List<Player>>> GetPlayersFromTeam(int teamId)
    {
        return await cache.Get(TimeSpan.FromDays(1), $"footballPlayers-{teamId}", async () =>
        {
            var uri = new Uri($"https://www.fotmob.com/api/teams?id={teamId}");
            var result = await cache.GetByUri(TimeSpan.FromMinutes(1), uri);
            return result.Match(GetTeamPlayersFromJson,
                err => new TeamNotFoundException(teamId, err)
            );
        });
    }

    private static MpmResult<List<Player>> GetTeamPlayersFromJson(string json)
    {
        try
        {
            var jObject = JObject.Parse(json);

            var squad = (JArray)jObject["squad"]!;

            var allPlayers = from position in squad
                let title = position["title"]!.Value<string>()!
                let members = (JArray)position["members"]!
                from member in members
                let id = member["id"]!.Value<int>()
                let name = member["name"]!.Value<string>()!
                select new Player(name, title, id);
            
            return allPlayers.ToList();
        }
        catch (Exception e)
        {
            return new FailedToGetApiDataException("Failed to load team players", e);
        }
    }

    /// <summary>
    /// Get a list of teams that are part of a league
    /// </summary>
    /// <param name="leagueId">The league id to query the data for</param>
    /// <returns>Result of LeagueTable</returns>
    /// /// <exception cref="LeagueNotFoundException">
    ///     Returned as failure if the league id does not correspond to a valid league entry
    ///     or the game league could not be loaded
    /// </exception>
    /// <exception cref="FailedToGetApiDataException"></exception>
    public async Task<MpmResult<List<Team>>> GetTeamsFromLeague(int leagueId)
    {
        return await cache.Get(TimeSpan.FromDays(1), $"footballTeams-{leagueId}", async () =>
        {
                var uri = new Uri($"https://www.fotmob.com/api/leagues?id={leagueId}");
                var result = await cache.GetByUri(TimeSpan.FromMinutes(1), uri);
                return result.Match(GetLeagueTeamsFromJson,
                    err => new LeagueNotFoundException(leagueId, err)
                );
        });
    }

    private static MpmResult<List<Team>> GetLeagueTeamsFromJson(string json)
    {
        try
        {
            var jObject = JObject.Parse(json);

            var table = ((JArray)jObject["table"]!)[0]["data"]!["table"];
            table ??= ((JArray)jObject["table"]!)[0]["data"]!["tables"]![0]!["table"]!;
            var all = (JArray)table["all"]!;

            var teams =  from team in all
                let name = team["name"]!.Value<string>()!
                let id = team["id"]!.Value<int>()
                select new Team(id, name);
            
            return teams.ToList();
        }
        catch (Exception e)
        {
            return new FailedToGetApiDataException("Failed to load league teams", e);
        }
    }

    /// <summary>
    /// Get a list of all football teams from all leagues
    /// </summary>
    /// <returns>List of team entries</returns>
    /// <exception cref="FailedToGetApiDataException"></exception>
    public async Task<MpmResult<List<Team>>> GetAllFootballTeams()
    {
        return await cache.Get<Task<MpmResult<List<Team>>>>(TimeSpan.FromDays(1), "allFootballTeams", async () =>
        {
            var allTeams = new List<Team>();
            var allLeagues = await GetAllFootballLeagues();
            if (allLeagues.IsFaulted) return allLeagues.Exception;

            var tasks = allLeagues.Value.Select(async leagueEntry =>
            {
                if (leagueEntry.Country == "international" || leagueEntry.Name.Contains("Cup"))
                    return;

                var result = await GetTeamsFromLeague(leagueEntry.Id);
                
                if (result.IsFaulted) 
                    throw new FailedToGetApiDataException("Failed to load team data", result.Exception);
                
                allTeams.AddRange(result.Value);
            });

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (AggregateException e)
            {
                return e;
            }
            catch (Exception e)
            {
                return new FailedToGetApiDataException("Unexpected Error occured while trying to load team data", e);
            }

            return allTeams;
        });
    }

    /// <summary>
    /// Get a list of all supported football leagues
    /// </summary>
    /// <returns>List of league entries</returns>
    /// <exception cref="FailedToGetApiDataException"></exception>
    public async Task<MpmResult<List<League>>> GetAllFootballLeagues()
    {
        return await cache.Get(TimeSpan.FromDays(1), "allFootballLeagues", async () =>
        {
            var uri = new Uri("https://www.fotmob.com/api/allLeagues");
            var result = await cache.GetByUri(TimeSpan.FromDays(1), uri);

            return result.Match<MpmResult<List<League>>>(
                json => Utils.Try(() => GetLeaguesFromJson(json)),
                err => new FailedToGetApiDataException("Failed to get league overview", err)
            );
        });
    }

    private static List<League> GetLeaguesFromJson(string json)
    {
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
    }
}
