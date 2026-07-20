using System;
using System.Collections.Generic;
using KaraW3B.Interpreters.Interfaces;

namespace KaraW3B.Interpreters.Tests.Mocks
{
    internal sealed class SongMock : IInterpretableSong
    {
        public Version Version { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Audio { get; set; }

        public decimal Bpm { get; set; }

        public TimeSpan? Gap { get; set; }

        public TimeSpan? Start { get; set; }

        public TimeSpan? End { get; set; }

        public string Video { get; set; }

        public TimeSpan? VideoGap { get; set; }

        public string Vocals { get; set; }

        public string Instrumental { get; set; }

        public TimeSpan? PreviewStart { get; set; }

        public string Cover { get; set; }

        public string Background { get; set; }

        public string AudioUrl { get; set; }

        public string VideoUrl { get; set; }

        public string CoverUrl { get; set; }

        public string BackgroundUrl { get; set; }

        public string Comment { get; set; }

        public string ProvidedBy { get; set; }

        public string Rendition { get; set; }

        public int? Year { get; set; }

        public Dictionary<int, string> Players { get; } = new();

        public List<ISongNote> Notes { get; } = new();

        public void AddPlayer(int playerNumber, string playerName)
        {
            Players.Add(playerNumber, playerName);
        }

        public Dictionary<int, string> GetPlayers()
        {
            return Players;
        }

        public void AddNote(ISongNote note)
        {
            Notes.Add(note);
        }

        public IReadOnlyCollection<ISongNote> GetNotes()
        {
            return Notes;
        }

        public List<string> Genres { get; } = new();

        public List<string> Languages { get; } = new();

        public List<string> Editions { get; } = new();

        public List<string> Tags { get; } = new();

        public List<string> Creators { get; } = new();

        public List<string> NotManagedHeaders { get; } = new();

        public ISongMedley Medley { get; set; }

        public void SetMedley(ISongMedley medley)
        {
            Medley = medley;
        }

        public ISongMedley GetMedley()
        {
            return Medley;
        }

        public void Reset()
        {
            Bpm = -1;
            Title = null;
            Artist = null;
            Audio = null;
            Gap = null;
            Start = null;
            End = null;
            Players.Clear();

            Cover = null;
            Background = null;
            Video = null;
            VideoGap = null;
            Vocals = null;
            Instrumental = null;
            PreviewStart = null;
            Medley = null;
            Year = null;

            Genres.Clear();
            Languages.Clear();
            Editions.Clear();
            Tags.Clear();
            Creators.Clear();

            ProvidedBy = null;
            Comment = null;
            AudioUrl = null;
            VideoUrl = null;
            CoverUrl = null;
            BackgroundUrl = null;
            Rendition = null;
            NotManagedHeaders.Clear();

            Notes.Clear();
        }
    }
}
