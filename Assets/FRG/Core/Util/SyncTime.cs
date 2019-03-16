using UnityEngine;
using System;
using System.Threading;


namespace FRG.Core
{
#if GAME_SERVER
    /// <summary>
    /// Provides time synchronization methods.
    /// </summary>
    /// <remarks>
    /// We may need to use lock free algorithms with this class. Wait until we see actual contention.
    /// We may end up doing more complicated things that need a different style of locking entirely.
    /// </remarks>
    public static class SyncTime
    {
        /// <summary>
        /// Should be System.Threading.Timeout.InfiniteTimeSpan, but that doesn't appear to be present in Unity3D mono.
        /// </summary>
        public static readonly TimeSpan InfiniteTimeSpan = Timeout.InfiniteTimeSpan;

        /// <seealso href="https://en.wikipedia.org/wiki/Unix_time"/>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const long MillisecondTo100Nanoseconds = 10000;

        private static object _syncRoot = new object();
        private static uint _lastTickCount = 0;
        private static TimeSpan _lastRealtimeUpdate = TimeSpan.Zero;

        private static bool _requiresResync = true;
        private static bool _wasEverSynced = false;
        private static DateTime _lastSync = DateTime.MinValue;
        private static TimeSpan _realtimeSecondsAtSync = TimeSpan.Zero;

        private static TimeSpan GetRealtimeSinceStartup()
        {
            // Need to handle overflow, so just work with unsigned.
            uint currentTickCount = unchecked((uint)Environment.TickCount);
            // unsigned tick count
            long elapsed = unchecked(currentTickCount - _lastTickCount);
            _lastRealtimeUpdate += TimeSpan.FromTicks(elapsed * MillisecondTo100Nanoseconds);
            _lastTickCount = currentTickCount;
            return _lastRealtimeUpdate;
        }

        /// <summary>
        /// The current time in UTC, as best the time service can figure.
        /// Not guaranteed to be always-increasing, so only use this if you actually care about the date.
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                lock (_syncRoot)
                {
                    if (!_wasEverSynced)
                    {
                        return DateTime.UtcNow;
                    }
                    return _lastSync + (GetRealtimeSinceStartup() - _realtimeSecondsAtSync);
                }
            }

            set
            {
                Debug.Assert(value >= UnixEpoch, "Cannot set the date so far back in time.");

                lock (_syncRoot)
                {
                    _lastSync = value.ToUniversalTime();
                    _realtimeSecondsAtSync = RealtimeSinceStartup;
                    _wasEverSynced = true;
                }
            }
        }

        /// <summary>
        /// A dirty flag that specifies that something changed, and
        /// time should be fetched. (Though the time fetching code may want to throttle.)
        /// Not managed by this class. Starts true.
        /// </summary>
        public static bool RequiresSync
        {
            get
            {
                lock (_syncRoot)
                {
                    return _requiresResync;
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    _requiresResync = value;
                }
            }
        }

        /// <summary>
        /// Whether time has yet been synced with another server.
        /// </summary>
        public static bool WasEverSynced
        {
            get
            {
                lock (_syncRoot)
                {
                    return _wasEverSynced;
                }
            }
        }

        /// <summary>
        /// The last time syncing value that was set.
        /// </summary>
        public static DateTime LastSync
        {
            get
            {
                lock (_syncRoot)
                {
                    return _lastSync;
                }
            }
        }

        /// <summary>
        /// The amount of time since startup, as a <see cref="TimeSpan"/>.
        /// A strictly always-increasing amount of time. (May show 0 increase within one frame, when not in focus, etc.)
        /// </summary>
        /// <remarks>Not necessarily an accurate reflection over long time periods.</remarks>
        public static TimeSpan RealtimeSinceStartup
        {
            get
            {
                lock (_syncRoot)
                {
                    return GetRealtimeSinceStartup();
                }
            }
        }

        /// <summary>
        /// Same as <see cref="RealtimeSinceStartup"/>, but on the client it
        /// does not change within a frame.
        /// </summary>
        public static TimeSpan UnscaledTime
        {
            get
            {
                lock (_syncRoot)
                {
                    return GetRealtimeSinceStartup();
                }
            }
        }
    }
#else
    /// <summary>
    /// Provides time synchronization methods.
    /// Only call on the client from a valid Unity context.
    /// (Correct thread, not in a constructor or serialization callback.)
    /// </summary>
    public static class SyncTime
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Should be System.Threading.Timeout.InfiniteTimeSpan, but that doesn't appear to be present in Unity3D mono.
        /// </summary>
        public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);

        private static bool _wasEverSynced = false;
        private static bool _requiresSync = true;
        private static DateTime _lastSync = DateTime.MinValue;
        private static TimeSpan _realtimeSecondsAtSync = TimeSpan.Zero;

        /// <summary>
        /// The current time in UTC, as best the time service can figure.
        /// Not guaranteed to be always-increasing, so only use this if you actually care about the date.
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (!_wasEverSynced)
                {
                    return DateTime.UtcNow;
                }
                return LastSync + (RealtimeSinceStartup - _realtimeSecondsAtSync);
            }

            set
            {
                Debug.Assert(value >= UnixEpoch, "Cannot set the date so far back in time.");

                _lastSync = value.ToUniversalTime();
                _realtimeSecondsAtSync = RealtimeSinceStartup;
                _wasEverSynced = true;
            }
        }

        /// <summary>
        /// Whether time has yet been synced with another server.
        /// </summary>
        public static bool WasEverSynced { get { return _wasEverSynced; } }

        /// <summary>
        /// A dirty flag that specifies that something changed, and
        /// time should be fetched. (Though the time fetching code may want to throttle.)
        /// Not managed by this class.
        /// </summary>
        public static bool RequiresSync { get { return _requiresSync; } set { _requiresSync = value; } }

        /// <summary>
        /// The last time syncing value that was set.
        /// </summary>
        public static DateTime LastSync { get { return _lastSync; } }

        /// <summary>
        /// The amount of time since startup, as a <see cref="TimeSpan"/>.
        /// A strictly always-increasing amount of time. (May show 0 increase within one frame, when not in focus, etc.)
        /// </summary>
        /// <remarks>Not necessarily an accurate reflection over long time periods.</remarks>
        public static TimeSpan RealtimeSinceStartup
        {
            get
            {
                return TimeSpan.FromSeconds(Time.realtimeSinceStartup);
            }
        }

        /// <summary>
        /// Same as <see cref="RealtimeSinceStartup"/>, but on the client it
        /// returns the same value for the whole frame.
        /// </summary>
        public static TimeSpan UnscaledTime
        {
            get
            {
                return TimeSpan.FromSeconds(Time.unscaledTime);
            }
        }
    }
#endif
}
