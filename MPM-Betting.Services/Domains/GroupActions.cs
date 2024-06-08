using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
    
    public async Task<MpmResult<List<MpmGroup>>> GetGroups()
    {
        if (m_User is null) return s_NoUserException;

        List<MpmGroup> groups = [];
        
        await foreach (var group in s_GetUserGroupsQuery.Invoke(m_DbContext, m_User))
        {
            groups.Add(group);
        }

        return groups;
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

        List<MpmGroup> groups = [];

        await foreach(var group in s_GetGroupsBySeasonChosen.Invoke(m_DbContext,id))
            groups.Add(group);
        
        return groups;
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
        
        var existingGroup = await s_GetGroupByName.Invoke(m_DbContext, name);
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
    
    private static readonly Func<MpmDbContext, int, Task<MpmGroup?>> s_GetGroupById =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int id) =>
            dbContext.Groups
                .FirstOrDefault(g => g.Id == id));
    
    public async Task<MpmResult<bool>> UpdateGroupName(MpmGroup group, string name)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(name);
        if (m_User is null) return s_NoUserException;
        
        if (BadWordRegex().IsMatch(name)) return s_BadWordException;
        
        if (name.Length > 30)
            return s_BadWordException;

        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        var groupEntry = await s_GetGroupById.Invoke(m_DbContext,group.Id);

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

        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);
        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        var groupEntry = await s_GetGroupById.Invoke(m_DbContext,group.Id);

        if (groupEntry is null)
            return s_GroupNotFoundException;
        
        groupEntry.Description = description;
        await m_DbContext.SaveChangesAsync();

        return true;
    }
    
    /// <summary>
    /// Adds a user to a group with a specific role
    /// </summary>
    /// <param name="group"></param>
    /// <param name="targetId">Expected to correspond to valid user</param>
    /// <param name="role"></param>
    /// <returns></returns>
    public async Task<MpmResult<bool>> AddUserToGroup(MpmGroup group, string targetId, EGroupRole role)
    {
        ArgumentNullException.ThrowIfNull(targetId);
        if (m_User is null) return s_NoUserException;
        
        if (role is EGroupRole.Owner) return s_AccessDeniedException;
        
        var existingUge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, targetId);
        if (existingUge is not null)
            return s_AlreadyExistsException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;

        await m_DbContext.UserGroupEntries.AddAsync(new UserGroupEntry
        {
            MpmUserId = targetId,
            GroupId = group.Id,
            Role = role
        });

        await m_DbContext.Notifications.AddAsync(new Notification
        {
            Date = DateTime.Now,
            TargetId = targetId,
            Message = $"You have been added to Group '{group.Name}' by {m_User.UserName}"
        });
        
        await m_DbContext.SaveChangesAsync();

        return true;
    }
    
    public async Task<MpmResult<bool>> UpdateGroupRole(MpmGroup group, MpmUser target, EGroupRole role)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(target);
        if (m_User is null) return s_NoUserException;

        if (role is EGroupRole.Owner) return s_AccessDeniedException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not EGroupRole.Owner)
            return s_AccessDeniedException;
        
        var targetUge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, target.Id);
        if (targetUge is null)
            return s_GroupNotFoundException;
        
        targetUge.Role = role;
        await m_DbContext.SaveChangesAsync();

        m_DbContext.Notifications.Add(new Notification(target,
            $"Your role in Group {group.Name} has been changed to {role} by {m_User.UserName}"));

        return true;
    }
    
    public async Task<MpmResult<bool>> RemoveUserFromGroup(MpmGroup group, string targetId)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(targetId);
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);

        if (uge?.Role is not (EGroupRole.Owner or EGroupRole.Admin))
            return s_AccessDeniedException;
        
        var targetUge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, targetId);
        if (targetUge is null)
            return s_GroupNotFoundException;
        
        if (targetUge.Role is EGroupRole.Owner)
            return s_AccessDeniedException;

        m_DbContext.UserGroupEntries.Remove(targetUge);

        m_DbContext.Notifications.Add(new Notification
        {
            Date = DateTime.Now,
            TargetId = targetId,
            Message = $"You have been removed from Group {group.Name} by {m_User.UserName}"
        });

        await m_DbContext.SaveChangesAsync();
        return true;
    }
    public async Task<MpmResult<List<UserGroupEntry>>> GetUsersByGroup(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        
        var query = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id ,m_User.Id);
        if (query is null)
            return s_AccessDeniedException;
        List<UserGroupEntry> uges = [];
        
        await foreach(var uge in s_GetUserGroupEntryByGroup.Invoke(m_DbContext, group))
            uges.Add(uge!); 
        
        return uges;
    }
    
     public async Task<MpmResult<int>> GetUserPosition(MpmGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if(m_User == null) return s_NoUserException;
        
        List<UserGroupEntry> uges = [];
        await foreach(var uge in s_GetUserGroupEntriesByGroup.Invoke(m_DbContext, group))
           uges.Add(uge!);
        
        uges = uges.OrderBy(uge => uge.Score).ToList();
        
        return uges.FindIndex(uge => uge.MpmUser == m_User) + 1;
    }
     
    public async Task<MpmResult<(MpmGroup group, UserGroupEntry entry)>> GetGroupByIdWithEntry(int id)
    {
        if (m_User is null) return s_NoUserException;

        var result = await s_GetGroupByIdWithEntryQuery.Invoke(m_DbContext, m_User, id);

        if (result is null) return s_GroupNotFoundException;

        return (result.Group, result);
    }
}