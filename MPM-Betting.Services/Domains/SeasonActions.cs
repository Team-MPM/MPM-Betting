using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.User;
using OneOf;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
    public async Task<List<BuiltinSeason>> GetAllBuiltinSeasons()
    {
        List<BuiltinSeason> seasons = [];

        await foreach (var season in s_GetAllBuiltinSeasonsQuery.Invoke(m_DbContext))
        {
            seasons.Add(season);
        }

        return seasons;
    }
    
     public async Task<MpmResult<List<SeasonEntry>>> GetGroupSeasons(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);
        if (uge is null)
            return s_AccessDeniedException;

        List<SeasonEntry> ses = [];
        
        await foreach(var se in s_GetSeasonEntriesByGroup.Invoke(m_DbContext, group))
            ses.Add(se!);
        
        return ses;
    }
    
    
    public async Task<MpmResult<bool>> AddSeasonToGroup(MpmGroup group, int seasonId)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var season = await s_GetSeasonById.Invoke(m_DbContext, seasonId);
        if(season is null)
            return s_SeasonNotFoundException;

        var existingSeasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(m_DbContext, group, seasonId);
        if (existingSeasonEntry is not null)
            return s_AlreadyExistsException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        await m_DbContext.SeasonEntries.AddAsync(new SeasonEntry()
        {
            GroupId = group.Id,
            SeasonId = seasonId
        });
        await m_DbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> RemoveSeasonFromGroup(MpmGroup group, int seasonId)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);
        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var seasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(m_DbContext, group, seasonId);
        if (seasonEntry is null)
            return s_GroupNotFoundException;
        
        m_DbContext.SeasonEntries.Remove(seasonEntry);
        await m_DbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<CustomSeason>> CreateCustomSeason(MpmGroup group, string name, string description, DateTime startDate, DateTime endDate, ESportType sportType)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        if (m_User is null) return s_NoUserException;
        
        if (BadWordRegex().IsMatch(name)) return s_BadWordException;
        if (BadWordRegex().IsMatch(description)) return s_BadWordException;
        
        if (name.Length > 50 || description.Length > 2000)
            return s_BadWordException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        if (startDate < DateTime.Now)
            return s_InvalidDateException;
        
        if (endDate < startDate)
            return s_InvalidDateException;
        
        var customSeason = new CustomSeason(name, description)
        {
            Start = startDate,
            End = endDate
        };
        m_DbContext.CustomSeasons.Add(customSeason);
        
        var seasonEntry = new SeasonEntry()
        {
            Group = group,
            Season = customSeason
        };
        m_DbContext.SeasonEntries.Add(seasonEntry);
        
        await m_DbContext.SaveChangesAsync();
        
        return customSeason;
    }
    
    
    
    public async Task<MpmResult<bool>> AddCustomSeasonEntry(MpmGroup group, CustomSeason season, Game game)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var seasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(m_DbContext, group, season.Id);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        var existingEntry = await s_GetCustomSeasonEntryBySeasonAndGame.Invoke(m_DbContext, season, game);
        if (existingEntry is not null)
            return s_AlreadyExistsException;
        
        m_DbContext.CustomSeasonEntries.Add(new CustomSeasonEntry(season, game));
        await m_DbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> RemoveCustomSeasonEntry(MpmGroup group, CustomSeason season, Game game)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        var seasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(m_DbContext, group, season.Id);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        var existingEntry = await s_GetCustomSeasonEntryBySeasonAndGame.Invoke(m_DbContext, season, game);
        if (existingEntry is null)
            return s_GroupNotFoundException;
        
        m_DbContext.CustomSeasonEntries.Remove(existingEntry);
        await m_DbContext.SaveChangesAsync();
        
        return true;
    }
    
     public async Task<MpmResult<List<Game>>> GetSeasonEntries(MpmGroup group, CustomSeason season)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);
        if (uge is null)
            return s_AccessDeniedException;
        
        var seasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(m_DbContext, group, season.Id);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        List<Game> games = [];

        await foreach (var game in s_GetSeasonEntriesByGameAndSeason.Invoke(m_DbContext, group, season))
            games.Add(game);
            
        return games;
    }
     
    public async Task<MpmResult<BuiltinSeason>> GetCurrentBuiltInSeasonById(int id)
    {
        //Returns most current, doesn't check if the season is active

        var season = await s_GetCurrentBuiltInSeasonById.Invoke(m_DbContext, id);
        
        
        if (season is null)
        {
            return s_SeasonNotFoundException;
        }
        
        return season;
    }
    
    public async Task<MpmResult<OneOf<BuiltinSeason, CustomSeason>>> GetConcreteSeasonById(int id)
    {
        var customSeason = await m_DbContext.CustomSeasons.FindAsync(id);
        if (customSeason is not null)
        {
            return new MpmResult<OneOf<BuiltinSeason, CustomSeason>>(customSeason);
        }
        
        var builtinSeason = await m_DbContext.BuiltinSeasons.FindAsync(id);
        if (builtinSeason is not null)
        {
            return new MpmResult<OneOf<BuiltinSeason, CustomSeason>>(builtinSeason);
        }
        
        return s_SeasonNotFoundException;
    }
}