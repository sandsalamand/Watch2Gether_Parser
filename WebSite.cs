using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamLabs_Helper
{
	static class WebSite
	{
		static private int updateInterval = 3;    //change to use WebSite as timer authority
		public static string defaultMessage { get; } = "<em>Server initializing.... If this takes longer than 10 seconds," +
			"then it's bugged. Check to make sure a youtube video is playing (not a built-in Blender short), then restart this app.</em>";
		private static HtmlStruct SiteHtml;
		private static string cssText = @"<style>
		body {
			font-size: 5vw;
		}
		</style>";

		struct HtmlStruct
		{
			public string head;
			public string bodyPrefix;
			public string bodySuffix;
		}

		static WebSite()
		{
			ProgramManager.Print("updateInterval: " + updateInterval);
			FormHtml();
		}

		public static string FormSite(string message)
		{
			if (message is not null)
				return (SiteHtml.head + SiteHtml.bodyPrefix + message + SiteHtml.bodySuffix);
			else
				return defaultMessage;
		}

		private static void FormHtml()
		{
			SiteHtml.head = FormHead(updateInterval, cssText);
			SiteHtml.bodyPrefix = "<body>";
			SiteHtml.bodySuffix = "</body></html>";
		}

		private static string FormHead(int refreshRate, string css)
		{
			return "<html><head><title>Watch2Gether Title Displayer</title>" + css + "<meta http-equiv=\"refresh\" content=\""
				 + refreshRate.ToString() + "\"></head><body>";
		}
	}
}
