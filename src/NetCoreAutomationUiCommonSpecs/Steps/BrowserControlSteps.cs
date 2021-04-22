using NetCoreAutomationUiCommon.core;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace NetCoreAutomationUiCommonSpecs.Steps
{
    [Binding]
    public sealed class BrowserControlSteps
    {
        private BrowserController _browserController;

        [Given(@"a Browser Controller object")]
        public void GivenABrowserControllerObject()
        {
            _browserController = new BrowserController();
        }

        [When(@"I call Set Browser with an Instance of ChromeBrowserController")]
        public void WhenICallSetBrowserWithAnInstanceOfChromeBrowserController()
        {
            _browserController.SetBrowser(new ChromeBrowserController());
        }

        [Then(@"the underlying browser object type is Chrome")]
        public void ThenTheUnderlyingBrowserObjectTypeIsChrome()
        {
            Assert.IsInstanceOf<ChromeDriver>(_browserController.CreateBrowserController());
        }
    }
}