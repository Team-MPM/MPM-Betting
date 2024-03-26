using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Football;

public class Player
{
    [Key] public int Id { get; set; }
    [StringLength(50)] public string FullName { get; set; } = "";
    public int TricotNumber { get; set; }
    [StringLength(10)] public string? CallName { get; set; }
}