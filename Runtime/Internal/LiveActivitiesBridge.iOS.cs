#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using TappGo.Modules;

namespace TappGo.LiveActivities.Internal
{
    /// <summary>
    /// The device implementation: the C entry points in <c>Runtime/Plugins/iOS/TappGoShim.LiveActivities.swift</c>.
    /// </summary>
    /// <remarks>
    /// Same members, same signatures and same envelopes as the Android body and the Editor stub beside it; the
    /// <c>#if</c> at the top of each decides which compiles. The ownership rules are <see cref="TappNative"/>'s.
    /// </remarks>
    internal static class LiveActivitiesBridge
    {
        internal static void Start(string json, Action<string> completion)
        {
            TappGo_StartLiveActivity(TappNative.Encode(json), TappNative.Pin(completion), TappNative.Reply);
        }

        internal static void Update(string json, Action<string> completion)
        {
            TappGo_UpdateLiveActivity(TappNative.Encode(json), TappNative.Pin(completion), TappNative.Reply);
        }

        internal static void End(string json, Action<string> completion)
        {
            TappGo_EndLiveActivity(TappNative.Encode(json), TappNative.Pin(completion), TappNative.Reply);
        }

        internal static string Active()
        {
            return TappNative.Take(TappGo_ActiveLiveActivities());
        }

        // Names match the C symbols exactly, so none of these needs an EntryPoint and a typo cannot silently
        // bind to the wrong function.

        [DllImport("__Internal")]
        private static extern void TappGo_StartLiveActivity(byte[] json, IntPtr context, TappNative.ReplyCallback reply);

        [DllImport("__Internal")]
        private static extern void TappGo_UpdateLiveActivity(byte[] json, IntPtr context, TappNative.ReplyCallback reply);

        [DllImport("__Internal")]
        private static extern void TappGo_EndLiveActivity(byte[] json, IntPtr context, TappNative.ReplyCallback reply);

        [DllImport("__Internal")]
        private static extern IntPtr TappGo_ActiveLiveActivities();
    }
}
#endif
