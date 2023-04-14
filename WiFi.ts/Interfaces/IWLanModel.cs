using System.Collections.Concurrent;

namespace WiFi.ts
{
    public interface IWLanModel
    {
        ConcurrentQueue<WiFiModel> Networks { get; }
    }
}
