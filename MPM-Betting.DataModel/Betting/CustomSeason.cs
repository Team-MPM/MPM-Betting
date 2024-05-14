using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class CustomSeason(string name, string description) : Season(name, description)
{
    public List<Game> Games = [];
    private CustomSeason() : this(null!, null!) {}
}