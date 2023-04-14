using System.Threading;
using System.Threading.Tasks;

namespace WiFi.ts
{

    #interface that prepares and enqueues a database to record the wifi broadcast information
    public interface IWiFiDatabase
    {
        Task CloseDB();
        Task Enqueue(WiFiModel wiFiModel);
        Task<bool> PrepareDB();
        IWLanModel wLanModel { get; }
    }
}
