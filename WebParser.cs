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
		public string ParseUrl(string url)
		{
			WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
			webDriver.Navigate().GoToUrl(url);

			var button = webDriver.FindElement(By.CssSelector(".ui.green.button"));
			button.Click();

			Thread.Sleep(4000);
			var collection = webDriver.FindElements(By.ClassName("w2g-player-video"));
			var iframe = FindElementsFromElement(By.CssSelector("iframe"), collection.First());
			webDriver.SwitchTo().Frame(iframe.First());
			var title = FindElements(By.CssSelector(".ytp-chrome-top"));

			string titleText = title.First().Text;
			var strArray = titleText.Split('\n');
			return strArray[0]; //return the string before the newline

			//wait.Until(webDriver => webDriver.FindElement(By.ClassName("ytp-title-text")).Displayed);

			//foreach (var webElement in text)
			//{
			//	return webElement.Text;
			//}
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
