using System;
using System.Configuration;
using NLog.Targets.NetworkJSON.ExtensionMethods;

namespace GDNetworkJSONService.Helpers
{
    public class AppSettingsHelper
    {
        public static string DiagnosticsScheduleMode
        {
            get
            {
                var scheduleMode = ConfigurationManager.AppSettings["DiagnosticsScheduleMode"].ToUpper();
                return !scheduleMode.IsNullOrEmpty() ? scheduleMode : "INTERVAL";
            }
        }
        public static DateTime DiagnosticsScheduledTime
        {
            get
            {
                DateTime schDateTime;
                if (DateTime.TryParse(ConfigurationManager.AppSettings["DiagnosticsScheduleTime"], out schDateTime))
                {
                    return schDateTime;
                }
                return DateTime.Now;
            }
        }
        public static int DiagnosticsIntervalSeconds
        {
            get
            {
                var secondsStr = ConfigurationManager.AppSettings["DiagnosticsIntervalSeconds"];
                var seconds = 0;
                //Default of 10 minutes if not present.
                return int.TryParse(secondsStr, out seconds) ? seconds : (600);
            }
        }

        public static bool SkipZeroDiagnostics
        {
            get
            {
                var skipZeroDiagsStr = ConfigurationManager.AppSettings["SkipZeroDiagnostics"];
                var skipZeroDiags = false;
                // Default of true if not present.
                return bool.TryParse(skipZeroDiagsStr, out skipZeroDiags) ? skipZeroDiags : true;
            }
        }

        public static int DbSelectCount
        {
            get
            {
                var dbSelectCountStr = ConfigurationManager.AppSettings["DBSelectCount"];
                var dbSelectCountInt = 0;
                return int.TryParse(dbSelectCountStr, out dbSelectCountInt) ? dbSelectCountInt : 10;
            }
        }

        public static int MinutesToDeadLetter
        {
            get
            {
                var tempMtdlStr = ConfigurationManager.AppSettings["MinutesToDeadLetter"];
                var tempMtdlInt = 0;
                // 4 days is default.
                return int.TryParse(tempMtdlStr, out tempMtdlInt) ? tempMtdlInt : 5760;
            }
        }

        #region Redis Settings
        public static string RedisHost
        {
            get
            {
                var redisHost = ConfigurationManager.AppSettings["RedisHost"];
                return !redisHost.IsNullOrEmpty() ? redisHost : "127.0.0.1";
            }
        }

        public static int RedisPort
        {
            get
            {
                var portStr = ConfigurationManager.AppSettings["RedisPort"];
                var portInt = 0;
                return int.TryParse(portStr, out portInt) ? portInt : 6379;
            }
        }

        public static string RedisKey
        {
            get
            {
                var redisKey = ConfigurationManager.AppSettings["RedisKey"];
                return !redisKey.IsNullOrEmpty() ? redisKey : "logKey";
            }
        }

        public static string RedisBackupKey
        {
            get
            {
                var redisKey = ConfigurationManager.AppSettings["RedisBackupKey"];
                return !redisKey.IsNullOrEmpty() ? redisKey : "logBackupKey";
            }
        }

        public static string RedisDataType
        {
            get
            {
                var redisDataType = ConfigurationManager.AppSettings["RedisDataType"];
                return !redisDataType.IsNullOrEmpty() ? redisDataType : "list";
            }
        }

        public static int RedisDb
        {
            get
            {
                var dbStr = ConfigurationManager.AppSettings["RedisDb"];
                var dbInt = 0;
                return int.TryParse(dbStr, out dbInt) ? dbInt : 0;
            }
        }


        public static string RedisPassword
        {
            get
            {
                var redisPassword = ConfigurationManager.AppSettings["RedisPassword"];
                return !redisPassword.IsNullOrEmpty() ? redisPassword : "";
            }
        }
        #endregion

    }
}
