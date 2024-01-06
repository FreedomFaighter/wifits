using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Data;

namespace WiFits
{
    public class WiFiSqlLiteDatabase : IWiFiDatabase
    {
        private SQLiteConnection _SingleConnection;

        private ConcurrentQueue<WiFiModel> _WiFiQueue = new ConcurrentQueue<WiFiModel>();

        private bool _isProcessing = false;

        private IWLanModel _IWLanModel;

        public IWLanModel wLanModel
        {
            get
            {
                return this._IWLanModel;
            }
        }

        public WiFiSqlLiteDatabase(IWLanModel wLanModel)
        {
            this._IWLanModel = wLanModel;
        }

        public async Task<bool> PrepareDB()
        {
#if DEBUG
            Console.WriteLine("Inside PrepareDB()");
#endif
            try
            {
                FileStream fileStream;
                String dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "WiFits.db3");
                if (!File.Exists(dbPath))
                {
                    fileStream = File.Open(dbPath, FileMode.Create);
                    fileStream.Close();
                }

                if (_SingleConnection == null)
                {
                    this._SingleConnection = new SQLiteConnection($"Data Source={dbPath};DbLinqProvider = sqlite");
                }
                await Task.Run(() => this._SingleConnection.Open());
                SQLiteCommand sqliteCommandCreateWiFiTable = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS WiFi(WiFiID INTEGER PRIMARY KEY AUTOINCREMENT, SSID TEXT, BSSID TEXT, CHANNEL INT, RSSI INT, DATETIMERECORDED DATETIME)", this._SingleConnection);
                await sqliteCommandCreateWiFiTable.ExecuteNonQueryAsync();
                //SqliteCommand sqliteCommandCreateBSSIDIndex = new SqliteCommand(@"CREATE UNIQUE INDEX IF NOT EXISTS IDX_BSSID_WiFi ON WiFi(BSSID)", this._SingleConnection);
                //await sqliteCommandCreateBSSIDIndex.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
                throw ex;
            }
        }

        public async Task CloseDB()
        {
            await Task.Run(() => this._SingleConnection.Close());
        }

        public async Task Enqueue(WiFiModel wiFiModel)
        {
#if DEBUG
            Console.WriteLine("INSIDE Enqueue.");
#endif
            _WiFiQueue.Enqueue(wiFiModel);

            if (!this._isProcessing)
            {
                this._isProcessing = true;
                await StartProcessingDBQueue();
            }
        }

        private async Task StartProcessingDBQueue()
        {
            while (!this._WiFiQueue.IsEmpty)
            {
                WiFiModel fiModel = new WiFiModel();

                try
                {
                    if (this._WiFiQueue.TryDequeue(out fiModel))
                    {
#if DEBUG
                        Console.WriteLine("Inside Start If");
#endif
                        SQLiteCommand sqliteCommand = this._SingleConnection.CreateCommand();
                        sqliteCommand.CommandText = @"INSERT INTO WiFi (SSID, BSSID, CHANNEL, RSSI, DATETIMERECORDED) VALUES (@SSID, @BSSID, @CHANNEL, @RSSI, DATETIME(@DATETIMERECORDED))";
                        sqliteCommand.CommandType = System.Data.CommandType.Text;
                        sqliteCommand.Parameters.Add(new SQLiteParameter("@SSID", DbType.String) { Value = fiModel.SSID });
                        sqliteCommand.Parameters.Add(new SQLiteParameter("@BSSID", DbType.String) { Value = fiModel.BSSID });
                        sqliteCommand.Parameters.Add(new SQLiteParameter("@CHANNEL", DbType.Int32) { Value = fiModel.Channel });
                        sqliteCommand.Parameters.Add(new SQLiteParameter("@RSSI", DbType.Int32) { Value = fiModel.Rssi });
                        sqliteCommand.Parameters.Add(new SQLiteParameter("@DATETIMERECORDED", DbType.String)
                        { Value = this.DateTimeSQLite(fiModel.DateTimeRecorded) });

                        int rows = await sqliteCommand.ExecuteNonQueryAsync();
#if DEBUG
                        Console.WriteLine(rows);
#endif
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.Message);
#endif
                }
            }
            this._isProcessing = false;
        }
        private string DateTimeSQLite(DateTime datetime)
        {
            string dateTimeFormat = "{0}-{1}-{2} {3}:{4}:{5}.{6}";
            return string.Format(dateTimeFormat, datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second, datetime.Millisecond);
        }
    }
}
