using System.Data.SQLite;

namespace NLog.Targets.NetworkJSON.LogStorageDB
{
    public class LogStorageDbGlobals
    {
        public static string ConnectionString { get; set; }

        public static int DbSelectCount { get; set; }

        public static int MinutesTillDeadLetter { get; set; }

        public static SQLiteConnection OpenNewConnection()
        {
            var dbConnection = new SQLiteConnection(ConnectionString);
            dbConnection.Open();
            return dbConnection;
        }
    }
}
