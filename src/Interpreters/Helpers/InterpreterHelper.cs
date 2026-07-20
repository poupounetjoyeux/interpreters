using KaraW3B.Interpreters.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using KaraW3B.Interpreters.Models;
using KaraW3B.Interpreters.Models.Internals;

namespace KaraW3B.Interpreters.Helpers
{
    internal static class InterpreterHelper
    {
        public static readonly HashSet<string> DefaultMandatoryHeaders = new()
        {
            "TITLE",
            "ARTIST",
            "BPM"
        };

        public static readonly HashSet<char> SupportedNoteTypes = new()
        {
            DefaultNoteType,
            'G',
            'R',
            '*',
            ':'
        };

        public static readonly Dictionary<string, string> DefaultHeaderAliases = new()
        {
            { "AUTHOR", "CREATOR" },
            { "PREVIEW", "PREVIEWSTART" }
        };

        public const int MaxRecommendedHeaderSize = 2048;
        public const char ListSplitter = ',';
        public const char EndOfFileMarker = 'E';
        public const char EndOfPhraseNoteType = '-';
        public const char DefaultNoteType = 'F';

        #region Common regex

        public static readonly Regex HeaderRegex =
            new("^#(?<headerName>[A-Z0-9]+): *(?<headerValue>.*) *$",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static readonly Regex NoteRegex =
            new(@"^(?<noteType>.) (?<startBeat>\d+) (?<duration>\d+) (?<pitch>-?\d+) (?<text>.*)$",
                RegexOptions.Compiled | RegexOptions.Singleline);

        public static readonly Regex EndOfPhraseRegex =
            new(@"^-( (?<startBeat>\d+))?",
                RegexOptions.Compiled | RegexOptions.Singleline);

        // Space is for yass :(
        public static readonly Regex PlayerNumberRegex =
            new("^P *(?<playerNumber>[1-9])$",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        #endregion

        public static char GetNoteType(string noteType)
        {
            if (noteType.Length != 1)
            {
                return DefaultNoteType;
            }

            var type = noteType[0];
            return !SupportedNoteTypes.Contains(type) ? DefaultNoteType : type;
        }

        public static void ComputeMedleyTimesFromBeats(InterpreterResult result, IInterpretableSong song, int medleyStartBeat, int medleyEndBeat)
        {
            var medleyStartTime = TimesHelper.GetTimeFromBeat(song.Bpm, medleyStartBeat, song.Gap);
            if (!medleyStartTime.HasValue)
            {
                result.AddError("Unable to compute the medley start time");
                return;
            }

            var medleyEndTime = TimesHelper.GetTimeFromBeat(song.Bpm, medleyEndBeat, song.Gap);
            if (!medleyEndTime.HasValue)
            {
                result.AddError("Unable to compute the medley end time");
                return;
            }

            song.SetMedley(new ParsedMedley
            {
                MedleyStart = medleyStartTime.Value,
                MedleyEnd = medleyEndTime.Value
            });
        }

        public static bool TryExtractPlayerNumber(string text, out int playerNumber)
        {
            var playerHeaderMatch = PlayerNumberRegex.Match(text);
            if (!playerHeaderMatch.Success)
            {
                playerNumber = 0;
                return false;
            }

            playerNumber = int.Parse(playerHeaderMatch.Groups["playerNumber"].Value, CultureInfo.InvariantCulture);
            return true;
        }
    }
}