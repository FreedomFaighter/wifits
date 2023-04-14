using System;
using CoreWlan;
using Foundation;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;

namespace WiFi.ts
{
    public class WLanModel : IWLanModel
    {
        private CWWiFiClient _client;

        private CWInterface _interface;

        private ConcurrentQueue<WiFiModel> _WiFis;

        public WLanModel()
        {
            this._client = new CWWiFiClient();

            this._interface = this._client.MainInterface;
        }

        public WLanModel(CWWiFiClient client) {
            this._client = client;

            this._interface = this._client.MainInterface;
        }

        public ConcurrentQueue<WiFiModel> Networks {
            get {
                NSError error = new NSError();

                List<CWNetwork> networks = this._interface.ScanForNetworksWithName(null, true, out error);
                DateTime now = DateTime.Now;
                this._WiFis = new ConcurrentQueue<WiFiModel>();

                foreach (var network in networks)
                {
                    this._WiFis.Enqueue(WiFiModelFactory.CreateWiFiModel(network, now));
                }

                return this._WiFis;
            }
        }
    }
}
