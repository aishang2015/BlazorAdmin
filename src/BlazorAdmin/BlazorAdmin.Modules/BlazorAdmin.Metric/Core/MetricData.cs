using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Metric.Core
{
    public record MetricDataRecord(DateTime? Time, string? Value);

    public class MetricData
    {
        public List<MetricDataRecord?> EFCoreQueriesPerSecondList { get; private set; } = new();
        public List<MetricDataRecord?> CpuUsageList { get; private set; } = new();
        private string? eFCoreQueriesPerSecond;
        private string? runtimeCpuUsage;


        #region EFCore

        public string? EFCoreActiveDbContexts { get; set; }
        public string? EFCoreTotalQueries { get; set; }
        public string? EFCoreQueriesPerSecond
        {
            get => eFCoreQueriesPerSecond; set
            {
                eFCoreQueriesPerSecond = value;
                EFCoreQueriesPerSecondList.Add(new MetricDataRecord(DateTime.Now, value));
                EFCoreQueriesPerSecondList = EFCoreQueriesPerSecondList.TakeLast(300).ToList();
            }
        }
        public string? EFCoreTotalSaveChanges { get; set; }
        public string? EFCoreSaveChangesPerSecond { get; set; }
        public string? EFCoreCompiledQueryCacheHitRate { get; set; }
        public string? EFCoreTotalExecutionStrategyOperationFailures { get; set; }
        public string? EFCoreExecutionStrategyOperationFailuresPerSecond { get; set; }
        public string? EFCoreTotalOptimisticConcurrencyFailures { get; set; }
        public string? EFCoreOptimisticConcurrencyFailuresPerSecond { get; set; }

        #endregion

        #region Runtime

        public string? RuntimeTimeInGc { get; set; }
        public string? RuntimeAllocRate { get; set; }
        public string? RuntimeCpuUsage
        {
            get => runtimeCpuUsage; set
            {
                runtimeCpuUsage = value;
                CpuUsageList.Add(new MetricDataRecord(DateTime.Now, value));
                CpuUsageList = CpuUsageList.TakeLast(300).ToList();
            }
        }
        public string? RuntimeExceptionCount { get; set; }
        public string? RuntimeGcHeapSize { get; set; }
        public string? RuntimeGen0GcCount { get; set; }
        public string? RuntimeGen0Size { get; set; }
        public string? RuntimeGen1GcCount { get; set; }
        public string? RuntimeGen1Size { get; set; }
        public string? RuntimeGen2GcCount { get; set; }
        public string? RuntimeGen2Size { get; set; }
        public string? RuntimeLohSize { get; set; }
        public string? RuntimePohSize { get; set; }
        public string? RuntimeGcFragmentation { get; set; }
        public string? RuntimeMonitorLockContentionCount { get; set; }
        public string? RuntimeActiveTimerCount { get; set; }
        public string? RuntimeAssemblyCount { get; set; }
        public string? RuntimeThreadpoolCompletedItemsCount { get; set; }
        public string? RuntimeThreadpoolQueueLength { get; set; }
        public string? RuntimeThreadpoolThreadCount { get; set; }
        public string? RuntimeWorkingSet { get; set; }
        public string? RuntimeIlBytesJitted { get; set; }
        public string? RuntimeMethodJittedCount { get; set; }
        public string? RuntimeGcCommittedBytes { get; set; }

        #endregion

        #region Hosting

        public string? HostingCurrentRequests { get; set; }
        public string? HostingFailedRequests { get; set; }
        public string? HostingRequestRate { get; set; }
        public string? HostingTotalRequests { get; set; }

        #endregion

        #region Socket

        public string? SocketsOutgoingConnectionsEstablished { get; set; }
        public string? SocketsIncomingConnectionsEstablished { get; set; }
        public string? SocketsBytesReceived { get; set; }
        public string? SocketsBytesSent { get; set; }

        #endregion
    }
}
