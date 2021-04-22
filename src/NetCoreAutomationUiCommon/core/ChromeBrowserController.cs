using NetCoreAutomationUiCommon.core.Contracts;

namespace NetCoreAutomationUiCommon.core
{
    public class ChromeBrowserController : IBrowserController
    {
        public IWebDriver CreateBrowser()
        {
            return new ChromeDriver();
        }
    }

    public class ChromeDriver : IWebDriver
    {
    }
}