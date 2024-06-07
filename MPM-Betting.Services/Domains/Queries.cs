using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
    private static readonly Func<MpmDbContext, MpmGroup, MpmUser, Task<UserGroupEntry?>> s_GetUserGroupEntryQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup Group, MpmUser user) =>
            dbContext.UserGroupEntries
                .FirstOrDefault(uge => uge.Group == Group && uge.MpmUser == user));

    private static readonly Func<MpmDbContext, MpmUser, IAsyncEnumerable<MpmGroup>> s_GetUserGroupsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.MpmUser == user)
                .Select(uge => uge.Group));

    private static readonly Func<MpmDbContext, MpmUser, string, Task<MpmGroup?>> s_GetGroupByNameQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user, string name) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.MpmUser == user && uge.Group.Name == name)
                .Select(uge => uge.Group)
                .FirstOrDefault());

    private static readonly Func<MpmDbContext, int, IAsyncEnumerable<MpmGroup>> s_GetGroupsBySeasonChosen =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int id) =>
            dbContext.SeasonEntries
                .Where(se => se.Id == id)
                .Select(se => se.Group));

    private static readonly Func<MpmDbContext, string, Task<MpmGroup>> s_GetGroupByName =
        EF.CompileAsyncQuery((MpmDbContext dbContext, string name) => dbContext.Groups
            .Where(g => g.Name == name)
            .FirstOrDefault());

    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<UserGroupEntry?>>
        s_GetUserGroupEntriesByGroup =
            EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup Group) =>
                dbContext.UserGroupEntries
                    .Where(uge => uge.Group == Group)
                    .Include(uge => uge.MpmUser));

    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<UserGroupEntry?>> s_GetUserGroupEntryByGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup Group) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.Group == Group)
                .Include(uge => uge.MpmUser));

    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<SeasonEntry?>> s_GetSeasonEntriesByGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
            dbContext.SeasonEntries
                .Where(se => se.Group == group));

    private static readonly Func<MpmDbContext, MpmUser, IAsyncEnumerable<FavoriteSeasons>> s_GetFavoriteSeasonsByUser =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.FavoriteSeasons
                .Where(fs => fs.User == user));
    
     private static readonly Func<MpmDbContext, MpmGroup, Season, Task<SeasonEntry>> s_GetSeasonEntriesByGroupAndSeason =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group, Season season) =>
            dbContext.SeasonEntries.FirstOrDefault(se => se.Season == season && se.Group == group));
    private static readonly Func<MpmDbContext, int, Task<Season>> s_GetSeasonById =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int id) =>
            dbContext.Seasons
                .FirstOrDefault(s => s.Id == id));
    
     private static readonly Func<MpmDbContext,Season,Game,Task<CustomSeasonEntry>> s_GetCustomSeasonEntryBySeasonAndGame =
        EF.CompileAsyncQuery((MpmDbContext dbContext, Season season, Game game) =>
            dbContext.CustomSeasonEntries
                .FirstOrDefault(cse => cse.Season == season && cse.Game == game));
     
        private static readonly Func<MpmDbContext, int, Task<BuiltinSeason?>> s_GetCurrentBuiltInSeasonById =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int id) =>
            dbContext.BuiltinSeasons
                .Where(bis => bis.Id == id)
                .OrderBy(bis => bis.Start)
                .FirstOrDefault());
    
        private static readonly Func<MpmDbContext, MpmGroup, CustomSeason, IAsyncEnumerable<Game>> s_GetSeasonEntriesByGameAndSeason =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group, CustomSeason season) =>
            dbContext.CustomSeasonEntries
                .Where(cse => cse.Season == season)
                .Select(cse => cse.Game));
   
        private static readonly Func<MpmDbContext,MpmUser,IAsyncEnumerable<Notification>> GetUnreadNotifications =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.Notifications
                .Where(n => n.Target == user && !n.IsRead));
        
        
    private static readonly Func<MpmDbContext, MpmUser, int, Task<UserGroupEntry?>>
        s_GetGroupByIdWithEntryQuery =
            EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user, int id) =>
                dbContext.UserGroupEntries
                    .Where(uge => uge.MpmUser == user && uge.Group.Id == id)
                    .Include(uge => uge.Group)
                    .FirstOrDefault());
   
    private static readonly Func<MpmDbContext, Season, MpmUser, Task<FavoriteSeasons>> s_GetFavoriteSeasonByUserAndSeason =
        EF.CompileAsyncQuery((MpmDbContext dbContext,Season s, MpmUser user) =>
            dbContext.FavoriteSeasons.FirstOrDefault(fs => fs.Season == s && fs.User == user));
    
     private static readonly Func<MpmDbContext,MpmGroup,IAsyncEnumerable<Message>> s_GetAllMessagesOfGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
            dbContext.Messages
                .Where(m => m.RecipientGroup == group));
    
   
    private static readonly Func<MpmDbContext, Game, MpmUser, Task<ResultBet?>> s_GetResultBetByGameAndUser =
        EF.CompileAsyncQuery((MpmDbContext dbContext, Game game, MpmUser user) =>
            dbContext.FootballResultBets
                .FirstOrDefault(b => b.Game == game && b.User == user));
    
    
    private static readonly Func<MpmDbContext, Game, MpmUser, Task<ScoreBet?>> s_GetScoreBetByGameAndUser =
        EF.CompileAsyncQuery((MpmDbContext dbContext, Game game, MpmUser user) =>
            dbContext.FootballScoreBets
                .FirstOrDefault(b => b.Game == game && b.User == user));
    
    private static readonly Func<MpmDbContext, IAsyncEnumerable<BuiltinSeason>> s_GetAllBuiltinSeasonsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext) =>
            dbContext.BuiltinSeasons);
    
    private static readonly Func<MpmDbContext, MpmUser, IAsyncEnumerable<ResultBet>> s_GetAllCompletedBets = 
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.FootballResultBets
                .Where(b => b.User == user && b.Completed)
                .Include(b => b.Game)
                .Include(b => b.User));
}