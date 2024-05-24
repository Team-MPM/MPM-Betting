using System.ComponentModel;
using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Pipes;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.Rewarding;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

//TODO: Add cancelation tokens

public partial class UserDomain(MpmDbContext dbContext)
{
    public class InvalidBetParameter : Exception;
    private static readonly InvalidBetParameter s_InvalidBetParameter = new();
    public class NoUserException : Exception;
    private static readonly NoUserException s_NoUserException = new();

    public class GroupNotFoundException : Exception;
    private static readonly GroupNotFoundException s_GroupNotFoundException = new();
    
    public class SeasonNotFoundException : Exception;
    private static readonly SeasonNotFoundException s_SeasonNotFoundException = new();

    public class AccessDeniedException : Exception;
    private static readonly AccessDeniedException s_AccessDeniedException = new();
    
    public class BadWordException : Exception;
    private static readonly BadWordException s_BadWordException = new();
    
    public class InvalidDateException : Exception;
    private static readonly InvalidDateException s_InvalidDateException = new();
    
    public class AlreadyExistsException : Exception;
    private static readonly AlreadyExistsException s_AlreadyExistsException = new();
    
    private static readonly Func<MpmDbContext, MpmGroup, MpmUser, Task<UserGroupEntry?>> s_GetUserGroupEntryQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup Group, MpmUser user) =>
            dbContext.UserGroupEntries
                .FirstOrDefault(uge => uge.Group == Group && uge.MpmUser == user));
    
    private static readonly Func<MpmDbContext, MpmUser, IAsyncEnumerable<MpmGroup>> s_GetUserGroupsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.MpmUser == user)
                .Select(uge => uge.Group));
    
    private static readonly Func<MpmDbContext, MpmUser, string, Task<MpmGroup?>> s_GetGroupByNameQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user, string name) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.MpmUser == user && uge.Group.Name == name)
                .Select(uge => uge.Group)
                .FirstOrDefault());
    
    private static readonly Func<MpmDbContext, int ,IAsyncEnumerable<MpmGroup>> s_GetGroupsBySeasonChosen =
        EF.CompileAsyncQuery((MpmDbContext dbContext,int id) =>
            dbContext.SeasonEntries
                .Where(se => se.Id == id)
                .Select(se => se.Group));

    private static readonly Func<MpmDbContext, string, Task<MpmGroup>> s_GetGroupByName =
        EF.CompileAsyncQuery((MpmDbContext dbContext, string name) => dbContext.Groups
            .Where(g => g.Name == name)
            .FirstOrDefault());
    
    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<UserGroupEntry?>> s_GetUserGroupEntriesByGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup Group) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.Group == Group)
                .Include(uge => uge.MpmUser ));
  
    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<UserGroupEntry?>> s_GetUserGroupEntryByGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup Group) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.Group == Group)
                .Include(uge =>uge.MpmUser ));

    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<SeasonEntry?>> s_GetSeasonEntriesByGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
            dbContext.SeasonEntries
                .Where(se => se.Group == group));
    
    private static readonly Func<MpmDbContext, MpmUser, IAsyncEnumerable<FavoriteSeasons>> s_GetFavoriteSeasonsByUser =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.FavoriteSeasons
                .Where(fs => fs.User == user));
    
    [GeneratedRegex("^[a@][s\\$][s\\$]$\n[a@][s\\$][s\\$]h[o0][l1][e3][s\\$]?\nb[a@][s\\$][t\\+][a@]rd \nb[e3][a@][s\\$][t\\+][i1][a@]?[l1]([i1][t\\+]y)?\nb[e3][a@][s\\$][t\\+][i1][l1][i1][t\\+]y\nb[e3][s\\$][t\\+][i1][a@][l1]([i1][t\\+]y)?\nb[i1][t\\+]ch[s\\$]?\nb[i1][t\\+]ch[e3]r[s\\$]?\nb[i1][t\\+]ch[e3][s\\$]\nb[i1][t\\+]ch[i1]ng?\nb[l1][o0]wj[o0]b[s\\$]?\nc[l1][i1][t\\+]\n^(c|k|ck|q)[o0](c|k|ck|q)[s\\$]?$\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]d \n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]r\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[i1]ng\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[s\\$]\n^cum[s\\$]?$\ncumm??[e3]r\ncumm?[i1]ngcock\n(c|k|ck|q)um[s\\$]h[o0][t\\+]\n(c|k|ck|q)un[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[i1][l1][l1][i1]ngu[s\\$]\n(c|k|ck|q)unn[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[t\\+][s\\$]?\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[e3]r\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[i1]ng\ncyb[e3]r(ph|f)u(c|k|ck|q)\nd[a@]mn\nd[i1]ck\nd[i1][l1]d[o0]\nd[i1][l1]d[o0][s\\$]\nd[i1]n(c|k|ck|q)\nd[i1]n(c|k|ck|q)[s\\$]\n[e3]j[a@]cu[l1]\n(ph|f)[a@]g[s\\$]?\n(ph|f)[a@]gg[i1]ng\n(ph|f)[a@]gg?[o0][t\\+][s\\$]?\n(ph|f)[a@]gg[s\\$]\n(ph|f)[e3][l1][l1]?[a@][t\\+][i1][o0]\n(ph|f)u(c|k|ck|q)\n(ph|f)u(c|k|ck|q)[s\\$]?\ng[a@]ngb[a@]ng[s\\$]?\ng[a@]ngb[a@]ng[e3]d\ng[a@]y\nh[o0]m?m[o0]\nh[o0]rny\nj[a@](c|k|ck|q)\\-?[o0](ph|f)(ph|f)?\nj[e3]rk\\-?[o0](ph|f)(ph|f)?\nj[i1][s\\$z][s\\$z]?m?\n[ck][o0]ndum[s\\$]?\nmast(e|ur)b(8|ait|ate)\nn+[i1]+[gq]+[e3]*r+[s\\$]*\n[o0]rg[a@][s\\$][i1]m[s\\$]?\n[o0]rg[a@][s\\$]m[s\\$]?\np[e3]nn?[i1][s\\$]\np[i1][s\\$][s\\$]\np[i1][s\\$][s\\$][o0](ph|f)(ph|f) \np[o0]rn\np[o0]rn[o0][s\\$]?\np[o0]rn[o0]gr[a@]phy\npr[i1]ck[s\\$]?\npu[s\\$][s\\$][i1][e3][s\\$]\npu[s\\$][s\\$]y[s\\$]?\n[s\\$][e3]x\n[s\\$]h[i1][t\\+][s\\$]?\n[s\\$][l1]u[t\\+][s\\$]?\n[s\\$]mu[t\\+][s\\$]?\n[s\\$]punk[s\\$]?\n[t\\+]w[a@][t\\+][s\\$]?", RegexOptions.IgnoreCase)]
    public static partial Regex BadWordRegex();
    
    private MpmUser? m_User;
    
    public void SetUser(MpmUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        m_User = user;
    }
    
    public async Task<MpmResult<List<MpmGroup>>> GetGroups()
    {
        if (m_User is null) return s_NoUserException;

        List<MpmGroup> groups = [];
        
        await foreach (var group in s_GetUserGroupsQuery.Invoke(dbContext, m_User))
        {
            groups.Add(group);
        }

        return groups;
    }
    
    public async Task<MpmResult<MpmGroup>> GetGroupByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (m_User is null) return s_NoUserException;
        
        var result = await s_GetGroupByNameQuery.Invoke(dbContext, m_User, name);
        
        if (result is null) return s_GroupNotFoundException;
        
        return result;
    }
    
    public async Task<MpmResult<List<MpmGroup>>> GetGroupsBySeasonChosen(int id)
    {
        if (m_User is null) return s_NoUserException;

        List<MpmGroup> _group = [];

        await foreach(var Group in s_GetGroupsBySeasonChosen.Invoke(dbContext,id))
            _group.Add(Group);
        
        return _group;
    }
    
    public async Task<MpmResult<MpmGroup>> CreateGroup(string name, string description)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        if (m_User is null) return s_NoUserException;

        if (BadWordRegex().IsMatch(name)) return s_BadWordException;
        if (BadWordRegex().IsMatch(description)) return s_BadWordException;
        
        if (name.Length > 30 || description.Length > 1024)
            return s_BadWordException;
        
        var existingGroup = await s_GetGroupByName.Invoke(dbContext, name);
        if (existingGroup is not null)
            return s_AlreadyExistsException;
        
        var group = new MpmGroup(m_User, name, description, []);
        
        dbContext.Groups.Add(group);
        dbContext.UserGroupEntries.Add(new UserGroupEntry(m_User, group) { Role = EGroupRole.Owner });
        
        await dbContext.SaveChangesAsync();
        
        return group;
    }
    
    public async Task<MpmResult<bool>> DeleteGroup(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        if (group.Creator != m_User) return s_AccessDeniedException;
        
        dbContext.Groups.Remove(group);
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> UpdateGroupName(MpmGroup group, string name)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(name);
        if (m_User is null) return s_NoUserException;
        
        if (BadWordRegex().IsMatch(name)) return s_BadWordException;
        
        if (name.Length > 30)
            return s_BadWordException;

        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        group.Name = name;
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> UpdateGroupDescription(MpmGroup group, string description)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(description);
        if (m_User is null) return s_NoUserException;
        
        if (BadWordRegex().IsMatch(description)) return s_BadWordException;
        
        if (description.Length > 1024)
            return s_BadWordException;

        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);
        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        group.Description = description;
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> AddUserToGroup(MpmGroup group, MpmUser target, EGroupRole role)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;
        
        if (role is EGroupRole.Owner) return s_AccessDeniedException;
        
        var existingUge = await s_GetUserGroupEntryQuery.Invoke(dbContext,group,target);
        if (existingUge is not null)
            return s_AlreadyExistsException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext,group,m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        dbContext.UserGroupEntries.Add(new UserGroupEntry(target, group) { Role = role });

        dbContext.Notifications.Add(new Notification(target, $"You have been added to Group {group.Name} by {m_User.UserName}"));
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> UpdateGroupRole(MpmGroup group, MpmUser target, EGroupRole role)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;

        if (role is EGroupRole.Owner) return s_AccessDeniedException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext,group,m_User);

        if (uge?.Role is not EGroupRole.Owner)
            return s_AccessDeniedException;
        
        var targetUge = await s_GetUserGroupEntryQuery.Invoke(dbContext,group,target);
        if (targetUge is null)
            return s_GroupNotFoundException;
        
        targetUge.Role = role;
        await dbContext.SaveChangesAsync();
        
        dbContext.Notifications.Add(new Notification(target, $"Your role in Group {group.Name} has been changed to {role} by {m_User.UserName}"));
        
        return true;
    }
    
    public async Task<MpmResult<bool>> RemoveUserFromGroup(MpmGroup group, MpmUser target)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext,group,m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var targetUge = await s_GetUserGroupEntryQuery.Invoke(dbContext,group,target);
        if (targetUge is null)
            return s_GroupNotFoundException;
        
        if (targetUge.Role is EGroupRole.Owner)
            return s_AccessDeniedException;
        
        dbContext.UserGroupEntries.Remove(targetUge);
        await dbContext.SaveChangesAsync();
        
        dbContext.Notifications.Add(new Notification(target, $"You have been removed from Group {group.Name} by {m_User.UserName}"));
        
        return true;
    }
    public async Task<MpmResult<List<UserGroupEntry>>> GetUsersByGroup(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var query = await s_GetUserGroupEntryQuery.Invoke(dbContext,group,m_User);
        if (query is null)
            return s_AccessDeniedException;
        List<UserGroupEntry> _uges = [];
        
        await foreach(var uge in s_GetUserGroupEntryByGroup.Invoke(dbContext,group))
            _uges.Add(uge); 
        
        return _uges;
    }
    
    public async Task<MpmResult<List<SeasonEntry>>> GetGroupSeasons(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;

        List<SeasonEntry> _se = [];
        
        await foreach(var se in s_GetSeasonEntriesByGroup.Invoke(dbContext, group))
            _se.Add(se);
        
        return _se;
    }
    private static readonly Func<MpmDbContext, MpmGroup, Season, Task<SeasonEntry>> s_GetSeasonEntriesByGroupAndSeason =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group, Season season) =>
            dbContext.SeasonEntries.FirstOrDefault(se => se.Season == season && se.Group == group));
    
    public async Task<MpmResult<bool>> AddSeasonToGroup(MpmGroup group, Season season)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        if (m_User is null) return s_NoUserException;

        var existingSeasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(dbContext, group, season);
        if (existingSeasonEntry is not null)
            return s_AlreadyExistsException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        dbContext.SeasonEntries.Add(new SeasonEntry(season.Name, group, season));
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> RemoveSeasonFromGroup(MpmGroup group, Season season)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);
        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var seasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(dbContext, group, season);
        if (seasonEntry is null)
            return s_GroupNotFoundException;
        
        dbContext.SeasonEntries.Remove(seasonEntry);
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<CustomSeason>> CreateCustomSeason(MpmGroup group, string name, string description, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        if (m_User is null) return s_NoUserException;
        
        if (BadWordRegex().IsMatch(name)) return s_BadWordException;
        if (BadWordRegex().IsMatch(description)) return s_BadWordException;
        
        if (name.Length > 50 || description.Length > 2000)
            return s_BadWordException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);

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
        dbContext.CustomSeasons.Add(customSeason);
        
        var seasonEntry = new SeasonEntry(name, group, customSeason);
        dbContext.SeasonEntries.Add(seasonEntry);
        
        await dbContext.SaveChangesAsync();
        
        return customSeason;
    }
    
    private static readonly Func<MpmDbContext,Season,Game,Task<CustomSeasonEntry>> s_GetCustomSeasonEntryBySeasonAndGame =
        EF.CompileAsyncQuery((MpmDbContext dbContext, Season season, Game game) =>
            dbContext.CustomSeasonEntries
                .FirstOrDefault(cse => cse.Season == season && cse.Game == game));
    
    public async Task<MpmResult<bool>> AddCustomSeasonEntry(MpmGroup group, CustomSeason season, Game game)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var seasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(dbContext, group, season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        var existingEntry = await s_GetCustomSeasonEntryBySeasonAndGame.Invoke(dbContext, season, game);
        if (existingEntry is not null)
            return s_AlreadyExistsException;
        
        dbContext.CustomSeasonEntries.Add(new CustomSeasonEntry(season, game));
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> RemoveCustomSeasonEntry(MpmGroup group, CustomSeason season, Game game)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        var seasonEntry = await s_GetSeasonEntriesByGroupAndSeason.Invoke(dbContext, group, season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        var existingEntry = await s_GetCustomSeasonEntryBySeasonAndGame.Invoke(dbContext, season, game);
        if (existingEntry is null)
            return s_GroupNotFoundException;
        
        dbContext.CustomSeasonEntries.Remove(existingEntry);
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    private static readonly Func<MpmDbContext, MpmGroup, CustomSeason, IAsyncEnumerable<Game>> s_GetSeasonEntriesByGameAndSeason =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group, CustomSeason season) =>
            dbContext.CustomSeasonEntries
                .Where(cse => cse.Season == season)
                .Select(cse => cse.Game));
    public async Task<MpmResult<List<Game>>> GetSeasonEntries(MpmGroup group, CustomSeason season)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var seasonEntry = s_GetSeasonEntriesByGroupAndSeason.Invoke(dbContext, group, season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        List<Game> _games = [];

        await foreach (var game in s_GetSeasonEntriesByGameAndSeason.Invoke(dbContext, group, season))
            _games.Add(game);
            
        return _games;
    }
    
    private static readonly Func<MpmDbContext, int, Task<BuiltinSeason?>> s_GetCurrentBuiltInSeasonById =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int id) =>
            dbContext.BuiltinSeasons
                .Where(bis => bis.Id == id)
                .OrderBy(bis => bis.Start)
                .FirstOrDefault());
    
    public async Task<MpmResult<BuiltinSeason>> GetCurrentBuiltInSeasonById(int id)
    {
        //Returns most current, doesnt check if season is active

        var query = await s_GetCurrentBuiltInSeasonById.Invoke(dbContext, id);
        
        
        if (query is null)
        {
            return s_SeasonNotFoundException;
        }
        
        return query;
    }
    
    public async Task<MpmResult<List<Notification>>> GetAllNotificationOfUser()
    {
        if (m_User is null) return s_NoUserException;
        
        var query = dbContext.Notifications
            .Where(n => n.Target == m_User);
        
        return await query.ToListAsync();
    }
    
    public async Task<MpmResult<List<Notification>>> GetAllNewNotificationOfUser()
    {
        if (m_User is null) return s_NoUserException;

        var query = dbContext.Notifications
            .Where(n => n.Target == m_User && !n.IsRead);
        
        return await query.ToListAsync();
    }
    private static readonly Func<MpmDbContext,MpmUser,IAsyncEnumerable<Notification>> GetUnreadNotifications =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.Notifications
                .Where(n => n.Target == user && !n.IsRead));
    public async Task<MpmResult<bool>> MarkAllNewNotificationAsRead()
    {
        if (m_User is null) return s_NoUserException;
        
        await foreach(var notification in GetUnreadNotifications.Invoke(dbContext,m_User))
        {
            notification.IsRead = true;
        }
        
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    private static readonly Func<MpmDbContext,MpmGroup,IAsyncEnumerable<Message>> s_GetAllMessagesOfGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
            dbContext.Messages
                .Where(m => m.RecipientGroup == group));
    
    public async Task<MpmResult<List<Message>>> GetAllMessagesOfgroup(MpmGroup group)
    {
        if (m_User is null) return s_NoUserException;
        
        var uge = s_GetUserGroupEntryByGroup.Invoke(dbContext, group);
        if (uge is null)
            return s_AccessDeniedException;
        
        List<Message> _messages = [];
        
        await foreach(var messages in s_GetAllMessagesOfGroup.Invoke(dbContext, group))
            _messages.Add(messages);

        return _messages;
    }

    public async Task<MpmResult<Message>> SendMessage(MpmGroup group, string text)
    {       
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        if(BadWordRegex().IsMatch(text)) return s_BadWordException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var message = new Message(m_User, group, text);
        
        return message;
    }
    private static readonly Func<MpmDbContext, Game, MpmUser, Task<ResultBet?>> s_GetResultBetByGameAndUser =
        EF.CompileAsyncQuery((MpmDbContext dbContext, Game game, MpmUser user) =>
            dbContext.FootballResultBets
                .FirstOrDefault(b => b.Game == game && b.User == user));
    
    public async Task<MpmResult<Bet>> CreateFootballResultBet(MpmGroup group, Game game, EResult result)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        if(result is EResult.None) return s_InvalidBetParameter;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var existingBet = await s_GetResultBetByGameAndUser.Invoke(dbContext, game, m_User);
        if (existingBet is not null) return s_AlreadyExistsException;
        if(game.GameState != EGameState.Upcoming) return s_InvalidDateException;
        
        
        var bet = new ResultBet(m_User, group, game, result);
        await dbContext.FootballResultBets.AddAsync(bet);
        await dbContext.SaveChangesAsync();

        return bet;
    }
    private static readonly Func<MpmDbContext, Game, MpmUser, Task<ScoreBet?>> s_GetScoreBetByGameAndUser =
        EF.CompileAsyncQuery((MpmDbContext dbContext, Game game, MpmUser user) =>
            dbContext.FootballScoreBets
                .FirstOrDefault(b => b.Game == game && b.User == user));
    public async Task<MpmResult<Bet>> CreateFootballScoreBet(MpmGroup group, Game game, int HomeScore, int AwayScore)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        if(HomeScore < 0 || AwayScore < 0) return s_InvalidBetParameter;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(dbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var existingBet = await s_GetScoreBetByGameAndUser.Invoke(dbContext, game, m_User);
        if (existingBet is not null) return s_AlreadyExistsException;
        if(game.GameState != EGameState.Upcoming) return s_InvalidDateException;
        
        
        var bet = new ScoreBet(m_User, group, game, HomeScore, AwayScore);
        await dbContext.FootballScoreBets.AddAsync(bet);
        await dbContext.SaveChangesAsync();

        return bet;
    }

    public async Task<MpmResult<Achievement>> CreateAchievement(string title, string description)
    {
        Achievement achievement = new Achievement(title, description);
        return achievement;
    }
    
    public async Task<MpmResult<BuiltinSeason>> AddSeasonToFavorites(BuiltinSeason s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (m_User == null) return s_NoUserException;
        
       await foreach (var fs in s_GetFavoriteSeasonsByUser.Invoke(dbContext, m_User))
           if (fs.Season == s)
               return s_AlreadyExistsException;

       dbContext.FavoriteSeasons.Add(new FavoriteSeasons(s, m_User));
       dbContext.SaveChangesAsync();

       return s;
    }
    private static readonly Func<MpmDbContext, Season, MpmUser, Task<FavoriteSeasons>> s_GetFavoriteSeasonByUserAndSeason =
        EF.CompileAsyncQuery((MpmDbContext dbContext,Season s, MpmUser user) =>
            dbContext.FavoriteSeasons.FirstOrDefault(fs => fs.Season == s && fs.User == user));
    public async Task<MpmResult<bool>> RemoveSeasonFromFavorites(BuiltinSeason s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (m_User == null) return s_NoUserException;
        
        var fs = await s_GetFavoriteSeasonByUserAndSeason.Invoke(dbContext, s, m_User);
        if (fs == null) return s_SeasonNotFoundException;

        dbContext.FavoriteSeasons.Remove(fs);
        dbContext.SaveChangesAsync();

        return true;
    }
    
    /// <returns>
    /// Position of the user in the group (not index)
    /// </returns>
    public async Task<MpmResult<int>> GetUserPosition(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if(m_User == null) return s_NoUserException;
        
        List<UserGroupEntry> _uges = [];
        await foreach(var uge in s_GetUserGroupEntriesByGroup.Invoke(dbContext, group))
           _uges.Add(uge);
        
        _uges.OrderBy(uge => uge.Score);
        
        return _uges.FindIndex(uge => uge.MpmUser == m_User)+1;
    }
}