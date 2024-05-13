using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Moq;
using MPM_Betting.Services;
using MPM_Betting.Services.Data;

namespace MPM_Betting.UnitTests.Data;

[TestFixture]
[TestOf(typeof(FootballApi))]
public class FootballApiTest
{
    private FootballApi m_FootballApi;
    
    [SetUp]
    public void Setup()
    {
        var loggerFactory = new LoggerFactory();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var distributedCache = new Mock<IDistributedCache>();
        var store = new Dictionary<string, byte[]>();
        distributedCache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((key, token) =>
                Task.FromResult(store.GetValueOrDefault(key)));
        distributedCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, value, options, token) =>
                store[key] = value)
            .Returns(Task.CompletedTask);
        var logger = new Logger<MpmCache>(loggerFactory);
        var httpClient = new HttpClient();
        var mpmCache = new MpmCache(memoryCache, distributedCache.Object, logger, httpClient);
        var footballLogger = new Logger<FootballApi>(loggerFactory);
        m_FootballApi = new FootballApi(footballLogger, mpmCache);
    }
    
    [Test]
    public async Task TestGetLeagues()
    {
        var result = await m_FootballApi.GetAllFootballLeagues();
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetTeams()
    {
        var result = await m_FootballApi.GetAllFootballTeams();
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetPlayers()
    {
        var result = await m_FootballApi.GetAllFootballPlayers();
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetTable()
    {
        var result = await m_FootballApi.GetLeagueTable(42, null);
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Table, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetGames()
    {
        var result = await m_FootballApi.GetGameEntries(42, null);
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetGameDetails()
    {
        var result = await m_FootballApi.GetGameDetails(4446289);
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
    }
    
    [Test]
    public async Task TestGetTeamsFromLeague()
    {
        var result = await m_FootballApi.GetTeamsFromLeague(42);
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetPlayersFromTeam()
    {
        var result = await m_FootballApi.GetPlayersFromTeam(42);
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetTableWithSeason()
    {
        var result = await m_FootballApi.GetLeagueTable(42, "2021");
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Table, Is.Not.Empty);
    }
    
    [Test]
    public async Task TestGetGamesWithDate()
    {
        var result = await m_FootballApi.GetGameEntries(42, new DateOnly(2021, 12, 12));
        Assume.That(result.IsSuccess, Is.True);
        Assume.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Not.Empty);
    }
}