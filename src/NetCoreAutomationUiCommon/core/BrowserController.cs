using NetCoreAutomationUiCommon.core.Contracts;

namespace NetCoreAutomationUiCommon.core
{
    public class BrowserController
    {
        private IBrowserController _browserController;

        public BrowserController()
        {
        }

        public BrowserController(IBrowserController browserController)
        {
            _browserController = browserController;
        }

        public void SetBrowser(IBrowserController browserController)
        {
            _browserController = browserController;
        }

        public IWebDriver CreateBrowserController()
        {
            return _browserController.CreateBrowser();
        }
    }
}