using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Rewarding;

namespace MPM_Betting.DataModel.User;

public class MpmUser : IdentityUser
{
    public List<UserGroupEntry> UserGroupEntries { get; set; } = [];
    
    public int Points { get; set; } = 0;
    public DateTime LastRedeemed { get; set; } = DateTime.Now;
    
    public List<Achievement> Achievments { get; set; } = [];
    
    public List<FavoriteSeasons> FavoriteSeasons { get; set; } = [];
}