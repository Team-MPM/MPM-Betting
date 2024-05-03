using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;

namespace MPM_Betting.DataModel.User;
/// <summary>
/// User Class
/// </summary>
public class MpmUser : IdentityUser
{
    public List<UserGroupEntry> UserGroupEntries { get; set; } = [];
    
}