using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
    public async Task<MpmResult<Bet>> CreateFootballResultBet(MpmGroup group, Game game, EResult result)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        if (result is EResult.None) return s_InvalidBetParameter;

        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var existingBet = await s_GetResultBetByGameAndUser.Invoke(m_DbContext, game, m_User);
        if (existingBet is not null) return s_AlreadyExistsException;
        if (game.GameState != EGameState.Upcoming) return s_InvalidDateException;


        var bet = new ResultBet()
        {
            UserId = m_User.Id,
            GroupId = group.Id,
            GameId = game.Id,
            Result = result,
            QuoteHome = 1,
            QuoteDraw = 1,
            QuoteAway = 1
        };
        await m_DbContext.FootballResultBets.AddAsync(bet);
        await m_DbContext.SaveChangesAsync();

        return bet;
    }
    
    public async Task<MpmResult<Bet>> CreateFootballScoreBet(MpmGroup group, Game game, int HomeScore, int AwayScore)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        if(HomeScore < 0 || AwayScore < 0) return s_InvalidBetParameter;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var existingBet = await s_GetScoreBetByGameAndUser.Invoke(m_DbContext, game, m_User);
        if (existingBet is not null) return s_AlreadyExistsException;
        if (game.GameState != EGameState.Upcoming) return s_InvalidDateException;


        var bet = new ScoreBet(HomeScore, AwayScore)
        {
            UserId = m_User.Id,
            GroupId = group.Id,
            GameId = game.Id,
        };
        await m_DbContext.FootballScoreBets.AddAsync(bet);
        await m_DbContext.SaveChangesAsync();

        return bet;
    }
    public async Task<MpmResult<List<Bet>>> GetAllCompletedBets()
    {
        if (m_User is null) return s_NoUserException;
        
        List<Bet> _bets = [];
        
        await foreach(var bet in s_GetAllCompletedBets.Invoke(m_DbContext, m_User))
            _bets.Add(bet);

        return _bets;
    }
}