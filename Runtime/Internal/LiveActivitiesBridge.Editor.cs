#if UNITY_EDITOR || (!UNITY_IOS && !UNITY_ANDROID)
using System;
using TappGo.Modules;

namespace TappGo.LiveActivities.Internal
{
    /// <summary>
    /// The Editor stub: logs what would have been sent, and answers. See <see cref="TappNative"/> for what a
    /// stub may and may not claim.
    /// </summary>
    internal static class LiveActivitiesBridge
    {
        internal static void Start(string json, Action<string> completion)
        {
            TappNative.Log("StartLiveActivity", json);
            completion?.Invoke($"{{\"ok\":true,\"value\":{{\"activityID\":\"{TappNative.StubValue}\"}}}}");
        }

        internal static void Update(string json, Action<string> completion)
        {
            TappNative.Log("UpdateLiveActivity", json);
            completion?.Invoke(TappNative.Ok);
        }

        internal static void End(string json, Action<string> completion)
        {
            TappNative.Log("EndLiveActivity", json);
            completion?.Invoke(TappNative.Ok);
        }

        /// <summary>
        /// Always empty.
        /// </summary>
        /// <remarks>
        /// Returning a fabricated running activity would let reconciliation code appear to work in the Editor
        /// and then behave differently on a device. Empty is the honest answer: nothing is running, because
        /// nothing can be.
        /// </remarks>
        internal static string Active()
        {
            TappNative.Log("ActiveLiveActivities");
            return "{\"ok\":true,\"value\":{\"activities\":[]}}";
        }
    }
}
#endif
