using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class BuiltinSeason(string name, string description) : Season(name, description)
{
    public List<Game> Games = [];
    
    private BuiltinSeason() : this(null!, null!) {}
}