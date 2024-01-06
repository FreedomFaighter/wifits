using System;

namespace WiFits
{
    public interface IWiFiModel
    {
        long? ID { get; set; }
        string SSID { get; set; }
        string BSSID { get; set; }
        int Channel { get; set; }
        int Rssi { get; set; }
        DateTime DateTimeRecorded { get; set; }
    }
}