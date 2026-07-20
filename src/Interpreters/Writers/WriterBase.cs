using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KaraW3B.Interpreters.Enums;
using KaraW3B.Interpreters.Helpers;
using KaraW3B.Interpreters.Interfaces;
using KaraW3B.Interpreters.Models.Exceptions;

namespace KaraW3B.Interpreters.Writers
{
    internal abstract class WriterBase
    {
        private StreamWriter _writer;
        protected IInterpretableSong Song { get; }

        protected WriterBase(IInterpretableSong song)
        {
            Song = song;
        }

        #region Abstractions

        protected abstract int BpmFactor { get; }

        protected abstract Version FormatVersion { get; }

        protected abstract Dictionary<string, Func<TimeSpan, double>> TimeHeaderFactories { get; }

        protected abstract Task WriteSpecificVersionCoreHeaders();

        protected abstract Task WriteSpecificVersionExtraHeaders();

        #endregion

        #region Utils

        protected async Task WriteHeader(string headerName, string headerValue)
        {
            if (headerValue == null)
            {
                return;
            }
            await _writer.WriteLineAsync($"#{headerName.ToUpperInvariant()}:{headerValue.Trim()}");
        }

        protected async Task WriteHeader(string headerName, decimal? headerValue)
        {
            if (!headerValue.HasValue)
            {
                return;
            }

            await WriteHeader(headerName, headerValue.Value.ToString(CultureInfo.InvariantCulture));
        }

        protected async Task WriteHeader(string headerName, int? headerValue)
        {
            if (!headerValue.HasValue)
            {
                return;
            }

            await WriteHeader(headerName, headerValue.Value.ToString(CultureInfo.InvariantCulture));
        }

        protected async Task WriteHeader(string headerName, List<string> headerValue)
        {
            if (headerValue == null || headerValue.Count == 0)
            {
                return;
            }

            await WriteHeader(headerName, string.Join(InterpreterHelper.ListSplitter, headerValue.Select(v => v.Trim())));
        }

        protected async Task WriteHeader(string headerName, TimeSpan? headerValue)
        {
            if (!headerValue.HasValue)
            {
                return;
            }

            if (TimeHeaderFactories == null || !TimeHeaderFactories.TryGetValue(headerName, out var factory))
            {
                throw new KaraW3BWriterException(
                    $"No time factory defined in parser {GetType().Name} for header #{headerName}");
            }

            await WriteHeader(headerName, factory(headerValue.Value).ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        public async Task WriteFileAsync(string filePath, bool overwrite)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new KaraW3BWriterException("An output file path is mandatory");
            }

            if (!overwrite && File.Exists(filePath))
            {
                throw new KaraW3BWriterException($"The file {filePath} already exists");
            }

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await using var streamWriter = new StreamWriter(fileStream);
            _writer = streamWriter;

            await WriteCommonCoreHeaders();
            await WriteSpecificVersionCoreHeaders();
            await WriteCommonExtraHeaders();
            await WriteSpecificVersionExtraHeaders();
            await WritePlayerHeaders();

            foreach (var songNotManagedHeader in Song.NotManagedHeaders)
            {
                await _writer.WriteLineAsync(songNotManagedHeader);
            }

            await WriteNotes();
            await _writer.WriteAsync(InterpreterHelper.EndOfFileMarker);
        }

        private async Task WriteCommonCoreHeaders()
        {
            if (FormatVersion != null)
            {
                await WriteHeader("VERSION", FormatVersion.ToString(3));
            }

            await WriteHeader("TITLE", Song.Title);
            await WriteHeader("ARTIST", Song.Artist);
            await WriteHeader("BPM", Song.Bpm / BpmFactor);
            await WriteHeader("GAP", Song.Gap);
            await WriteHeader("START", Song.Start);
            await WriteHeader("END", Song.End);
        }

        private async Task WriteCommonExtraHeaders()
        {
            await WriteHeader("COVER", Song.Cover);
            await WriteHeader("BACKGROUND", Song.Background);
            await WriteHeader("VIDEO", Song.Video);
            await WriteHeader("VIDEOGAP", Song.VideoGap);
            await WriteHeader("PREVIEWSTART", Song.PreviewStart);
            await WriteHeader("YEAR", Song.Year);

            await WriteHeader("GENRE", Song.Genres);
            await WriteHeader("LANGUAGE", Song.Languages);
            await WriteHeader("EDITION", Song.Editions);
            await WriteHeader("CREATOR", Song.Creators);

            await WriteHeader("COMMENT", Song.Comment);
        }

        private async Task WritePlayerHeaders()
        {
            foreach (var (playerNumber, playerName) in Song.GetPlayers())
            {
                await WriteHeader($"P{playerNumber}", playerName);
            }
        }

        private async Task WriteNotes()
        {
            var songPlayers = Song.GetPlayers();
            if (songPlayers.Count == 0)
            {
                foreach (var note in Song.GetNotes().OrderBy(n => n.StartBeat))
                {
                    await WriteNote(note);
                }
            }
            else
            {
                foreach (var playerNotes in Song.GetNotes().GroupBy(n => n.PlayerNumber))
                {
                    await _writer.WriteLineAsync($"P{playerNotes.Key}");
                    foreach (var note in playerNotes)
                    {
                        await WriteNote(note);
                    }
                }
            }
        }

        private async Task WriteNote(ISongNote note)
        {
            if (note.Type == NoteType.EndOfPhrase)
            {
                await _writer.WriteLineAsync($"- {note.StartBeat.ToString(CultureInfo.InvariantCulture)}");
                return;
            }

            if (!note.Duration.HasValue || !note.Pitch.HasValue || string.IsNullOrEmpty(note.Text))
            {
                // Note has missing info, ignoring it
                return;
            }

            await _writer.WriteLineAsync(
                $"{note.Type.GetNoteType()} {note.StartBeat.ToString(CultureInfo.InvariantCulture)} {note.Duration?.ToString(CultureInfo.InvariantCulture)} {note.Pitch?.ToString(CultureInfo.InvariantCulture)} {note.Text}");
        }
    }
}
