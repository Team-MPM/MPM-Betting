using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;

namespace MPM_Betting.DataModel.User;

public class MpmUser : IdentityUser
{
    public List<UserGroupEntry> UserGroupEntries { get; set; } = [];
}