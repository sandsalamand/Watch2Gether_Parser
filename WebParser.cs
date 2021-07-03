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
		private MoveDirection moveDirection;

		private enum Direction
		{
			Right = 1,
			Left = -1,
			None = 0
		}

		private struct MoveDirection
		{
			private Direction realValue;

			public Direction Direction
			{
				get    //toggle the direction on every get
				{
					var beforeInvert = realValue;
					realValue = (Direction)(-1 * (int)realValue);
					return beforeInvert;
				}
				set { realValue = value; }
			}
		}

		public WebParser()
		{
			webDriver = new EdgeDriver(@"C:\WebDrivers\bin\");  //TODO: change to same directory as project files
			moveDirection.Direction = Direction.Right;
		}

		public bool FindIFrameOnPage(string url)
		{
			WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
			try {
				webDriver.Navigate().GoToUrl(url);
			}
			catch {
				Console.WriteLine("URL does not exist");
				return false;
			}

			var button = webDriver.FindElement(By.CssSelector(".ui.green.button"));
			button.Click();     //find and click the button to join the watch2gether room

			Thread.Sleep(4000);
			var collection = webDriver.FindElements(By.ClassName("w2g-player-video"));
			var iframes = FindElementsFromElement(By.CssSelector("iframe"), collection.First());
			webDriver.SwitchTo().Frame(iframes.First());

			mouseActions = new Actions(webDriver);
			var mainVideo = FindElements(By.CssSelector("body"));
			mouseActions.MoveToElement(mainVideo.First());
			mouseActions.Build().Perform();
			return true;
		}

		public string GetIFrameTitle()
		{
			//moves the mouse back and forth to keep the youtube title visible
			mouseActions = new Actions(webDriver);
			mouseActions.MoveByOffset(10 * ((int)moveDirection.Direction), 0);
			mouseActions.Build().Perform();

			var title = FindElements(By.CssSelector(".ytp-chrome-top"));
			string titleText = title.First().Text;

			//returns the string before the newline to remove "Watch Later" and "Share" from the string
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

		~WebParser()
		{
			webDriver.Quit();
		}
	}
}
