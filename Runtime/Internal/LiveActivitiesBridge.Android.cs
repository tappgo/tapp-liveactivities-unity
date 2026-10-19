#if UNITY_ANDROID && !UNITY_EDITOR
using System;

namespace TappGo.LiveActivities.Internal
{
    /// <summary>
    /// Android has no Live Activities, and says so permanently.
    /// </summary>
    /// <remarks>
    /// The code is the one the iOS SDK reports when the user has them off, so a host has one thing to branch
    /// on for "not available here". Constant envelopes, no JNI: there is no native member to reach.
    /// </remarks>
    internal static class LiveActivitiesBridge
    {
        private const string Unavailable =
            "{\"ok\":false,\"code\":\"liveActivitiesUnavailable\",\"message\":\"Android has no Live Activities.\"}";

        internal static void Start(string json, Action<string> completion)
        {
            completion?.Invoke(Unavailable);
        }

        internal static void Update(string json, Action<string> completion)
        {
            completion?.Invoke(Unavailable);
        }

        internal static void End(string json, Action<string> completion)
        {
            completion?.Invoke(Unavailable);
        }

        internal static string Active()
        {
            return "{\"ok\":true,\"value\":{\"activities\":[]}}";
        }
    }
}
#endif
