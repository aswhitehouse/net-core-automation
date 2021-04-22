using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreAutomationUiCommon.core;

namespace NetCoreAutomationUiCommonTests.common.tests
{
    [TestClass]
    public class BrowserControllerSelectionTests
    {
        private readonly BrowserController _browserController;

        public BrowserControllerSelectionTests()
        {
            _browserController = new BrowserController();
        }

        [TestMethod]
        public void SelectAChromeBrowserInstance_ChromeStrategyIsCalled()
        {
            _browserController.SetBrowser(new ChromeBrowserController());
            var chromeBrowserController = _browserController.CreateBrowserController();
            Assert.IsInstanceOfType(chromeBrowserController, typeof(ChromeDriver));
        }
    }
}