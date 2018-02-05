﻿using System.Collections.Generic;
using System.Configuration;
using GDNetworkJSONService.Helpers;
using NLog.Targets.NetworkJSON.ExtensionMethods;
using NLog.Targets.NetworkJSON.LogStorageDB;


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
                        $"                            and is moved to the {DeadLetterLogStorageTable.TableName} table.",
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
            if (LocalLogStorage.IsNullOrEmpty())
            {
                LocalLogStorage = ConfigurationManager.ConnectionStrings["LocalLogStorage"]?.ConnectionString;
                if (LocalLogStorage.IsNullOrEmpty())
                {
                    ErrorInfo.Add("LocalLogStorage is not properly setup in the application configuration and was not passed at the command line.");
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

        private string _localLogStorage;
        public string LocalLogStorage
        {
            get
            {
                return _localLogStorage;
            }
            set
            {
                if (value.IsNullOrEmpty())
                {
                    _localLogStorage = "";
                    return;
                }
                if(value.Length > 3 && (value.StartsWith("\"") || value.StartsWith("'")))
                {
                    _localLogStorage = value.Substring(0, value.Length - 2);
                }
                else
                {
                    _localLogStorage = value;
                }
                ParameterInfo.Add($"Local Log Storage Connection String = {_localLogStorage}");
            }
        }

        public int DbSelectCount
        {
            get
            {
                return LogStorageDbGlobals.DbSelectCount;
            }
            set
            {
                if (value < 1)
                {
                    return;
                }
                LogStorageDbGlobals.DbSelectCount = value;
                ParameterInfo.Add($"DB Read Count = {LogStorageDbGlobals.DbSelectCount}");
            }
        }

        public int MinutesToDeadLetter
        {
            get
            {
                return LogStorageDbGlobals.MinutesTillDeadLetter;
            }
            set
            {
                if (value < 1)
                {
                    return;
                }
                LogStorageDbGlobals.MinutesTillDeadLetter = value;
                ParameterInfo.Add($"Minutes Till Dead Letter = {LogStorageDbGlobals.MinutesTillDeadLetter}");
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
