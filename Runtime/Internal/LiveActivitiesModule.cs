using TappGo.Modules;
using UnityEngine;

namespace TappGo.LiveActivities.Internal
{
    /// <summary>
    /// What core asks this package at <c>configure</c>: whether its settings are usable, and its keys.
    /// </summary>
    internal sealed class LiveActivitiesModule : TappModule
    {
        internal const string PackageName = "com.tapp.go.liveactivities";

        public override string Id => PackageName;

        /// <summary>
        /// Stands in for the asset in <c>Resources</c>. A test sets it; nothing else does, so the running app
        /// and the build always read the one asset the developer edits.
        /// </summary>
        internal static TappLiveActivitiesSettings Override;

        internal static TappLiveActivitiesSettings Own()
        {
            return Override != null ? Override : TappLiveActivitiesSettings.Load();
        }

        /// <summary>
        /// iOS needs the app id: this package is installed to show Live Activities, and without the id the
        /// native surface configures nothing and every start fails — on a device, long after the build passed.
        /// Android asks for nothing, because it has nothing to configure.
        /// </summary>
        public override string Validate(TappGoSettings settings, TappPlatform platform)
        {
            if (platform != TappPlatform.iOS)
            {
                return null;
            }

            var own = Own();
            if (own == null)
            {
                return "The Live Activities package is installed but has no settings. Open Project Settings → " +
                       "Tapp and create them.";
            }

            return string.IsNullOrWhiteSpace(own.liveActivityAppId)
                ? "liveActivityAppId is empty. Set it in Project Settings → Tapp → Live Activities."
                : null;
        }

        public override void ContributeConfiguration(JsonBuilder json, TappGoSettings settings, TappPlatform platform)
        {
            var own = Own();
            if (platform != TappPlatform.iOS || own == null || string.IsNullOrWhiteSpace(own.liveActivityAppId))
            {
                return;
            }

            json.AddObject("liveActivities", new JsonBuilder()
                .Add("appID", own.liveActivityAppId)
                .Add("pushEnabled", own.pushEnabled));
        }

        /// <summary>
        /// Before any scene loads, so the module is in place before the host's first <c>Tapp.Configure</c> —
        /// the first thing to ask the registry.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            TappModules.Register(new LiveActivitiesModule());
        }
    }
}
