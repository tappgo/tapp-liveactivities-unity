# Tapp for Unity — Live Activities

Server-configured **Live Activities** on iOS, from Unity — without shipping a new app build.

One of Tapp's product packages. It needs **Tapp Core** (`com.tapp.go`) at the same version, and nothing else:
install it without the Widgets package and your app does not carry the widgets library at all.

## Install

```json
{
  "dependencies": {
    "com.tapp.go": "https://github.com/tappgo/tapp-unity.git#v2.1.2",
    "com.tapp.go.liveactivities": "https://github.com/tappgo/tapp-liveactivities-unity.git#v2.1.2"
  }
}
```

Both lines, on the **same tag** — core checks, and fails the build on a mixed set. Requirements, the two iOS
floors and what to expect in the Editor and on the Simulator are in
[core's README](https://github.com/tappgo/tapp-unity#readme).

## Configure

**Project Settings → Tapp → Live Activities.** Set your **Live Activity app id** from the Tapp back office;
leave *Push enabled* on unless Tapp tells you otherwise. Stored at
`Assets/Resources/TappLiveActivitiesSettings.asset` — commit it.

Then call `Tapp.Configure()` once at startup, as core's README shows. Tapp does not configure itself,
and the calls below need it.

Installing this package is what makes the iOS build link the `TappLiveActivities` module, declare
`NSSupportsLiveActivities`, add the push entitlement and put the Live Activity into the widget extension Tapp
creates. There is no switch to flip and nothing to do in Xcode.

## Use

```csharp
using TappGo;

TappLiveActivities.Start(new TappActivityRequest {
    Id      = "match-4471",   // from your Tapp back office
    EntryId = "screen-1",
    Seconds = 90 * 60,        // how long the countdowns run — a duration, not a deadline
}, result => {
    if (!result.Ok) Debug.LogWarning(result.Code);
});
TappLiveActivities.Update("match-4471", "screen-1");        // reloads that card, or restates its clock
TappLiveActivities.End("match-4471", "screen-1", 30 * 60);  // stays on screen half an hour more

foreach (var card in TappLiveActivities.Active().Value) { … } // read from iOS, so correct after a relaunch
```

**A card is the id and the screen together.** One activity id runs one card per screen, so starting
`"screen-2"` puts a second card up beside the first rather than moving it, and updates and ends name the pair.
`TappLiveActivities.Active()` reports both halves for everything that is up, which is what makes a card
started before a relaunch drivable without your keeping notes.

**Calls for the same card run in the order you make them**, from 2.1.2, which pins the iOS SDK at 2.2.3: an
`End` issued right after its `Start`, before the start's callback has run, waits for the start and then ends
the card. Through 2.1.1 that end could answer `Ok` for a card that was not up yet and leave it running, so on
those versions chain the end off the start's callback. Calls for different cards, and everything else, are
independent of one another.

**On Android** every call answers `liveActivitiesUnavailable` — no `#if` needed — and this package adds
nothing to the Android build.

| Moved from 2.0 | To |
|---|---|
| `Tapp.StartLiveActivity` / `UpdateLiveActivity` / `EndLiveActivity` | `TappLiveActivities.Start` / `Update` / `End` |
| `Tapp.ActiveLiveActivities` | `TappLiveActivities.Active` |

## Errors

| Code | What to do |
|---|---|
| `liveActivitiesUnavailable` | The player switched Live Activities off — or this is Android. Carry on silently. |
| `liveActivityAlreadyRunning` | That id already has a card on that screen. Update it, end it, or start a different screen. |
| `liveActivityStartFailed` | iOS refused; the message carries its reason. |

## Links

- [Documentation](https://documentation.tappgo.com/v2/unity/overview) · [Changelog](CHANGELOG.md)
