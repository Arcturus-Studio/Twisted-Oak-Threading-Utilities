using System;
using System.Threading;
using System.Threading.Tasks;

namespace TwistedOak.Threading {
    ///<summary>
    ///Throttles work until the previous work's task has completed.
    ///When new work is provided before the previous work's task has completed, any existing throttled work is discarded and cancelled.
    ///</summary>
    public sealed class DiscardRedundantWorkThrottle {
        private sealed class Job {
            public Func<Task> Work;
            public TaskCompletionSource CompletionSource;
        }
        private static readonly Job RunningButNothingQueuedJob = new Job();

        ///<summary>Holds the interlocked running/qeued state.</summary>
        ///<remarks>
        ///if null: not running and nothing queued
        ///elif RunningButNothingQueuedJob: running something but nothing queued
        ///elif X -> running something and X queued
        ///</remarks>
        private Job _queuedJob;
        private readonly SynchronizationContext _runContext;

        ///<summary>Constructs a throttle that runs work on the given context.</summary>
        ///<param name="runContext">
        ///The synchronization context that work is run on.
        ///Defaults to the thread pool when null.
        ///</param>
        public DiscardRedundantWorkThrottle(SynchronizationContext runContext = null) {
            this._runContext = runContext ?? new SynchronizationContext();
        }

        private async void ProcessActionsAsync() {
            do {
                // get last set action
                var n = Interlocked.Exchange(ref _queuedJob, RunningButNothingQueuedJob);
                // always re-enter the running context, to ensure the throttle doesn't dominate it
                await _runContext;
                // await completion of potentially asynchronous work
                await n.CompletionSource.EventuallySetFromTask(Tasks.FromEvaluation(n.Work).Unwrap());
                // exit when no queued work was set during the previous work
            } while (!ReferenceEquals(Interlocked.CompareExchange(ref _queuedJob, null, RunningButNothingQueuedJob), RunningButNothingQueuedJob));
        }

        ///<summary>
        ///Sets the next (asynchronous) work to be run, either now or when the currently running action has finished, to be the given function.
        ///The work will not be considered completed until the resulting task has completed.
        ///</summary>
        ///<returns>A task for the work's eventual result, failure, or cancelation (if it is replaced before it can run).</returns>
        public Task SetNextToAsyncFunction(Func<Task> asyncWork) {
            if (asyncWork == null) throw new ArgumentNullException("asyncWork");
            var t = new TaskCompletionSource();
            var prev = Interlocked.Exchange(ref _queuedJob, new Job {Work = asyncWork, CompletionSource = t});
            if (prev == null) { // there was no queued work or running work
                ProcessActionsAsync();
            } else if (!ReferenceEquals(prev, RunningButNothingQueuedJob)) { // we replaced some queued work
                prev.CompletionSource.SetCanceled();
            } // else there's running work and we were the first to queue, and nothing needs to be done
            return t.Task;
        }

        ///<summary>
        ///Sets the next (asynchronous) work to be run, either now or when the currently running action has finished, to be the given function.
        ///The work will not be considered completed until the resulting task has completed.
        ///</summary>
        ///<returns>A task for the work's eventual result, failure, or cancelation (if it is replaced before it can run).</returns>
        public async Task<T> SetNextToAsyncFunction<T>(Func<Task<T>> func) {
            if (func == null) throw new ArgumentNullException("func");
            var result = default(T);
            await SetNextToAsyncFunction(async () => { result = await func(); });
            return result;
        }

        ///<summary>Sets the next work to be run, either now or when the currently running action has finished, to be the given action.</summary>
        ///<returns>A task for the work's eventual success, failure, or cancelation (if it is replaced before it can run).</returns>
        public Task SetNextToAction(Action action) {
            if (action == null) throw new ArgumentNullException("action");
            return SetNextToAsyncFunction(() => { action(); return Tasks.RanToCompletion(); });
        }

        ///<summary>Sets the next work to be run, either now or when the currently running action has finished, to be the given function.</summary>
        ///<returns>A task for the work's eventual result, failure, or cancelation (if it is replaced before it can run).</returns>
        public Task<T> SetNextToFunction<T>(Func<T> func) {
            if (func == null) throw new ArgumentNullException("func");
            return SetNextToAsyncFunction(() => Task.FromResult(func()));
        }
    }
}
