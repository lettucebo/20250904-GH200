using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace SimpleWeb.UITest
{
    [TestClass]
    public class EdgeDriverTest
    {
        // In order to run the below test(s), 
        // please follow the instructions from https://docs.microsoft.com/en-us/microsoft-edge/webdriver-chromium
        // to install Microsoft Edge WebDriver.

        private EdgeDriver _driver;
        private readonly string _appUrl = "https://www.bing.com/";

        [TestInitialize]
        public void EdgeDriverInitialize()
        {
            // Initialize edge driver 
            var options = new EdgeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Normal
            };
            _driver = new EdgeDriver(options);
        }

        [TestMethod]
        public void VerifyPageTitle()
        {
            // Replace with your own test logic
            _driver.Navigate().GoToUrl(_appUrl + "/");
            _driver.FindElement(By.Id("sb_form_q")).SendKeys("Azure Pipelines");
            _driver.FindElement(By.Id("sb_form_q")).Submit();
            Assert.IsTrue(_driver.Title.Contains("Azure Pipelines"), "Verified title of the page");

        }

        [TestCleanup]
        public void EdgeDriverCleanup()
        {
            _driver.Quit();
        }
    }
}
