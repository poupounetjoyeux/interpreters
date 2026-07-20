using KaraW3B.Interpreters.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace KaraW3B.Interpreters.Models.Internals
{
    internal sealed class ParsedMedley : ISongMedley
    {
        public TimeSpan MedleyStart { get; init; }

        public TimeSpan MedleyEnd { get; init; }
    }
}
