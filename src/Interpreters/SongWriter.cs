using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Interpreters.Interfaces;
using KaraW3B.Interpreters.Models.Exceptions;
using KaraW3B.Interpreters.Writers;

namespace KaraW3B.Interpreters
{
    public static class SongWriter
    {
        public static Task WriteSongAsync(IInterpretableSong song, string filePath, bool overwrite,
            CancellationToken cancellationToken)
        {
            var writer = GetWriter(song);
            return writer.WriteFileAsync(filePath, overwrite);
        }

        private static WriterBase GetWriter(IInterpretableSong song)
        {
            if (song.Version == null)
            {
                return new UnversionedFormatWriter(song);
            }

            if (song.Version.Major == 1)
            {
                return new V1FormatWriter(song);
            }

            if (song.Version.Major == 2)
            {
                return new V2FormatWriter(song);
            }

            throw new KaraW3BWriterException($"$The version {song.Version.ToString(3)} has no writer implementation");
        }
    }
}
