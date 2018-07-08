using System;
using Mono.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using AsyncQueue;
using System.Threading;

namespace WiFi.ts
{
    static public class WiFiDatabase
    {
        public class WiFiModel
        {
            public Int64? ID { get; set; }
            public String SSID { get; set; }
            public String BSSID { get; set; }
            public int RSSI { get; set; }
            public int noiseMeasurement { get; set; }
            public int channel { get; set; }
            public DateTime dateTimeLogged { get; set; }
        }

        static internal WiFiModel WiFiModelFactory(String SSID, String BSSID, int RSSI, int noiseMeasurement, int channel, DateTime dateTimeLogged)
        {
            return new WiFiModel()
            {
                SSID = SSID,
                BSSID = BSSID,
                RSSI = RSSI,
                noiseMeasurement = noiseMeasurement,
                channel = channel,
                dateTimeLogged = dateTimeLogged
            };
        }

        static public String Log;

        static public void AttachLog(ref string log)
        {
            WiFiDatabase.Log = log;
        }

        static private SqliteConnection SingleConnection;

        static public CancellationToken cancellationToken;

        static public async Task<bool> PrepareDB()
        {
            try
            {
                FileStream fileStream;
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DateTime.Now.Ticks.ToString() + "WiFi.ts.db3");
                if (!File.Exists(dbPath))
                {
                    Log += "dbFile doesn't exist";
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

        static AsyncQueue<WiFiModel> wiFiQueue = new AsyncQueue<WiFiModel>();

        static bool isProcessing = false;

        static public async Task Enqueue(WiFiModel wiFiModel)
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
                    String command = string.Format("INSERT INTO WiFi(SSID, BSSID, RSSI, NOISEMEASUREMENT, CHANNEL, DATETIMELOGGED) VALUES (?, ?, ?, ?, ?, ?)");
                    if (WiFiDatabase.SingleConnection != null)
                    {
                        SqliteCommand sqliteCommandInsert = new SqliteCommand(command.ToString(), WiFiDatabase.SingleConnection);
                        sqliteCommandInsert.Parameters.Add(first.SSID);
                        sqliteCommandInsert.Parameters.Add(first.BSSID);
                        sqliteCommandInsert.Parameters.Add(first.RSSI);
                        sqliteCommandInsert.Parameters.Add(first.noiseMeasurement);
                        sqliteCommandInsert.Parameters.Add(first.channel);
                        sqliteCommandInsert.Parameters.Add(first.dateTimeLogged);

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
