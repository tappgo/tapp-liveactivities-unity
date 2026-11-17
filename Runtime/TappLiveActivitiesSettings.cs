using UnityEngine;

namespace TappGo
{
    /// <summary>
    /// The Live Activities settings, edited in <b>Project Settings → Tapp</b> beside core's.
    /// </summary>
    /// <remarks>
    /// Stored at <c>Assets/Resources/TappLiveActivitiesSettings.asset</c> — in your project, not in the
    /// package, so it survives upgrades, and in an asset of its own so this package can be added and removed
    /// without touching core's. Every field has a default for the same reason core's do.
    /// </remarks>
    public sealed class TappLiveActivitiesSettings : ScriptableObject
    {
        /// <summary>Where the asset lives, and the name <c>Resources.Load</c> looks for.</summary>
        internal const string ResourceName = "TappLiveActivitiesSettings";

        [Tooltip("Your Tapp app id for Live Activities, from the Tapp back office. Required.")]
        public string liveActivityAppId = "";

        [Tooltip("Register for push-to-start and push updates. Leave on unless Tapp tells you otherwise.")]
        public bool pushEnabled = true;

        /// <summary>The single settings asset, or <c>null</c> if none exists.</summary>
        public static TappLiveActivitiesSettings Load()
        {
            return Resources.Load<TappLiveActivitiesSettings>(ResourceName);
        }
    }
}
