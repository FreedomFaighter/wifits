using AppKit;
using Foundation;
using System.Threading.Tasks;

namespace WiFi.ts
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            
        }

        public override void WillTerminate(NSNotification notification)
        {
            Task.Run(() => WiFiDatabase.CloseDB());
        }
    }
}
