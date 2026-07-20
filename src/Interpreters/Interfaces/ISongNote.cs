namespace KaraW3B.Interpreters.Interfaces
{
    public interface ISongNote
    {
        int FileLine { get; }
        char Type { get; }
        int PlayerNumber { get; }
        int StartBeat { get; }
        int? Duration { get; }
        int? Pitch { get; }
        string Text { get; }
    }
}