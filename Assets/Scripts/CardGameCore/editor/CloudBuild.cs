using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace FRG.Taco
{
    /// <summary>
    /// Was used as solution to upload build .ipa file to our FTP but turns out
    /// the link unity sends is also iOS install link like ours, so this is not needed so far.
    /// Still may weant to use it later.
    /// </summary>
    public class CloudBuild : MonoBehaviour
    {
        private class FtpUploadData
        {
            // ftp related
            public string FtpServerRootDirPath = "ftp://develop.freerangegames.com/develop.freerangegames.com";
            public string GameDirName;
            public string FtpUsername = "purposely_left_out";
            public string FtpPassword = "purposely_left_out";
            public string Platform;

            // resolved on runtime
            public string HttpRoot = "https://develop.freerangegames.com";
            public string FtpRevisionDirPath;
            public string FtpIpaDirPath;
            public string FileName;
            public string RadomDirName;
            public int FileSizeBytes;
            public string PathToBuildResult;
            public string FtpIpaFilePath;
            public string FtpPlistPath;
        }

        private class GameBuildData
        {
            // resolved from cloud build manifest
            public string ProjectId;
            public DateTime Date;
            public string BundleId;
            public string SvnRevision;

            // resolved on runtime
            public string IpaUrl;
            public string PlistUrl;
        }

        private static GameBuildData buildData = new GameBuildData();
        private static FtpUploadData ftpUploadData = new FtpUploadData();

        public static void CloudBuildPreExport_21Run_iOS_InHouseDistribution(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            try
            {
                // test data
                //buildData.ProjectId = "Friendly Wager 21 Run";
                //buildData.BundleId = "com.freerange.21Run";
                //buildData.SvnRevision = "9001";

                buildData.Date = DateTime.UtcNow;

                string projectId;
                manifest.TryGetValue<string>("projectId", out projectId);
                buildData.ProjectId = projectId;

                string bundleId;
                manifest.TryGetValue<string>("bundleId", out bundleId);
                buildData.BundleId = bundleId;

                string scmCommitId;
                manifest.TryGetValue<string>("scmCommitId", out scmCommitId);
                buildData.SvnRevision = scmCommitId;

                PlayerSettings.bundleVersion = scmCommitId;
                PlayerSettings.SplashScreen.showUnityLogo = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void CloudBuildPostExport_21Run_iOS_InHouseDistribution(string pathToIpa)
        {
            try
            {
                ftpUploadData.GameDirName = "21Run";
                ftpUploadData.Platform = "ios";
                ftpUploadData.FileSizeBytes = (int)(new FileInfo(pathToIpa).Length);
                ftpUploadData.PathToBuildResult = pathToIpa;
                ftpUploadData.FileName = Path.GetFileName(pathToIpa);
                ftpUploadData.RadomDirName = Guid.NewGuid().ToString().Substring(0, 8);
                ftpUploadData.FtpRevisionDirPath = ftpUploadData.FtpServerRootDirPath + "/" + ftpUploadData.GameDirName + "/" + ftpUploadData.Platform + "/" + buildData.SvnRevision;
                ftpUploadData.FtpIpaDirPath = ftpUploadData.FtpRevisionDirPath + "/" + ftpUploadData.RadomDirName;
                ftpUploadData.FtpIpaFilePath = ftpUploadData.FtpIpaDirPath + "/" + ftpUploadData.FileName;
                ftpUploadData.FtpPlistPath = ftpUploadData.FtpRevisionDirPath + "/" + ftpUploadData.FileName + ".plist";

                buildData.IpaUrl = ftpUploadData.HttpRoot + ftpUploadData.GameDirName + "/" + buildData.SvnRevision + "/" + ftpUploadData.RadomDirName + "/" + ftpUploadData.FileName;
                buildData.PlistUrl = ftpUploadData.HttpRoot + "/" + ftpUploadData.GameDirName + "/" + buildData.SvnRevision + "/" + ftpUploadData.FileName + ".plist";

                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(ftpUploadData.FtpUsername, ftpUploadData.FtpPassword);
                    UploadIpa(ftpUploadData, client);
                    UploadPlistFile(buildData, ftpUploadData, client);
                    UploadHtmlInstallPage(buildData, ftpUploadData, client);
                    UploadHtaccess(buildData, ftpUploadData, client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void UploadIpa(FtpUploadData ftpUploadData, WebClient client)
        {
            // create dir, this may fail if it already exists
            CreateDirectoryOnFtp(ftpUploadData.FtpRevisionDirPath, ftpUploadData);
            // add ipa to random dir name, as it will be public with custom .htaccess
            CreateDirectoryOnFtp(ftpUploadData.FtpRevisionDirPath + "/" + ftpUploadData.RadomDirName, ftpUploadData);

            Console.WriteLine($"Uploading ipa file to FTP, file path: {ftpUploadData.PathToBuildResult}");
            client.UploadFile(ftpUploadData.FtpIpaFilePath, WebRequestMethods.Ftp.UploadFile, ftpUploadData.PathToBuildResult);
            Console.WriteLine($"File {ftpUploadData.PathToBuildResult} has been uploaded.");
        }

        private static void UploadPlistFile(GameBuildData buildData, FtpUploadData ftpUploadData, WebClient client)
        {
            Console.WriteLine($"Uploading Plist file.");
            string fileContent = string.Format(
                PlistTemplate,
                buildData.ProjectId,
                buildData.BundleId,
                buildData.SvnRevision);

            UploadFileWIthContent(client, ftpUploadData.FtpPlistPath, fileContent);
        }

        private static void UploadHtmlInstallPage(GameBuildData buildData, FtpUploadData ftpUploadData, WebClient client)
        {
            Console.WriteLine($"Uploading HTML install page.");
            string fileContent = string.Format(
                InstallAppHtmlTemplate,
                buildData.ProjectId,
                buildData.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                buildData.BundleId,
                buildData.SvnRevision,
                ftpUploadData.FileSizeBytes.ToString("N0"),
                ftpUploadData.FtpPlistPath);

            UploadFileWIthContent(client, ftpUploadData.FtpRevisionDirPath + "/index.html", fileContent);
        }

        private static void UploadHtaccess(GameBuildData buildData, FtpUploadData ftpUploadData, WebClient client)
        {
            Console.WriteLine($"Uploading .htaccess file.");

            UploadFileWIthContent(client, ftpUploadData.FtpIpaDirPath + "/.htaccess", "Satisfy Any");
        }

        private static void UploadFileWIthContent(WebClient client, string ftpPath, string fileContent)
        {
            var filePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            File.WriteAllText(filePath, fileContent);
            client.UploadFile(ftpPath, WebRequestMethods.Ftp.UploadFile, filePath);
            File.Delete(filePath);
            Console.WriteLine($"File has been uploaded.");
        }

        private static void CreateDirectoryOnFtp(string ftpDirPath, FtpUploadData ftpUploadData)
        {
            Console.WriteLine($"Creating directory, path: {ftpDirPath}");
            try
            {
                WebRequest request = WebRequest.Create(ftpDirPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(ftpUploadData.FtpUsername, ftpUploadData.FtpPassword);
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("Create direcory FTP: response:" + resp.StatusCode.ToString());
                }
            }
            catch (WebException e)
            {
                Console.WriteLine($"Error creating directory, status: {e.Status}. Directory probably already exists. Continuing.");
            }
        }

        const string InstallAppHtmlTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""utf-8"">
    <title> Install app</title>
  </head>
  <body>
    <p>This is autogenerated html file for installing iOS apps using a https link. Install the game by clicking on the link below</p>
    <ul>
      <li>Game title: {0}</li>
      <li>Date created: {1}</li>
      <li>Bundle id: {2}</li>
      <li>Bundle version: {3}</li>
      <li>File size: {4} bytes</li>
    </ul>
    <a style=""font-size: 32px"" href=""itms-services://?action=download-manifest&amp;url={5}"">Install</a>
  </body>
</html>
";

        const string RedirectHtmlTemplate = @"
<!DOCTYPE HTML>
<html lang=""en-US"">
    <head>
        <meta charset=""UTF-8"">
        <meta http-equiv=""refresh"" content=""0;url={0}"">
        <script type = ""text/javascript"">
            window.location.href = ""{0}""
        </script>
        <title>Page Redirection</title>
    </head>
    <body>
        If you are not redirected automatically, follow the<a href='{0}'>{0}</a>
    </body>
</html>
";
        const string PlistTemplate = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>items</key>
    <array>
        <dict>
            <key>assets</key>
            <array>
                <dict>
                    <key>kind</key>
                    <string>software-package</string>
                    <key>url</key>
                    <string>{0}</string>
                </dict>
            </array>
            <key>metadata</key>
            <dict>
                <key>bundle-identifier</key>
                <string>{1}</string>
                <key>bundle-version</key>
                <string>{2}</string>
                <key>kind</key>
                <string>software</string>
                <key>title</key>
                <string>{0}</string>
            </dict>
        </dict>
    </array>
</dict>
</plist>
";
    }

    #region compatibility with unity cloud build
#if !UNITY_CLOUD_BUILD
    namespace UnityEngine.CloudBuild
    {
        public class BuildManifestObject
        {
            // Tries to get a manifest value - returns true if key was found and could be cast to type T, false otherwise.
            public bool TryGetValue<T>(string key, out T result) { result = default(T); return false; }

            // Retrieve a manifest value or throw an exception if the given key isn't found.
            public T GetValue<T>(string key) { return default(T); }

            // Sets the value for a given key.
            public void SetValue(string key, object value) { }

            // Copy values from a dictionary. ToString() will be called on dictionary values before being stored.
            public void SetValues(Dictionary<string, object> sourceDict) { }

            // Remove all key/value pairs
            public void ClearValues() { }

            // Returns a Dictionary that represents the current BuildManifestObject
            public Dictionary<string, object> ToDictionary() { return null; }

            // Returns a JSON formatted string that represents the current BuildManifestObject
            public string ToJson() { return null; }

            // Returns an INI formatted string that represents the current BuildManifestObject
            public override string ToString() { return null; }
        }
    }
#endif
    #endregion
}