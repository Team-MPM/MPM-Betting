namespace MPM_Betting.DataModel.User;

public class FavouriteFootballLeague
{
    public string UserId { get; set; }
    public MpmUser User;
    
    public int LeagueId { get; set; }
}