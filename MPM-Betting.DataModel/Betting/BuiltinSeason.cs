using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class BuiltinSeason(string name) : Season(name)
{
    public List<Game> Games = [];
    
    private BuiltinSeason() : this(null!) {}
}