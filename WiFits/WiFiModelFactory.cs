﻿using System;
using CoreWlan;

namespace WiFits
{
    static public class WiFiModelFactory
    {
        static public WiFiModel CreateWiFiModel(CWNetwork network, DateTime timeRecorded)
        {
            WiFiModel model = new WiFiModel(null, Convert.ToString(network.Ssid)
                                            , Convert.ToString(network.Bssid)
                                            , Convert.ToInt32(network.WlanChannel.ChannelNumber)
                                            , Convert.ToInt32(network.RssiValue)
                                            , timeRecorded);

            return model;
        }
    }
}
