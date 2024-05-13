using System.Net;

namespace MPM_Betting.IntegrationTests;

public class AppHostTests
{
    private DistributedApplication m_App;
    
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.MPM_Betting_Aspire_AppHost>();

        m_App = await appHost.BuildAsync();
        await m_App.StartAsync();
    }

    [Test]
    public async Task TestBlazorConnection()
    {
        var httpClient = new HttpClientWithRetry(m_App.CreateHttpClient("blazor"));
        
        var response = await httpClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task TestApiConnection()
    {
        var httpClient = new HttpClientWithRetry(m_App.CreateHttpClient("api"));
        
        var response = await httpClient.GetAsync("/football/leagues");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task TestFunctionsConnection()
    {
        var httpClient = new HttpClientWithRetry(m_App.CreateHttpClient("functions"));
        
        var response = await httpClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task TestDbManagerConnection()
    {
        var httpClient = m_App.CreateHttpClient("dbmanager");
        
        var response = await httpClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task TestPrometheusConnection()
    {
        var httpClient = m_App.CreateHttpClient("prometheus");
        
        var response = await httpClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task TestGrafanaConnection()
    {
        var httpClient = m_App.CreateHttpClient("grafana");
        
        var response = await httpClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task TestMailDevConnection()
    {
        var httpClient = m_App.CreateHttpClient("maildev");
        
        var response = await httpClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public void TestSqlConnection()
    {
        var endpoint = m_App.GetEndpoint("sql", "tcp");
        Assert.That(TcpConnectionTester.TestConnection(endpoint.Host, endpoint.Port), Is.True);
    }
    
    [Test]
    public void TestRedisConnection()
    {
        var endpoint = m_App.GetEndpoint("redis", "tcp");
        Assert.That(TcpConnectionTester.TestConnection(endpoint.Host, endpoint.Port), Is.True);
    }
    
    [Test]
    public void TestAzuriteConnection()
    {
        var blob = m_App.GetEndpoint("azurite", "blob");
        var queue = m_App.GetEndpoint("azurite", "queue");
        var table = m_App.GetEndpoint("azurite", "table");
        Assert.Multiple(() =>
        {
            Assert.That(TcpConnectionTester.TestConnection(blob.Host, blob.Port), Is.True);
            Assert.That(TcpConnectionTester.TestConnection(queue.Host, queue.Port), Is.True);
            Assert.That(TcpConnectionTester.TestConnection(table.Host, table.Port), Is.True);
        });
    }
}