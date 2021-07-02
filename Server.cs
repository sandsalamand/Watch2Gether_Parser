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
		const string temp = "<html><head><title>Localhost server -- port 5000</title></head>" +
				"<body>Welcome to the <strong>Localhost server</strong> -- <em>port 5000!</em></body></html>";
		string responseString;

		public Server(string message = temp)
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
			_httpListener.Start(); // start server (Run application as Administrator!)
			Console.WriteLine("Server started.");
			Thread _responseThread = new Thread(new ThreadStart(ResponseThread));
			_responseThread.Start(); // start the response thread
		}

		void ResponseThread()
		{
			while (true)
			{
				HttpListenerContext context = _httpListener.GetContext(); // get a context
																		  // Now, you'll find the request URL in context.Request.Url
				byte[] _responseArray = Encoding.UTF8.GetBytes(responseString); // get the bytes to response
				context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length); // write bytes to the output stream
				context.Response.KeepAlive = false; // set the KeepAlive bool to false
				context.Response.Close(); // close the connection
				Console.WriteLine("Response given to a request.");
			}
		}
	}
}
