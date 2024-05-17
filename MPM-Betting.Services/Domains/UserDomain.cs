﻿using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

//TODO: Add cancelation tokens

public partial class UserDomain(MpmDbContext dbContext)
{
    private class NoUserException : Exception;
    private static readonly NoUserException s_NoUserException = new();

    private class GroupNotFoundException : Exception;
    private static readonly GroupNotFoundException s_GroupNotFoundException = new();
    
    private class SeasonNotFoundException : Exception;
    private static readonly SeasonNotFoundException s_SeasonNotFoundException = new();

    private class AccessDeniedException : Exception;
    private static readonly AccessDeniedException s_AccessDeniedException = new();
    
    private class BadWordException : Exception;
    private static readonly BadWordException s_BadWordException = new();
    
    private class InvalidDateException : Exception;
    private static readonly InvalidDateException s_InvalidDateException = new();
    
    private class AlreadyExistsException : Exception;
    private static readonly AlreadyExistsException s_AlreadyExistsException = new();
    
    [GeneratedRegex("^[a@][s\\$][s\\$]$\n[a@][s\\$][s\\$]h[o0][l1][e3][s\\$]?\nb[a@][s\\$][t\\+][a@]rd \nb[e3][a@][s\\$][t\\+][i1][a@]?[l1]([i1][t\\+]y)?\nb[e3][a@][s\\$][t\\+][i1][l1][i1][t\\+]y\nb[e3][s\\$][t\\+][i1][a@][l1]([i1][t\\+]y)?\nb[i1][t\\+]ch[s\\$]?\nb[i1][t\\+]ch[e3]r[s\\$]?\nb[i1][t\\+]ch[e3][s\\$]\nb[i1][t\\+]ch[i1]ng?\nb[l1][o0]wj[o0]b[s\\$]?\nc[l1][i1][t\\+]\n^(c|k|ck|q)[o0](c|k|ck|q)[s\\$]?$\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]d \n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]r\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[i1]ng\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[s\\$]\n^cum[s\\$]?$\ncumm??[e3]r\ncumm?[i1]ngcock\n(c|k|ck|q)um[s\\$]h[o0][t\\+]\n(c|k|ck|q)un[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[i1][l1][l1][i1]ngu[s\\$]\n(c|k|ck|q)unn[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[t\\+][s\\$]?\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[e3]r\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[i1]ng\ncyb[e3]r(ph|f)u(c|k|ck|q)\nd[a@]mn\nd[i1]ck\nd[i1][l1]d[o0]\nd[i1][l1]d[o0][s\\$]\nd[i1]n(c|k|ck|q)\nd[i1]n(c|k|ck|q)[s\\$]\n[e3]j[a@]cu[l1]\n(ph|f)[a@]g[s\\$]?\n(ph|f)[a@]gg[i1]ng\n(ph|f)[a@]gg?[o0][t\\+][s\\$]?\n(ph|f)[a@]gg[s\\$]\n(ph|f)[e3][l1][l1]?[a@][t\\+][i1][o0]\n(ph|f)u(c|k|ck|q)\n(ph|f)u(c|k|ck|q)[s\\$]?\ng[a@]ngb[a@]ng[s\\$]?\ng[a@]ngb[a@]ng[e3]d\ng[a@]y\nh[o0]m?m[o0]\nh[o0]rny\nj[a@](c|k|ck|q)\\-?[o0](ph|f)(ph|f)?\nj[e3]rk\\-?[o0](ph|f)(ph|f)?\nj[i1][s\\$z][s\\$z]?m?\n[ck][o0]ndum[s\\$]?\nmast(e|ur)b(8|ait|ate)\nn+[i1]+[gq]+[e3]*r+[s\\$]*\n[o0]rg[a@][s\\$][i1]m[s\\$]?\n[o0]rg[a@][s\\$]m[s\\$]?\np[e3]nn?[i1][s\\$]\np[i1][s\\$][s\\$]\np[i1][s\\$][s\\$][o0](ph|f)(ph|f) \np[o0]rn\np[o0]rn[o0][s\\$]?\np[o0]rn[o0]gr[a@]phy\npr[i1]ck[s\\$]?\npu[s\\$][s\\$][i1][e3][s\\$]\npu[s\\$][s\\$]y[s\\$]?\n[s\\$][e3]x\n[s\\$]h[i1][t\\+][s\\$]?\n[s\\$][l1]u[t\\+][s\\$]?\n[s\\$]mu[t\\+][s\\$]?\n[s\\$]punk[s\\$]?\n[t\\+]w[a@][t\\+][s\\$]?", RegexOptions.IgnoreCase)]
    private static partial Regex BadWordRegex();
    
    private MpmUser? m_User;
    
    public void SetUser(MpmUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        m_User = user;
    }
    
    public async Task<MpmResult<List<MpmGroup>>> GetGroups()
    {
        if (m_User is null) return s_NoUserException;
        
        var query = dbContext.UserGroupEntries
            .Where(uge => uge.MpmUser == m_User)
            .Select(uge => uge.Group);
        
        return await query.ToListAsync();
    }
    
    public async Task<MpmResult<MpmGroup>> GetGroupByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (m_User is null) return s_NoUserException;
        
        var query = dbContext.UserGroupEntries
            .Where(uge => uge.MpmUser == m_User && uge.Group.Name == name)
            .Select(uge => uge.Group);

        var result = await query.FirstOrDefaultAsync();
        
        if (result is null) return s_GroupNotFoundException;
        
        return result;
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
        
        var existingGroup = dbContext.Groups.FirstOrDefault(g => g.Name == name);
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

        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

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

        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

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
        
        var existingUge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == target);
        if (existingUge is not null)
            return s_AlreadyExistsException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        dbContext.UserGroupEntries.Add(new UserGroupEntry(target, group) { Role = role });
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> UpdateGroupRole(MpmGroup group, MpmUser target, EGroupRole role)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;

        if (role is EGroupRole.Owner) return s_AccessDeniedException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not EGroupRole.Owner)
            return s_AccessDeniedException;
        
        var targetUge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == target);
        if (targetUge is null)
            return s_GroupNotFoundException;
        
        targetUge.Role = role;
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<bool>> RemoveUserFromGroup(MpmGroup group, MpmUser target)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var targetUge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == target);
        if (targetUge is null)
            return s_GroupNotFoundException;
        
        if (targetUge.Role is EGroupRole.Owner)
            return s_AccessDeniedException;
        
        dbContext.UserGroupEntries.Remove(targetUge);
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<List<UserGroupEntry>>> GetGroupUsers(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var query = dbContext.UserGroupEntries
            .Where(e => e.Group == group);
        
        return await query.ToListAsync();
    }
    
    public async Task<MpmResult<List<SeasonEntry>>> GetGroupSeasons(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var query = dbContext.SeasonEntries
            .Where(se => se.Group == group);
        
        return await query.ToListAsync();
    }
    
    public async Task<MpmResult<bool>> AddSeasonToGroup(MpmGroup group, Season season)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        if (m_User is null) return s_NoUserException;
        
        var existingSeasonEntry = dbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (existingSeasonEntry is not null)
            return s_AlreadyExistsException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

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
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var seasonEntry = dbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
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
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

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
    
    public async Task<MpmResult<bool>> AddCustomSeasonEntry(MpmGroup group, CustomSeason season, Game game)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(game);
        if (m_User is null) return s_NoUserException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var seasonEntry = dbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        var existingEntry = dbContext.CustomSeasonEntries.FirstOrDefault(cse => cse.Season == season && cse.Game == game);
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
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var seasonEntry = dbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        var existingEntry = dbContext.CustomSeasonEntries.FirstOrDefault(cse => cse.Season == season && cse.Game == game);
        if (existingEntry is null)
            return s_GroupNotFoundException;
        
        dbContext.CustomSeasonEntries.Remove(existingEntry);
        await dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<MpmResult<List<Game>>> GetSeasonEntries(MpmGroup group, CustomSeason season)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(season);
        if (m_User is null) return s_NoUserException;
        
        var uge = dbContext.UserGroupEntries.FirstOrDefault(uge => uge.Group == group && uge.MpmUser == m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var seasonEntry = dbContext.SeasonEntries.FirstOrDefault(se => se.Group == group && se.Season == season);
        if (seasonEntry is null)
            return s_SeasonNotFoundException;
        
        var query = dbContext.CustomSeasonEntries
            .Where(cse => cse.Season == season)
            .Select(cse => cse.Game);
        
        return await query.ToListAsync();
    }
    
    public async Task<MpmResult<BuiltinSeason>> GetCurrentBuiltInSeasonById( int id)
    {
        //Returns most current, doesnt check if season is active

        var query = dbContext.BuiltinSeasons
            .Where(bis => bis.Id == id).OrderBy(bis =>bis.Start);
        var season = query.FirstOrDefault();
        
        if (season is null)
        {
            return s_SeasonNotFoundException;
        }
        
        return season;
    }
}