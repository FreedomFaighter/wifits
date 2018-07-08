using System;

namespace WiFi.ts
{
    public class WiFiModel : IWiFiModel
    {
        private Int64? id;
        private String ssid;
        private String bssid;
        private Int32 channel;
        private DateTime dateTimeRecorded;

        public Int64? ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public String SSID
        {
            get { return this.ssid; }
            set { this.ssid = value; }
        }

        public String BSSID
        {
            get { return this.bssid; }
            set { this.bssid = value; }
        }

        public Int32 Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }

        public DateTime DateTimeRecorded
        {
            get { return this.dateTimeRecorded; }
            set { this.dateTimeRecorded = value; }
        }

        public WiFiModel(Int64? ID, String SSID, String BSSID, Int32 Channel, DateTime DateTimeRecorded)
        {
            this.id = ID;
            this.ssid = SSID;
            this.bssid = BSSID;
            this.channel = Channel;
            this.dateTimeRecorded = DateTimeRecorded;
        }

        public WiFiModel()
        {

        }
    }
}
