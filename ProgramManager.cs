using System;
using System.Timers;
using System.Threading.Tasks;

namespace StreamLabs_Helper
{
	static class ProgramManager
	{
		static string url;
		static System.Timers.Timer timer;
		static WebParser parser;
		static Server server;
		public const double timerInterval = 3000;
		private static bool stayOpen = true;
		private static bool cleanUpCalled = false;
		private delegate Task<WebParser.ParsingStatus> ParsingAction();
		static ParsingAction parsingAction;
		static ParsingAction parseTitle;
		static ParsingAction findIframe;

		static public void RunProgram(string[] args)
		{
			using (server = new Server())
			{
				parseTitle = new ParsingAction(ParseAndUpdateServer);
				UserData userDataCopy = DataManager.GetUserData();
				string serverParams = userDataCopy.Prefs.mode;
				url = userDataCopy.Prefs.url;
				switch (args.Length)
				{
					case 0:
						break;
					case 1:
						serverParams = args[0];
						break;
					case 2:
						url = args[1];
						goto case 1;
					default:
						break;
				}
				if (!server.StartServer(serverParams))
				{
					Error("Server failed to start.", true);
					CloseProgram();
					return;
				}
				parser = new WebParser();
				switch (FindIFrameDontUpdateDelegate().Result)
				{
					case WebParser.ParsingStatus.Success:
						Print("success");
						parsingAction += parseTitle;
						MainLoop();
						break;
					case WebParser.ParsingStatus.Failure:   //if parser failed to find the iframe, add iframe-parsing to the list of actions
						Print("Failed to find an active youtube video on the url " + url + ". Close Program? y/n");
						if (Console.ReadLine().Contains('y'))
						{
							CloseProgram();
							return;
						}
						findIframe = new ParsingAction(FindIFrame);
						parsingAction += findIframe;
						MainLoop();
						break;
					case WebParser.ParsingStatus.FatalError:
						CloseProgram();
						break;
				}
				if (stayOpen)   //close program if not already told to
					CloseProgram();
			}
			return;
		}

		static void MainLoop()
		{
			Print("\nProgram started. Press X to safely terminate\n");
			Set_Timer(timerInterval);
			while (Console.ReadKey().Key != ConsoleKey.X && stayOpen)
			{
				//keep the program alive
			}
		}

		static void Set_Timer(double interval)
		{
			timer = new System.Timers.Timer(interval);
			timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
			timer.Start();
		}

		static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			timer.Stop();
			parsingAction.Invoke();
			Set_Timer(timerInterval);
		}

		static async Task<WebParser.ParsingStatus> FindIFrameDontUpdateDelegate()
		{
			var result = await parser.FindIFrameOnPage(url);
			Print("iframeResult: " + result);
			return result;
		}

		static async Task<WebParser.ParsingStatus> FindIFrame()
		{
			var result = await parser.FindIFrameOnPage(url);
			Print("iframeResult: " + result);
			if (result == WebParser.ParsingStatus.Success && parsingAction.GetInvocationList()[0].Method.Name == findIframe.Method.Name)
			{
				parsingAction -= findIframe;
				parsingAction += parseTitle;	//if iframe parsing succeeds, replace iframe checking with title checking in main loop
			}
			return result;
		}

		static async Task<WebParser.ParsingStatus> ParseAndUpdateServer()
		{
			try
			{
				string foundText = await parser.GetIFrameTitle();
				if (foundText is null) {
					return WebParser.ParsingStatus.Failure;
				}
				Print("foundText: " + foundText);
				server.UpdateResponse(foundText);
				return WebParser.ParsingStatus.Success;
			}
			catch
			{
				Error("parser failed");
				return WebParser.ParsingStatus.Failure;
			}
		}

		public static void Print(string message)
		{
			Console.WriteLine((message ?? "empty message") + Environment.NewLine);
		}

		public static void Error(string message = "unspecified error", bool fatal = false)
		{
			Print(message + "\n");
			if (fatal)
			{
				CloseProgram();
				Print("A fatal error has occurred. Press any key to close the program...");
				Console.ReadKey();
			}
		}

		public static bool ProgramStatus()
		{
			return stayOpen;
		}

		public static void CloseProgram()
		{
			stayOpen = false;
			if (!cleanUpCalled)
				CleanUp();
			//Environment.Exit(0);
			Print("\nThe Program Has Successfully Closed. You can close this window safely.\n");
		}

		static void CleanUp()
		{
			cleanUpCalled = true;
			server?.Close();
			server?.Dispose();
			parser?.Destroy();
			if (timer is not null)
			{
				timer.Stop();
				timer.Close();
			}
		}
	}
}
