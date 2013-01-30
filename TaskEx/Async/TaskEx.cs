using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwistedOak.Util.TaskEx {
    ///<summary>Contains extension methods related to tasks.</summary>
    public static class TaskEx {
        private static readonly Task CachedCompletedTask = Task.FromResult(true);
        private static readonly Task CachedCancelledTask = CancelledTaskT<bool>();

        ///<summary>A task that has run to completion.</summary>
        public static Task CompletedTask { get { return CachedCompletedTask; } }
        ///<summary>A task that has been cancelled.</summary>
        public static Task CancelledTask { get { return CachedCancelledTask; } }
        ///<summary>Returns a task that has faulted with the given exception.</summary>
        public static Task FaultedTask(Exception exception) {
            var t = new TaskCompletionSource();
            t.SetException(exception);
            return t.Task;
        }
        ///<summary>Returns a task that has faulted with the given exceptions.</summary>
        public static Task FaultedTask(IEnumerable<Exception> exceptions) {
            var t = new TaskCompletionSource();
            t.SetException(exceptions);
            return t.Task;
        }

        ///<summary>Returns a typed task that has been cancelled.</summary>
        public static Task<T> CancelledTaskT<T>() {
            var t = new TaskCompletionSource<T>();
            t.SetCanceled();
            return t.Task;
        }

        ///<summary>Returns a typed task that has faulted with the given exception.</summary>
        public static Task<T> FaultedTaskT<T>(Exception exception) {
            var t = new TaskCompletionSource<T>();
            t.SetException(exception);
            return t.Task;
        }

        ///<summary>Returns a typed task that has faulted with the given exceptions.</summary>
        public static Task<T> FaultedTaskT<T>(IEnumerable<Exception> exceptions) {
            var t = new TaskCompletionSource<T>();
            t.SetException(exceptions);
            return t.Task;
        }

        ///<summary>Determines if the task has ran to completion, as opposed to being faulted, cancelled, or not yet completed.</summary>
        public static bool IsRanToCompletion(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            return task.Status == TaskStatus.RanToCompletion;
        }

        ///<summary>Will observe the task's exception, if any, so that it is not considered propagated to the unobserved exception handler.</summary>
        public static void ObserveAnyException(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            task.ContinueWith(t => t.IsFaulted ? t.Exception : null, TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>A task that runs to completion (with the given task as the result) when the given task either runs to completion, faults, or cancels.</summary>
        public static Task<Task<T>> AnyTypeOfCompletion<T>(this Task<T> task) {
            return task.ContinueWith(e => e, TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>A task that runs to completion (with the given task as the result) when the given task either runs to completion, faults, or cancels.</summary>
        public static Task<Task> AnyTypeOfCompletion(this Task task) {
            return task.ContinueWith(e => e, TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>A task that is cancelled if the given task contains an OperationCanceledException, or else contains the same eventual result.</summary>
        public static Task WithCanceledExceptionToCancellation(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            var r = new TaskCompletionSource();
            task.ContinueWith(
                t => {
                    if (t.IsFaulted && t.Exception.InnerExceptions.Count == 1 && t.Exception.InnerExceptions[0] is OperationCanceledException) {
                        r.SetCanceled();
                    } else {
                        r.SetFromCompletedTask(t);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return r.Task;
        }

        ///<summary>A task that is cancelled if the given task contains an OperationCanceledException, or else contains the same eventual result.</summary>
        public static Task<T> WithCanceledExceptionToCancellation<T>(this Task<T> task) {
            if (task == null) throw new ArgumentNullException("task");
            var r = new TaskCompletionSource<T>();
            task.ContinueWith(
                t => {
                    if (t.IsFaulted && t.Exception.InnerExceptions.Count == 1 && t.Exception.InnerExceptions[0] is OperationCanceledException) {
                        r.SetCanceled();
                    } else {
                        r.SetFromCompletedTask(t);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return r.Task;
        }

        ///<summary>A task that faults if the given task is cancelled, or else contains the same eventual result.</summary>
        public static Task WithCancellationToTaskCanceledException(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            var r = new TaskCompletionSource();
            task.ContinueWith(
                t => {
                    if (t.IsCanceled) {
                        r.SetException(new TaskCanceledException());
                    } else {
                        r.SetFromCompletedTask(t);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return r.Task;
        }

        ///<summary>A task that faults if the given task is cancelled, or else contains the same eventual result.</summary>
        public static Task<T> WithCancellationToTaskCanceledException<T>(this Task<T> task) {
            if (task == null) throw new ArgumentNullException("task");
            var r = new TaskCompletionSource<T>();
            task.ContinueWith(
                t => {
                    if (t.IsCanceled) {
                        r.SetException(new TaskCanceledException());
                    } else {
                        r.SetFromCompletedTask(t);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return r.Task;
        }

        ///<summary>
        ///The eventual result of evaluating a function after a task runs to completion.
        ///The projection is guaranteed to be evaluated in the same synchronization context as the caller.
        ///Cancellation and exceptions are propagated.
        ///</summary>
        public static async Task<T> Select<T>(this Task task, Func<T> projection) {
            if (task == null) throw new ArgumentNullException("task");
            if (projection == null) throw new ArgumentNullException("projection");
            await task;
            return projection();
        }

        ///<summary>
        ///The eventual result of applying a projection to the task's result.
        ///The projection is guaranteed to be evaluated in the same synchronization context as the caller.
        ///Cancellation and exceptions are propagated.
        ///</summary>
        public static async Task<TOut> Select<TIn, TOut>(this Task<TIn> task, Func<TIn, TOut> projection) {
            if (task == null) throw new ArgumentNullException("task");
            if (projection == null) throw new ArgumentNullException("projection");
            return projection(await task);
        }

        ///<summary>
        ///Replaces a task's eventual cancellation with the result of evaluating a function.
        ///If the task runs to completion or faults, then the result is propagated without evaluating the function.
        ///The function is guaranteed to be evaluated in the same synchronization context as the caller.
        ///</summary>
        public static async Task HandleCancelled(this Task task, Action cancelledHandler) {
            if (task == null) throw new ArgumentNullException("task");
            if (cancelledHandler == null) throw new ArgumentNullException("cancelledHandler");
            await task.AnyTypeOfCompletion();
            if (task.IsCanceled) {
                cancelledHandler();
            } else {
                await task;
            }
        }

        ///<summary>
        ///Replaces a task's eventual cancellation with the result of evaluating a function.
        ///If the task runs to completion or faults, then the result is propagated without evaluating the function.
        ///The function is guaranteed to be evaluated in the same synchronization context as the caller.
        ///</summary>
        public static async Task<T> SelectCancelled<T>(this Task<T> task, Func<T> cancelledProjection) {
            if (task == null) throw new ArgumentNullException("task");
            if (cancelledProjection == null) throw new ArgumentNullException("cancelledProjection");
            await task.AnyTypeOfCompletion();
            if (task.IsCanceled) return cancelledProjection();
            return await task;
        }

        /// <summary>
        /// Replaces a task's eventual failure of a given type with the result of evaluating a function.
        /// Otherwise the result of the task is propagated without evaluating the function.
        /// The function is guaranteed to be evaluated in the same synchronization context as the caller.
        /// </summary>
        public static async Task HandleFaulted<TCaughtException>(this Task task, Action<TCaughtException> faultProjection) where TCaughtException : Exception {
            if (task == null) throw new ArgumentNullException("task");
            if (faultProjection == null) throw new ArgumentNullException("faultProjection");
            await task.AnyTypeOfCompletion();
            if (task.IsFaulted && task.Exception.InnerExceptions.Count == 1) {
                var ex = task.Exception.InnerExceptions[0] as TCaughtException;
                if (ex != null) {
                    faultProjection(ex);
                    return;
                }
            }
            await task;
        }

        /// <summary>
        /// Replaces a task's eventual failure of a given type with the result of evaluating a function.
        /// Otherwise the result of the task is propagated without evaluating the function.
        /// The function is guaranteed to be evaluated in the same synchronization context as the caller.
        /// </summary>
        public static async Task<TValue> SelectFaulted<TValue, TCaughtException>(this Task<TValue> task, Func<TCaughtException, TValue> faultHandler)
            where TCaughtException : Exception {
            if (task == null) throw new ArgumentNullException("task");
            if (faultHandler == null) throw new ArgumentNullException("faultHandler");
            await task.AnyTypeOfCompletion();
            if (task.IsFaulted && task.Exception.InnerExceptions.Count == 1) {
                var ex = task.Exception.InnerExceptions[0] as TCaughtException;
                if (ex != null) return faultHandler(ex);
            }
            return await task;
        }

        ///<summary>A task that will complete when all of the supplied tasks have completed.</summary>
        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) {
            return Task.WhenAll(tasks);
        }
        ///<summary>A task that will complete when all of the supplied tasks have completed.</summary>
        public static Task WhenAll(this IEnumerable<Task> tasks) {
            return Task.WhenAll(tasks);
        }
        ///<summary>A task that will complete when any of the supplied tasks have completed.</summary>
        public static Task<Task<T>> WhenAny<T>(this IEnumerable<Task<T>> tasks) {
            return Task.WhenAny(tasks);
        }
        ///<summary>A task that will complete when any of the supplied tasks have completed.</summary>
        public static Task<Task> WhenAny(this IEnumerable<Task> tasks) {
            return Task.WhenAny(tasks);
        }
    }
}
