using System;

namespace StreamLabs_Helper
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var parser = new WebParser();
			string foundText;
			foundText = parser.ParseUrl("https://w2g.tv/rooms/v3gjuklie0gy3pbec3?lang=en");
			Server server = new Server(foundText);
		}
	}
}
