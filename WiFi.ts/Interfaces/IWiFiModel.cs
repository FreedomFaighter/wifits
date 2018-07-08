using System;

namespace WiFi.ts
{
    public interface IWiFiModel
    {
        long? ID { get; set; }
        string SSID { get; set; }
        string BSSID { get; set; }
        int Channel { get; set; }
        DateTime DateTimeRecorded { get; set; }
    }
}