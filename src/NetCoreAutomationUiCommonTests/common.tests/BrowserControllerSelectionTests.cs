using NetCoreAutomationUiCommon.core;
using NUnit.Framework;

namespace NetCoreAutomationUiCommonTests.common.tests
{
    public class BrowserControllerSelectionTests
    {
        private readonly BrowserController _browserController;

        public BrowserControllerSelectionTests()
        {
            _browserController = new BrowserController();
        }

        [Test]
        public void SelectAChromeBrowserInstance_ChromeStrategyIsCalled()
        {
            _browserController.SetBrowser(new ChromeBrowserController());
            var chromeBrowserController = _browserController.CreateBrowserController();
            Assert.IsInstanceOf<ChromeDriver>(chromeBrowserController);
        }
    }
}