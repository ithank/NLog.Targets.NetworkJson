﻿using System.Data.SQLite;

namespace GDNetworkJSONService.LocalLogStorageDB
{
    internal class LogStorageDbGlobals
    {
        public static string ConnectionString { get; set; }

        public static int DbSelectCount { get; set; }

        public static int MinutesTillDeadLetter { get; set; }

        //public static int MaxLockTrys { get; set; } = 5;

        public static SQLiteConnection OpenNewConnection()
        {
            var dbConnection = new SQLiteConnection(ConnectionString);
            dbConnection.Open();
            return dbConnection;
        }


        public static void CompactDatabase(SQLiteConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "vacuum";
                cmd.ExecuteNonQuery();
            }
        }

        // Code does not appear to be necessary as SQLite appears to handle 
        // SQLite error (5): database is locked
        // all by itself.
        //public static int ExecuteCommandLockSafe(SQLiteCommand command)
        //{
        //    return ExecuteCommandLockSafe(command, MaxLockTrys);
        //}

        //public static int ExecuteCommandLockSafe(SQLiteCommand command, int maxLockTrys)
        //{
        //    var numRowsAffected = -1;
        //    for(var inc = 0; inc < maxLockTrys; inc++)
        //    {
        //        try
        //        {
        //            numRowsAffected = command.ExecuteNonQuery();
        //            break;
        //        }
        //        catch (SQLiteException ex)
        //        {
        //            if (ex.ResultCode != SQLiteErrorCode.Busy)
        //            {
        //                throw;
        //            }
        //            Console.WriteLine($"FAILED {inc}");
        //            if (inc == MaxLockTrys - 1)
        //            {
        //                throw;
        //            }
        //            Thread.Sleep(5);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //    return numRowsAffected;
        //}
    }
}
