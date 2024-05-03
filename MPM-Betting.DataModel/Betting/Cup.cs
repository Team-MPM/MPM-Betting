namespace MPM_Betting.DataModel.Betting;

public class Cup(int ID, string Name) : ACompetitionType(ID, Name)
{
    public ERound Round { get; set; }
}