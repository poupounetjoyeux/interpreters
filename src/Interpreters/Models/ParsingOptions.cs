using System;
using System.Text;
using KaraW3B.Interpreters.Helpers;

namespace KaraW3B.Interpreters.Models
{
    internal sealed class ParsingOptions
    {
        public Encoding Encoding { get; private set; } = EncodingHelper.GetDefaultEncoding();
        public int? EncodingHeaderLine { get; private set; }

        public Version Version { get; private set; }
        public int? VersionHeaderLine { get; private set; }

        public ParsingOptions WithEncoding(Encoding encoding, int parsedEncodingHeaderLine)
        {
            Encoding = encoding;
            EncodingHeaderLine = parsedEncodingHeaderLine;
            return this;
        }

        public ParsingOptions WithVersion(Version version, int parsedVersionHeaderLine)
        {
            Version = version;
            VersionHeaderLine = parsedVersionHeaderLine;
            return this;
        }
    }
}