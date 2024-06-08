using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
    private static readonly Func<MpmDbContext, int, string, Task<UserGroupEntry?>> s_GetUserGroupEntryQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int groupId, string userId) =>
            dbContext.UserGroupEntries
                .FirstOrDefault(uge => uge.GroupId == groupId && uge.MpmUserId == userId));

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

    private static readonly Func<MpmDbContext, string, Task<MpmGroup?>> s_GetGroupByName =
        EF.CompileAsyncQuery((MpmDbContext dbContext, string name) => dbContext.Groups
            .FirstOrDefault(g => g.Name == name));

    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<UserGroupEntry?>>
        s_GetUserGroupEntriesByGroup =
            EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
                dbContext.UserGroupEntries
                    .Where(uge => uge.Group == group)
                    .Include(uge => uge.MpmUser));

    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<UserGroupEntry?>> s_GetUserGroupEntryByGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
            dbContext.UserGroupEntries
                .Where(uge => uge.Group == group)
                .Include(uge => uge.MpmUser));

    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<SeasonEntry?>> s_GetSeasonEntriesByGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
            dbContext.SeasonEntries
                .Where(se => se.Group == group)
                .Include(se => se.Season));

    private static readonly Func<MpmDbContext, MpmGroup, int, Task<SeasonEntry?>>
        s_GetSeasonEntriesByGroupAndSeason =
            EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group, int seasonId) =>
                dbContext.SeasonEntries.FirstOrDefault(se => se.SeasonId == seasonId && se.Group == group));

    private static readonly Func<MpmDbContext, int, Task<Season?>> s_GetSeasonById =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int id) =>
            dbContext.Seasons
                .FirstOrDefault(s => s.Id == id));

    private static readonly Func<MpmDbContext, Season, Game, Task<CustomSeasonEntry?>>
        s_GetCustomSeasonEntryBySeasonAndGame =
            EF.CompileAsyncQuery((MpmDbContext dbContext, Season season, Game game) =>
                dbContext.CustomSeasonEntries
                    .FirstOrDefault(cse => cse.Season == season && cse.Game == game));

    private static readonly Func<MpmDbContext, int, Task<BuiltinSeason?>> s_GetCurrentBuiltInSeasonById =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int id) =>
            dbContext.BuiltinSeasons
                .Where(bis => bis.Id == id)
                .OrderBy(bis => bis.Start)
                .FirstOrDefault());

    private static readonly Func<MpmDbContext, MpmGroup, CustomSeason, IAsyncEnumerable<Game>>
        s_GetSeasonEntriesByGameAndSeason =
            EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group, CustomSeason season) =>
                dbContext.CustomSeasonEntries
                    .Where(cse => cse.Season == season)
                    .Select(cse => cse.Game));

    private static readonly Func<MpmDbContext, MpmUser, IAsyncEnumerable<Notification>> s_GetUnreadNotifications =
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
    
    
    
    private static readonly Func<MpmDbContext, MpmGroup, IAsyncEnumerable<Message>> s_GetAllMessagesOfGroup =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmGroup group) =>
            dbContext.Messages
                .Where(m => m.RecipientGroup == group));

    private static readonly Func<MpmDbContext, IAsyncEnumerable<BuiltinSeason>> s_GetAllBuiltinSeasonsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext) =>
            dbContext.BuiltinSeasons);
    
    
    
    
    
    private static readonly Func<MpmDbContext,MpmUser,IAsyncEnumerable<Bet>> s_GetAllBetsForUserQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.Bets
                .Where(b => b.User == user)
                .Include(b => b.Game));
    
    private static readonly Func<MpmDbContext,int,IAsyncEnumerable<Bet>> s_GetAllBetsForGroupQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int groupId) =>
            dbContext.Bets
                .Where(b => b.GroupId == groupId)
                .Include(b => b.Game));
    
    private static readonly Func<MpmDbContext,int,IAsyncEnumerable<Bet>> s_GetAllBetsForGameQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int gameId) =>
            dbContext.Bets
                .Where(b => b.GameId == gameId)
                .Include(b => b.Game));
    
    private static readonly Func<MpmDbContext,IAsyncEnumerable<Bet>> s_GetAllBetsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext) => dbContext.Bets
                .Include(b => b.Game));
    
    
    
    
    
    private static readonly Func<MpmDbContext,IAsyncEnumerable<GameBet>> s_GetAllFootballGameBetsQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext) => dbContext.FootballGameBets
                .Include(b => b.Game));
    
    private static readonly Func<MpmDbContext,MpmUser,IAsyncEnumerable<GameBet>> s_GetAllFootballGameBetsForUserQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, MpmUser user) =>
            dbContext.FootballGameBets
                .Where(b => b.User == user)
                .Include(b => b.Game));
    
    private static readonly Func<MpmDbContext,int,IAsyncEnumerable<GameBet>> s_GetAllFootballGameBetsForGroupQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int groupId) =>
            dbContext.FootballGameBets
                .Where(b => b.GroupId == groupId)
                .Include(b => b.Game));
    
    private static readonly Func<MpmDbContext,int,IAsyncEnumerable<GameBet>> s_GetAllFootballGameBetsForGameQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int gameId) =>
            dbContext.FootballGameBets
                .Where(b => b.GameId == gameId)
                .Include(b => b.Game));
    
    
    
    
    private static readonly Func<MpmDbContext, int, string, MpmGroup?, Task<bool>> s_UserHasFootballGameBetQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, int referenceId, string userId, MpmGroup? group) =>
            dbContext.FootballGameBets
                .Any(b => b.Game.ReferenceId == referenceId && b.Group == group && b.UserId == userId));
    
    
    
    private static readonly Func<MpmDbContext, string, Task<MpmUser?>> s_GetUserByNameOrMailQuery =
        EF.CompileAsyncQuery((MpmDbContext dbContext, string name) =>
            dbContext.Users.FirstOrDefault(u => u.UserName == name && u.Email == name));
    
}