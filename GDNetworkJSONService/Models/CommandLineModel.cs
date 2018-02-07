using System.Collections.Generic;
using System.Configuration;
using GDNetworkJSONService.Helpers;
using NLog.Targets.NetworkJSON.ExtensionMethods;


namespace GDNetworkJSONService.Models
{
    internal class CommandLineModel
    {
        public const string AppBinaryName = "NetworkJsonGDService";
        
        private List<string> __helpInfo;
        public List<string> HelpInfo
        {
            get
            {
                if (__helpInfo == null)
                {
                    __helpInfo = new List<string>
                    {
                        @"This service acts as the guaranteed delivery service for NetworkJSON logging.",
                        @"It can be run as a windows service or at the command line. It runs on localhost",
                        @"and provides the 'store and forward' capability of the NLog.Targets.NetworkJSON",
                        @"NLog Target.",
                        @" ",
                        $"{AppBinaryName} /CONSOLE [/ENDPOINT=http://localhost:portnumber] [/DBSELECTCOUNT=X]",
                        $"     [/MTDL=X]",
                        @" ",
                        @"  /CONSOLE                  Run this app in console mode, if this is NOT SET then",
                        @"                            the application will attempt to start as a service.",
                        @"  /ENDPOINT                 The optional endpoint to use as the listener for client",
                        @"                            programs. If not set then the default is retrieved from",
                        @"                            the application configuration file.",
                        @"  /DBSELECTCOUNT            The number of log messages to read from the Log Storage DB",
                        $"                            in a single SELECT statement.",
                        @"  /MTDL                     The number of minutes on SUBSEQUENT retries of attempting to",
                        @"                            send a log message before it is considered a 'Dead Letter'",
                        $"                            and is moved to the DeadLetter List.",
                        @"  /?                        Display this help information.",
                        @" "
                    };
                }

                return (__helpInfo);
            }
        }

        private List<string> __errorInfo = new List<string>();
        public List<string> ErrorInfo
        {
            get { return (__errorInfo); }
        }

        private List<string> __parameterInfo = new List<string>();
        public List<string> ParameterInfo
        {
            get { return (__parameterInfo); }
        }

        public ParseCommandLineStatus LoadParameterInfo(string[] args)
        {
            var parameterStatus = ParseCommandLineStatus.ExecuteProgram;
            if (args.Length == 0 || args[0].IsSameCommandLineArg("?") || args[0].IsSameCommandLineArg("help"))
            {
                parameterStatus = ParseCommandLineStatus.DisplayHelp;
            }
            else
            {
                foreach (var arg in args)
                {
                    if (arg.IsSameCommandLineArg("console"))
                    {
                        ConsoleMode = true;
                    }
                    ProcessOptionalCommandLineEntry(arg);
                }
            }
            if (Endpoint.IsNullOrEmpty())
            {
                Endpoint = ConfigurationManager.AppSettings["Endpoint"];
                if (Endpoint.IsNullOrEmpty())
                {
                    ErrorInfo.Add("Endpoint is not properly setup in the application configuration and was not passed at the command line.");
                }
            }
            if (NetworkJsonEndpoint.IsNullOrEmpty())
            {
                NetworkJsonEndpoint = ConfigurationManager.AppSettings["NetworkJsonEndpoint"];
                if (NetworkJsonEndpoint.IsNullOrEmpty())
                {
                    ErrorInfo.Add("NetworkJsonEndpoint is not properly setup in the application configuration and was not passed at the command line.");
                }
            }
            if (DbSelectCount < 1)
            {
                DbSelectCount = AppSettingsHelper.DbSelectCount;
            }
            if (MinutesToDeadLetter < 1)
            {
                MinutesToDeadLetter = AppSettingsHelper.MinutesToDeadLetter;
            }
            
            //Redis
            if (RedisHost.IsNullOrEmpty())
            {
                RedisHost = AppSettingsHelper.RedisHost;
            }
            if (RedisPort < 1)
            {
                RedisPort = AppSettingsHelper.RedisPort;
            }
            if (RedisDB < 1)
            {
                RedisDB = AppSettingsHelper.RedisDb;
            }
            if (RedisKey.IsNullOrEmpty())
            {
                RedisKey = AppSettingsHelper.RedisKey;
            }
            if (RedisBackupKey.IsNullOrEmpty())
            {
                RedisBackupKey = AppSettingsHelper.RedisBackupKey;
            }
            if (RedisDataType.IsNullOrEmpty())
            {
                RedisDataType = AppSettingsHelper.RedisDataType;
            }
            if (RedisPassword.IsNullOrEmpty())
            {
                RedisPassword = AppSettingsHelper.RedisPassword;
            }




            if (ErrorInfo.Count > 0)
            {
                parameterStatus = ParseCommandLineStatus.DisplayError;
                ErrorInfo.Add("Use /? to display help information about this program.");
            }
            return parameterStatus;
        }

        private void ProcessOptionalCommandLineEntry(string commandLineEntry)
        {
            if (commandLineEntry.StartsWithCommandLineArg("endpoint"))
            {
                var arg = commandLineEntry.Split('=');
                if (arg.Length == 2)
                {
                    Endpoint = arg[1];
                }
            }
            if (commandLineEntry.StartsWithCommandLineArg("dbselectcount"))
            {
                var arg = commandLineEntry.Split('=');
                var dbReadCount = -1;

                if ((arg.Length == 2) && int.TryParse(arg[1], out dbReadCount) && dbReadCount > 0)
                {
                    DbSelectCount = dbReadCount;
                }
                else
                {
                    ErrorInfo.Add("Invalid DBSELECTCOUNT parameter");
                }
            }
            if (commandLineEntry.StartsWithCommandLineArg("mtdl"))
            {
                var arg = commandLineEntry.Split('=');
                var mtdl = -1;

                if ((arg.Length == 2) && int.TryParse(arg[1], out mtdl) && mtdl > 0)
                {
                    MinutesToDeadLetter = mtdl;
                }
                else
                {
                    ErrorInfo.Add("Invalid MTDL parameter");
                }
            }
        }

        public void SetForServiceRun()
        {
            
        }

        #region Properties

        private bool _consoleMode;
        public bool ConsoleMode
        {
            get
            {
                return _consoleMode;
            }
            set
            {
                _consoleMode = value;
                ParameterInfo.Add($"Console Mode = {_consoleMode}");
            }
        }

        private string _endpoint;
        public string Endpoint
        {
            get
            {
                return _endpoint;
            }
            set
            {
                _endpoint = value;
                ParameterInfo.Add($"Service Endpoint = {_endpoint}");
            }
        }

        private string _networkJsonEndpoint;
        public string NetworkJsonEndpoint
        {
            get
            {
                return _networkJsonEndpoint;
            }
            set
            {
                _networkJsonEndpoint = value;
                ParameterInfo.Add($"NetworkJsonEndpoint = {_networkJsonEndpoint}");
            }
        }


        private int _dbSelectCount;
        public int DbSelectCount
        {
            get
            {
                return _dbSelectCount;
            }
            set
            {
                if (value < 1)
                {
                    return;
                }
                _dbSelectCount = value;
                ParameterInfo.Add($"DB Read Count = {_dbSelectCount}");
            }
        }

        private int _minutesToDeadLetter;
        public int MinutesToDeadLetter
        {
            get
            {
                return _minutesToDeadLetter;
            }
            set
            {
                if (value < 1)
                {
                    return;
                }
                _minutesToDeadLetter = value;
                ParameterInfo.Add($"Minutes Till Dead Letter = {_minutesToDeadLetter}");
            }
        }

        private string _redisHost;
        public string RedisHost
        {
            get
            {
                return _redisHost;
            }
            set
            {
                _redisHost = value;
                ParameterInfo.Add($"Redis Host = {_redisHost}");
            }
        }
        private int _redisPort;
        public int RedisPort
        {
            get
            {
                return _redisPort;
            }
            set
            {
                if (value < 1)
                {
                    return;
                }
                _redisPort = value;
                ParameterInfo.Add($"Redis Port = {_redisPort}");
            }
        }

        private string _redisKey;
        public string RedisKey
        {
            get
            {
                return _redisKey;
            }
            set
            {
                _redisKey = value;
                ParameterInfo.Add($"Redis Key = {_redisKey}");
            }
        }

        private string _redisBackupKey;
        public string RedisBackupKey
        {
            get
            {
                return _redisBackupKey;
            }
            set
            {
                _redisBackupKey = value;
                ParameterInfo.Add($"Redis Key = {_redisBackupKey}");
            }
        }

        private string _redisDataType;
        public string RedisDataType
        {
            get
            {
                return _redisDataType;
            }
            set
            {
                _redisDataType = value;
                ParameterInfo.Add($"Redis Data Type = {_redisDataType}");
            }
        }
        private int _redisDb;
        public int RedisDB
        {
            get
            {
                return _redisDb;
            }
            set
            {
                if (value < 0)
                {
                    return;
                }
                _redisDb = value;
                ParameterInfo.Add($"Redis DB = {_redisDb}");
            }
        }

        private string _redisPassword;
        public string RedisPassword
        {
            get
            {
                return _redisPassword;
            }
            set
            {
                _redisPassword = value;
                var tmp = _redisPassword == "" ? "<none>" : "************";
                ParameterInfo.Add($"Redis Password = {tmp}");
            }
        }

        #endregion Properties

        public enum ParseCommandLineStatus
        {
            DisplayHelp,
            DisplayError,
            ExecuteProgram
        }
    }
}
