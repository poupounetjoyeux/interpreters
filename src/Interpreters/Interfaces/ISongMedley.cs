using System;

namespace KaraW3B.Interpreters.Interfaces
{
    public interface ISongMedley
    {
        TimeSpan MedleyStart { get; }
        TimeSpan MedleyEnd { get; }
    }
}