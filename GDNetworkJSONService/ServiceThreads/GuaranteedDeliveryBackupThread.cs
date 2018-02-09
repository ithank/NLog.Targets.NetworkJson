using System;
using System.Collections.Generic;
using System.Threading;
using NLog.Targets.NetworkJSON;
using StackExchange.Redis;


namespace GDNetworkJSONService.ServiceThreads
{
    internal class GuaranteedDeliveryBackupThread
    {
        public static IDatabase RedisDb;
        public static int TotalSuccessCount;
        public static int TotalFailedCount;


        public static void ThreadMethod(GuaranteedDeliveryThreadDelegate threadData, IDatabase redisDb)
        {
            //RedisConnectionManager redisConnectionManager;
            //redisConnectionManager = new RedisConnectionManager(threadData.Host, threadData.Port, threadData.Db, threadData.Password);
            RedisDb = redisDb;
            var targets = new Dictionary<string, NetworkJsonTarget>();
            var endpoint = threadData.EndPoint;
            while (!threadData.IsAppShuttingDown)
            {
                try
                {
                    //var redisDb = redisConnectionManager.GetDatabase();

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
                            targets.Remove(endpoint);
                            Interlocked.Increment(ref TotalFailedCount);
                            Thread.Sleep(500);
                        }
                    }

                }
                catch (Exception ex)
                {
                    //redisConnectionManager?.Dispose();
                    targets.Clear();
                    Thread.Sleep(1000);
                }
            }
            threadData.ThreadHasShutdown();
        }
    }
}
