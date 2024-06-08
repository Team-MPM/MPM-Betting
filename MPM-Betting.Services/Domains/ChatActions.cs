using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
     public async Task<MpmResult<List<Message>>> GetAllMessagesOfGroup(MpmGroup group)
    {
        if (m_User is null) return s_NoUserException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);
        if (uge is null)
            return s_AccessDeniedException;
        
        List<Message> messages = [];
        
        await foreach(var message in s_GetAllMessagesOfGroup.Invoke(m_DbContext, group))
            messages.Add(message);

        return messages;
    }

    public async Task<MpmResult<Message>> SendMessage(MpmGroup group, string text)
    {       
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        if(BadWordRegex().IsMatch(text)) return s_BadWordException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group.Id, m_User.Id);
        if (uge is null)
            return s_AccessDeniedException;
        
        var message = new Message(m_User, group, text);
        
        return message;
    }
}