using KaraW3B.SDK.Client.Connectors.Collections;
using KaraW3B.SDK.Client.Connectors.Songs;

namespace KaraW3B.SDK.Client.Connectors
{
    public interface IKaraW3BConnector
    {
        ILibrariesConnector Libraries { get; }
        ISongsConnector Songs { get; }
    }
}