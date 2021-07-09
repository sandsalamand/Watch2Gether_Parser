using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Security.Principal;


namespace StreamLabs_Helper
{
	class Server
	{
		static HttpListener _httpListener = new HttpListener();
		Thread _responseThread;
		const string responsePrefix = "<html><head><title>Watch2Gether Title Displayer</title></head><body>";
		const string responseSuffix = "</body></html>";
		const string defaultWelcome = responsePrefix + "<em>Server initializing.... If this takes longer than 10 seconds," +
			"then it's bugged. Check to make sure a youtube video is playing (not a built-in Blender short), then restart this app.</em>" + responseSuffix;
		private string responseString;
		private bool pause = false;
		private object threadLock = new object();

		public Server(string message = defaultWelcome)
		{
			responseString = message ?? defaultWelcome;
		}

		public bool StartServer(string mode)
		{
			Console.WriteLine("Starting server...");
			mode ??= "local";   //sets mode to local if null argument was supplied
			switch (mode)
			{
				case "local":
					_httpListener.Prefixes.Add("http://+:80/Temporary_Listen_Addresses/2525/");
					break;
				case "network":
					ProgramManager.Error("\n\n\n\n\n\n\nhello"); //debugging
					if (IsAdministrator())
					{
						try {
							_httpListener.Prefixes.Add("http://localhost:5000"); }
						catch {
							ProgramManager.Error("Failed to add prefix", fatal:true); }
					}
					else
						ProgramManager.Error("Program requires administrator to run in this mode.", fatal: true);
					break;
				default:
					ProgramManager.Error("Invalid mode argument");
					goto case "local";
			}
			try {
				_httpListener.Start(); // requires exe to be run as administrator
			} 
			catch {
				ProgramManager.Error("failed to start server", true);
			}
			Console.WriteLine("Server started.");
			_responseThread = new Thread(new ThreadStart(ResponseThread));
			_responseThread.Start(); // start the response thread
			return true;
			}

		void ResponseThread()
		{
			while (ProgramManager.ProgramStatus())
			{
				if (pause) //pauses the thread if pause is true
				{
					lock (threadLock)
					{
						Console.WriteLine("paused");
						Console.ReadKey();
						Monitor.Wait(threadLock);
					}
				}
				try
				{
					HttpListenerContext context = _httpListener.GetContext();
					byte[] _responseArray = Encoding.UTF8.GetBytes(responseString); // encodes string in UTF-8
					context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length);
					context.Response.KeepAlive = false;
					context.Response.Close();
					Console.WriteLine("Response given to a request.");
				}
				catch
				{
					Console.WriteLine("httpListener killed");
					ProgramManager.CloseProgram();
				}
			}
		}

		public void UpdateResponse(string response) //updates the reponse that the server will display
		{
			responseString = responsePrefix + response + responseSuffix;
		}

		public static bool IsAdministrator()
		{
			var identity = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		public void Close()
		{
			pause = true;
			_httpListener.Stop();
		}

		~Server() //called when instance is destroyed, just in case it's not destroyed properly
		{
			pause = true;
			_httpListener.Stop();
		}
	}
}
