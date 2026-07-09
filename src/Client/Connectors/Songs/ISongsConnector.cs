using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.SDK.Models.Songs;
using KaraW3B.SDK.Models.Songs.Alerts;
using KaraW3B.SDK.Models.Songs.Files;
using KaraW3B.SDK.Models.Songs.Notes;

namespace KaraW3B.SDK.Client.Connectors.Songs
{
    public interface ISongsConnector
    {
        Task<SongDto> GetSongAsync(Guid songId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<SongNoteDto> GetSongNotesAsync(Guid songId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<SongAlertDto> GetSongAlertsAsync(Guid songId, CancellationToken cancellationToken = default);

        Task<Stream> GetSongFileStreamAsync(Guid songId, FileType fileType,
            CancellationToken cancellationToken = default);
    }
}