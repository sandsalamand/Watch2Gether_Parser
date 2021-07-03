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
		const string url = "https://w2g.tv/rooms/3hz9u1a99k0twpzyln?lang=en";

		static void Main(string[] args)
		{
			server = new Server();
			parser = new WebParser();
			if (parser.FindIFrameOnPage(url))
			{
				Thread.Sleep(3000);
				Set_Timer(timerInterval);
				while (Console.ReadKey().Key != ConsoleKey.F8)
				{
					//keep the program alive
				}
			}
			CleanUp();
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
				Console.WriteLine("parser failed");
			}
		}

		static void CleanUp()
		{
			server.Close();
			parser.Destroy();
			timer.Stop();
			timer.Close();
		}
	}
}
