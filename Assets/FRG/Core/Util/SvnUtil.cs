using System.Diagnostics;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using FRG.SharedCore;
using System.Threading;
using System.Linq;

namespace FRG.Core {

    /// <summary>
    /// An exception working with SVN.
    /// </summary>
    public class SvnException : IOException
    {
        public SvnException() { }
        public SvnException(string message) : base(message) { }
        public SvnException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Uses svn command line to do operations.
    /// </summary>
    public static class SvnUtil
    {
        /// <summary>
        /// Checks if SVN is avaliable through command prompt. Value is cached.
        /// </summary>
        public static bool IsSvnAvaliable
        {
            get { return !string.IsNullOrEmpty(Version); }
        }

        private static bool isVersionChecked = false;
        private static string _version = null;
        public static string Version
        {
            get
            {
                if (!isVersionChecked)
                {
                    isVersionChecked = true;
                    RunSvnCommand(new string[] { "--version", "--quiet" }, true, out _version);
                }
                return _version;
            }
        }

        public static bool IsWorkingCopy(string path)
        {
            return !string.IsNullOrEmpty(GetWorkingCopy(path));
        }

        public static bool IsSameWorkingCopy(string leftPath, string rightPath)
        {
            string leftCopy = GetWorkingCopy(leftPath);
            if (string.IsNullOrEmpty(leftCopy)) return false;

            string rightCopy = GetWorkingCopy(rightPath);
            return !string.IsNullOrEmpty(leftPath) && string.Equals(leftCopy, rightCopy, StringComparison.Ordinal);
        }

        public static string GetWorkingCopy(string path)
        {
            path = Path.GetFullPath(path);
            while (true)
            {
                if (string.IsNullOrEmpty(path)) return null;

                if (Directory.Exists(path))
                {
                    char? c = GetVersionChar(path);
                    if (c != '?' && c != '\0' && c != null)
                    {
                        break;
                    }
                }

                string nextPath = Path.GetDirectoryName(path);
                if (string.Equals(path, nextPath, StringComparison.Ordinal)) return null;
                path = nextPath;
            }

            string output;
            if (!RunSvnCommand(new string[] { "info", path }, true, out output))
            {
                return null;
            }

            Match match = Statics.WorkingCopy.Match(output);
            if (match.Success)
            {
                return match.Captures[0].Value;
            }
            return null;
        }
        
        public static char? GetVersionChar(string path)
        {
            if (!IsSvnAvaliable) return null;

            string output;
            if (!RunSvnCommand(new string[] { "status", "--verbose", "--quiet", "--depth=empty", path }, true, out output)) return null;

            char c = output.Length > 0 ? output[0] : '\0';
            switch (c)
            {
                default:
                    return '\0';
                case ' ':
                case 'A':
                case 'D':
                case 'M':
                case 'R':
                case 'C':
                case 'X':
                case 'I':
                case '?':
                case '!':
                case '~':
                    return c;
            }
        }

        public static void ForceAddWithoutReplace(string path, bool useSvn)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new SvnException("Cannot force add a file that does not exist.");
            }

            if (!useSvn)
            {
                return;
            }

            char? c = GetVersionChar(path);
            // Already added or command didn't work
            if (c == null || c == ' ' || c == 'M' || c == 'A')
            {
                return;
            }

            // If there is history, revert it
            if (c != '\0' && c != '?' && c != 'I')
            {
                string tempPath = path + ".temp";
                File.Move(path, tempPath);
                try
                {
                    // Ignore result
                    RunSvnCommand("revert", "--quiet", path);
                }
                finally
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    else
                    {
                        File.Delete(path);
                    }
                    File.Move(tempPath, path);
                }

                c = GetVersionChar(path);
                // Already added
                if (c == ' ' || c == 'M' || c == 'A')
                {
                    return;
                }
            }

            RunSvnCommand("add", "--quiet", "--parents", path);
        }

        public static void ForceDelete(string path, bool useSvn)
        {
            if (useSvn)
            {
                RunSvnCommand("delete", "--quiet", "--force", path);
            }
            File.Delete(path);
        }

        private static void QuietDelete(string path)
        {
            string unused;
            RunSvnCommand(new string[] { "delete", "--quiet", "--force", path }, true, out unused);
        }

        public static void RepairMove(string sourcePath, string destinationPath, bool useSvn)
        {
            if (!File.Exists(destinationPath))
            {
                throw new SvnException("For repair move operation, destination file does not exist: \"" + destinationPath + "\".");
            }
            if (File.Exists(sourcePath))
            {
                throw new SvnException("For repair move operation, source file already exists: \"" + sourcePath + "\".");
            }

            if (!useSvn)
            {
                return;
            }

            bool moveSucceeded = false;
            bool swapped = false;
            try
            {
                File.Move(destinationPath, sourcePath);
                swapped = true;

                QuietDelete(destinationPath);
                moveSucceeded = RunSvnCommand("move", "--quiet", "--force", sourcePath, destinationPath);

                if (moveSucceeded)
                {
                    swapped = false;
                }
            }
            finally
            {
                if (swapped)
                {
                    File.Move(sourcePath, destinationPath);
                }
            }
        }

        public static void Move(string sourcePath, string destinationPath, bool useSvn)
        {
            if (File.Exists(destinationPath))
            {
                throw new SvnException("File already exists: \"" + destinationPath + "\".");
            }

            if (useSvn)
            {
                QuietDelete(destinationPath);
                if (RunSvnCommand("move", "--quiet", "--force", sourcePath, destinationPath))
                {
                    return;
                }
            }

            File.Move(sourcePath, destinationPath);
        }

        public static void Copy(string sourcePath, string destinationPath, bool useSvn)
        {
            if (File.Exists(destinationPath))
            {
                throw new SvnException("File already exists: \"" + destinationPath + "\".");
            }

            if (useSvn)
            {
                QuietDelete(destinationPath);
                if (RunSvnCommand("copy", "--quiet", sourcePath, destinationPath))
                {
                    return;
                }
            }

            File.Copy(sourcePath, destinationPath);
        }

        private static bool RunSvnCommand(params string[] args)
        {
            string output;
            return RunSvnCommand(args, false, out output);
        }

        private static bool TrySvnCommand(params string[] args)
        {
            string output;
            return RunSvnCommand(args, true, out output);
        }

        private static bool RunSvnCommand(string[] args, bool suppressLogs, out string output)
        {
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "svn";
            p.StartInfo.Arguments = ArrayUtil.Join(" ", args, arg => ShellQuote(arg));
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;

            string stdout = "";
            string stderr = "";

            try
            {
                p.Start();

                Thread stdoutThread = new Thread(() => { stdout = p.StandardOutput.ReadToEnd(); });
                Thread stderrThread = new Thread(() => { stderr = p.StandardError.ReadToEnd(); });

                stdoutThread.Start();
                stderrThread.Start();
                stdoutThread.Join();
                stderrThread.Join();

                p.WaitForExit();
            }
            catch (Exception e)
            {
                ReflectionUtil.CheckDangerousException(e);

                if (!suppressLogs)
                {
                    UnityEngine.Debug.LogWarning("Error running SVN command: " + p.StartInfo.FileName + " " + p.StartInfo.Arguments +
                        "\nExit Code: " + p.ExitCode.ToString() + "\n" + e);
                }

                output = null;
                return false;
            }

            string outputPart = "";
            if (!string.IsNullOrEmpty(stdout)) outputPart += "\n\nStdout:\n" + stdout;
            if (!string.IsNullOrEmpty(stderr)) outputPart += "\n\nStderr:\n" + stderr;
            //logger.Warn(p.StartInfo.FileName + " " + p.StartInfo.Arguments +
            //    "\nExit Code: " + p.ExitCode.ToString() + outputPart);

            if (p.ExitCode != 0 || !string.IsNullOrEmpty(stderr))
            {
                if (!suppressLogs)
                {
                    string errorPart = "";
                    if (!string.IsNullOrEmpty(stderr)) errorPart = "\n\nStderr:\n" + stderr;
                    UnityEngine.Debug.LogWarning((p.ExitCode != 0 ? "Error" : "Warning") + " while running SVN command: " + p.StartInfo.FileName + " " + p.StartInfo.Arguments +
                        "\nExit Code: " + p.ExitCode.ToString() + errorPart);
                }
            }

            if (p.ExitCode != 0)
            {
                output = null;
                return false;
            }

            output = stdout ?? "";
            return true;
        }

        private static bool IsWindows()
        {
            return (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.WinCE || Environment.OSVersion.Platform == PlatformID.Xbox);
        }

        private static string ShellQuote(string argument)
        {
            if (IsWindows())
            {
                if (string.IsNullOrEmpty(argument))
                {
                    return "\"\"";
                }
                if (Statics.UnsafeCharactersWindows.IsMatch(argument))
                {
                    StringBuilder builder = new StringBuilder();
                    bool quote = argument.Contains(" ") || argument.Contains("\t");
                    if (quote)
                    {
                        builder.Append('"');
                    }

                    int backslashes = 0;
                    foreach (char c in argument)
                    {
                        switch (c)
                        {
                            case '\\':
                                backslashes += 1;
                                break;
                            case '"':
                                AddBackslashes(builder, backslashes * 2);
                                backslashes = 0;
                                builder.Append('"');
                                break;
                            default:
                                AddBackslashes(builder, backslashes);
                                backslashes = 0;
                                builder.Append(c);
                                break;
                        }
                    }

                    AddBackslashes(builder, backslashes);
                    if (quote)
                    {
                        builder.Append('"');
                    }
                    argument = builder.ToString();
                }
                return argument;
            }
            else
            {
                if (string.IsNullOrEmpty(argument))
                {
                    return "''";
                }
                if (Statics.UnsafeCharactersUnix.IsMatch(argument))
                {
                    argument = "'" + argument.Replace("'", "'\"'\"'") + "'";
                }
                return argument;
            }
        }

        private static void AddBackslashes(StringBuilder builder, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                builder.Append('\\');
            }
        }

        private static class Statics
        {
            public static Regex UnsafeCharactersUnix = new Regex(@"[^A-Za-z0-9_@%+=:,./-]", RegexOptions.CultureInvariant);
            public static Regex UnsafeCharactersWindows = new Regex(@"[ \t\""]", RegexOptions.CultureInvariant);

            public static Regex WorkingCopy = new Regex(@"^\s*Working\s*Copy\s*Root\s*Path:\s(.*)$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }
    }
}


