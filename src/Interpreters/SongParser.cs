using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Interpreters.Helpers;
using KaraW3B.Interpreters.Interfaces;
using KaraW3B.Interpreters.Models;
using KaraW3B.Interpreters.Models.Exceptions;
using KaraW3B.Interpreters.Parsers;

namespace KaraW3B.Interpreters
{
    public static class SongParser
    {
        private static readonly Regex EncodingRegex = new("^#ENCODING: *(?<encoding>.+) *$",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly Regex VersionRegex = new("^#VERSION: *(?<version>.+) *$",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static async Task<InterpreterResult> ParseSongAsync(FileInfo songFile, IInterpretableSong song,
            CancellationToken cancellationToken)
        {
            var result = new InterpreterResult();
            await ParseSongInternalAsync(result, songFile, song, new ParsingOptions(), cancellationToken);
            return result;
        }

        private static ParserBase GetParser(IInterpretableSong song, InterpreterResult result)
        {
            if (song.Version == null)
            {
                return new UnversionedFormatParser(song, result);
            }

            if (song.Version.Major == 1)
            {
                return new V1FormatParser(song, result);
            }

            if (song.Version.Major == 2)
            {
                return new V2FormatParser(song, result);
            }

            throw new KaraW3BParserException($"$The version {song.Version.ToString(3)} has no parser implementation");
        }

        private static async Task ParseSongInternalAsync(InterpreterResult result, FileInfo songFile, IInterpretableSong song, ParsingOptions options,
            CancellationToken cancellationToken)
        {
            if (!songFile.Exists)
            {
                throw new KaraW3BParserException($"The file {songFile.FullName} doesn't exist");
            }

            song.Reset();
            song.Version = options.Version;

            StreamReader reader = null;
            var line = 0;
            try
            {
                var fileStream = new FileStream(
                    songFile.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                reader = new StreamReader(fileStream, options.Encoding);

                var parser = GetParser(song, result);

                var eofMarkerFound = false;
                var allHeadersParsed = false;
                var parsedHeadersCount = 0;
                var mandatoryHeadersChecked = false;
                while (true)
                {
                    var fileLine = await reader.ReadLineAsync(cancellationToken);
                    if (fileLine == null)
                    {
                        break;
                    }

                    line++;
                    if (string.IsNullOrEmpty(fileLine.Trim()))
                    {
                        continue;
                    }

                    if (IsEofMarker(fileLine))
                    {
                        eofMarkerFound = true;
                        break;
                    }

                    if (!allHeadersParsed)
                    {
                        var parsedSpecificHeader = false;
                        if (TryParseSpecificEncoding(result, options, fileLine, line, out var reloadOptions))
                        {
                            if (parsedHeadersCount > 0)
                            {
                                result.AddWarning(
                                    "The #ENCODING header must be on top of the song file to improve loading performances");
                            }

                            parsedSpecificHeader = true;
                        }

                        if (TryParseSpecificVersion(options, fileLine, line, out reloadOptions))
                        {
                            if (parsedHeadersCount > 0)
                            {
                                result.AddWarning(
                                    "The #VERSION header must be on top of the song file to improve loading performances");
                            }

                            parsedSpecificHeader = true;
                        }

                        if (reloadOptions != null)
                        {
                            reader.Dispose();
                            reader = null;
                            await ParseSongInternalAsync(result, songFile, song, reloadOptions,
                                cancellationToken);
                            return;
                        }

                        if (parsedSpecificHeader)
                        {
                            continue;
                        }

                        if (parser.TryParseFileHeaderLine(fileLine, line))
                        {
                            parsedHeadersCount++;
                            continue;
                        }
                    }

                    if (!mandatoryHeadersChecked)
                    {
                        parser.CheckMandatoryHeadersAreDefined();
                        mandatoryHeadersChecked = true;
                    }

                    if (parser.TryParseFileNoteLine(fileLine, line))
                    {
                        allHeadersParsed = true;
                        continue;
                    }

                    result.AddError("The line cannot be parsed", line);
                }

                if (!eofMarkerFound)
                {
                    result.AddWarning(
                        "The song doesn't contains the 'E' EOF marker");
                }

                parser.PostParsing();
            }
            catch (Exception e)
            {
                result.AddFatal($"There was an exception when parsing the song file: {e}", line);
            }
            finally
            {
                reader?.Dispose();
            }
        }

        private static bool TryParseSpecificEncoding(InterpreterResult result, ParsingOptions options, string fileLine, int line,
            out ParsingOptions reloadOptions)
        {
            reloadOptions = null;
            var declaredEncoding = EncodingRegex.Match(fileLine);
            if (!declaredEncoding.Success)
            {
                return false;
            }

            if (options.EncodingHeaderLine == line)
            {
                return true;
            }

            if (options.EncodingHeaderLine.HasValue)
            {
                throw new KaraW3BParserException("The #ENCODING header is duplicated");
            }

            var sanitizedEncoding = EncodingHelper.SanitizeEncodingName(declaredEncoding.Groups["encoding"].Value);
            if (EncodingHelper.IsDefaultEncoding(sanitizedEncoding))
            {
                result.AddWarning(
                    "The #ENCODING header is deprecated. Your song is already in UTF-8 (which is recommended), you can just remove it!",
                    line);
            }
            else
            {
                result.AddWarning(
                    "The #ENCODING header is deprecated. All file should be in UTF-8 (without BOM)", line);
                reloadOptions = options.WithEncoding(EncodingHelper.GetEncoding(sanitizedEncoding), line);
            }

            return true;
        }

        private static bool TryParseSpecificVersion(ParsingOptions options, string fileLine, int line,
            out ParsingOptions reloadOptions)
        {
            reloadOptions = null;
            var declaredVersion = VersionRegex.Match(fileLine);
            if (!declaredVersion.Success)
            {
                return false;
            }

            if (options.VersionHeaderLine == line)
            {
                return true;
            }

            if (options.VersionHeaderLine.HasValue)
            {
                throw new KaraW3BParserException("The #VERSION header is duplicated");
            }

            if (!Version.TryParse(declaredVersion.Groups["version"].Value, out var version))
            {
                throw new KaraW3BParserException("The #VERSION header cannot be parsed. Format must be X.Y.Z");
            }

            reloadOptions = options.WithVersion(version, line);
            return true;
        }

        private static bool IsEofMarker(string line)
        {
            return line.Trim().Equals(InterpreterHelper.EndOfFileMarker.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
