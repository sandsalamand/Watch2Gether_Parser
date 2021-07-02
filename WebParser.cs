using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace StreamLabs_Helper
{
	class WebParser
	{
		private static IWebDriver webDriver;
		Actions mouseActions;

		public WebParser()
		{
			webDriver = new EdgeDriver(@"C:\WebDrivers\bin\"); //
		}

		public void FindIFrameOnPage(string url)
		{
			WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
			webDriver.Navigate().GoToUrl(url);

			var button = webDriver.FindElement(By.CssSelector(".ui.green.button"));
			button.Click();     //find and click the button to join the watch2gether room

			Thread.Sleep(4000);
			var collection = webDriver.FindElements(By.ClassName("w2g-player-video"));
			var iframes = FindElementsFromElement(By.CssSelector("iframe"), collection.First());
			webDriver.SwitchTo().Frame(iframes.First());
		}

		public string GetIFrameTitle()
		{
			mouseActions = new Actions(webDriver);
			var mainVideo = FindElements(By.CssSelector("body")); //performance could be improved by caching this
			mouseActions.MoveToElement(mainVideo.First()); //make the watch later icon visible
			var mouseOverVideo = mouseActions.Build();
			mouseOverVideo.Perform();

			mouseActions = new Actions(webDriver);
			var watchLaterIcon = FindElements(By.ClassName("ytp-watch-later-icon"));
			mouseActions.MoveToElement(watchLaterIcon.First()); //make the title visible
			var mouseOverWatchLater = mouseActions.Build();
			mouseOverWatchLater.Perform();

			var title = FindElements(By.CssSelector(".ytp-chrome-top"));
			string titleText = title.First().Text;

			//returns the string before the newline (to remove "Watch Later" and "Share" from the string)
			var strArray = titleText.Split('\n');
			return strArray[0];		
		}

		//loop until the elements load
		private IReadOnlyCollection<IWebElement> FindElements(By by)
		{
			while (true)
			{
				var elements = webDriver.FindElements(by);

				if (elements.Count > 0)
					return elements;

				Thread.Sleep(10);
			}
		}

		//same as above, but search from specific node
		private IReadOnlyCollection<IWebElement> FindElementsFromElement(By by, IWebElement element)
		{
			while (true)
			{
				var elements = element.FindElements(by);

				if (elements.Count > 0)
					return elements;

				Thread.Sleep(10);
			}
		}

		public void Destroy()
		{
			webDriver.Quit();
		}
	}
}
