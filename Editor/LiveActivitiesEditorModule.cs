using System.Collections.Generic;
using TappGo.Editor.Modules;
using TappGo.LiveActivities.Internal;
using TappGo.Modules;
using UnityEditor;
using UnityEngine;

namespace TappGo.LiveActivities.Editor
{
    /// <summary>
    /// What Live Activities need from the Editor: a section on the settings page, and an iOS build that links
    /// the module, declares support for it, and renders the card.
    /// </summary>
    /// <remarks>
    /// Data only — core's build hook applies it. There is nothing for Android: it has no Live Activities, so
    /// this package adds no artifact and stages nothing there.
    /// </remarks>
    [InitializeOnLoad]
    internal sealed class LiveActivitiesEditorModule : TappEditorModule
    {
        /// <summary>The Swift package product, and the module the generated bundle imports.</summary>
        internal const string Product = "TappLiveActivities";

        static LiveActivitiesEditorModule()
        {
            TappEditorModules.Register(new LiveActivitiesEditorModule());

            // The runtime module adds itself when a player starts, which the Editor only does in Play mode.
            // The settings page and the build hook validate through the same registry, so without this they
            // ask no product anything: an empty app id reads "Ready" and a batch build passes with it.
            TappModules.Register(new LiveActivitiesModule());
        }

        public override string Id => LiveActivitiesModule.PackageName;

        public override string DisplayName => "Live Activities";

        public override ScriptableObject Settings(bool create)
        {
            return TappModuleSettings.LoadOrCreate<TappLiveActivitiesSettings>(
                TappLiveActivitiesSettings.ResourceName, create, Seed);
        }

        /// <summary>Carries over what a pre-split <c>TappGoSettings.asset</c> had set for this product.</summary>
        internal static void Seed(TappLiveActivitiesSettings created)
        {
            Seed(created, TappModuleSettings.Legacy());
        }

        internal static void Seed(TappLiveActivitiesSettings created, IReadOnlyDictionary<string, string> legacy)
        {
            if (legacy.TryGetValue("liveActivityAppId", out var appId))
            {
                created.liveActivityAppId = appId;
            }

            if (legacy.TryGetValue("pushEnabled", out var push))
            {
                created.pushEnabled = push != "0";
            }
        }

        public override TappIosContribution Ios(TappGoSettings settings)
        {
            var own = LiveActivitiesModule.Own();
            var contribution = new TappIosContribution
            {
                // Only produces `aps-environment`, which the SDK reads to choose an APNs host. The provisioning
                // profile is what actually decides it at signing, so this is a default rather than a promise.
                PushNotifications = own == null || own.pushEnabled,
            };

            contribution.SwiftProducts.Add(Product);
            contribution.ExtensionFrameworks.Add("ActivityKit.framework");

            // Without this key every start fails with liveActivitiesUnavailable — the same code the player's
            // own Settings toggle produces, so the two are indistinguishable from inside the app. Setting it
            // is what makes that code mean what it says.
            contribution.AppPlistFlags.Add("NSSupportsLiveActivities");

            contribution.BundleImports.Add(Product);
            contribution.BundleSetup.Add(
                $"try? TappLiveActivities.configureExtension(appGroupIdentifier: \"{settings.appGroupIdentifier}\")");
            contribution.BundleBody.Add("TappLiveActivity()");
            return contribution;
        }
    }
}
