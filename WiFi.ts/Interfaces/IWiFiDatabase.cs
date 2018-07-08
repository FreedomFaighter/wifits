using System.Threading;
using System.Threading.Tasks;

namespace WiFi.ts
{
    public interface IWiFiDatabase
    {
        Task CloseDB();
        Task Enqueue(WiFiModel wiFiModel);
        Task<bool> PrepareDB();
        IWLanModel wLanModel { get; }
    }
}