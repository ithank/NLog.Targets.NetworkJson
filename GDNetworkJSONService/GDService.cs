using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using GDNetworkJSONService;
using GDNetworkJSONService.ExtensionMethods;
using GDNetworkJSONService.Helpers;
using GDNetworkJSONService.Loggers;
using GDNetworkJSONService.Models;
using GDNetworkJSONService.ServiceThreads;
using Microsoft.Owin;
using NLog.Targets.NetworkJSON;
using NLog.Targets.NetworkJSON.ExtensionMethods;
using StackExchange.Redis;

[assembly: OwinStartup(typeof(OwinStartup))]

namespace GDNetworkJSONService
{
    partial class GDService : ServiceBase
    {
        private RedisConnectionManager _redisConnectionManager;
        private IDatabase _redisDb;
        private GuaranteedDeliveryThreadDelegate _guaranteedDeliveryThreadDelegate;
        private GuaranteedDeliveryThreadDelegate _guaranteedDeliveryBackupThreadDelegate;
        private bool _isRunning = true;
        private Timer _diagnosticsTimer;
        private long _diagnosticsInterval;

        private readonly CommandLineModel _commandLineModel;

        public GDService(CommandLineModel model)
        {
            _commandLineModel = model;
            ServiceName = "Guaranteed Delivery NetworkJSON Service";
            InitializeComponent();
        }

        public void OnStartConsoleMode()
        {
            OnStart(null);
        }

        public void OnStopConsoleMode()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            var instrumentationlogger = LoggerFactory.GetInstrumentationLogger();
            instrumentationlogger.InitializeExecutionLogging($"{this.GetRealServiceName(ServiceName)} Startup");

            if (!VerifyRedisIsRunning())
            {
                throw new Exception("Could not find a running instance of Redis!!");
            }

            try
            {               
                
                _redisConnectionManager = new RedisConnectionManager(_commandLineModel.RedisHost, _commandLineModel.RedisPort, _commandLineModel.RedisDB, _commandLineModel.RedisPassword);
                try
                {
                    _redisDb = _redisConnectionManager.GetDatabase();
                    _redisDb.Ping();
                }
                catch (Exception ex)
                {
                    OnStop();
                    throw new Exception("Redis communication failure.  Perhaps it's listening on a different port", ex);
                }

            }
            catch (Exception ex)
            {
                OnStop();
                throw new Exception("Failed to connect to Redis.", ex);
            }


/*
            var redisInfo = GetRedisInfoData();
            if (!redisInfo.IsNullOrEmpty())
            {
                var redisLogger = RedisCliInfoParser.Parse(redisInfo);
            }
*/


            try
            {
                _guaranteedDeliveryThreadDelegate = new GuaranteedDeliveryThreadDelegate
                {
                    BackupKey = _commandLineModel.RedisBackupKey,
                    Db = _commandLineModel.RedisDB,
                    EndPoint = _commandLineModel.NetworkJsonEndpoint,
                    Host = _commandLineModel.RedisHost,
                    Key = _commandLineModel.RedisKey,
                    Password = _commandLineModel.RedisPassword
                };

                var thread = new Thread(() => GuaranteedDeliveryThread.ThreadMethod(_guaranteedDeliveryThreadDelegate, _redisDb));
                thread.Start();
                instrumentationlogger.PushInfoWithTime("Guaranteed Delivery Thread Started.");

                _guaranteedDeliveryThreadDelegate = new GuaranteedDeliveryThreadDelegate
                {
                    BackupKey = _commandLineModel.RedisBackupKey,
                    Db = _commandLineModel.RedisDB,
                    EndPoint = _commandLineModel.NetworkJsonEndpoint,
                    Host = _commandLineModel.RedisHost,
                    Key = _commandLineModel.RedisKey,
                    Password = _commandLineModel.RedisPassword
                };

                thread = new Thread(() => GuaranteedDeliveryBackupThread.ThreadMethod(_guaranteedDeliveryThreadDelegate, _redisDb));
                thread.Start();
                instrumentationlogger.PushInfoWithTime("Guaranteed Delivery Thread Started.");

                SetupDiagnosticsSchedule();
            }
            catch (Exception ex)
            {
                throw new Exception("Guaranteed Delivery or Diagnostics Thread Initialization Failed.", ex);
            }

            instrumentationlogger.LogExecutionComplete(0);
        }

/*        private bool StartRedis()
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "redis-server.exe";
            p.StartInfo.Arguments = "redis.windows.conf";
            p.Start();
            Thread.Sleep(3);
            return VerifyRedisIsRunning();
        }*/

        private bool VerifyRedisIsRunning()
        {
            Process[] pname = Process.GetProcessesByName("redis-server");
            return pname.Length > 0;
        }

        private void SetupDiagnosticsSchedule()
        {
            var messageLogger = LoggerFactory.GetMessageLogger();
            try
            {
                if (!_isRunning) return;
                _diagnosticsTimer = new Timer(DiagnosticsCallback);
                
                var mode = AppSettingsHelper.DiagnosticsScheduleMode;
                messageLogger.PushInfo($"Diagnostics Schedule Mode: {mode}");

                //Set the Default Time.
                var scheduledTime = DateTime.MinValue;

                if (mode.ToUpper() == "DAILY")
                {
                    //Get the Scheduled Time from AppSettings.
                    scheduledTime = AppSettingsHelper.DiagnosticsScheduledTime;
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next day.
                        scheduledTime = scheduledTime.AddDays(1);
                    }
                    _diagnosticsInterval = (24 * 60 * 60 * 1000);
                }

                if (mode.ToUpper() == "INTERVAL")
                {
                    //Get the Interval in Seconds from AppSettings.
                    var intervalSeconds = AppSettingsHelper.DiagnosticsIntervalSeconds;
                    //Set the Scheduled Time by adding the Interval to Current Time.
                    scheduledTime = DateTime.Now.AddSeconds(intervalSeconds);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next Interval.
                        scheduledTime = scheduledTime.AddMinutes(intervalSeconds);
                    }
                    _diagnosticsInterval = (intervalSeconds * 1000);
                }

                var timeSpan = scheduledTime.Subtract(DateTime.Now);
                var schedule = $"{timeSpan.Days} day(s) {timeSpan.Hours} hour(s) {timeSpan.Minutes} minute(s) {timeSpan.Seconds} seconds(s)";

                messageLogger.PushInfo($"Diagnostics scheduled to run in: {schedule}");

                messageLogger.PushInfo($"Diagnostics scheduled with an interval of: {_diagnosticsInterval} ms");

                messageLogger.LogInfo("Diagnostics Thread Scheduled");

                //Get the difference in Minutes between the Scheduled and Current Time.
                var dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                _diagnosticsTimer.Change(dueTime, _diagnosticsInterval);

            }
            catch (Exception ex)
            {
                messageLogger.LogError("Diagnostics Thread Scheduling Error", ex);
            }
        }

        private void DiagnosticsCallback(object state)
        {
            if (!_isRunning) return;
            var diagnosticsLogger = LoggerFactory.GetDiagnosticsInstrumentationLogger();
            diagnosticsLogger.LogItemsSentFirstTry = Interlocked.Exchange(ref GuaranteedDeliveryThread.TotalSuccessCount, 0);
            diagnosticsLogger.LogItemsFailedFirstTry = Interlocked.Exchange(ref GuaranteedDeliveryThread.TotalFailedCount, 0);
            diagnosticsLogger.LogItemsSentOnRetry = Interlocked.Exchange(ref GuaranteedDeliveryBackupThread.TotalSuccessCount, 0);
            diagnosticsLogger.LogItemsFailedOnRetry = Interlocked.Exchange(ref GuaranteedDeliveryBackupThread.TotalFailedCount, 0);
            diagnosticsLogger.DiagnosticsIntervalMS = _diagnosticsInterval;

            if (AppSettingsHelper.SkipZeroDiagnostics)
            {
                if (diagnosticsLogger.LogItemsReceived > 0 || diagnosticsLogger.LogItemsSentFirstTry > 0 ||
                    diagnosticsLogger.LogItemsFailedFirstTry > 0 || diagnosticsLogger.LogItemsSentOnRetry > 0 ||
                    diagnosticsLogger.LogItemsFailedOnRetry > 0 || diagnosticsLogger.BacklogCount > 0 || diagnosticsLogger.DeadLetterCount > 0)
                {
                    diagnosticsLogger.LogFullDiagnostics();
                }
            }
            else
            {
                diagnosticsLogger.LogFullDiagnostics();
            }

            var redisInfo = GetRedisInfoData();
            if (!redisInfo.IsNullOrEmpty())
            {
                var parser = new RedisCliInfoParser();
                var redisInfoDict = RedisCliInfoParser.Parse(redisInfo);
                var redisLogger = LoggerFactory.GetRedisMonitorLogger();
                redisLogger.Stats = redisInfoDict;
                redisLogger.LogStats();
            }


        }

        private string GetRedisInfoData()
        {
            try
            {
                var ps = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    FileName = "redis-cli.exe",
                    UseShellExecute = false,
                    Arguments = "info",
                    RedirectStandardOutput = true
                };

                var proc = Process.Start(ps);
                string output = proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();
                return output;
            }
            catch (Exception ex)
            {
                //ignore
            }
            return string.Empty;

        }

        protected override void OnStop()
        {
            _diagnosticsTimer?.Dispose();
            var logger = LoggerFactory.GetInstrumentationLogger();
            var numMs = 0;
            logger.InitializeExecutionLogging($"{this.GetRealServiceName(ServiceName)} Shutdown");

            _isRunning = false;
            _guaranteedDeliveryThreadDelegate?.RegisterThreadShutdown();
            _guaranteedDeliveryBackupThreadDelegate?.RegisterThreadShutdown();

            var inc = 0;
            while ((_guaranteedDeliveryThreadDelegate != null &&_guaranteedDeliveryThreadDelegate.IsRunning) 
                        || (_guaranteedDeliveryBackupThreadDelegate != null && _guaranteedDeliveryBackupThreadDelegate.IsRunning) && inc < 40)
            {
                numMs += 250;
                Thread.Sleep(250);
                inc++;
            }

            // Waited max of 10 seconds and it is still running
            if (inc == 40 && (_guaranteedDeliveryThreadDelegate != null && _guaranteedDeliveryThreadDelegate.IsRunning || _guaranteedDeliveryBackupThreadDelegate != null && _guaranteedDeliveryBackupThreadDelegate.IsRunning))
            {
                logger.PushError("Guaranteed Delivery Thread(s) did not shut down in 10 seconds.");
                logger.LogExecutionCompleteAsError(failedItemCount: 0);
            }
            else
            {
                logger.PushInfo($"Guaranteed Delivery Thread(s) shut down in {numMs} milliseconds.");
                logger.LogExecutionComplete(0);
            }

            _redisConnectionManager?.Dispose();
        }

        private string _realServiceName;
        public string RealServiceName
        {
            get
            {
                if (_realServiceName.IsNullOrEmpty())
                {
                    _realServiceName = this.GetRealServiceName(ServiceName);
                }
                return(_realServiceName);
            }
        }
    }
}
