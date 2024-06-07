using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.Rewarding;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Domains;

//TODO: Add cancellation tokens

/// <summary>
/// Basically the domain for everything.
/// IMPORTANT: You need to call SetUser before calling any other method!!!
/// <exception cref="Exception">All Errors returned can be looked up in Domains/Errors.cs</exception>
/// </summary>
/// <param name="dbContextFactory">Injected via DI</param>
public partial class UserDomain(IDbContextFactory<MpmDbContext> dbContextFactory)
{
    private readonly MpmDbContext m_DbContext = dbContextFactory.CreateDbContext();

    private MpmUser? m_User;

    
    [GeneratedRegex("^[a@][s\\$][s\\$]$\n[a@][s\\$][s\\$]h[o0][l1][e3][s\\$]?\nb[a@][s\\$][t\\+][a@]rd \nb[e3][a@][s\\$][t\\+][i1][a@]?[l1]([i1][t\\+]y)?\nb[e3][a@][s\\$][t\\+][i1][l1][i1][t\\+]y\nb[e3][s\\$][t\\+][i1][a@][l1]([i1][t\\+]y)?\nb[i1][t\\+]ch[s\\$]?\nb[i1][t\\+]ch[e3]r[s\\$]?\nb[i1][t\\+]ch[e3][s\\$]\nb[i1][t\\+]ch[i1]ng?\nb[l1][o0]wj[o0]b[s\\$]?\nc[l1][i1][t\\+]\n^(c|k|ck|q)[o0](c|k|ck|q)[s\\$]?$\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]d \n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[e3]r\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[i1]ng\n(c|k|ck|q)[o0](c|k|ck|q)[s\\$]u(c|k|ck|q)[s\\$]\n^cum[s\\$]?$\ncumm??[e3]r\ncumm?[i1]ngcock\n(c|k|ck|q)um[s\\$]h[o0][t\\+]\n(c|k|ck|q)un[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[i1][l1][l1][i1]ngu[s\\$]\n(c|k|ck|q)unn[i1][l1][i1]ngu[s\\$]\n(c|k|ck|q)un[t\\+][s\\$]?\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[e3]r\n(c|k|ck|q)un[t\\+][l1][i1](c|k|ck|q)[i1]ng\ncyb[e3]r(ph|f)u(c|k|ck|q)\nd[a@]mn\nd[i1]ck\nd[i1][l1]d[o0]\nd[i1][l1]d[o0][s\\$]\nd[i1]n(c|k|ck|q)\nd[i1]n(c|k|ck|q)[s\\$]\n[e3]j[a@]cu[l1]\n(ph|f)[a@]g[s\\$]?\n(ph|f)[a@]gg[i1]ng\n(ph|f)[a@]gg?[o0][t\\+][s\\$]?\n(ph|f)[a@]gg[s\\$]\n(ph|f)[e3][l1][l1]?[a@][t\\+][i1][o0]\n(ph|f)u(c|k|ck|q)\n(ph|f)u(c|k|ck|q)[s\\$]?\ng[a@]ngb[a@]ng[s\\$]?\ng[a@]ngb[a@]ng[e3]d\ng[a@]y\nh[o0]m?m[o0]\nh[o0]rny\nj[a@](c|k|ck|q)\\-?[o0](ph|f)(ph|f)?\nj[e3]rk\\-?[o0](ph|f)(ph|f)?\nj[i1][s\\$z][s\\$z]?m?\n[ck][o0]ndum[s\\$]?\nmast(e|ur)b(8|ait|ate)\nn+[i1]+[gq]+[e3]*r+[s\\$]*\n[o0]rg[a@][s\\$][i1]m[s\\$]?\n[o0]rg[a@][s\\$]m[s\\$]?\np[e3]nn?[i1][s\\$]\np[i1][s\\$][s\\$]\np[i1][s\\$][s\\$][o0](ph|f)(ph|f) \np[o0]rn\np[o0]rn[o0][s\\$]?\np[o0]rn[o0]gr[a@]phy\npr[i1]ck[s\\$]?\npu[s\\$][s\\$][i1][e3][s\\$]\npu[s\\$][s\\$]y[s\\$]?\n[s\\$][e3]x\n[s\\$]h[i1][t\\+][s\\$]?\n[s\\$][l1]u[t\\+][s\\$]?\n[s\\$]mu[t\\+][s\\$]?\n[s\\$]punk[s\\$]?\n[t\\+]w[a@][t\\+][s\\$]?", RegexOptions.IgnoreCase)]
    public static partial Regex BadWordRegex();
    
    public void SetUser(MpmUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        m_User = user;
    }
    
    
    
    // TODO: Refactor
    public async Task AddLeagueToFavourites(int Id)
    {
        if (m_User is null)
            throw s_NoUserException;
        
        m_DbContext.UserFavouriteSeasons.Add(new UserHasFouvoriteSeasons() {UserId = m_User.Id, LeaueeId = Id});
        await m_DbContext.SaveChangesAsync();
    }
    
    public async Task RemoveLeagueFromFavourites(int Id)
    {
        if (m_User is null)
            throw s_NoUserException;
        
        if(m_DbContext.UserFavouriteSeasons.FirstOrDefault(s => s.User == m_User && s.LeaueeId == Id) is null)
            throw s_SeasonNotFoundException;

        m_DbContext.UserFavouriteSeasons.Remove(
            m_DbContext.UserFavouriteSeasons.First(s => s.User == m_User && s.LeaueeId == Id));
        await m_DbContext.SaveChangesAsync();
    }

    public List<int> GetFavouriteLeaguesForUser() => m_DbContext.UserFavouriteSeasons.Where(s => s.User == m_User).Select(s => s.LeaueeId).ToList();
}