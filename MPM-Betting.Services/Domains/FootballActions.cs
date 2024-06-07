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
}