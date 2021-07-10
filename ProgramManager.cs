using System;
using System.Timers;
using System.Threading;

namespace StreamLabs_Helper
{
	static class ProgramManager
	{
		static System.Timers.Timer timer;
		static WebParser parser;
		static Server server;
		const double timerInterval = 3000;
		static string url = "https://w2g.tv/rooms/lf8nb4ap6btrxj7kd0?lang=en";
		private static bool stayOpen = true;
		private static bool cleanUpCalled = false;
		public delegate WebParser.ParsingStatus ParsingAction();
		static ParsingAction parsingAction;

		static public void RunProgram(string[] args)
		{
			server = new Server();
			string serverParams = "local";
			parsingAction = new ParsingAction(ParseAndUpdateServer);

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

			if (!server.StartServer(serverParams)) //default to using temporary listeners (local mode)
			{
				Error("Server failed to start.", true);
				return;
			}
			parser = new WebParser();
			switch (parser.FindIFrameOnPage(url))
			{
				case WebParser.ParsingStatus.Success:
					MainLoop();
					break;
				case WebParser.ParsingStatus.Failure:
					parsingAction += new ParsingAction(FindIFrame);   //TODO: remove this from delegate once it is found
					MainLoop();
					break;
				case WebParser.ParsingStatus.FatalError:
					goto default;
				default:
					CloseProgram();
					return;
			}
			CloseProgram();
			return;
		}

		static void MainLoop()
		{
			Console.WriteLine("\nProgram started. Press X to safely terminate\n");
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

		static WebParser.ParsingStatus FindIFrame()
		{
			var result = parser.FindIFrameOnPage(url);
			if (result == WebParser.ParsingStatus.Success)
				parsingAction -= new ParsingAction(FindIFrame);    //not sure if this works, might have to store a reference to the FindIFrame Action or call from outside this func
			return result;
		}

		static WebParser.ParsingStatus ParseAndUpdateServer()
		{
			string foundText;
			try
			{
				foundText = parser.GetIFrameTitle();
				server.UpdateResponse(foundText);
				return WebParser.ParsingStatus.Success;
			}
			catch
			{
				Error("parser failed");
				return WebParser.ParsingStatus.Failure;
			}
		}

		public static void Error(string message = "unspecified error", bool fatal = false)
		{
			Console.WriteLine(message + "\n");
			if (fatal)
			{
				CloseProgram();
				Console.WriteLine("A fatal error has occurred. Press any key to close the program...");
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
		}

		static void CleanUp()
		{
			cleanUpCalled = true;
			if (server is not null)
				server.Dispose();
			if (parser is not null)
				parser.Destroy();
			if (timer is not null)
			{
				timer.Stop();
				timer.Close();
			}
		}
	}
}
