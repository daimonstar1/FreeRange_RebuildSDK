using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System;

namespace GameTaco
{
	public class BuildScript
	{
		private static string gameName = "GameTaco-Bubble Shooter";

		public static readonly bool isReleaseVersion = CheckReleaseVersion();
		public static readonly string buildPath = (!isReleaseVersion) ? "C:\\jenkins\\games\\staging" : "C:\\jenkins\\games\\production";
                public static readonly string macBuildPath = (!isReleaseVersion) ? "/Users/luatnguyen/Jenkins/games/staging" : "/Users/luatnguyen/Jenkins/games/production";
                 
		static bool CheckReleaseVersion() {
			string isRelease = string.Empty;
			string[] args = System.Environment.GetCommandLineArgs ();
			for (int i = 0; i < args.Length; i++) {
				if (args [i] == "-isProd") {
					isRelease = "true";
				}
			}
			if(!string.IsNullOrEmpty(isRelease)) {
				return true;
			}
			else
			{
				return false;
			}
		}

		static void MacUniversal()
		{
			var buildTarget = BuildTarget.StandaloneOSX;
			var localPathName = macBuildPath + "/MacOs/";
			BuildPlayerTaco(localPathName, buildTarget);
		}

		static void Win32()
		{
			var buildTarget = BuildTarget.StandaloneWindows;
			var localPathName = buildPath + "\\Win32\\";
			BuildPlayerTaco(localPathName, buildTarget);
		}

		static void Win64()
		{
			var buildTarget = BuildTarget.StandaloneWindows;
			var localPathName = buildPath + "\\Win64\\";
			BuildPlayerTaco(localPathName, buildTarget);
		}

		static void IOS()
		{
			var buildTarget = BuildTarget.iOS;
			var localPathName = macBuildPath + "/IOS/";
			BuildPlayerTaco(localPathName, buildTarget);
		}

    	public static void Android()
		{
			var buildTarget = BuildTarget.Android;
			var localPathName = buildPath + "\\Android\\";
			BuildPlayerTaco(localPathName, buildTarget);
		}

		static void BuildPlayerTaco(string localPath, BuildTarget buildTarget) {

			string[] levels = GetLevelsFromBuildSettings();
			if (levels.Length == 0)
			{
				Debug.Log("Nothing to build.");
				return;
			}

			if (!Directory.Exists(localPath))
				Directory.CreateDirectory(localPath);


			PlayerSettings.companyName = "GameTaco";
			PlayerSettings.productName = "Bubble Shooter";	

			//Setting For IOS
			PlayerSettings.bundleVersion = "1";
			PlayerSettings.iOS.buildNumber = "1.3.8";
			PlayerSettings.statusBarHidden = true;
			PlayerSettings.useAnimatedAutorotation = true;
			//PlayerSettings.applicationIdentifier = "com.GameTaco.ColorFlex";
			PlayerSettings.iPhoneBundleIdentifier = "com.3037.bubbles";
			PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
			PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Suspend;
			PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
			PlayerSettings.iOS.allowHTTPDownload = true;
			
			//Seting For Android
			localPath = localPath + GetBuildTargetName(buildTarget);

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.scenes = levels;
			buildPlayerOptions.locationPathName = localPath;
			buildPlayerOptions.target = buildTarget;
			#if UNITY_2018
				PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
			#else
				buildPlayerOptions.options = BuildOptions.Il2CPP;
			#endif
			if(!isReleaseVersion) {
				buildPlayerOptions.options = BuildOptions.Development;
			}
			BuildPipeline.BuildPlayer(buildPlayerOptions);

		}

		static string[] GetLevelsFromBuildSettings()
		{
			List<string> levels = new List<string>();
			for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
			{
				if (EditorBuildSettings.scenes[i].enabled)
					levels.Add(EditorBuildSettings.scenes[i].path);
			}

			return levels.ToArray();
		}

		static string GetBuildTargetName(BuildTarget target)
		{
			switch (target)
			{
			case BuildTarget.Android:
				return gameName +".apk";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return gameName + ".exe";
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.StandaloneOSX:
				return gameName + ".app";
			//case BuildTarget.WebPlayer:
			//case BuildTarget.WebPlayerStreamed:
			case BuildTarget.WebGL:
			case BuildTarget.iOS:
				return "";
				// Add more build targets for your own.
			default:
				Debug.Log("Target not implemented.");
				return null;
			}
		}
	}
}
