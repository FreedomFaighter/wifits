using System;
using Mono.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using AsyncQueue;
using System.Threading;
using CoreWlan;

namespace WiFi.ts
{
    internal class WiFiModel
    {
        private Int64? id;
        private String ssid;
        private String bssid;
        private Int32 channel;
        private DateTime dateTimeLogged;

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

        public DateTime DateTimeLogged {
            get { return this.dateTimeLogged; }
            set { this.dateTimeLogged = value; }
        }

        public WiFiModel(Int64? ID, String SSID, String BSSID, Int32 Channel, DateTime DateTimeLogged)
        {
            this.id = ID;
            this.ssid = SSID;
            this.bssid = BSSID;
            this.channel = Channel;
            this.dateTimeLogged = DateTimeLogged;
        }
    }

    static public class WiFiDatabase
    {
        static internal WiFiModel WiFiModelFactory(CWNetwork network, DateTime dateTimeLogged)
        {
            WiFiModel wife = new WiFiModel(
                null,
                Convert.ToString(network.Ssid),
                Convert.ToString(network.Bssid),
                Convert.ToInt32(network.WlanChannel.ChannelNumber),
                dateTimeLogged
            );

            return wife; 
        }

        static private SqliteConnection SingleConnection;

        static public CancellationToken cancellationToken;

        static public async Task<bool> PrepareDB()
        {
            try
            {
                FileStream fileStream;
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DateTime.Now.Ticks.ToString() + "-WiFi.ts.db3");
                if (!File.Exists(dbPath))
                {
                    fileStream = File.Open(dbPath, FileMode.OpenOrCreate);
                    fileStream.Close();
                }

                //make SingleConnection
                if (WiFiDatabase.SingleConnection == null)
                    WiFiDatabase.SingleConnection = new SqliteConnection("Data Source=" + dbPath);
                await Task.Run(() => WiFiDatabase.SingleConnection.Open());
                SqliteCommand sqliteCommandCreateWiFiTable = new SqliteCommand(@"CREATE TABLE IF NOT EXISTS WiFi(WiFiID INT PRIMARY KEY AUTOINCREMENT, SSID TEXT, BSSIDID TEXT, RSSI INT, NOISEMEASUREMENT INT, CHANNEL INT, DATETIMELOGGED DATETIME)", WiFiDatabase.SingleConnection);
                await sqliteCommandCreateWiFiTable.ExecuteNonQueryAsync();
                SqliteCommand sqliteCommandCreateBSSIDIndex = new SqliteCommand(@"CREATE UNIQUE INDEX IF NOT EXISTS IDX_BSSID_WiFi ON WiFi(BSSID)");
                await sqliteCommandCreateBSSIDIndex.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public async Task CloseDB()
        {
            await Task.Run(() => WiFiDatabase.SingleConnection.Close());
        }

        static internal AsyncQueue<WiFiModel> wiFiQueue = new AsyncQueue<WiFiModel>();

        static bool isProcessing = false;

        static internal async Task Enqueue(WiFiModel wiFiModel)
        {
            wiFiQueue.Enqueue(wiFiModel);

            if (!isProcessing)
            {
                isProcessing = true;
                await StartProcessingDBQueue();
            }
        }

        private static async Task StartProcessingDBQueue()
        {
            while (wiFiQueue.HasPromises)
            {
                isProcessing = true;
                var first = wiFiQueue.DequeueAsync(ref cancellationToken).Result;
                //process the current task
                await Task.Run(() =>
                {
                    int reconnectAttempts = 0;
                    String command = string.Format("INSERT INTO WiFi(SSID, BSSID, CHANNEL, DATETIMELOGGED) VALUES (?, ?, ?, ?, ?)");
                    if (WiFiDatabase.SingleConnection != null)
                    {
                        SqliteCommand sqliteCommandInsert = new SqliteCommand(command.ToString(), WiFiDatabase.SingleConnection);
                        sqliteCommandInsert.Parameters.Add(first.SSID);
                        sqliteCommandInsert.Parameters.Add(first.BSSID);
                        sqliteCommandInsert.Parameters.Add(first.Channel);
                        sqliteCommandInsert.Parameters.Add(first.DateTimeLogged);

                        sqliteCommandInsert.ExecuteNonQueryAsync();
                    }
                    else
                    {
                    reconnectAttempt:
                        reconnectAttempts++;
                        if (!WiFiDatabase.PrepareDB().Result && reconnectAttempts < 7)
                        {
                            goto reconnectAttempt;
                        }
                        else
                        {
                            throw new Exception("Error reconnecting to DB");
                        }
                    }
                });
            }
            isProcessing = false;
        }
    }
}
