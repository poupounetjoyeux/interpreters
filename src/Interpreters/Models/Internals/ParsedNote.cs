using KaraW3B.Interpreters.Interfaces;

namespace KaraW3B.Interpreters.Models.Internals
{
    internal sealed class ParsedNote : ISongNote
    {
        public int FileLine { get; init; }

        public char Type { get; init; }

        public int PlayerNumber { get; init; }

        public int StartBeat { get; set; }

        public int? Duration { get; init; }

        public int? Pitch { get; init; }

        public string Text { get; init; }
    }
}
