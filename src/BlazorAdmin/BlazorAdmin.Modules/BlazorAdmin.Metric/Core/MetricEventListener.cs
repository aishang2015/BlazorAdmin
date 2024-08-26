using System.Diagnostics.Tracing;

namespace BlazorAdmin.Metric.Core
{
    public class MetricEventListener : EventListener
    {
        public MetricData Metrics { get; private set; } = new MetricData();

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            //Console.WriteLine(eventSource.Name);
            if (eventSource.Name.Equals("Microsoft.EntityFrameworkCore") ||
                eventSource.Name.Equals("System.Runtime") ||
                eventSource.Name.Equals("Microsoft.AspNetCore.Hosting") ||
                eventSource.Name.Equals("System.Net.Sockets"))
            {

                var arguments = new Dictionary<string, string?>()
                {
                    ["EventCounterIntervalSec"] = "1"
                };

                EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All, arguments);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            eventData.Payload.ToList().ForEach(x =>
            {
                var (counterName, counterValue) = GetRelevantMetric(x as IDictionary<string, object>);
                var result = counterName switch
                {
                    "active-db-contexts" => Metrics.EFCoreActiveDbContexts = counterValue,
                    "total-queries" => Metrics.EFCoreTotalQueries = counterValue,
                    "queries-per-second" => Metrics.EFCoreQueriesPerSecond = counterValue,
                    "total-save-changes" => Metrics.EFCoreTotalSaveChanges = counterValue,
                    "save-changes-per-second" => Metrics.EFCoreSaveChangesPerSecond = counterValue,
                    "compiled-query-cache-hit-rate" => Metrics.EFCoreCompiledQueryCacheHitRate = counterValue,
                    "total-execution-strategy-operation-failures" => Metrics.EFCoreTotalExecutionStrategyOperationFailures = counterValue,
                    "execution-strategy-operation-failures-per-second" => Metrics.EFCoreExecutionStrategyOperationFailuresPerSecond = counterValue,
                    "total-optimistic-concurrency-failures" => Metrics.EFCoreTotalOptimisticConcurrencyFailures = counterValue,
                    "optimistic-concurrency-failures-per-second" => Metrics.EFCoreOptimisticConcurrencyFailuresPerSecond = counterValue,

                    "time-in-gc" => Metrics.RuntimeTimeInGc = counterValue,
                    "alloc-rate" => Metrics.RuntimeAllocRate = counterValue,
                    "cpu-usage" => Metrics.RuntimeCpuUsage = counterValue,
                    "exception-count" => Metrics.RuntimeExceptionCount = counterValue,
                    "gc-heap-size" => Metrics.RuntimeGcHeapSize = counterValue,
                    "gen-0-gc-count" => Metrics.RuntimeGen0GcCount = counterValue,
                    "gen-0-size" => Metrics.RuntimeGen0Size = counterValue,
                    "gen-1-gc-count" => Metrics.RuntimeGen1GcCount = counterValue,
                    "gen-1-size" => Metrics.RuntimeGen1Size = counterValue,
                    "gen-2-gc-count" => Metrics.RuntimeGen2GcCount = counterValue,
                    "gen-2-size" => Metrics.RuntimeGen2Size = counterValue,
                    "loh-size" => Metrics.RuntimeLohSize = counterValue,
                    "poh-size" => Metrics.RuntimePohSize = counterValue,
                    "gc-fragmentation" => Metrics.RuntimeGcFragmentation = counterValue,
                    "monitor-lock-contention-count" => Metrics.RuntimeMonitorLockContentionCount = counterValue,
                    "active-timer-count" => Metrics.RuntimeActiveTimerCount = counterValue,
                    "assembly-count" => Metrics.RuntimeAssemblyCount = counterValue,
                    "threadpool-completed-items-count" => Metrics.RuntimeThreadpoolCompletedItemsCount = counterValue,
                    "threadpool-queue-length" => Metrics.RuntimeThreadpoolQueueLength = counterValue,
                    "threadpool-thread-count" => Metrics.RuntimeThreadpoolThreadCount = counterValue,
                    "working-set" => Metrics.RuntimeWorkingSet = counterValue,
                    "il-bytes-jitted" => Metrics.RuntimeIlBytesJitted = counterValue,
                    "method-jitted-count" => Metrics.RuntimeMethodJittedCount = counterValue,
                    "gc-committed-bytes" => Metrics.RuntimeGcCommittedBytes = counterValue,

                    "current-requests" => Metrics.HostingCurrentRequests = counterValue,
                    "failed-requests" => Metrics.HostingFailedRequests = counterValue,
                    "requests-per-second" => Metrics.HostingRequestRate = counterValue,
                    "total-requests" => Metrics.HostingTotalRequests = counterValue,

                    "outgoing-connections-established" => Metrics.SocketsOutgoingConnectionsEstablished = counterValue,
                    "incoming-connections-established" => Metrics.SocketsIncomingConnectionsEstablished = counterValue,
                    "bytes-received" => Metrics.SocketsBytesReceived = counterValue,
                    "bytes-sent" => Metrics.SocketsBytesSent = counterValue,

                    _ => string.Empty
                };
            });
        }

        private static (string, string) GetRelevantMetric(IDictionary<string, object> eventPayload)
        {
            if (eventPayload == null)
                return (null, null);

            var counterName = "";
            var counterValue = "";
            if (eventPayload.TryGetValue("Name", out object displayValue))
            {
                counterName = displayValue.ToString();
            }
            if (eventPayload.TryGetValue("Mean", out object value) ||
                eventPayload.TryGetValue("Increment", out value))
            {
                counterValue = value.ToString();
            }

            return (counterName, counterValue);
        }
    }
}
