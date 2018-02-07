using System;
using System.Collections.Generic;
using System.Threading;
using NLog.Targets.NetworkJSON;
using StackExchange.Redis;

namespace GDNetworkJSONService.ServiceThreads
{
    internal class GuaranteedDeliveryThreadDelegate
    {
        #region Public Properties

        public bool IsRunning { get; private set; }

        public bool IsAppShuttingDown { get; private set; }

        // Redis settings
        public string Host { get; set; }
        public int Port { get; set; }
        public int Db { get; set; }
        public string Password { get; set; }
        public string Key { get; set; }
        public string BackupKey { get; set; }
        public string EndPoint { get; set; }
        #endregion

        public void RegisterThreadShutdown()
        {
            IsAppShuttingDown = true;
        }

        public void ThreadHasShutdown()
        {
            IsRunning = false;
        }
    }

    internal class GuaranteedDeliveryThread
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

                    var logMessage = redisDb.ListLeftPop(threadData.Key);
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
                                currentTarget = new NetworkJsonTarget {Endpoint = endpoint};
                                targets.Add(endpoint, currentTarget);
                            }

                            currentTarget.Write(logMessage);
                            Interlocked.Increment(ref TotalSuccessCount);
                        }
                        catch (Exception ex)
                        {
                            // Fail the message, backup thread will take over for this message until dead letter time.
                            PushLogMessageToBackupList(logMessage);
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

        private static void PushLogMessageToBackupList(RedisValue message)
        {
            try
            {
                using (RedisConnectionManager redisConnectionManager =
                    new RedisConnectionManager(_threadData.Host, _threadData.Port, _threadData.Db,
                        _threadData.Password))
                {
                    var redisDB = redisConnectionManager.GetDatabase();
                    redisDB.ListRightPushAsync(_threadData.BackupKey, message);  //push to the end
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
