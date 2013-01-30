using System;
using System.Threading.Tasks;
using TwistedOak.Element.Util;

namespace TwistedOak.Util.TaskEx {
    ///<summary>Contains extension methods for manipulating task completion sources.</summary>
    public static class TaskCompletionSourceExtensions {
        ///<summary>Transitions the task source into the same state as the given task, once the task eventually completes.</summary>
        ///<returns>A task containing the eventual result (or exception) of the transition.</returns>
        public static Task EventuallySetFromTask(this TaskCompletionSource source, Task task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            return task.ContinueWith(t => SetFromCompletedTask(source, t), TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>Transitions the task source into the same state as the given task, once the task eventually completes.</summary>
        ///<returns>A task containing the eventual result (or exception) of the transition.</returns>
        public static Task EventuallySetFromTask<T>(this TaskCompletionSource<T> source, Task<T> task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            return task.ContinueWith(t => SetFromCompletedTask(source, t), TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>Attempts to transition the task source into the same state as the given task, once the task eventually completes.</summary>
        ///<returns>A task containing the eventual result of the attempted transition.</returns>
        public static Task<bool> EventuallyTrySetFromTask(this TaskCompletionSource source, Task task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            return task.ContinueWith(t => TrySetFromCompletedTask(source, t), TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>Attempts to transition the task source into the same state as the given task, once the task eventually completes.</summary>
        ///<returns>A task containing the eventual result of the attempted transition.</returns>
        public static Task<bool> EventuallyTrySetFromTask<T>(this TaskCompletionSource<T> source, Task<T> task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            return task.ContinueWith(t => TrySetFromCompletedTask(source, t), TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>Transitions the task source into the same state as the given completed task.</summary>
        public static void SetFromCompletedTask(this TaskCompletionSource source, Task task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            if (!task.IsCompleted) throw new ArgumentException("!task.IsCompleted");
            
            switch (task.Status) {
            case TaskStatus.RanToCompletion:
                source.SetRanToCompletion();
                break;
            case TaskStatus.Faulted:
                source.SetException(task.Exception.Collapse());
                break;
            case TaskStatus.Canceled:
                source.SetCanceled();
                break;
            default:
                throw new InvalidOperationException("Unexpected task state");
            }
        }

        ///<summary>Transitions the task source into the same state as the given completed task.</summary>
        public static void SetFromCompletedTask<T>(this TaskCompletionSource<T> source, Task<T> task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            if (!task.IsCompleted) throw new ArgumentException("!task.IsCompleted");

            switch (task.Status) {
            case TaskStatus.RanToCompletion:
                source.SetResult(task.Result);
                break;
            case TaskStatus.Faulted:
                source.SetException(task.Exception.Collapse());
                break;
            case TaskStatus.Canceled:
                source.SetCanceled();
                break;
            default:
                throw new InvalidOperationException("Unexpected task state");
            }
        }

        ///<summary>Attempts to transition the task source into the same state as the given completed task.</summary>
        public static bool TrySetFromCompletedTask(this TaskCompletionSource source, Task task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            if (!task.IsCompleted) throw new ArgumentException("!task.IsCompleted");

            switch (task.Status) {
            case TaskStatus.RanToCompletion:
                return source.TrySetRanToCompletion();
            case TaskStatus.Faulted:
                return source.TrySetException(task.Exception.Collapse());
            case TaskStatus.Canceled:
                return source.TrySetCanceled();
            default:
                throw new InvalidOperationException("Unexpected task state");
            }
        }

        ///<summary>Attempts to transition the task source into the same state as the given completed task.</summary>
        public static bool TrySetFromCompletedTask<T>(this TaskCompletionSource<T> source, Task<T> task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            if (!task.IsCompleted) throw new ArgumentException("!task.IsCompleted");
            
            switch (task.Status) {
            case TaskStatus.RanToCompletion:
                return source.TrySetResult(task.Result);
            case TaskStatus.Faulted:
                return source.TrySetException(task.Exception.Collapse());
            case TaskStatus.Canceled:
                return source.TrySetCanceled();
            default:
                throw new InvalidOperationException("Unexpected task state");
            }
        }

#pragma warning disable 1998 // await operator not used in async method
        /// <summary>Safely evaluates a function, returning a Task based on its result or failure.</summary>
        /// <remarks>The 'async' modifier packages thrown exceptions into the resulting task, despite no awaits.</remarks>
        public static async Task<T> EvalIntoTask<T>(this Func<T> func) {
            if (func == null) throw new ArgumentNullException("func");
            return func();
        }
        /// <summary>Safely executes an action, returning a Task based on its success or failure.</summary>
        /// <remarks>The 'async' modifier packages thrown exceptions into the resulting task, despite no awaits.</remarks>
        public static async Task ExecuteIntoTask(this Action action) {
            if (action == null) throw new ArgumentNullException("action");
            action();
        }
#pragma warning restore 1998
    }
}
