using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class Season(string name)
{
    [Key]
    public int Id { get; set; }
    
    [StringLength(50)]
    public string Name { get; set; } = name;
    
    public ESportType Sport { get; set; }
    
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    
    public int ReferenceId { get; set; }
}