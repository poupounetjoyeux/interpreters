using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Interpreters.Enums;
using KaraW3B.Interpreters.Interfaces;
using KaraW3B.Interpreters.Models.Analyzes;

namespace KaraW3B.Interpreters.Helpers
{
    /// <summary>
    ///     Song error checkers are based on https://github.com/UltraStar-Deluxe/format and https://usdx.eu/format/
    ///     They aim to be able to not reject the maximum of files wherever they are up to date or not
    ///     The goal is also to add recommendations to be compatible with the more recent format
    /// </summary>
    public static class SongValidationHelper
    {
        public static async Task<FullAnalyzeResult> CheckFullSongErrorsAsync(
            IAnalyzableSong song,
            IEnumerable<ISongNote> notes, CancellationToken cancellationToken)
        {
            var result = new FullAnalyzeResult();
            result.InfoErrors.AddRange(await CheckInfosErrorsAsync(song, cancellationToken));
            result.NotesErrors.AddRange(await CheckNotesErrorsAsync(song, notes, cancellationToken));
            return result;
        }

        public static bool IsBpmValid(this IAnalyzableSong song)
        {
            return IsBpmValid(song.Bpm);
        }

        public static bool IsBpmValid(decimal bpm)
        {
            return bpm >= 1;
        }

        #region Infos

        public static Task<List<InfoAnalyzeError>> CheckInfosErrorsAsync(IAnalyzableSong song,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var errors = new List<InfoAnalyzeError>();

                errors.AddRange(CheckMandatoryValues(song));
                errors.AddRange(CheckPaths(song));

                if (CheckBpm(song.Bpm) is { } bpmError)
                {
                    errors.Add(bpmError);
                }

                errors.AddRange(CheckStartEnd(song.Start, song.End));
                errors.AddRange(CheckMedley(song.GetMedley()));
                errors.AddRange(CheckPlayers(song));
                errors.AddRange(CheckTimes(song));
                errors.AddRange(CheckUris(song));
                errors.AddRange(CheckLanguages(song.Version, song.Languages));

                return errors;
            }, cancellationToken);
        }

        private static IEnumerable<InfoAnalyzeError> CheckMandatoryValues(IAnalyzableSong song)
        {
            if (string.IsNullOrEmpty(song.Audio))
            {
                yield return new InfoAnalyzeError("A valid audio file is mandatory");
            }

            if (string.IsNullOrEmpty(song.Title))
            {
                yield return new InfoAnalyzeError("A title is mandatory");
            }

            if (string.IsNullOrEmpty(song.Artist))
            {
                yield return new InfoAnalyzeError("An artist is mandatory");
            }
        }

        public static InfoAnalyzeError CheckBpm(decimal bpm)
        {
            return IsBpmValid(bpm) ? null : new InfoAnalyzeError("The song BPM must be at least 1");
        }

        public static IEnumerable<InfoAnalyzeError> CheckStartEnd(TimeSpan? start, TimeSpan? end)
        {
            if (start is { TotalMilliseconds: < 0 })
            {
                yield return new InfoAnalyzeError("Start cannot be negative");
            }

            if (end is { TotalMilliseconds: < 0 })
            {
                yield return new InfoAnalyzeError("End cannot be negative");
            }

            if (start.HasValue && end.HasValue && end.Value <= start.Value)
            {
                yield return new InfoAnalyzeError("End must be greater than start");
            }
        }

        public static IEnumerable<InfoAnalyzeError> CheckMedley(ISongMedley medley)
        {
            if (medley == null)
            {
                yield break;
            }

            if (medley.MedleyStart >= medley.MedleyEnd)
            {
                yield return new InfoAnalyzeError("Medley end must be greater than medley start");
            }
        }

        public static IEnumerable<InfoAnalyzeError> CheckPaths(IAnalyzableSong song)
        {
            var pathsToCheck = new Dictionary<string, string>
            {
                { "audio", song.Audio },
                { "video", song.Video },
                { "cover", song.Cover },
                { "background", song.Background },
                { "vocals", song.Vocals },
                { "instrumental", song.Instrumental },
            };

            foreach (var pathToCheck in pathsToCheck)
            {
                if (!string.IsNullOrEmpty(pathToCheck.Value) && Path.IsPathFullyQualified(pathToCheck.Value))
                {
                    yield return new InfoAnalyzeError(
                        $"The {pathToCheck.Key} path must be a relative path");
                }
            }
        }

        public static IEnumerable<InfoAnalyzeError> CheckLanguages(Version version, IEnumerable<string> languages)
        {
            if (version == null)
            {
                // Unversioned files didn't respect this
                yield break;
            }

            foreach (var invalidLanguage in languages.Where(l => !LanguagesHelper.IsValidLanguage(l)))
            {
                yield return new InfoAnalyzeError(
                    $"The language '{invalidLanguage}' is not an ISO 639.2 english name language");
            }
        }

        public static IEnumerable<InfoAnalyzeError> CheckPlayers(IAnalyzableSong song)
        {
            var songPlayers = song.GetPlayers();
            foreach (var songPlayer in songPlayers)
            {
                if (string.IsNullOrEmpty(songPlayer.Value))
                {
                    yield return new InfoAnalyzeError(
                        $"The player {songPlayer.Key} has no name defined (#P{songPlayer.Key} header)");
                }

                if (songPlayer.Key > songPlayers.Count)
                {
                    yield return new InfoAnalyzeError(
                        $"Prefer using player indexes by ascending order. #P{songPlayer.Key} could be replaced by a lower player number value",
                        true);
                }
            }
        }

        public static IEnumerable<InfoAnalyzeError> CheckUris(IAnalyzableSong song)
        {
            var urisToCheck = new Dictionary<string, string>
            {
                { "audio", song.AudioUrl },
                { "video", song.VideoUrl },
                { "cover", song.CoverUrl },
                { "background", song.BackgroundUrl }
            };

            foreach (var uriToCheck in urisToCheck)
            {
                if (!string.IsNullOrEmpty(uriToCheck.Value) &&
                    !Uri.IsWellFormedUriString(uriToCheck.Value, UriKind.Absolute))
                {
                    yield return new InfoAnalyzeError(
                        $"The {uriToCheck.Key} URL is not a valid URL according to RFC 1738", true);
                }
            }
        }

        public static IEnumerable<InfoAnalyzeError> CheckTimes(IAnalyzableSong song)
        {
            var timesToCheck = new Dictionary<string, TimeSpan?>
            {
                { "GAP", song.Gap },
                { "preview start", song.PreviewStart }
            };

            foreach (var timeToCheck in timesToCheck)
            {
                var timeError = CheckPositiveOrZeroTime(timeToCheck.Key, timeToCheck.Value);
                if (timeError != null)
                {
                    yield return timeError;
                }
            }
        }

        private static InfoAnalyzeError CheckPositiveOrZeroTime(string timeName, TimeSpan? time)
        {
            if (!time.HasValue)
            {
                return null;
            }

            if (time.Value == TimeSpan.Zero)
            {
                return new InfoAnalyzeError($"The {timeName} is set to 0, you can remove it from your file", true);
            }

            return time.Value.TotalMilliseconds < 0 ? new InfoAnalyzeError($"The {timeName} cannot be negative") : null;
        }

        #endregion

        public static Task<List<NoteAnalyzeError>> CheckNotesErrorsAsync(IAnalyzableSong song,
            IEnumerable<ISongNote> songNotes,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var errors = new List<NoteAnalyzeError>();
                var processedPlayers = new HashSet<int>();

                foreach (var playerNotes in songNotes.GroupBy(n => n.PlayerNumber))
                {
                    processedPlayers.Add(playerNotes.Key);
                    var orderedPlayerNotes = playerNotes.OrderBy(n => n.StartBeat).ToArray();

                    for (var i = 0; i < orderedPlayerNotes.Length; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var analyzedNote = orderedPlayerNotes[i];

                        if (analyzedNote.StartBeat < 0)
                        {
                            errors.Add(new NoteAnalyzeError("The note cannot have a negative start beat",
                                analyzedNote));
                        }

                        if (analyzedNote.Type == NoteType.EndOfPhrase)
                        {
                            if (i == 0)
                            {
                                errors.Add(new NoteAnalyzeError("The first note must never be an end of phrase",
                                    analyzedNote));
                                continue;
                            }

                            if (orderedPlayerNotes[i - 1].Type == NoteType.EndOfPhrase)
                            {
                                errors.Add(new NoteAnalyzeError("There is subsequent end of phrase markers",
                                    analyzedNote));
                            }

                            if (i == orderedPlayerNotes.Length - 1)
                            {
                                errors.Add(new NoteAnalyzeError("The last note must never be an end of phrase",
                                    analyzedNote));
                            }
                        }
                        else
                        {
                            if (!analyzedNote.Pitch.HasValue)
                            {
                                errors.Add(new NoteAnalyzeError("The pitch is mandatory", analyzedNote));
                            }

                            if (string.IsNullOrEmpty(analyzedNote.Text))
                            {
                                errors.Add(new NoteAnalyzeError("A text is mandatory for note", analyzedNote));
                            }

                            if (analyzedNote.Duration < 1)
                            {
                                errors.Add(new NoteAnalyzeError("Duration is less than 1", analyzedNote));
                            }
                        }

                        if (i == 0)
                        {
                            continue;
                        }

                        var previousNote = orderedPlayerNotes[i - 1];
                        var previousEndBeat = previousNote.StartBeat;
                        if (previousNote.Duration.HasValue)
                        {
                            previousEndBeat += previousNote.Duration.Value;
                        }

                        if (analyzedNote.StartBeat < previousEndBeat)
                        {
                            errors.Add(new NoteAnalyzeError("There is an overlap with the previous note",
                                analyzedNote));
                        }
                    }
                }

                if (processedPlayers.Count == 0)
                {
                    errors.Add(new NoteAnalyzeError("There is no note defined"));
                    return errors;
                }

                // No player info or only standard player one
                var songPlayers = song.GetPlayers();
                if (processedPlayers.Count == 1 && songPlayers.Count == 0)
                {
                    return errors;
                }

                foreach (var playerNumber in processedPlayers.Where(playerNumber =>
                             !songPlayers.ContainsKey(playerNumber)))
                {
                    errors.Add(new NoteAnalyzeError(
                        $"There is notes for player {playerNumber} defined but no corresponding player name using #P{playerNumber} header"));
                }

                foreach (var songPlayer in songPlayers.Where(songPlayer => !processedPlayers.Contains(songPlayer.Key)))
                {
                    errors.Add(new NoteAnalyzeError(
                        $"There is no notes defined for player {songPlayer.Key} that is defined with the #P{songPlayer.Key} header"));
                }

                return errors;
            }, cancellationToken);
        }
    }
}