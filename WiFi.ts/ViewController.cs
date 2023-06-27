using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

#if DEBUG
using System.Diagnostics;
#endif

using AppKit;
using Foundation;

namespace WiFi.ts
{
    public partial class ViewController : NSViewController
    {
        private Timer timer;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {

            base.ViewDidLoad();

            LogWTF.BeginInvokeOnMainThread(
                new Action(
                    () => {
                        LogWTF.Editable = false;
                        LogWTF.Enabled = true;
                        LogWTF.Scrollable = true;
                        LogWTF.StringValue = "Starting up!";
                    }
                )
            );

            IWLanModel wLanModel = new WLanModel();

            IWiFiDatabase database = new WiFiSqlLiteDatabase(wLanModel);

            try
            {
                Task.Run(() => database.PrepareDB());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            int dueTime = -1;

            int period = -1;
            try
            {
                BufferBlock<WiFiModel> networkBuffer = new BufferBlock<WiFiModel>();

                ISourceBlock<WiFiModel> sourceBlock = networkBuffer;

                ITargetBlock<WiFiModel> targetBlock = networkBuffer;
                
                this.timer = new Timer((_Object) =>
                {
                    Task.Run(async () =>
                    {
                        while (await sourceBlock.OutputAvailableAsync())
                        {
                            WiFiModel temp = sourceBlock.Receive();

                            await database.Enqueue(temp);
                        }
                    });
                    //Generates a queue of the networks current broadcasting and recognized by the API
                    //due to the looping nature of the recording this information a Queue is formed to not congest the attempt to record this information in a database
                    Task.Run(() =>
                    {
                        ConcurrentQueue<WiFiModel> targetQueue = database.wLanModel.Networks;
                        switch (targetQueue)
                        {
                            case null:
                                break;
                            default:
                                while (targetQueue.Count > 0)
                                {
                                    WiFiModel fiModel = new WiFiModel();
                                    if (targetQueue.TryDequeue(out fiModel))
                                    {
                                        targetBlock.Post(fiModel);
                                    }
                                }
                                break;
                        }


                        targetBlock.Complete();
                    });
                }, new object()
                                  , dueTime
                                  , period);
            }
            catch (ArgumentOutOfRangeException aoore)
            {
#if DEBUG
                Console.WriteLine(aoore.Message);
#endif
                System.Environment.Exit(1);
            }
            catch (NotSupportedException nse)
            {
#if DEBUG
                Console.WriteLine(nse.Message);
#endif
                System.Environment.Exit(2);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                System.Environment.Exit(3);
            }

            Task.Run(() => database.CloseDB());
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
