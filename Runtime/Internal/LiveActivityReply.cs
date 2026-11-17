using System;
using TappGo.Modules;

namespace TappGo.LiveActivities.Internal
{
    /// <summary>
    /// Reads the Live Activity replies. The envelope rules every Tapp reply shares are
    /// <see cref="TappEnvelope"/>'s; what is here is the shape of each <c>value</c>.
    /// </summary>
    internal static class LiveActivityReply
    {
        /// <summary><c>{"value":{"activityID":"…"}}</c>.</summary>
        internal static TappResult<string> ParseActivityId(string json)
        {
            var envelope = TappEnvelope.Decode<ActivityStartedEnvelope>(json);
            if (envelope == null)
            {
                return TappEnvelope.Unreadable<string>(json);
            }

            return envelope.ok
                ? TappEnvelope.Success(envelope.value?.activityID)
                : TappEnvelope.Failure<string>(envelope.code, envelope.message);
        }

        /// <summary>
        /// <c>{"value":{"activities":[{"id":…,"activityID":…,"entryID":…,"state":…,"updateToken":…}]}}</c>.
        /// </summary>
        internal static TappResult<TappActivityInfo[]> ParseActivities(string json)
        {
            var envelope = TappEnvelope.Decode<ActivitiesEnvelope>(json);
            if (envelope == null)
            {
                return TappEnvelope.Unreadable<TappActivityInfo[]>(json);
            }

            if (!envelope.ok)
            {
                return TappEnvelope.Failure<TappActivityInfo[]>(envelope.code, envelope.message);
            }

            var reported = envelope.value?.activities ?? Array.Empty<ActivityDto>();
            var activities = new TappActivityInfo[reported.Length];
            for (var index = 0; index < reported.Length; index++)
            {
                var dto = reported[index];
                // `state` is kept as whatever the system said, including a value this build has never heard
                // of — see TappActivityState. Mapping it to an enum here is what would lose it.
                //
                // `entryID` is absent for a card that names no screen, and JsonUtility leaves a missing string
                // as "" rather than null — normalised here, because the difference is "unaddressable card"
                // versus "screen the empty string", and every caller would otherwise have to test for both.
                activities[index] = new TappActivityInfo(
                    dto.id,
                    dto.activityID,
                    string.IsNullOrEmpty(dto.entryID) ? null : dto.entryID,
                    new TappActivityState(dto.state),
                    // Normalised for the same reason as `entryID`: absent means the SDK holds no token for this
                    // card — the ordinary state in the second after a start, and permanently once it has ended —
                    // and every caller would otherwise have to test for both null and "".
                    string.IsNullOrEmpty(dto.updateToken) ? null : dto.updateToken);
            }

            return TappEnvelope.Success(activities);
        }

        // Wire shapes. Field names match docs/NATIVE-CONTRACT.md exactly and are never renamed; `Serializable`
        // classes rather than structs because JsonUtility leaves a missing nested object as null, which is how
        // a malformed `value` is detected instead of silently reading as empty.

        [Serializable]
        private class ActivityStartedEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public ActivityStartedDto value;
        }

        [Serializable]
        private class ActivitiesEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public ActivitiesDto value;
        }

        [Serializable]
        private class ActivityStartedDto
        {
            public string activityID;
        }

        /// <summary>
        /// The list reply's <c>value</c> is an <b>object wrapping</b> the array, not the array itself.
        /// </summary>
        /// <remarks>
        /// Deliberate on the native side — a count, a <c>truncated</c> flag or a cursor can be added beside
        /// <c>activities</c> later, where a bare array at the top of <c>value</c> could only grow by breaking
        /// every wrapper reading it. Getting this wrong is silent: <c>JsonUtility</c> handed an object where
        /// it expects an array leaves the field <c>null</c> without an error, so the reply stays
        /// <c>ok</c> and every running activity disappears.
        /// </remarks>
        [Serializable]
        private class ActivitiesDto
        {
            public ActivityDto[] activities;
        }

        [Serializable]
        private class ActivityDto
        {
            public string id;
            public string activityID;
            public string entryID;
            public string state;
            public string updateToken;
        }
    }
}
