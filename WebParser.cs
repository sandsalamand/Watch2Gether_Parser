using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using Microsoft.Edge.SeleniumTools;
using System.Diagnostics;
using System.IO;

namespace StreamLabs_Helper
{
	class WebParser
	{
		private static IWebDriver webDriver;
		Actions mouseActions;
		private MoveDirection moveDirection;
		private const string backSlash = "\\";
		private string driverDirectory;
		private string[] edgeArguments =  new string[]	{
			"headless", "disable-gpu", "window-size=1600,1200", "disable-extensions", "mute-audio", "enable-logging=false" };

		public enum ParsingStatus
		{
			Success = 1,
			Failure = 0,
			FatalError = -1
		}

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
			driverDirectory = GetApplicationExecutableDirectoryName() + backSlash + "Drivers";
			EdgeOptions edgeOptions = new EdgeOptions();
			edgeOptions.UseChromium = true;
			edgeOptions.AddArguments(edgeArguments);
			webDriver = new Microsoft.Edge.SeleniumTools.EdgeDriver(driverDirectory, edgeOptions);
			moveDirection.Direction = Direction.Right;
		}

		public ParsingStatus FindIFrameOnPage(string url)
		{
			WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
			try {
				webDriver.Navigate().GoToUrl(url);
			}
			catch {
				ProgramManager.Error("URL does not exist", true);
				return ParsingStatus.FatalError;
			}
			try
			{
				var button = webDriver.FindElement(By.CssSelector(".ui.green.button"));
				if (button is null)
					return ParsingStatus.FatalError;
				button.Click();     //find and click the button to join the watch2gether room
			}
			catch {
				ProgramManager.Error("not an accessible Watch2Gether url");
				return ParsingStatus.Failure;
			}
			try {
				var collection = webDriver.FindElements(By.ClassName("w2g-player-video"));
				var iframes = FindElementsFromElement(By.CssSelector("iframe"), collection.First());
				webDriver.SwitchTo().Frame(iframes.First());

				mouseActions = new Actions(webDriver);
				var mainVideo = FindElements(By.CssSelector("body"));
				mouseActions.MoveToElement(mainVideo.First());
				mouseActions.Build().Perform();
			}
			catch {
				ProgramManager.Error("failed to parse");
				return ParsingStatus.Failure;
			}
			return ParsingStatus.Success;
		}

		public string GetIFrameTitle()
		{
			try
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
			catch
			{
				ProgramManager.Error("failed to get iframe");
				return null;
			}
		}

		//loop until the elements load
		private IReadOnlyCollection<IWebElement> FindElements(By by)
		{
			int callCounter = 0;
			while (webDriver is not null && callCounter < 8000) //should time out after 8 sec
			{
				var elements = webDriver.FindElements(by);

				if (elements.Count > 0)
					return elements;

				Thread.Sleep(10);
				callCounter++;
			}
			return null;
		}

		//same as above, but search from specific node
		private IReadOnlyCollection<IWebElement> FindElementsFromElement(By by, IWebElement element)
		{
			int callCounter = 0;
			while (webDriver is not null && callCounter < 8000) //should time out after 8 sec
			{
				var elements = element.FindElements(by);

				if (elements.Count > 0)
					return elements;

				Thread.Sleep(10);
				callCounter++;
			}
			return null;
		}

		private string GetApplicationExecutableDirectoryName()
		{
			return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
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
