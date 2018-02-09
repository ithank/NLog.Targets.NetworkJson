using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets.NetworkJSON.ExtensionMethods;
using PV.Logging;

namespace GDNetworkJSONService.Loggers
{
    internal class RedisMonitorLogger : LoggerBase
    {
        public RedisMonitorLogger(Assembly parentApplication, string applicationLoggingId)
            : base(parentApplication, LogMessageTypes.Instrumentation.ToString(), applicationLoggingId)
        {
            EmptyAllQueuesOnLogWrite = true;
        }

        #region Properties
        public Dictionary<string, string> Stats { get; set; }
        public long MonitorIntervalMS { get; set; }

/*        #region Server Properties

        public string RedisVersion { get; set; }
        public string RedisMode { get; set; }
        public string OS { get; set; }
        public string ArchBits { get; set; }
        public int ProcessID { get; set; }
        public int TcpPort { get; set; }
        public long UptimeInSeconds { get; set; }
        public long UptimeInDays { get; set; }
        public string Hz { get; set; }
        public string LruClock { get; set; }
        public string ConfigFile { get; set; }

        #endregion
        #region Client Properties

        public int ConnectedClients { get; set; }
        public long ClientLongestOutputList { get; set; }
        public long ClientBiggestInputBuf { get; set; }
        public long BlockedClients { get; set; }
        #endregion

        #region Memory Properties

        public long UsedMemory { get; set; }
        public string UsedMemoryHuman { get; set; }
        public long UsedMemoryRss { get; set; }
        public long UsedMemoryPeak { get; set; }
        public string UsedMemoryPeakHuman { get; set; }
        public long UsedMemoryLua { get; set; }
        public double MemFragmentationRatio { get; set; }
        public string MemAllocator { get; set; }
        #endregion

        #region Persistence Properties
        public bool Loading { get; set; }
        public int RdbChangesSinceLastSave { get; set; }
        public int RdbBgSaveInProgress { get; set; }
        public int RdbLastSaveTime { get; set; }
        public string RdbLastBgSaveStatus { get; set; }
        public int RdbLastBgSaveTimeSec { get; set; }
        public int RdbCurrentBgSaveTimeSec { get; set; }
        public int AofEnabled { get; set; }
        public bool AofRewriteInProgress { get; set; }
        public bool AofRewriteScheduled { get; set; }
        public int AofLastRewriteTimeSec { get; set; }
        public int AofCurrentRewriteTimeSec { get; set; }
        public string AofLastBgRewriteStatus { get; set; }
        public string AofLastWriteStatus { get; set; }
        #endregion

        #region Stats Properties

        public long TotalConnectionsReceived { get; set; }
        public long TotalCommandsProcessed { get; set; }
        public long InstantaneousOpsPerSec { get; set; }
        public long TotalNetInputBytes { get; set; }
        public long TotalNetOutputBytes { get; set; }
        public double InstantaneousInputKbps { get; set; }
        public double InstantaneousOutputKbps { get; set; }
        public long RejectedConnections { get; set; }
        public bool SyncFull { get; set; }
        public bool SyncPartialOk { get; set; }
        public bool SyncPartialErr { get; set; }
        public long ExpiredKeys { get; set; }
        public long EvictedKeys { get; set; }
        public long KeyspaceHits { get; set; }
        public long KeyspaceMisses { get; set; }
        public long PubSubChannels { get; set; }
        public long PubSubPatterns { get; set; }
        public long LatestForkUsec { get; set; }
        public long MigrateCachedSockets { get; set; }
        #endregion

        #region Replication Properties
        public string Role { get; set; }
        public int ConnectedSlaves { get; set; }
        public long MasterReplOffset { get; set; }
        public long ReplBacklogActive { get; set; }
        public long ReplBacklogSize { get; set; }
        public long ReplBacklogFirstByteOffset { get; set; }
        public long ReplBacklogHistLen { get; set; }

        #endregion

        #region CPU Properties
        public double UsedCpuSys { get; set; }
        public double UsedCpuUser { get; set; }
        public double UsedCpuSysChildren { get; set; }
        public double UsedCpuUserChildren { get; set; }

        #endregion

        public bool ClusterEnabled { get; set; }*/


        #endregion

        #region Instrumentation Logging Methods

        private const string MonitorMessage = "Redis Monitoring Stats";

        public void LogStats()
        {
            if (Stats != null)
            {
                foreach (var statItem in Stats)
                {
                    PushInfo(statItem.Key, statItem.Value);
                }
            }
            PushInfo(nameof(MonitorIntervalMS), MonitorIntervalMS.ToString());
            LogInfo(MonitorMessage);


            /*            PushInfo(nameof(RedisVersion), RedisVersion.ToString());
                        PushInfo(nameof(RedisMode), RedisMode.ToString());
                        PushInfo(nameof(OS), OS.ToString());
                        PushInfo(nameof(ArchBits), ArchBits.ToString());
                        PushInfo(nameof(ProcessID), ProcessID.ToString());
                        PushInfo(nameof(TcpPort), TcpPort.ToString());
                        PushInfo(nameof(UptimeInSeconds), UptimeInSeconds.ToString());
                        PushInfo(nameof(UptimeInDays), UptimeInDays.ToString());
                        PushInfo(nameof(Hz), Hz.ToString());
                        PushInfo(nameof(LruClock), LruClock.ToString());
                        PushInfo(nameof(ConfigFile), ConfigFile.ToString());

                        PushInfo(nameof(ConnectedClients), ConnectedClients.ToString());
                        PushInfo(nameof(ClientLongestOutputList), ClientLongestOutputList.ToString());
                        PushInfo(nameof(ClientBiggestInputBuf), ClientBiggestInputBuf.ToString());
                        PushInfo(nameof(BlockedClients), BlockedClients.ToString());

                        PushInfo(nameof(UsedMemory), UsedMemory.ToString());
                        PushInfo(nameof(UsedMemoryHuman), UsedMemoryHuman.ToString());
                        PushInfo(nameof(UsedMemoryRss), UsedMemoryRss.ToString());
                        PushInfo(nameof(UsedMemoryPeak), UsedMemoryPeak.ToString());
                        PushInfo(nameof(UsedMemoryPeakHuman), UsedMemoryPeakHuman.ToString());
                        PushInfo(nameof(UsedMemoryLua), UsedMemoryLua.ToString());
                        PushInfo(nameof(MemFragmentationRatio), MemFragmentationRatio.ToString());
                        PushInfo(nameof(MemAllocator), MemAllocator.ToString());

                        PushInfo(nameof(Loading), Loading.ToString());
                        PushInfo(nameof(RdbChangesSinceLastSave), RdbChangesSinceLastSave.ToString());
                        PushInfo(nameof(RdbBgSaveInProgress), RdbBgSaveInProgress.ToString());
                        PushInfo(nameof(RdbLastSaveTime), RdbLastSaveTime.ToString());
                        PushInfo(nameof(RdbLastBgSaveStatus), RdbLastBgSaveStatus.ToString());
                        PushInfo(nameof(RdbLastBgSaveTimeSec), RdbLastBgSaveTimeSec.ToString());
                        PushInfo(nameof(RdbCurrentBgSaveTimeSec), RdbCurrentBgSaveTimeSec.ToString());
                        PushInfo(nameof(AofEnabled), AofEnabled.ToString());
                        PushInfo(nameof(AofRewriteInProgress), AofRewriteInProgress.ToString());
                        PushInfo(nameof(AofRewriteScheduled), AofRewriteScheduled.ToString());
                        PushInfo(nameof(AofLastRewriteTimeSec), AofLastRewriteTimeSec.ToString());
                        PushInfo(nameof(AofCurrentRewriteTimeSec), AofCurrentRewriteTimeSec.ToString());
                        PushInfo(nameof(AofLastBgRewriteStatus), AofLastBgRewriteStatus.ToString());
                        PushInfo(nameof(AofLastWriteStatus), AofLastWriteStatus.ToString());

                        PushInfo(nameof(TotalConnectionsReceived), TotalConnectionsReceived.ToString());
                        PushInfo(nameof(TotalCommandsProcessed), TotalCommandsProcessed.ToString());
                        PushInfo(nameof(InstantaneousOpsPerSec), InstantaneousOpsPerSec.ToString());
                        PushInfo(nameof(TotalNetInputBytes), TotalNetInputBytes.ToString());
                        PushInfo(nameof(TotalNetOutputBytes), TotalNetOutputBytes.ToString());
                        PushInfo(nameof(InstantaneousInputKbps), InstantaneousInputKbps.ToString());
                        PushInfo(nameof(InstantaneousOutputKbps), InstantaneousOutputKbps.ToString());
                        PushInfo(nameof(RejectedConnections), RejectedConnections.ToString());
                        PushInfo(nameof(SyncFull), SyncFull.ToString());
                        PushInfo(nameof(SyncPartialOk), SyncPartialOk.ToString());
                        PushInfo(nameof(SyncPartialErr), SyncPartialErr.ToString());
                        PushInfo(nameof(ExpiredKeys), ExpiredKeys.ToString());
                        PushInfo(nameof(EvictedKeys), EvictedKeys.ToString());
                        PushInfo(nameof(KeyspaceHits), KeyspaceHits.ToString());
                        PushInfo(nameof(KeyspaceMisses), KeyspaceMisses.ToString());
                        PushInfo(nameof(PubSubChannels), PubSubChannels.ToString());
                        PushInfo(nameof(PubSubPatterns), PubSubPatterns.ToString());
                        PushInfo(nameof(LatestForkUsec), LatestForkUsec.ToString());
                        PushInfo(nameof(MigrateCachedSockets), MigrateCachedSockets.ToString());

                        PushInfo(nameof(ConnectedSlaves), ConnectedSlaves.ToString());
                        PushInfo(nameof(MasterReplOffset), MasterReplOffset.ToString());
                        PushInfo(nameof(ReplBacklogActive), ReplBacklogActive.ToString());
                        PushInfo(nameof(ReplBacklogSize), ReplBacklogSize.ToString());
                        PushInfo(nameof(ReplBacklogFirstByteOffset), ReplBacklogFirstByteOffset.ToString());
                        PushInfo(nameof(ReplBacklogHistLen), ReplBacklogHistLen.ToString());

                        PushInfo(nameof(UsedCpuSys), UsedCpuSys.ToString());
                        PushInfo(nameof(UsedCpuUser), UsedCpuUser.ToString());
                        PushInfo(nameof(UsedCpuSysChildren), UsedCpuSysChildren.ToString());
                        PushInfo(nameof(UsedCpuUserChildren), UsedCpuUserChildren.ToString());

                        PushInfo(nameof(ClusterEnabled), ClusterEnabled.ToString());*/
        }

        #endregion

        #region Override Methods

        protected sealed override void SetCustomProperties(LogEventInfo logEvent)
        {

            if (Stats != null)
            {
                foreach (var statItem in Stats)
                {
                    logEvent.Properties[statItem.Key.SafeToCamelCase()] = statItem.Value;
                }
            }

            /*            logEvent.Properties["redisVersion"] = RedisVersion;
                        logEvent.Properties["redisMode"] = RedisMode;
                        logEvent.Properties["oa"] = OS;
                        logEvent.Properties["archBits"] = ArchBits;
                        logEvent.Properties["processId"] = ProcessID;
                        logEvent.Properties["tcpPort"] = TcpPort;
                        logEvent.Properties["uptimeInSeconds"] = UptimeInSeconds;
                        logEvent.Properties["uptimeInDays"] = UptimeInDays;
                        logEvent.Properties["hz"] = Hz;
                        logEvent.Properties["lruClock"] = LruClock;
                        logEvent.Properties["configFile"] = ConfigFile;

                        logEvent.Properties["connectedClients"] = ConnectedClients;
                        logEvent.Properties["clientLongestOutputList"] = ClientLongestOutputList;
                        logEvent.Properties["clientBiggestInputBuf"] = ClientBiggestInputBuf;
                        logEvent.Properties["blockedClients"] = BlockedClients;

                        logEvent.Properties["usedMemory"] = UsedMemory;
                        logEvent.Properties["usedMemoryHuman"] = UsedMemoryHuman;
                        logEvent.Properties["usedMemoryRss"] = UsedMemoryRss;
                        logEvent.Properties["usedMemoryPeak"] = UsedMemoryPeak;
                        logEvent.Properties["usedMemoryPeakHuman"] = UsedMemoryPeakHuman;
                        logEvent.Properties["usedMemoryLua"] = UsedMemoryLua;
                        logEvent.Properties["memFragmentationRatio"] = MemFragmentationRatio;
                        logEvent.Properties["memAllocator"] = MemAllocator;

                        logEvent.Properties["loading"] = Loading;
                        logEvent.Properties["rdbChangesSinceLastSave"] = RdbChangesSinceLastSave;
                        logEvent.Properties["rdbBgSaveInProgress"] = RdbBgSaveInProgress;
                        logEvent.Properties["rdbLastSaveTime"] = RdbLastSaveTime;
                        logEvent.Properties["rdbLastBgSaveStatus"] = RdbLastBgSaveStatus;
                        logEvent.Properties["rdbLastBgSaveTimeSec"] = RdbLastBgSaveTimeSec;
                        logEvent.Properties["rdbCurrentBgSaveTimeSec"] = RdbCurrentBgSaveTimeSec;
                        logEvent.Properties["aofEnabled"] = AofEnabled;
                        logEvent.Properties["aofRewriteInProgress"] = AofRewriteInProgress;
                        logEvent.Properties["aofRewriteScheduled"] = AofRewriteScheduled;
                        logEvent.Properties["aofLastRewriteTimeSec"] = AofLastRewriteTimeSec;
                        logEvent.Properties["aofCurrentRewriteTimeSec"] = AofCurrentRewriteTimeSec;
                        logEvent.Properties["aofLastBgRewriteStatus"] = AofLastBgRewriteStatus;
                        logEvent.Properties["aofLastWriteStatus"] = AofLastWriteStatus;

                        logEvent.Properties["totalConnectionsReceived"] = TotalConnectionsReceived;
                        logEvent.Properties["totalCommandsProcessed"] = TotalCommandsProcessed;
                        logEvent.Properties["instantaneousOpsPerSec"] = InstantaneousOpsPerSec;
                        logEvent.Properties["totalNetInputBytes"] = TotalNetInputBytes;
                        logEvent.Properties["totalNetOutputBytes"] = TotalNetOutputBytes;
                        logEvent.Properties["instantaneousInputKbps"] = InstantaneousInputKbps;
                        logEvent.Properties["instantaneousOutputKbps"] = InstantaneousOutputKbps;
                        logEvent.Properties["rejectedConnections"] = RejectedConnections;
                        logEvent.Properties["syncFull"] = SyncFull;
                        logEvent.Properties["syncPartialOk"] = SyncPartialOk;
                        logEvent.Properties["syncPartialErr"] = SyncPartialErr;
                        logEvent.Properties["ExpiredKeys"] = ExpiredKeys;
                        logEvent.Properties["EvictedKeys"] = EvictedKeys;
                        logEvent.Properties["KeyspaceHits"] = KeyspaceHits;
                        logEvent.Properties["KeyspaceMisses"] = KeyspaceMisses;
                        logEvent.Properties["pubsubChannels"] = PubSubChannels;
                        logEvent.Properties["pubsubPatterns"] = PubSubPatterns;
                        logEvent.Properties["latestForkUsec"] = LatestForkUsec;
                        logEvent.Properties["migrateCachedSockets"] = MigrateCachedSockets;

                        logEvent.Properties["role"] = Role;
                        logEvent.Properties["connectedSlaves"] = ConnectedSlaves;
                        logEvent.Properties["masterReplOffset"] = MasterReplOffset;
                        logEvent.Properties["replBacklogActive"] = ReplBacklogActive;
                        logEvent.Properties["replBacklogSize"] = ReplBacklogSize;
                        logEvent.Properties["replBacklogFirstByteOffset"] = ReplBacklogFirstByteOffset;
                        logEvent.Properties["replBacklogHistlen"] = ReplBacklogHistLen;

                        logEvent.Properties["usedCpuSys"] = UsedCpuSys;
                        logEvent.Properties["usedCpuUser"] = UsedCpuUser;
                        logEvent.Properties["usedCpuSysChildren"] = UsedCpuSysChildren;
                        logEvent.Properties["usedCpuUserChildren"] = UsedCpuUserChildren;

                        logEvent.Properties["clusterEnabled"] = ClusterEnabled;*/

            logEvent.Properties["diagnosticsIntervalMS"] = MonitorIntervalMS;
        }

        #endregion
    }
}
