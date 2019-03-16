using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// The most basic log listener I could make to see if it will help with 
    /// debuging logs on iOS and Android where getting to log files is very hard.
    /// </summary>
    public class LogListener
    {
        public struct LogEntry
        {
            public string text;
            public string stackTrace;
            public LogType type;
        }

        public static bool Enabled { get { return _enabled; } set { if (value) Enable(); else Disable(); } }
        public static int LogCount { get { return logs.Count; } }
        public static List<LogEntry> RawLogs { get { return logs; } }

        static bool _enabled;
        static List<LogEntry> logs = new List<LogEntry>(100);
        static string cachedLog;
        static bool cachedLogUsesStack;
        static bool cacheIsDirty;

        private static void Enable()
        {
            if (_enabled) return;
            _enabled = true;
            Application.logMessageReceived += ReceiveLog;
        }

        private static void Disable()
        {
            if (!_enabled) return;
            _enabled = false;
            Application.logMessageReceived -= ReceiveLog;
        }

        private static void ReceiveLog(string condition, string stackTrace, LogType type)
        {
            logs.Add(new LogEntry() { text = condition, stackTrace = stackTrace, type = type });
            cacheIsDirty = true;
        }

        public static string GetOutput(bool includeStackTrace)
        {
            // return output string from cache
            if (!cacheIsDirty && cachedLogUsesStack == includeStackTrace)
                return cachedLog;

            cacheIsDirty = false;
            cachedLogUsesStack = includeStackTrace;

            StringBuilder builder = new StringBuilder();
            foreach (var log in logs)
            {
                builder.AppendLine(log.text);
                if (includeStackTrace)
                    builder.AppendLine(log.stackTrace);
            }
            cachedLog = builder.ToString();
            return cachedLog;
        }

        public static void Clear()
        {
            logs.Clear();
            cacheIsDirty = true;
        }
    }
}