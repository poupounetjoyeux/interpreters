using KaraW3B.Interpreters.Helpers;
using KaraW3B.Interpreters.Tests.Mocks;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KaraW3B.Interpreters.Tests.Interpreters
{
    [TestFixture]
    public class V2FormatInterpreterTest
    {
        [Test]
        public void TestSongIsCorrectlyParsed()
        {
            var song = new SongMock();

            var songTestFile =
                new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "V2.txt"));
            Assert.That(songTestFile.Exists, Is.True, $"The test file {songTestFile.FullName} cannot be found");

            Assert.DoesNotThrowAsync((Func<Task>)(() => SongParser.ParseSongAsync(songTestFile, song, CancellationToken.None)), "The song parser throws an exception");

            Assert.That(song.Version, Is.EqualTo(new Version(2, 0, 0)));
            Assert.That(song.Title, Is.EqualTo("Test Title"));
            Assert.That(song.Artist, Is.EqualTo("Test Artist"));
            Assert.That(song.Bpm, Is.EqualTo(480.40m));
            Assert.That(song.Gap, Is.EqualTo(TimeSpan.FromMilliseconds(15397)));
            Assert.That(song.Audio, Is.EqualTo("audio.mp3"));
            Assert.That(song.Video, Is.EqualTo("video.mp4"));
            Assert.That(song.Cover, Is.EqualTo("cover.jpg"));

            Assert.That(song.Editions, Is.EquivalentTo(new[]{ "Années2020" }));
            Assert.That(song.Languages, Is.EquivalentTo(new[]{ "French" }));
            Assert.That(song.Genres, Is.EquivalentTo(new[]{ "Variété" }));
            Assert.That(song.Creators, Is.EquivalentTo(new[]{ "Test" }));
            Assert.That(song.Year, Is.EqualTo(2020));

            Assert.That(song.NotManagedHeaders, Is.EquivalentTo(new[]{ "#NOTMANAGED:Hey!" }));

            Assert.That(song.Notes, Has.Count.EqualTo(9));
            Assert.That(song.Notes.Any(n => n.Type == InterpreterHelper.EndOfPhraseNoteType), Is.True);
            Assert.That(song.Notes.Any(n => n.Type == '*'), Is.True);
            Assert.That(song.Notes.Any(n => n.Type == 'G'), Is.True);
            Assert.That(song.Notes.Any(n => n.Type == 'F'), Is.True);
            Assert.That(song.Notes.Any(n => n.Type == ':'), Is.True);
            Assert.That(song.Notes.Any(n => n.Type == 'R'), Is.True);

            var firstNote = song.Notes.OrderBy(n => n.StartBeat).First();
            Assert.That(firstNote.PlayerNumber, Is.EqualTo(1));
            Assert.That(firstNote.Text, Is.EqualTo("Hello"));
            Assert.That(firstNote.Type, Is.EqualTo(':'));
            Assert.That(firstNote.StartBeat, Is.EqualTo(0));
            Assert.That(firstNote.Duration, Is.EqualTo(4));
            Assert.That(firstNote.Pitch, Is.EqualTo(5));
        }

        [Test]
        public void TestSongIsCorrectlyWrote()
        {
            var song = new SongMock
            {
                Version = new Version(2, 0, 0)
            };

            var songTestFile =
                new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "V2.txt"));
            Assert.That(songTestFile.Exists, Is.True, $"The test file {songTestFile.FullName} cannot be found");

            Assert.DoesNotThrowAsync((Func<Task>)(() => SongParser.ParseSongAsync(songTestFile, song, CancellationToken.None)), "The song parser throws an exception");

            var tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{Guid.NewGuid()}.txt");

            Assert.DoesNotThrowAsync((Func<Task>)(() => SongWriter.WriteSongAsync(song, tempFilePath, true, CancellationToken.None)));

            Assert.That(File.ReadAllText(tempFilePath), Is.EqualTo(File.ReadAllText(songTestFile.FullName).Replace("P 4 4 3 ~,", "F 4 4 3 ~,")));

            File.Delete(tempFilePath);
        }
    }
}