using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace MPM_Betting.Scraper.Football;

public class FootballNavigator
{
    private const string FotmobBaseUrl = "https://www.fotmob.com/en-GB";
    private readonly WebDriver m_Driver;
    
    public FootballNavigator()
    {
        m_Driver = new ChromeDriver();
        m_Driver.Navigate().GoToUrl(FotmobBaseUrl);
        
        // Accept Cookie stuff
        try
        {
            var element = m_Driver.FindElement(By.XPath("//p[@class='fc-button-label' and text()='Consent']"));
            element.Click();
        }
        catch (NoSuchElementException)
        {
            Console.WriteLine("Consent element not found");
        }
    }
    
    private void NavigateToLeague(League league)
    {
        m_Driver.Navigate().GoToUrl($"{FotmobBaseUrl}/leagues/{(int)league}");
    }

    /// <summary>
    /// Scrapes all league pages for league table stats
    /// </summary>
    /// <returns> - </returns>
    public void GetLeagueData()
    {
        foreach (var league in Enum.GetValues<League>())
        {
            NavigateToLeague(league);
            if (CheckForTableLoad()) continue;
            
            var element = m_Driver.FindElement(By.XPath("//article[@class='TableContainer']"));
            Console.WriteLine(element.Text);
        }
    }

    /// <summary>
    /// Checks if the "TableContainer" element has finished loading
    /// Timeout of 10 seconds
    /// </summary>
    /// <returns> true if table was found </returns>
    private bool CheckForTableLoad()
    {
        try
        {
            var wait = new WebDriverWait(m_Driver, TimeSpan.FromSeconds(10))
            {
                Message = $"League page did not finish loading within 10 seconds. Skipping...",
                PollingInterval = TimeSpan.FromSeconds(1)
            };
            wait.Until(driver =>
            {
                try
                {
                    m_Driver.FindElement(By.XPath("//article[@class='TableContainer']"));
                    return true;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });
        }
        catch (WebDriverTimeoutException)
        {
            Console.WriteLine($"League page did not finish loading within 10 seconds. Skipping...");
            return true;
        }

        return false;
    }
}
