Feature: Browser Instance Type Control

Scenario: Initialise the browser controller with a Chrome Browser object
	Given a Browser Controller object
	When I call Set Browser with an Instance of ChromeBrowserController
	Then the underlying browser object type is Chrome