using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.IO;

namespace SingularAdjaraTest
{
    class Program
    {
        public static By username = By.CssSelector("input[data-id='username']");
        public static By password = By.CssSelector("input[data-id='password']");
        public static By loginButton = By.CssSelector("button[data-id='login-btn']");
        public static By liveCasino = By.CssSelector("a[href*='/Casino']");
        public static By adjJackpotRoulette = By.CssSelector("div[alt='Adjarabet Jackpot Roulette']");
        public static By lastWinNum = By.CssSelector("div[class*='LastNumbers--button-group'] span[class*='LastNumbers']");
        public static By winNumWrapper = By.CssSelector("span[class*='Label WinningNumber']>span");
        public static By iFrameElement = By.CssSelector("iframe[src*='https://sngroulette.adjarabet.com/jackpot-live-roulette/']");
        public static By newRound = By.CssSelector("span[class*='Progress__text']>span");
        public static IWebDriver driver = new ChromeDriver();
        public static WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMinutes(10));
        public static WebDriverWait waitForThreeSec = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
        public static IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

        static void Main(string[] args)
        {
            try
            {
                /*
                 * Parsing txt document that contains username and password with the following format:
                 * Username: ***
                 * Password: ***
                 */
                string[] lines = System.IO.File.ReadAllLines(@"userCredentials.txt");
                var user = new User();

                user.Username = lines[0].Substring(lines[0].IndexOf("Username: ") + 10);
                user.Password = lines[1].Substring(lines[1].IndexOf("Password: ") + 10);

                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                OpenSiteAndLogin(user);
                GoToLiveCasinoPage();
                OpenGame();

                while (true)
                {
                    CheckNumbers();
                    Console.WriteLine(DateTime.UtcNow + " Passed");
                    WaitForNewRound();
                }
            }
            catch (Exception e)
            {
                driver.Close();
                Console.WriteLine(e);
            }

        }
        private static void OpenSiteAndLogin(User user)
        {
            driver.Navigate().GoToUrl("https://www.adjarabet.com/ka");
            driver.FindElement(username).SendKeys(user.Username);
            driver.FindElement(password).SendKeys(user.Password);
            WaitAndClick(driver, loginButton, waitForThreeSec);
        }
        private static void GoToLiveCasinoPage()
        {
            driver.FindElements(liveCasino)[1].Click();

        }
        private static void OpenGame()
        {
            js.ExecuteScript("arguments[0].click();", driver.FindElement(adjJackpotRoulette));
            wait.Until(ExpectedConditions.ElementIsVisible(iFrameElement));
            driver.SwitchTo().Frame(driver.FindElement(iFrameElement));
        }
        private static void CheckNumbers()
        {
            wait.Until(ExpectedConditions.ElementExists(winNumWrapper));
            int lastWinNumberPopup = Convert.ToInt16(driver.FindElements(winNumWrapper)[1].Text);
            Console.Write($"Popup Num: {lastWinNumberPopup}  ");
            wait.Until(ExpectedConditions.ElementIsVisible(lastWinNum));
            int lastWinNumStats = Convert.ToInt16(driver.FindElements(lastWinNum)[0].Text);
            Console.WriteLine($"Stat Num: {lastWinNumStats}  ");
            Assert.AreEqual(lastWinNumStats, lastWinNumberPopup);
        }
        private static void WaitForNewRound()
        {
            wait.Until(ExpectedConditions.ElementExists(newRound));

        }
        private static void WaitAndClick(IWebDriver driver, By by, WebDriverWait wait)
        {
            wait.Until(ExpectedConditions.ElementIsVisible(by));
            driver.FindElement(by).Click();
        }
    }
}
