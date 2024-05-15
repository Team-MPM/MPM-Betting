using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.User;

public class Message
{
    [Key] 
    public int Id { get; set; }

    public MpmUser Sender { get; set; }

    public MpmGroup? RecipientGroup { get; set; }
    
    public MpmUser? RecipientUser { get; set; }

    public DateTime Time { get; set; }

    [StringLength(5000)]
    public string Content { get; set; }

    public Message(MpmUser sender, MpmGroup group, string content)
    {
        Sender = sender;
        RecipientGroup = group;
        Content = content;
    }
    
    public Message(MpmUser sender, MpmUser user, string content)
    {
        Sender = sender;
        RecipientUser = user;
        Content = content;
    }
    
    private Message() : this(null!, (MpmUser)null!, null!) { }
}