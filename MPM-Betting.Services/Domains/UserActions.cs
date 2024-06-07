using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel.Rewarding;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
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
        
        await foreach(var notification in GetUnreadNotifications.Invoke(m_DbContext,m_User))
        {
            notification.IsRead = true;
        }
        
        await m_DbContext.SaveChangesAsync();
        
        return true;
    }
    
    public Achievement CreateAchievement(string title, string description)
    {
        var achievement = new Achievement(title, description);
        return achievement;
    }
}