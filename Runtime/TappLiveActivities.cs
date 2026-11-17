using System;
using TappGo.LiveActivities.Internal;
using TappGo.Modules;

namespace TappGo
{
    /// <summary>
    /// Tapp Live Activities: start, reload and end the cards on the Lock Screen and in the Dynamic Island.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Live Activity is rendered natively, by the widget extension Tapp adds to your Xcode project. Nothing
    /// here draws anything: you name a campaign and a screen, both configured server-side, and the SDK
    /// resolves the content. That is what lets the design change without a new build.
    /// </para>
    /// <para>
    /// <b>iOS only, and not an error to guard against.</b> On Android every call answers
    /// <see cref="TappErrorCode.LiveActivitiesUnavailable"/> — the same code an iOS player who switched Live
    /// Activities off produces — so one branch covers "not available here" on both.
    /// </para>
    /// <para>
    /// Configure the SDK first with <see cref="Tapp.Configure"/>; Tapp does nothing until it is called.
    /// <b>In the Editor every call is a no-op that logs</b>, and <b>callbacks arrive on the main thread</b>.
    /// </para>
    /// </remarks>
    public static class TappLiveActivities
    {
        /// <summary>
        /// Starts a Live Activity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Requires the player to have Live Activities enabled; if they don't, this fails with
        /// <see cref="TappErrorCode.LiveActivitiesUnavailable"/> — expected, not exceptional, so don't surface
        /// it as an error.
        /// </para>
        /// <para>
        /// <b>A card is an id and a screen.</b> Starting that same pair again fails with
        /// <see cref="TappErrorCode.LiveActivityAlreadyRunning"/>, but naming a <i>different</i>
        /// <see cref="TappActivityRequest.EntryId"/> puts a second card up alongside the first — one campaign
        /// can have several of its screens running at once, each with its own clock, content and end. A card
        /// never changes screen: to show another, start it.
        /// </para>
        /// </remarks>
        /// <param name="request">Which activity and which screen.</param>
        /// <param name="completion">
        /// Called on the main thread. On success the value is the system's activity identifier — useful for
        /// logs, but you don't need to keep it: later calls address the activity by its Tapp id.
        /// </param>
        public static void Start(TappActivityRequest request, Action<TappResult<string>> completion = null)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.EntryId))
            {
                TappEnvelope.DeliverLocal(completion, TappEnvelope.Rejected<string>(TappErrorCode.InvalidRequest, "Id and EntryId are both required."));
                return;
            }

            var json = ScreenJson(request.Id, request.EntryId, request.Seconds);
            LiveActivitiesBridge.Start(json, reply => TappEnvelope.Deliver(completion, reply, LiveActivityReply.ParseActivityId));
        }

        /// <summary>
        /// Reloads the card <paramref name="id"/>/<paramref name="entryId"/> names, and optionally restarts its
        /// clock.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>The screen addresses the card; it does not move it.</b> A card's screen is fixed for its whole
        /// life, so this names <i>which</i> card to reload rather than somewhere to send one. To show a
        /// different screen, <see cref="Start"/> it — that puts a second card up alongside this one.
        /// </para>
        /// <para>
        /// <b>A card that isn't running succeeds and does nothing</b>, whether nothing is up for that id or
        /// nothing is on that screen. The player can dismiss a card at any moment, including between your
        /// deciding to update and this call — that race is ordinary, so it is absorbed here. Don't guard against
        /// it with <see cref="Active"/>.
        /// </para>
        /// </remarks>
        /// <param name="id">The activity's Tapp id, as started.</param>
        /// <param name="entryId">The screen naming which card to reload, as started. Case is ignored.</param>
        /// <param name="seconds">
        /// A new duration for the card's countdowns, measured from <b>now</b> — so <c>300</c> is "five more
        /// minutes" whenever it arrives, not five minutes from when the card started. <c>null</c> leaves
        /// whatever clock is running exactly as it is, so reloading content never disturbs a countdown.
        /// </param>
        /// <param name="completion">Called on the main thread.</param>
        public static void Update(
            string id,
            string entryId,
            int? seconds = null,
            Action<TappResult> completion = null)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(entryId))
            {
                TappEnvelope.DeliverLocal(completion, TappEnvelope.Rejected(TappErrorCode.InvalidRequest, "id and entryId are both required."));
                return;
            }

            var json = ScreenJson(id, entryId, seconds);
            LiveActivitiesBridge.Update(json, reply => TappEnvelope.Deliver(completion, reply, TappEnvelope.Parse));
        }

        /// <summary>
        /// Ends the card <paramref name="id"/>/<paramref name="entryId"/> names. Does nothing if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>The screen is always named</b>, exactly as it is when starting: a campaign can legitimately have
        /// two of its screens up at once, so an "end everything" would silently take down a card you meant to
        /// leave running. To end all of them, call this once per screen — each is independent, so there is no
        /// ordering to get right, and <see cref="Active"/> reports the screens to pass.
        /// </para>
        /// <para>
        /// <b>Ending and dismissing are two different moments.</b> The activity stops being live immediately —
        /// nothing can reach it afterwards — while <paramref name="seconds"/> only governs how much longer it
        /// stays on the Lock Screen.
        /// </para>
        /// <para>
        /// <b>That delay is one-shot.</b> iOS caps it at four hours and reports nothing when it does, so an
        /// over-long value silently becomes four hours and <i>cannot be revised</i> — not by you and not from
        /// the server. The only ways back are the player swiping the activity away, or calling this again with
        /// no delay.
        /// </para>
        /// </remarks>
        /// <param name="id">The activity's Tapp id, as started.</param>
        /// <param name="entryId">The screen whose card to end, as started. Case is ignored.</param>
        /// <param name="seconds">
        /// How much longer the ended card stays on screen, in <b>whole seconds from the end</b>, or <c>null</c>
        /// to remove it at once. Zero or a negative value dismisses at once too.
        /// <para>
        /// <b>Not the same <c>seconds</c> as <see cref="TappActivityRequest.Seconds"/></b>, which runs the
        /// design's countdowns and marks content stale while ending nothing. This one only defers the
        /// disappearance of a card that has already ended. They share a name because the native API does.
        /// </para>
        /// </param>
        /// <param name="completion">Called on the main thread.</param>
        public static void End(
            string id,
            string entryId,
            int? seconds = null,
            Action<TappResult> completion = null)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(entryId))
            {
                TappEnvelope.DeliverLocal(completion, TappEnvelope.Rejected(TappErrorCode.InvalidRequest, "id and entryId are both required."));
                return;
            }

            var json = new JsonBuilder()
                .Add("id", id)
                .Add("entryID", entryId)
                .AddInt("seconds", seconds)
                .Build();

            LiveActivitiesBridge.End(json, reply => TappEnvelope.Deliver(completion, reply, TappEnvelope.Parse));
        }

        /// <summary>
        /// Every Tapp Live Activity card iOS currently reports.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Read from the system, not from anything this SDK remembers, so it is correct after a relaunch —
        /// activities outlive your process. Use it to reconcile on startup, or to check whether a card is
        /// already up before starting it again.
        /// </para>
        /// <para>
        /// Each entry carries the screen it is on, which is what makes it drivable: that
        /// <see cref="TappActivityInfo.EntryId"/> is what <see cref="Update"/> and
        /// <see cref="End"/> take, so nothing here has to be remembered between launches.
        /// </para>
        /// </remarks>
        public static TappResult<TappActivityInfo[]> Active()
        {
            return LiveActivityReply.ParseActivities(LiveActivitiesBridge.Active());
        }

        /// <summary>The request shape a start and an update share — they're addressed identically by design.</summary>
        private static string ScreenJson(string id, string entryId, int? seconds)
        {
            return new JsonBuilder()
                .Add("id", id)
                .Add("entryID", entryId)
                .AddInt("seconds", seconds)
                .Build();
        }
    }
}
