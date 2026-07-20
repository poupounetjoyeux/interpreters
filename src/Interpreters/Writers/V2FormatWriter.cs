using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaraW3B.Interpreters.Interfaces;

namespace KaraW3B.Interpreters.Writers
{
    internal sealed class V2FormatWriter: V1FormatWriter
    {
        protected override Version FormatVersion => new(2, 0, 0);

        protected override int BpmFactor => 1;

        protected override Dictionary<string, Func<TimeSpan, double>> TimeHeaderFactories => new()
        {
            { "GAP", t => t.TotalMilliseconds },
            { "VIDEOGAP", t => t.TotalMilliseconds },
            { "PREVIEWSTART", t => t.TotalMilliseconds },
            { "START", t => t.TotalMilliseconds },
            { "END", t => t.TotalMilliseconds },
            { "MEDLEYSTART", t => t.TotalMilliseconds },
            { "MEDLEYEND", t => t.TotalMilliseconds }
        };

        public V2FormatWriter(IInterpretableSong song) : base(song)
        {
        }

        protected override async Task WriteMedleyHeaders()
        {
            var medley = Song.GetMedley();
            if (medley == null)
            {
                return;
            }

            await WriteHeader("MEDLEYSTART", medley.MedleyStart);
            await WriteHeader("MEDLEYEND", medley.MedleyEnd);
        }
    }
}
