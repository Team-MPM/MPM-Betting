using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.Rewarding;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

//TODO: Add cancellation tokens

public partial class UserDomain(IDbContextFactory<MpmDbContext> dbContextFactory)
{
    private readonly MpmDbContext m_DbContext = dbContextFactory.CreateDbContext();

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

    [GeneratedRegex(
        "^[a@][s\\$][s\\$]$\n[a@][s\\$][s\\$]h[o0][l1][e3][s\\$]?\nb[a@][s\\$][t\\+][a@]rd \nb[e3][a@][s\\$][t\\+][i1][a@]?[l1]([i1][t\\+]y)?\nb[e3][a@][s\\$][t\\+][i1][l1][i1][t\\+]y\nb[e3][s\\$][t\\+][i1][a@][l1]([i1][t\\+]y)?\nb[i1][t\\+]ch[s\\$]?\nb[i1][t\\+]ch[e3]r[s\\$]?\nb[i1][t\\+]ch[e3][s\\$]\nb[i1][t\\+]ch[i1]ng?\nb[l1][o0]wj[o0]b[s\\$]?\nc[l1][i1][t\\+]\n^(c|k|ck|q)[o0](c|k|ck|q)[s\\$]?$\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]d \n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]r\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[i1]ng\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[s\\$]\n^cum[s\\$]?$\ncumm??[e3]r\ncumm?[i1]ngcock\n(c|k|ck|q)um[s\\$]h[o0][t\\+]\n(c|k|ck|q)un[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[i1][l1][l1][i1]ngu[s\\$]\n(c|k|ck|q)unn[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[t\\+][s\\$]?\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[e3]r\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[i1]ng\ncyb[e3]r(ph|f)u(c|k|ck|q)\nd[a@]mn\nd[i1]ck\nd[i1][l1]d[o0]\nd[i1][l1]d[o0][s\\$]\nd[i1]n(c|k|ck|q)\nd[i1]n(c|k|ck|q)[s\\$]\n[e3]j[a@]cu[l1]\n(ph|f)[a@]g[s\\$]?\n(ph|f)[a@]gg[i1]ng\n(ph|f)[a@]gg?[o0][t\\+][s\\$]?\n(ph|f)[a@]gg[s\\$]\n(ph|f)[e3][l1][l1]?[a@][t\\+][i1][o0]\n(ph|f)u(c|k|ck|q)\n(ph|f)u(c|k|ck|q)[s\\$]?\ng[a@]ngb[a@]ng[s\\$]?\ng[a@]ngb[a@]ng[e3]d\ng[a@]y\nh[o0]m?m[o0]\nh[o0]rny\nj[a@](c|k|ck|q)\\-?[o0](ph|f)(ph|f)?\nj[e3]rk\\-?[o0](ph|f)(ph|f)?\nj[i1][s\\$z][s\\$z]?m?\n[ck][o0]ndum[s\\$]?\nmast(e|ur)b(8|ait|ate)\nn+[i1]+[gq]+[e3]*r+[s\\$]*\n[o0]rg[a@][s\\$][i1]m[s\\$]?\n[o0]rg[a@][s\\$]m[s\\$]?\np[e3]nn?[i1][s\\$]\np[i1][s\\$][s\\$]\np[i1][s\\$][s\\$][o0](ph|f)(ph|f) \np[o0]rn\np[o0]rn[o0][s\\$]?\np[o0]rn[o0]gr[a@]phy\npr[i1]ck[s\\$]?\npu[s\\$][s\\$][i1][e3][s\\$]\npu[s\\$][s\\$]y[s\\$]?\n[s\\$][e3]x\n[s\\$]h[i1][t\\+][s\\$]?\n[s\\$][l1]u[t\\+][s\\$]?\n[s\\$]mu[t\\+][s\\$]?\n[s\\$]punk[s\\$]?\n[t\\+]w[a@][t\\+][s\\$]?",
        RegexOptions.IgnoreCase)]
    public static partial Regex BadWordRegex();

    private MpmUser? m_User;

    public void SetUser(MpmUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        m_User = user;
    }

    private static readonly Func<MpmDbContext, string, IAsyncEnumerable<MpmGroup>> s_GetUserGroupsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, string userId) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.MpmUser.Id == userId)
                .Select(uge => uge.Group));

    public async Task<MpmResult<List<MpmGroup>>> GetGroups()
    {
        if (m_User is null) return s_NoUserException;

        List<MpmGroup> groups = [];

        await foreach (var group in s_GetUserGroupsQuery.Invoke(m_DbContext, m_User.Id))
        {
            groups.Add(group);
        }

        return groups;
    }

    private static readonly Func<MpmDbContext, MpmUser, string, Task<MpmGroup?>> s_GetGroupByNameQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user, string name) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.MpmUser == user && uge.Group.Name == name)
                .Select(uge => uge.Group)
                .FirstOrDefault());

    private static readonly Func<MpmDbContext, MpmUser, int, Task<MpmGroup?>> s_GetGroupByIdQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user, int id) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.MpmUser == user && uge.Group.Id == id)
                .Select(uge => uge.Group)
                .FirstOrDefault());

    private static readonly Func<MpmDbContext, MpmUser, int, Task<UserGroupEntry?>>
        s_GetGroupByIdWithEntryQuery =
            EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user, int id) =>
                dbContext.UserGroupEntries
                    .Where(uge => uge.MpmUser == user && uge.Group.Id == id)
                    .Include(uge => uge.Group)
                    .FirstOrDefault());

    public async Task<MpmResult<(MpmGroup group, UserGroupEntry entry)>> GetGroupByIdWithAccess(int id)
    {
        if (m_User is null) return s_NoUserException;

        var result = await s_GetGroupByIdWithEntryQuery.Invoke(m_DbContext, m_User, id);

        if (result is null) return s_GroupNotFoundException;

        return (result.Group, result);
    }

    public async Task<MpmResult<MpmGroup>> GetGroupById(int id)
    {
        if (m_User is null) return s_NoUserException;

        var result = await s_GetGroupByIdQuery.Invoke(m_DbContext, m_User, id);

        if (result is null) return s_GroupNotFoundException;

        return result;
    }

    public async Task<MpmResult<MpmGroup>> GetGroupByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (m_User is null) return s_NoUserException;

        var result = await s_GetGroupByNameQuery.Invoke(m_DbContext, m_User, name);

        if (result is null) return s_GroupNotFoundException;

        return result;
    }

    public async Task<MpmResult<List<MpmGroup>>> GetGroupsBySeasonChosen(int id)
    {
        if (m_User is null) return s_NoUserException;

        var query = m_DbContext.SeasonEntries
            .Where(se => se.Id == id)
            .Select(se => se.Group);

        return await query.ToListAsync();
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

        var existingGroup = m_DbContext.Groups.FirstOrDefault(g => g.Name == name);
        if (existingGroup is not null)
            return s_AlreadyExistsException;

        var group = new MpmGroup(null!, name, description, [])
        {
            CreatorId = m_User.Id
        };

        m_DbContext.Groups.Add(group);
        m_DbContext.UserGroupEntries.Add(new UserGroupEntry(m_User.Id, group) { Role = EGroupRole.Owner });

        try
        {
            await m_DbContext.SaveChangesAsync();
        }
        catch
        {
            return s_AlreadyExistsException;
        }

        return group;
    }

    public async Task<MpmResult<bool>> DeleteGroup(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;

        if (group.Creator != m_User) return s_AccessDeniedException;

        m_DbContext.Groups.Remove(group);
        await m_DbContext.SaveChangesAsync();

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

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        var groupEntry = m_DbContext.Groups.FirstOrDefault(g => g.Id == group.Id);

        if (groupEntry is null)
            return s_GroupNotFoundException;
        
        groupEntry.Name = name; 
        await m_DbContext.SaveChangesAsync();

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

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var groupEntry = m_DbContext.Groups.FirstOrDefault(g => g.Id == group.Id);

        if (groupEntry is null)
            return s_GroupNotFoundException;
        
        groupEntry.Description = description;
        await m_DbContext.SaveChangesAsync();

        return true;
    }

    public async Task<MpmResult<bool>> AddUserToGroup(MpmGroup group, MpmUser target, EGroupRole role)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;

        if (role is EGroupRole.Owner) return s_AccessDeniedException;

        var existingUge =
            m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == target);
        if (existingUge is not null)
            return s_AlreadyExistsException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        m_DbContext.UserGroupEntries.Add(new UserGroupEntry(target.Id, group) { Role = role });

        m_DbContext.Notifications.Add(new Notification(target,
            $"You have been added to Group {group.Name} by {m_User.UserName}"));
        await m_DbContext.SaveChangesAsync();

        return true;
    }

    public async Task<MpmResult<bool>> UpdateGroupRole(MpmGroup group, MpmUser target, EGroupRole role)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;

        if (role is EGroupRole.Owner) return s_AccessDeniedException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not EGroupRole.Owner)
            return s_AccessDeniedException;

        var targetUge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == target);
        if (targetUge is null)
            return s_GroupNotFoundException;

        targetUge.Role = role;
        await m_DbContext.SaveChangesAsync();

        m_DbContext.Notifications.Add(new Notification(target,
            $"Your role in Group {group.Name} has been changed to {role} by {m_User.UserName}"));

        return true;
    }

    public async Task<MpmResult<bool>> RemoveUserFromGroup(MpmGroup group, MpmUser target)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        var targetUge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == target);
        if (targetUge is null)
            return s_GroupNotFoundException;

        if (targetUge.Role is EGroupRole.Owner)
            return s_AccessDeniedException;

        m_DbContext.UserGroupEntries.Remove(targetUge);
        await m_DbContext.SaveChangesAsync();

        m_DbContext.Notifications.Add(new Notification(target,
            $"You have been removed from Group {group.Name} by {m_User.UserName}"));

        return true;
    }

    public async Task<MpmResult<List<UserGroupEntry>>> GetGroupUsers(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var query = m_DbContext.UserGroupEntries
            .Where(e => e.Group == group)
            .Include(uge => uge.MpmUser);

        return await query.ToListAsync();
    }

    public async Task<MpmResult<List<SeasonEntry>>> GetGroupSeasons(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var query = m_DbContext.SeasonEntries
            .Where(se => se.Group == group)
            .Include(se => se.Season);

        return await query.ToListAsync();
    }

    public async Task<MpmResult<bool>> AddSeasonToGroup(MpmGroup group, int seasonId)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;

        var existingSeasonEntry =
            m_DbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.SeasonId == seasonId);
        if (existingSeasonEntry is not null)
            return s_AlreadyExistsException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        m_DbContext.SeasonEntries.Add(new SeasonEntry
        {
            GroupId = group.Id,
            SeasonId = seasonId
        });
        await m_DbContext.SaveChangesAsync();

        return true;
    }

    public async Task<MpmResult<bool>> RemoveSeasonFromGroup(MpmGroup group, Season season)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        if (m_User is null) return s_NoUserException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        var seasonEntry = m_DbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (seasonEntry is null)
            return s_GroupNotFoundException;

        m_DbContext.SeasonEntries.Remove(seasonEntry);
        await m_DbContext.SaveChangesAsync();

        return true;
    }

    public async Task<MpmResult<CustomSeason>> CreateCustomSeason(MpmGroup group, string name, string description,
        DateTime startDate, DateTime endDate, ESportType sport)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        if (m_User is null) return s_NoUserException;

        if (BadWordRegex().IsMatch(name)) return s_BadWordException;
        if (BadWordRegex().IsMatch(description)) return s_BadWordException;

        if (name.Length > 50 || description.Length > 2000)
            return s_BadWordException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        if (startDate < DateTime.Now)
            return s_InvalidDateException;

        if (endDate < startDate)
            return s_InvalidDateException;

        var customSeason = new CustomSeason(name, description)
        {
            Start = startDate,
            End = endDate,
            Sport = sport
        };
        
        var seasonEntry = new SeasonEntry{ GroupId = group.Id, Season = customSeason};
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

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        var seasonEntry = m_DbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;

        var existingEntry =
            m_DbContext.CustomSeasonEntries.FirstOrDefault(cse => cse.Season == season && cse.Game == game);
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

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        var seasonEntry = m_DbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;

        var existingEntry =
            m_DbContext.CustomSeasonEntries.FirstOrDefault(cse => cse.Season == season && cse.Game == game);
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

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var seasonEntry = m_DbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;

        var query = m_DbContext.CustomSeasonEntries
            .Where(cse => cse.Season == season)
            .Select(cse => cse.Game);

        return await query.ToListAsync();
    }

    public async Task<MpmResult<BuiltinSeason>> GetCurrentBuiltInSeasonById(int id)
    {
        //Returns most current, doesn't check if the season is active

        var query = m_DbContext.BuiltinSeasons
            .Where(bis => bis.Id == id).OrderBy(bis => bis.Start);
        var season = await query.FirstOrDefaultAsync();

        if (season is null)
        {
            return s_SeasonNotFoundException;
        }

        return season;
    }

    public async Task<MpmResult<List<Notification>>> GetAllNotificationOfUser()
    {
        if (m_User is null) return s_NoUserException;

        var query = m_DbContext.Notifications
            .Where(n => n.Target == m_User);

        return await query.ToListAsync();
    }

    public async Task<MpmResult<List<Notification>>> GetAllNewNotificationOfUser()
    {
        if (m_User is null) return s_NoUserException;

        var query = m_DbContext.Notifications
            .Where(n => n.Target == m_User && !n.IsRead);

        return await query.ToListAsync();
    }

    public async Task<MpmResult<bool>> MarkAllNewNotificationAsRead()
    {
        if (m_User is null) return s_NoUserException;

        var query = m_DbContext.Notifications
            .Where(n => n.Target == m_User && !n.IsRead);


        foreach (var notification in query)
        {
            notification.IsRead = true;
        }

        await m_DbContext.SaveChangesAsync();

        return true;
    }

    public async Task<MpmResult<List<Message>>> GetAllMessagesOfGroup(MpmGroup group)
    {
        if (m_User is null) return s_NoUserException;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var query = m_DbContext.Messages
            .Where(m => m.RecipientGroup == group);

        return await query.ToListAsync();
    }

    public async Task<MpmResult<Message>> SendMessage(MpmGroup group, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        if (BadWordRegex().IsMatch(text)) return s_BadWordException;

        var uge = await m_DbContext.UserGroupEntries.FirstOrDefaultAsync(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var message = new Message(m_User, group, text);

        return message;
    }

    public async Task<MpmResult<Bet>> CreateFootballResultBet(MpmGroup group, Game game, EResult result)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        if (result is EResult.None) return s_InvalidBetParameter;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var existingBet = m_DbContext.FootballResultBets.FirstOrDefault(b => b.Game == game && b.User == m_User);
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

    public async Task<MpmResult<Bet>> CreateFootballScoreBet(MpmGroup group, Game game, int homeScore, int awayScore)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        if (homeScore < 0 || awayScore < 0) return s_InvalidBetParameter;

        var uge = m_DbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;

        var existingBet = m_DbContext.FootballResultBets.FirstOrDefault(b => b.Game == game && b.User == m_User);
        if (existingBet is not null) return s_AlreadyExistsException;
        if (game.GameState != EGameState.Upcoming) return s_InvalidDateException;


        var bet = new ScoreBet()
        {
            UserId = m_User.Id,
            GroupId = group.Id,
            GameId = game.Id,
            HomeScore = homeScore,
            AwayScore = awayScore
        };
        await m_DbContext.FootballScoreBets.AddAsync(bet);
        await m_DbContext.SaveChangesAsync();

        return bet;
    }

    public Achievement CreateAchievement(string title, string description)
    {
        var achievement = new Achievement(title, description);
        return achievement;
    }

    private static readonly Func<MpmDbContext, IAsyncEnumerable<BuiltinSeason>> s_GetAllBuiltinSeasonsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext) =>
            dbContext.BuiltinSeasons);

    public async Task<List<BuiltinSeason>> GetAllBuiltinSeasons()
    {
        List<BuiltinSeason> seasons = [];

        await foreach (var season in s_GetAllBuiltinSeasonsQuery.Invoke(m_DbContext))
        {
            seasons.Add(season);
        }

        return seasons;
    }
}