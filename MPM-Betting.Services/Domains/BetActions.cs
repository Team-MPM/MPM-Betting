using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
    public async Task<MpmResult<List<Bet>>> GetAllBets()
    {
        var allBets = new List<Bet>();
        await foreach (var bet in s_GetAllBetsQuery(m_DbContext))
            allBets.Add(bet);
        
        return allBets;
    }

    public async Task<MpmResult<List<Bet>>> GetAllBetsForUser()
    {
        if (m_User is null)
            throw s_NoUserException;
        
        var bets = new List<Bet>();
        await foreach (var bet in s_GetAllBetsForUserQuery(m_DbContext, m_User))
            bets.Add(bet);
        
        return bets;
    }
    
    public async Task<MpmResult<List<Bet>>> GetAllBetsForGroup(int groupId)
    {
        var bets = new List<Bet>();
        await foreach (var bet in s_GetAllBetsForGroupQuery(m_DbContext, groupId))
            bets.Add(bet);
        
        return bets;
    }
    
    public async Task<MpmResult<List<Bet>>> GetAllBetsForGame(int gameId)
    {
        var bets = new List<Bet>();
        await foreach (var bet in s_GetAllBetsForGameQuery(m_DbContext, gameId))
            bets.Add(bet);
        
        return bets;
    }
    
    public async Task<MpmResult<List<GameBet>>> GetAllFootballGameBets()
    {
        var allBets = new List<GameBet>();
        await foreach (var bet in s_GetAllFootballGameBetsQuery(m_DbContext))
            allBets.Add(bet);
        
        return allBets;
    }

    public async Task<MpmResult<List<GameBet>>> GetAllBetsFootballGameForUser()
    {
        if (m_User is null)
            throw s_NoUserException;
        
        var bets = new List<GameBet>();
        await foreach (var bet in s_GetAllFootballGameBetsForUserQuery(m_DbContext, m_User))
            bets.Add(bet);
        
        return bets;
    }
    
    public async Task<MpmResult<List<GameBet>>> GetAllFootballGameBetsForGroup(int groupId)
    {
        var bets = new List<GameBet>();
        await foreach (var bet in s_GetAllFootballGameBetsForGroupQuery(m_DbContext, groupId))
            bets.Add(bet);
        
        return bets;
    }
    
    public async Task<MpmResult<List<GameBet>>> GetAllFootballGameBetsForGame(int gameId)
    {
        var bets = new List<GameBet>();
        await foreach (var bet in s_GetAllFootballGameBetsForGameQuery(m_DbContext, gameId))
            bets.Add(bet);
        
        return bets;
    }
    // public async Task<MpmResult<List<Bet>>> GetAllCompletedBets()
    // {
    //     if (m_User is null) return s_NoUserException;
    //     
    //     List<Bet> _bets = [];
    //     
    //     await foreach(var bet in s_GetAllCompletedBets.Invoke(m_DbContext, m_User))
    //         _bets.Add(bet);
    //
    //     return _bets;
    // }
}