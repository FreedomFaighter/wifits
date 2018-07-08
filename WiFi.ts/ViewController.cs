using System;
using System.Threading;
using System.Threading.Tasks;
#if DEBUG
    using System.Diagnostics;
#endif

using AppKit;
using Foundation;
using CoreWlan;
using System.Collections.Generic;
using AsyncQueue;

namespace WiFi.ts
{
    public partial class ViewController : NSViewController
    {
        static Timer timer;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {

            base.ViewDidLoad();

            CancellationToken cancellationToken = new CancellationToken(false);

            LogWTF.Editable = false;
            LogWTF.Enabled = true;
            LogWTF.Scrollable = true;

            LogWTF.StringValue = "Starting up!";

            try
            {
                Task.Run(() => WiFiDatabase.PrepareDB());
            }
            catch (Exception ex)
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

            try
            {
                CWWiFiClient client = new CWWiFiClient();
                Interface = client.MainInterface;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            AsyncQueue<WiFiModel> networkQueue = new AsyncQueue<WiFiModel>();

            var delay = 1000;

            var interval = 5000     ;

            var dt = new DateTime();

            ViewController.timer = new Timer((obj) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    NSError nSError = new NSError();
                    CWNetwork[] cWNetworks = Interface.ScanForNetworksWithName(null, true, out nSError);
                    if (nSError == default(NSError))
                    {

                    }
                    dt = DateTime.Now;
                    if (cWNetworks.Length > 0)
                        foreach (CWNetwork network in cWNetworks)
                        {
                            networkQueue.Enqueue(WiFiDatabase.WiFiModelFactory(network, dt));
                        }
                }
                else
                {
                    timer.Dispose();
                }
            }, new object(), delay, interval);



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
