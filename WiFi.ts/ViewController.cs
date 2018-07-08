using System;
using System.Threading;
using System.Threading.Tasks;
#if DEBUG
    using System.Diagnostics;
#endif

using AppKit;
using Foundation;
using CoreData;
using CoreWlan;
using CoreLocation;
using System.Collections.Generic;

namespace WiFi.ts
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            
            base.ViewDidLoad();
            LogWTF.Editable = false;
            LogWTF.Enabled = true;
            LogWTF.Scrollable = true;

            LogWTF.StringValue = "Starting up!";

            try
            {
                Task.Run(() => WiFiDatabase.PrepareDB());
            }
            catch(Exception ex) 
            {
                System.Console.WriteLine(ex.Message);
            }
#if DEBUG
            Debug.WriteLine("42");
#endif
            CWInterface Interface = new CWInterface();
#if DEBUG
            Debug.WriteLine("46");
#endif
            try {
                CWWiFiClient client = new CWWiFiClient();
                Interface = client.MainInterface;
            }
            catch(Exception ex) {
                Console.WriteLine(ex.Message);
            }
            Timer timer = new Timer(HandleTimerCallback;)
            while (true)
            {
                Task.Run(async () =>
                {
                    NSError nSError;
                    Queue<CWNetwork> networkQueue = new Queue<CWNetwork>(Interface.ScanForNetworksWithName(null, true, out nSError));
                    while (networkQueue.Count > 0)
                    {
                        var firstNetwork = networkQueue.Dequeue();
                        WiFiDatabase.WiFiModel model = WiFiDatabase.WiFiModelFactory(firstNetwork.Ssid, firstNetwork.Bssid
                                                                           , Convert.ToInt32(firstNetwork.Rssi)
                                                                           , Convert.ToInt32(firstNetwork.NoiseMeasurement)
                                                                           , Convert.ToInt32(firstNetwork.WlanChannel)
                                                                                     , DateTime.Now);
                        await WiFiDatabase.Enqueue(model);
                    }
                });
            }
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
