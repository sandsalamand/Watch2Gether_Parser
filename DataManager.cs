using System.IO;
using System.Reflection;

namespace StreamLabs_Helper
{
	public class DataManager
	{
		private static string defaultUrl = "https://w2g.tv/rooms/challengerandymusic-h749vkhemnt5dmld09?lang=en";
		private static string prefFileName = "Preferences.txt";

		private static string preferenceFilePath;
		private static UserData userData;

		//constructor method
		static DataManager()
		{
			var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			var topLevelDir = System.IO.Directory.GetParent(exePath).FullName;
			preferenceFilePath = System.IO.Path.Combine(topLevelDir, prefFileName);

			if (File.Exists(preferenceFilePath))
				userData = XmlHelper.FromXmlFile<UserData>(preferenceFilePath);
			else
				SetupNew();
		}

		static private void SetupNew()
		{
			userData = new UserData
			{
				Prefs = new Preferences() {
					url = defaultUrl,
					mode = "local"
				}
			};
			XmlHelper.ToXmlFile(userData, preferenceFilePath);
		}
		
		public static void RefreshData()
		{
			userData = XmlHelper.FromXmlFile<UserData>(preferenceFilePath);
		}

		public static Preferences GetPreferences()
		{
			return userData.Prefs;
		}

		public static UserData GetUserData()
		{
			return userData;
		}

		public static void WriteDataToFile(UserData data)
		{
			userData = data;
			XmlHelper.ToXmlFile(userData, preferenceFilePath);
		}
	}
}
