using System;
using System.Collections.Generic;

namespace KaraW3B.Interpreters.Interfaces
{
    public interface IInterpretableSong
    {
        Version Version { get; set; }
        string Title { get; set; }
        string Artist { get; set; }
        string Audio { get; set; }
        decimal Bpm { get; set; }
        TimeSpan? Gap { get; set; }
        TimeSpan? Start { get; set; }
        TimeSpan? End { get; set; }
        string Video { get; set; }
        TimeSpan? VideoGap { get; set; }
        string Vocals { get; set; }
        string Instrumental { get; set; }
        TimeSpan? PreviewStart { get; set; }
        string Cover { get; set; }
        string Background { get; set; }
        string AudioUrl { get; set; }
        string VideoUrl { get; set; }
        string CoverUrl { get; set; }
        string BackgroundUrl { get; set; }
        string Comment { get; set; }
        string ProvidedBy { get; set; }
        string Rendition { get; set; }
        int? Year { get; set; }
        void AddPlayer(int playerNumber, string playerName);
        Dictionary<int, string> GetPlayers();
        void AddNote(ISongNote note);
        IReadOnlyCollection<ISongNote> GetNotes();
        List<string> Genres { get; }
        List<string> Languages { get; }
        List<string> Editions { get; }
        List<string> Tags { get; }
        List<string> Creators { get; }
        List<string> NotManagedHeaders { get; }
        void SetMedley(ISongMedley medley);
        ISongMedley GetMedley();
        void Reset();
    }
}
