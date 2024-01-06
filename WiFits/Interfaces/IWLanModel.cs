using System.Collections.Concurrent;

namespace WiFits
{
    //interface to setup a concurrent model of the network recording in the case that more then one thread is accessing the information during recording
    public interface IWLanModel
    {
        ConcurrentQueue<WiFiModel> Networks { get; }
    }
}
