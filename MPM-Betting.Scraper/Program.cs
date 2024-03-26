
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

WebDriver driver = new ChromeDriver();
driver.Navigate().GoToUrl("https://www.google.at");
var element = driver.FindElement(By.TagName("h1"));
Console.WriteLine(element.Text);