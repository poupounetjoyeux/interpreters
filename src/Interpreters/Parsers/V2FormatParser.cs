using System;
using System.Collections.Generic;
using KaraW3B.Interpreters.Interfaces;
using KaraW3B.Interpreters.Models.Internals;

namespace KaraW3B.Interpreters.Parsers
{
    internal sealed class V2FormatParser : V1FormatParser
    {
        private TimeSpan? _medleyStartTime;
        private TimeSpan? _medleyEndTime;

        public V2FormatParser(IInterpretableSong song) : base(song)
        {
        }

        protected override int BpmFactor => 1;

        protected override Dictionary<string, string> DeprecatedHeaderAliases => new()
        {
            { "INSTRUMENTALS", "INSTRUMENTAL" },
            { "MEDLEYSTARTBEAT", "MEDLEYSTART" },
            { "MEDLEYENDBEAT", "MEDLEYEND" }
        };

        protected override Dictionary<string, Func<double, TimeSpan>> TimeHeaderFactories => new()
        {
            { "GAP", TimeSpan.FromMilliseconds },
            { "VIDEOGAP", TimeSpan.FromMilliseconds },
            { "PREVIEWSTART", TimeSpan.FromMilliseconds },
            { "START", TimeSpan.FromMilliseconds },
            { "END", TimeSpan.FromMilliseconds },
            { "MEDLEYSTART", TimeSpan.FromMilliseconds },
            { "MEDLEYEND", TimeSpan.FromMilliseconds }
        };

        protected override bool HandleMedleyHeader(string headerName, string headerValue, int line)
        {
            switch (headerName)
            {
                case "MEDLEYSTART":
                    ParseTimeHeaderValue(headerName, headerValue, line, medleyStart => _medleyStartTime = medleyStart);
                    return true;

                case "MEDLEYEND":
                    ParseTimeHeaderValue(headerName, headerValue, line, medleyEnd => _medleyEndTime = medleyEnd);
                    return true;
            }

            return false;
        }

        protected override void ComputeMedley()
        {
            if (!_medleyStartTime.HasValue && !_medleyEndTime.HasValue)
            {
                return;
            }

            if (!_medleyStartTime.HasValue)
            {
                Error("#MEDLEYSTART is mandatory when #MEDLEYENDBEAT is specified");
                return;
            }

            if (!_medleyEndTime.HasValue)
            {
                Error("#MEDLEYEND is mandatory when #MEDLEYSTARTBEAT is specified");
                return;
            }

            Song.SetMedley(new ParsedMedley
            {
                MedleyStart = _medleyStartTime.Value,
                MedleyEnd = _medleyEndTime.Value
            });
        }
    }
}