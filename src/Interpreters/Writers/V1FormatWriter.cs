using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaraW3B.Interpreters.Helpers;
using KaraW3B.Interpreters.Interfaces;

namespace KaraW3B.Interpreters.Writers
{
    internal class V1FormatWriter : WriterBase
    {
        public V1FormatWriter(IInterpretableSong song) : base(song)
        {
        }

        protected override int BpmFactor => 4;

        protected override Version FormatVersion => new(1, 1, 0);

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
            await WriteHeader("AUDIO", Song.Audio);
        }

        protected override async Task WriteSpecificVersionExtraHeaders()
        {
            await WriteHeader("AUDIOURL", Song.AudioUrl);
            await WriteHeader("VIDEOURL", Song.VideoUrl);
            await WriteHeader("COVERURL", Song.CoverUrl);
            await WriteHeader("BACKGROUNDURL", Song.BackgroundUrl);
            await WriteHeader("PROVIDEDBY", Song.ProvidedBy);
            await WriteHeader("TAGS", Song.Tags);
            await WriteHeader("VOCALS", Song.Vocals);
            await WriteHeader("INSTRUMENTAL", Song.Instrumental);
            await WriteHeader("RENDITION", Song.Rendition);
            await WriteMedleyHeaders();
        }

        protected virtual async Task WriteMedleyHeaders()
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
