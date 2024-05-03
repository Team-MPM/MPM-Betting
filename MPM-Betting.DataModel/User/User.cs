using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace MPM_Betting.DataModel.User;
/// <summary>
/// ffgfg
/// </summary>
public class User
{
    [Key, Column("USER_ID")]
    public int Id { get; set; }
    //IDK solls in die DB
    public int ExternalId { get; set; }
    [StringLength(20), Required]
    [Column("USER_NAME")]
    public string Name { get; set; } = "";
    [StringLength(100), Required]
    [Column("USER_PASSWORD_HASH")]
    public string HashPassword { get; set; } = "";
    [StringLength(100), Required]
    [Column("USER_EMAIL")]
    public string Email { get; set; } = "";
    [Column("USER_EMAIL_CONFIRMED"),Required]
    public bool HasConfirmedEmail { get; set; } = false;
    [Column("USER_PHONENUMBER"),Required]
    public int Phonenumber { get; set; }
    public bool HasConfirmedPhonenumber { get; set; } = false;
    
    [Column("USER_POINTS")]
    public int Points { get; set; } = 0;
}