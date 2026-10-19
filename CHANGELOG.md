# Changelog

All notable changes to `com.tapp.go.liveactivities`. Follows [Semantic Versioning](https://semver.org). Every Tapp package is
released at one version, and needs Tapp Core (`com.tapp.go`) at that same version.

## [2.2.0] — 2026-10-19

Resolves iOS `tappgo/tapp-ios` **2.3.0** and Android `com.tappgo` **2.2.0**, as pinned by Tapp Core, both released on
2026-10-19. No change to this package's API: it is released to keep the three at one version. Nothing a card shows
changes with it: the iOS SDK 2.3.0 decodes a **pool** (an image or text node published with several candidates, one
drawn per resolution of the card's content) so a card never fails to decode one, but the Live Activity editor
publishes no pools today, and Live Activity content is not published per language, so `Tapp.SetLanguage` (new in
Tapp Core 2.2.0) does not affect a card.

## [2.1.2] — 2026-10-15

Resolves iOS `tappgo/tapp-ios` **2.2.3** and Android `com.tappgo` **2.1.1**, as pinned by Tapp Core.

- **An `End` issued right after its `Start` now ends the card.** `Start`, `Update` and `End` for the same card
  take effect in the order you call them; before, each ran on its own, and an end that reached iOS before the
  start had put the card up answered `Ok` and left the card running. Nothing to change in your code: chaining
  the end off the start's callback still works and is no longer needed. Fixed in the iOS SDK 2.2.3.

## [2.1.1] — 2026-10-11

Resolves iOS `tappgo/tapp-ios` **2.2.2** and Android `com.tappgo` **2.1.1**, as pinned by Tapp Core. No change to
Live Activities: neither native release touched them, and this package is released to keep the three at one
version.

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
