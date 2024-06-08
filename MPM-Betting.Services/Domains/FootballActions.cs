using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
     public async Task AddLeagueToFavourites(int id)
    {
        if (m_User is null)
            throw s_NoUserException;
        
        m_DbContext.UserFavouriteSeasons.Add(new FavouriteFootballLeague() {UserId = m_User.Id, LeagueId = id});
        await m_DbContext.SaveChangesAsync();
    }
    
    public async Task RemoveLeagueFromFavourites(int id)
    {
        if (m_User is null)
            throw s_NoUserException;
        
        if(m_DbContext.UserFavouriteSeasons.FirstOrDefault(s => s.User == m_User && s.LeagueId == id) is null)
            throw s_SeasonNotFoundException;

        m_DbContext.UserFavouriteSeasons.Remove(
            m_DbContext.UserFavouriteSeasons.First(s => s.User == m_User && s.LeagueId == id));
        await m_DbContext.SaveChangesAsync();
    }

    public List<int> GetFavouriteLeaguesForUser() => m_DbContext.UserFavouriteSeasons.Where(s => s.User == m_User).Select(s => s.LeagueId).ToList();

    public async Task<MpmResult<GameBet>> PlaceGameBet(double quote, int homeScore, int awayScore,
        int referenceId, int points, MpmGroup? group = null)
    {
        if (m_User is null)
            return s_NoUserException;
        
        if (quote < 1 || homeScore < 0 || awayScore < 0 || points < 1)
            return s_InvalidBetParameter;
        
        if (m_User.Points < points)
            return s_InvalidBetParameter;
        
        if (await s_UserHasFootballGameBetQuery(m_DbContext, referenceId, m_User.Id, group))
            return s_AlreadyExistsException;

        for (var i = 0; i < 50; i++)
        {
            var response = await httpclient.GetAsync($"/football/track/game/{referenceId}");
            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                case HttpStatusCode.Processing:
                    await Task.Delay(200);
                    continue;
                case HttpStatusCode.Found:
                    break;
                default:
                    logger.LogError("Failed to track game with reference id {ReferenceId}, code: {HttpStatusCode}", referenceId, response.StatusCode);
                    return s_InvalidBetParameter;
            }
            
            break;
        }
        
        var game = await m_DbContext.Games.FirstOrDefaultAsync(g => g.ReferenceId == referenceId);

        if (game is null)
        {
            logger.LogWarning("Game with reference id {ReferenceId} not found", referenceId);
            return s_InvalidBetParameter;
        }

        var bet = new GameBet()
        {
            UserId = m_User.Id,
            GameId = game.Id,
            GroupId = group?.Id,
            Quote = quote,
            HomeScore = homeScore,
            AwayScore = awayScore,
            Type = EBetType.FootballGame,
            Points = points,
        };

        m_User.Points -= points;
        m_DbContext.Users.Update(m_User);
        
        await m_DbContext.FootballGameBets.AddAsync(bet);
        await m_DbContext.SaveChangesAsync();
        
        return bet;
    }
}