using System.Collections.Generic;

namespace KaraW3B.Interpreters.Models.Analyzes
{
    public sealed class FullAnalyzeResult
    {
        public List<InfoAnalyzeError> InfoErrors { get; } = new();
        public List<NoteAnalyzeError> NotesErrors { get; } = new();
    }
}