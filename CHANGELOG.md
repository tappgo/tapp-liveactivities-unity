# Changelog

All notable changes to `com.tapp.go.liveactivities`. Follows [Semantic Versioning](https://semver.org). Every Tapp package is
released at one version, and needs Tapp Core (`com.tapp.go`) at that same version.

## [2.1.0] — 2026-10-05

Resolves iOS `tappgo/tapp-ios` **2.2.1** and Android `com.tappgo` **2.1.0**, as pinned by Tapp Core.

- **First release as a package of its own.** Live Activities were part of the single `com.tapp.go` package
  through 2.0; they are now installed separately, beside Tapp Core at the same version. A project that does not
  install this package does not link the Live Activities module at all.
- **Installed by Git URL** from the generated public repo [`tappgo/tapp-liveactivities-unity`](https://github.com/tappgo/tapp-liveactivities-unity),
  beside Tapp Core from `tappgo/tapp-unity` at the same tag. Signed with Unity's package signature,
  attributed to the Tapp organization.
- **`TappLiveActivities`** — `Start`, `Update`, `End`, `Active` — replaces `Tapp.StartLiveActivity`,
  `UpdateLiveActivity`, `EndLiveActivity` and `ActiveLiveActivities`. Same arguments, same results, same wire
  payloads.
- Settings of its own (`TappLiveActivitiesSettings.asset`: the Live Activity app id and *Push enabled*),
  created from Project Settings → Tapp and **filled from a pre-split `TappGoSettings.asset`**.
- Installing the package is what makes the iOS build link `TappLiveActivities`, declare
  `NSSupportsLiveActivities`, add the push entitlement and put the Live Activity into the widget extension.
  On Android every call answers `liveActivitiesUnavailable`, and nothing is added to the Gradle project.
