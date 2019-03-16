using UnityEngine;
using UnityEditor;

namespace GameTaco
{

	public class TacoMenuEditor
	{
		[MenuItem("TacoTime/Clear PlayerPrefs")]
		private static void ClearOption()
		{
			PlayerPrefs.DeleteAll();
		}


		[MenuItem("TacoTime/End Game With Fake Score")]
		private static void EndGame()
		{
			TacoSetup.Instance.BroadcastMessage ("TacoPostScore", 1000);
		}
		/*
		[MenuItem("TacoTime/Quit Game")]
		private static void QuitGame()
		{
			TacoSetup.Instance.BroadcastMessage ("PromptToQuitGame", 1000);
		}

		[MenuItem("TacoTime/Simulate Game Crash/ Application quit")]
		private static void UnityDidExit()
		{
			TacoSetup.Instance.BroadcastMessage ("UnityDidExit", 1000);
		}
		*/
	}

}