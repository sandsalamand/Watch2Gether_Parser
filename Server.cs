using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace StreamLabs_Helper
{
	class Server : IDisposable
	{
		static volatile HttpListener _httpListener = new HttpListener();
		Thread _responseThread;
		private string responseString;
		private object threadLock = new object();
		private bool shouldRun = true;

		public Server(string message = null)
		{
			responseString = WebSite.FormSite(message);
		}

		public bool StartServer(string mode)
		{
			mode ??= "local";   //sets mode to local if null argument was supplied
			Console.WriteLine("Starting server in " + mode + " mode");

			switch (mode)
			{
				case "local":
					_httpListener.Prefixes.Add("http://+:80/Temporary_Listen_Addresses/2525/");
					break;
				case "network":
					if (IsAdministrator())
					{
						try
						{
							_httpListener.Prefixes.Add("http://localhost:5000/");
						}
						catch
						{
							ProgramManager.Error("Failed to add prefix, switching to run in local mode");
							goto case "local";
						}
					}
					else
					{
						ProgramManager.Error("Program requires administrator to run in this mode.", fatal: true);
						return false;
					}
					break;
				default:
					ProgramManager.Error("Invalid mode argument");
					goto case "local";
			}
			try {
				_httpListener.Start(); // requires exe to be run as administrator for network mode
			} 
			catch {
				ProgramManager.Error("failed to start server", true);
				return false;
			}
			Console.WriteLine("Server started.");
			_responseThread = new Thread(new ThreadStart(ResponseThread));
			_responseThread.Start(); // start the response thread
			return true;
			}

		void ResponseThread()
		{
			while (ProgramManager.ProgramStatus() && shouldRun)
			{
				try
				{
					if (_httpListener is not null)
					{
						HttpListenerContext context = _httpListener.GetContext();
						byte[] _responseArray = Encoding.UTF8.GetBytes(responseString); // encodes string in UTF-8
						context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length);
						context.Response.KeepAlive = false;
						context.Response.Close();
						Console.WriteLine("Response given to a request.");
					}
				}
				catch
				{
					Console.WriteLine("httpListener killed");
					ProgramManager.CloseProgram();
				}
			}
		}

		public void UpdateResponse(string newResponse) //updates the reponse that the server will display
		{
			responseString = WebSite.FormSite(newResponse);
		}

		public static bool IsAdministrator()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var identity = WindowsIdentity.GetCurrent();
				var principal = new WindowsPrincipal(identity);
				return principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			else
				return true;
		}

		public void Close()
		{
			shouldRun = false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}
			if (_httpListener == null)
			{
				return;
			}
			lock (threadLock)
			{
				if (_httpListener == null)
				{
					return;
				}
				_httpListener?.Stop();//double check
				_httpListener = null;
			}
		}
	}
}
