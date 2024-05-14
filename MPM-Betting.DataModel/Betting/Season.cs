using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class Season(string name, string description)
{
    [Key]
    public int Id { get; set; }
    
    [StringLength(50)]
    public string Name { get; set; } = name;

    [StringLength(2000)]
    public string Description { get; set; } = description;

    public ESportType Sport { get; set; }
    
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}