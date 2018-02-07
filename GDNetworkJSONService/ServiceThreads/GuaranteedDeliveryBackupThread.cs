using System;
using System.Collections.Generic;
using System.Threading;
using NLog.Targets.NetworkJSON;


namespace GDNetworkJSONService.ServiceThreads
{
    internal class GuaranteedDeliveryBackupThread
    {
        public static int TotalSuccessCount;
        public static int TotalFailedCount;
        private static GuaranteedDeliveryThreadDelegate _threadData;


        public static void ThreadMethod(GuaranteedDeliveryThreadDelegate threadData)
        {
            _threadData = threadData;
            RedisConnectionManager redisConnectionManager;
            redisConnectionManager = new RedisConnectionManager(threadData.Host, threadData.Port, threadData.Db, threadData.Password);

            var targets = new Dictionary<string, NetworkJsonTarget>();
            var endpoint = threadData.EndPoint;
            while (!threadData.IsAppShuttingDown)
            {
                try
                {
                    var redisDb = redisConnectionManager.GetDatabase();

                    //var logMessage = redisDb.ListLeftPop(threadData.BackupKey);
                    var logMessage = redisDb.ListGetByIndex(threadData.BackupKey, 0); // get the oldest/top of list
                    if (logMessage.IsNullOrEmpty)
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        try
                        {
                            NetworkJsonTarget currentTarget = null;
                            if (!targets.TryGetValue(endpoint, out currentTarget))
                            {
                                currentTarget = new NetworkJsonTarget { Endpoint = endpoint };
                                targets.Add(endpoint, currentTarget);
                            }

                            currentTarget.Write(logMessage);
                            Interlocked.Increment(ref TotalSuccessCount);

                            //now delete it
                            redisDb.ListRemove(threadData.BackupKey, logMessage);
                        }
                        catch (Exception ex)
                        {
                            // Fail the message, backup thread will take over for this message until dead letter time.
                            /////PushLogMessageToBackupList(logMessage);
                            targets.Remove(endpoint);
                            Interlocked.Increment(ref TotalFailedCount);
                            Thread.Sleep(500);
                        }
                    }

                }
                catch (Exception ex)
                {
                    redisConnectionManager?.Dispose();
                    targets.Clear();
                    Thread.Sleep(1000);
                }
            }
            threadData.ThreadHasShutdown();
        }

/*        private static void PushLogMessageToBackupList(RedisValue message)
        {
            try
            {
                using (RedisConnectionManager redisConnectionManager =
                    new RedisConnectionManager(_threadData.Host, _threadData.Port, _threadData.Db,
                        _threadData.Password))
                {
                    var redisDB = redisConnectionManager.GetDatabase();
                    redisDB.ListRightPushAsync(_threadData.BackupKey, message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }*/
    }
}
