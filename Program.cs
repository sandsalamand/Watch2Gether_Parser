using System;
using System.Timers;
using System.Threading;

namespace StreamLabs_Helper
{
	class Program
	{
		static System.Timers.Timer timer;
		static WebParser parser;
		static Server server;
		const double timerInterval = 3000;
		static string url = "https://w2g.tv/h749vkhemnt5dmld09";
		private static bool stayOpen = true;
		private static bool cleanUpCalled = false;

		static void Main(string[] args)
		{
			server = new Server();
			string serverParams = "local";

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
			if (parser.FindIFrameOnPage(url))
			{
				Console.WriteLine("\nProgram started. Press X to safely terminate\n");
				Set_Timer(timerInterval);
				while (Console.ReadKey().Key != ConsoleKey.X && stayOpen)
				{
					//keep the program alive
				}
			}
			if (!cleanUpCalled)
				CloseProgram();
			return;
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
			ParseAndUpdateServer();
			Set_Timer(timerInterval);
		}

		static void ParseAndUpdateServer()
		{
			string foundText;
			try
			{
				foundText = parser.GetIFrameTitle();
				server.UpdateResponse(foundText);
			}
			catch
			{
				Error("parser failed");
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
			CleanUp();
		}

		static void CleanUp()
		{
			cleanUpCalled = true;
			server.Close();
			parser.Destroy();
			timer.Stop();
			timer.Close();
		}
	}
}
