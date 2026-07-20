using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaraW3B.Interpreters.Helpers;
using KaraW3B.Interpreters.Interfaces;

namespace KaraW3B.Interpreters.Writers
{
    internal sealed class UnversionedFormatWriter : WriterBase
    {
        public UnversionedFormatWriter(IInterpretableSong song) : base(song)
        {
        }

        protected override int BpmFactor => 4;

        protected override Version FormatVersion => null;

        protected override Dictionary<string, Func<TimeSpan, double>> TimeHeaderFactories => new()
        {
            { "GAP", t => t.TotalMilliseconds },
            { "VIDEOGAP", t => t.TotalSeconds },
            { "PREVIEWSTART", t => t.TotalSeconds },
            { "START", t => t.TotalSeconds },
            { "END", t => t.TotalMilliseconds }
        };

        protected override async Task WriteSpecificVersionCoreHeaders()
        {
            await WriteHeader("MP3", Song.Audio);
        }

        protected override async Task WriteSpecificVersionExtraHeaders()
        {
            var medley = Song.GetMedley();
            if (medley == null)
            {
                return;
            }

            await WriteHeader("MEDLEYSTARTBEAT", TimesHelper.GetBeatFromTime(Song.Bpm, medley.MedleyStart, Song.Gap));
            await WriteHeader("MEDLEYENDBEAT", TimesHelper.GetBeatFromTime(Song.Bpm, medley.MedleyEnd, Song.Gap));
        }
    }
}
