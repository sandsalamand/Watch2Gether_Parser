using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace StreamLabs_Helper
{
	class WebParser
	{
		private static IWebDriver webDriver;

		public WebParser()
		{
			webDriver = new EdgeDriver(@"C:\WebDrivers\bin\");

		}
		public void FindIFrameOnPage(string url)
		{
			WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
			webDriver.Navigate().GoToUrl(url);

			var button = webDriver.FindElement(By.CssSelector(".ui.green.button"));
			button.Click();

			Thread.Sleep(4000);
			var collection = webDriver.FindElements(By.ClassName("w2g-player-video"));
			var iframes = FindElementsFromElement(By.CssSelector("iframe"), collection.First());
			webDriver.SwitchTo().Frame(iframes.First());
		}

		public string GetIFrameTitle()
		{
			var title = FindElements(By.CssSelector(".ytp-chrome-top"));

			string titleText = title.First().Text;
			var strArray = titleText.Split('\n');
			return strArray[0]; //return the string before the newline
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
	}
}
