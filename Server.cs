using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace StreamLabs_Helper
{
	class Server
	{
		static HttpListener _httpListener = new HttpListener();
		Thread _responseThread;
		const string defaultWelcome = "<html><head><title>Localhost server -- port 5000</title></head>" +
				"<body>Welcome to the <strong>Localhost server</strong> -- <em>port 5000!</em></body></html>";
		private string responseString;
		private bool pause = false;
		private object threadLock = new object();

		public Server(string message = defaultWelcome)
		{
			if (message != null)
			{
				responseString = message;
			}
			StartServer();
		}

		void StartServer()
		{
			Console.WriteLine("Starting server...");
			_httpListener.Prefixes.Add("http://localhost:5000/"); // add prefix "http://localhost:5000/"
			_httpListener.Start(); // requires exe to be run as administrator
			Console.WriteLine("Server started.");
			_responseThread = new Thread(new ThreadStart(ResponseThread));
			_responseThread.Start(); // start the response thread
		}

		void ResponseThread()
		{
			while (true)
			{
				if (pause) //pauses the thread if pause is true
				{
					lock (threadLock)
					{
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
				}
			}
		}

		public void UpdateResponse(string response) //updates the reponse that the server will display
		{
			responseString = response;
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
