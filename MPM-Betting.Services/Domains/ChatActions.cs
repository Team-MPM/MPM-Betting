using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
     public async Task<MpmResult<List<Message>>> GetAllMessagesOfgroup(MpmGroup group)
    {
        if (m_User is null) return s_NoUserException;
        
        var uge = s_GetUserGroupEntryByGroup.Invoke(m_DbContext, group);
        if (uge is null)
            return s_AccessDeniedException;
        
        List<Message> _messages = [];
        
        await foreach(var messages in s_GetAllMessagesOfGroup.Invoke(m_DbContext, group))
            _messages.Add(messages);

        return _messages;
    }

    public async Task<MpmResult<Message>> SendMessage(MpmGroup group, string text)
    {       
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(group);
        if (m_User is null) return s_NoUserException;
        if(BadWordRegex().IsMatch(text)) return s_BadWordException;
        
        var uge = await s_GetUserGroupEntryQuery.Invoke(m_DbContext, group, m_User);
        if (uge is null)
            return s_AccessDeniedException;
        
        var message = new Message(m_User, group, text);
        
        return message;
    }
}