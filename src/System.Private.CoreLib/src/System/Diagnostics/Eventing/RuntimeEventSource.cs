// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;

namespace System.Diagnostics.Tracing
{
    /// <summary>
    /// RuntimeEventSource is an EventSource that represents events emitted by the managed runtime.
    /// </summary>
    [EventSource(Guid="49592C0F-5A05-516D-AA4B-A64E02026C89", Name = "System.Runtime")]
    internal sealed class RuntimeEventSource : EventSource
    {
        private static RuntimeEventSource s_RuntimeEventSource;
        private PollingCounter? _gcHeapSizeCounter;
        private IncrementingPollingCounter? _gen0GCCounter;
        private IncrementingPollingCounter? _gen1GCCounter;
        private IncrementingPollingCounter? _gen2GCCounter;
        private IncrementingPollingCounter? _exceptionCounter;
        private PollingCounter? _cpuTimeCounter;
        private PollingCounter? _workingSetCounter;
        private PollingCounter? _threadPoolThreadCounter;
        private IncrementingPollingCounter? _monitorContentionCounter;
        private PollingCounter? _threadPoolQueueCounter;
        private IncrementingPollingCounter? _completedItemsCounter;
        private PollingCounter? _gcTimeCounter;
        private PollingCounter? _gen0SizeCounter;
        private PollingCounter? _gen1SizeCounter;
        private PollingCounter? _gen2SizeCounter;
        private PollingCounter? _lohSizeCounter;
        private IncrementingPollingCounter? _allocRateCounter;
        private PollingCounter? _assemblyCounter;
        private PollingCounter? _timerCounter;

        private const int EnabledPollingIntervalMilliseconds = 1000; // 1 second

        public static void Initialize()
        {
            s_RuntimeEventSource = new RuntimeEventSource();
        }
        
        private RuntimeEventSource(): base(new Guid(0x49592C0F, 0x5A05, 0x516D, 0xAA, 0x4B, 0xA6, 0x4E, 0x02, 0x02, 0x6C, 0x89), "System.Runtime", EventSourceSettings.EtwSelfDescribingEventFormat)
        {
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                // NOTE: These counters will NOT be disposed on disable command because we may be introducing 
                // a race condition by doing that. We still want to create these lazily so that we aren't adding
                // overhead by at all times even when counters aren't enabled.

                // On disable, PollingCounters will stop polling for values so it should be fine to leave them around.
                _cpuTimeCounter = _cpuTimeCounter ?? new PollingCounter("cpu-usage", this, () => RuntimeEventSourceHelper.GetCpuUsage()) { DisplayName = "CPU Usage" };
                _workingSetCounter = _workingSetCounter ?? new PollingCounter("working-set", this, () => (double)(Environment.WorkingSet / 1000000)) { DisplayName = "Working Set" };
                _gcHeapSizeCounter = _gcHeapSizeCounter ?? new PollingCounter("gc-heap-size", this, () => (double)(GC.GetTotalMemory(false) / 1000000)) { DisplayName = "GC Heap Size" };
                _gen0GCCounter = _gen0GCCounter ?? new IncrementingPollingCounter("gen-0-gc-count", this, () => GC.CollectionCount(0)) { DisplayName = "Gen 0 GC Count", DisplayRateTimeScale = new TimeSpan(0, 1, 0) };
                _gen1GCCounter = _gen1GCCounter ?? new IncrementingPollingCounter("gen-1-gc-count", this, () => GC.CollectionCount(1)) { DisplayName = "Gen 1 GC Count", DisplayRateTimeScale = new TimeSpan(0, 1, 0) };
                _gen2GCCounter = _gen2GCCounter ?? new IncrementingPollingCounter("gen-2-gc-count", this, () => GC.CollectionCount(2)) { DisplayName = "Gen 2 GC Count", DisplayRateTimeScale = new TimeSpan(0, 1, 0) };
                _exceptionCounter = _exceptionCounter ?? new IncrementingPollingCounter("exception-count", this, () => Exception.GetExceptionCount()) { DisplayName = "Exception Count", DisplayRateTimeScale = new TimeSpan(0, 0, 1) };
                _threadPoolThreadCounter = _threadPoolThreadCounter ?? new PollingCounter("threadpool-thread-count", this, () => ThreadPool.ThreadCount) { DisplayName = "ThreadPool Thread Count" };
                _monitorContentionCounter = _monitorContentionCounter ?? new IncrementingPollingCounter("monitor-lock-contention-count", this, () => Monitor.LockContentionCount) { DisplayName = "Monitor Lock Contention Count", DisplayRateTimeScale = new TimeSpan(0, 0, 1) }; 
                _threadPoolQueueCounter = _threadPoolQueueCounter ?? new PollingCounter("threadpool-queue-length", this, () => ThreadPool.PendingWorkItemCount) { DisplayName = "ThreadPool Queue Length" };
                _completedItemsCounter = _completedItemsCounter ?? new IncrementingPollingCounter("threadpool-completed-items-count", this, () => ThreadPool.CompletedWorkItemCount) { DisplayName = "ThreadPool Completed Work Item Count", DisplayRateTimeScale = new TimeSpan(0, 0, 1) };
                _gcTimeCounter = _gcTimeCounter ?? new PollingCounter("time-in-gc", this, () => GC.GetLastGCPercentTimeInGC()) { DisplayName = "Time in GC" };
                _gen0SizeCounter = _gen0SizeCounter ?? new PollingCounter("gen-0-size", this, () => GC.GetGenerationSize(0)) { DisplayName = "Gen 0 Size" };
                _gen1SizeCounter = _gen1SizeCounter ?? new PollingCounter("gen-1-size", this, () => GC.GetGenerationSize(1)) { DisplayName = "Gen 1 Size" };
                _gen2SizeCounter = _gen2SizeCounter ?? new PollingCounter("gen-2-size", this, () => GC.GetGenerationSize(2)) { DisplayName = "Gen 2 Size" };
                _lohSizeCounter = _lohSizeCounter ?? new PollingCounter("loh-size", this, () => GC.GetGenerationSize(3)) { DisplayName = "LOH Size" };
                _allocRateCounter = _allocRateCounter ?? new IncrementingPollingCounter("alloc-rate", this, () => GC.GetTotalAllocatedBytes()) { DisplayName = "Allocation Rate", DisplayRateTimeScale = new TimeSpan(0, 0, 1) };
                _assemblyCounter = _assemblyCounter ?? new PollingCounter("assembly-count", this, () => System.Reflection.Assembly.GetAssemblyCount()) { DisplayName = "Number of Assemblies Loaded" };
                _timerCounter = _timerCounter ?? new PollingCounter("active-timer-count", this, () => Timer.ActiveCount) { DisplayName = "Number of Active Timers" };
            }
        }
    }
}
