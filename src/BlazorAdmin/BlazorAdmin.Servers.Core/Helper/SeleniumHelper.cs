using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace BlazorAdmin.Core.Helper
{
    public static class SeleniumHelper
    {
        // var author = driver.FindElement(By.CssSelector("a[rel='author']"))?.Text;
        // driver.Quit()
        public static EdgeDriver InitialEdgeDriver(string url, bool isHeadless = false)
        {
            var edgeOptions = new EdgeOptions();
            edgeOptions.PageLoadStrategy = PageLoadStrategy.Eager; // 或者 

            if (isHeadless)
            {
                edgeOptions.AddArgument("--headless");
                edgeOptions.AddArgument("--disable-gpu");
            }

            var driver = new EdgeDriver(edgeOptions);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(5000);

            driver.Navigate().GoToUrl(url);

            IWindow window = driver.Manage().Window;
            window.Maximize();

            return driver;
        }
    }
}
