#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion



public static class GameDetails
{

	#region Private Static Variables

	private static string s_GameTitle = "Prison Break";
	private static string s_GameCatchline = "Power is a weed that only grows in the vacant space of an abandoned mind";
	private static string s_GameVersion = "v1.0.0";
	private static string s_Environment = "Development";
	private static List<string> s_LoadingStatusQuotes = new List<string>
	{
		"KIRRRRYU-CHAAAAAN!",
		"The man who gets beat down isn't the loser",
		"The guy who can't tough it out to the end, he's the one who loses",
		"In the yakuza life, there are no KO's.",
		"As long as I'm alive, I'll keep getting back up for more.",
		"Right, wrong.. Nobody's got a clue what the difference is in this town. "
	};

	#endregion

	#region Public Static Variables 

	public static string Title => s_GameTitle;
	public static string Catchline => s_GameCatchline;
	public static string Version => s_GameVersion;
	public static string Environment => s_Environment;

	public static List<string> LoadingStatusQuotes => s_LoadingStatusQuotes;

	#endregion

}