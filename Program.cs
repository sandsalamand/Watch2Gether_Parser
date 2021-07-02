using System;
using System.Timers;

namespace StreamLabs_Helper
{
	class Program
	{
		static Timer timer;
		static WebParser parser;
		static Server server;
		const double timerInterval = 8000;
		static void Main(string[] args)
		{
			bool shouldRun = true;
			server = new Server();
			parser = new WebParser();
			parser.FindIFrameOnPage("https://w2g.tv/rooms/v3gjuklie0gy3pbec3?lang=en");
			Set_Timer(timerInterval);
			while (shouldRun)
			{
				
			}
		}
		static void Set_Timer(double interval)
		{
			timer = new Timer(interval);
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
			foundText = parser.GetIFrameTitle();
			server.UpdateResponse(foundText);
		}
	}
}
