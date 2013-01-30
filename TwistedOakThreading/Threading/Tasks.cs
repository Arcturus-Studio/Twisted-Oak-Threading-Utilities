using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwistedOak.Threading {
    ///<summary>Contains factory methods and properties for tasks.</summary>
    public static class Tasks {
        ///<summary>A task that ran to completion.</summary>
        public static Task RanToCompletion() { return CachedRanToCompletion; }
        private static readonly Task CachedRanToCompletion = Task.FromResult(true);

        ///<summary>A task that ran to completion with the given result.</summary>
        public static Task<T> RanToCompletion<T>(T result) {
            return Task.FromResult(result);
        }

        ///<summary>A cancelled task.</summary>
        public static Task Cancelled() { return CachedCancelled; }
        private static readonly Task CachedCancelled = Cancelled<bool>();

        ///<summary>A generic cancelled task.</summary>
        public static Task<T> Cancelled<T>() {
            var t = new TaskCompletionSource<T>();
            t.SetCanceled();
            return t.Task;
        }

        ///<summary>A task that faulted with the given exception.</summary>
        public static Task Faulted(Exception exception) {
            var t = new TaskCompletionSource();
            t.SetException(exception);
            return t.Task;
        }

        ///<summary>A task that faulted with the given exceptions.</summary>
        public static Task Faulted(IEnumerable<Exception> exceptions) {
            var t = new TaskCompletionSource();
            t.SetException(exceptions);
            return t.Task;
        }

        ///<summary>A generic task that faulted with the given exception.</summary>
        public static Task<T> Faulted<T>(Exception exception) {
            var t = new TaskCompletionSource<T>();
            t.SetException(exception);
            return t.Task;
        }

        ///<summary>A generic task that faulted with the given exceptions.</summary>
        public static Task<T> Faulted<T>(IEnumerable<Exception> exceptions) {
            var t = new TaskCompletionSource<T>();
            t.SetException(exceptions);
            return t.Task;
        }

#pragma warning disable 1998 // await operator not used in async method
        ///<summary>Synchronously evaluates a function, returning a Task based on its result or failure.</summary>
        ///<remarks>The 'async' modifier packages thrown exceptions into the resulting task, despite no awaits.</remarks>
        public static async Task<T> FromEvaluation<T>(Func<T> func) {
            if (func == null) throw new ArgumentNullException("func");
            return func();
        }
        ///<summary>Synchronously executes an action, returning a Task based on its success or failure.</summary>
        ///<remarks>The 'async' modifier packages thrown exceptions into the resulting task, despite no awaits.</remarks>
        public static async Task FromExecution(Action action) {
            if (action == null) throw new ArgumentNullException("action");
            action();
        }
#pragma warning restore 1998
    }
}
