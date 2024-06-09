namespace MPM_Betting.DataModel.Betting;

public class BuiltinSeason(string name, string description) : Season(name, description)
{
    public int ReferenceId { get; set; }
    private BuiltinSeason() : this(null!, null!) {}
}