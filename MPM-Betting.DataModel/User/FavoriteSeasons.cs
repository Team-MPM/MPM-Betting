using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.User;

public class FavoriteSeasons
{
    public MpmUser User { get; set; }
    public string UserId { get; set; }
    public BuiltinSeason Season { get; set; }
    public int SeasonId { get; set; }

    public FavoriteSeasons(BuiltinSeason s, MpmUser u)
    {
        User = u;
        Season = s;
    }
}