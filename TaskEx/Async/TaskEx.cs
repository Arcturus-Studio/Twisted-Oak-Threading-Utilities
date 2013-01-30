using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwistedOak.Element.Util;

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

        ///<summary>A task that runs to completion (with the given task as the result) when the given task either runs to completion, faults, or cancels.</summary>
        public static Task<Task<T>> AnyTypeOfCompletion<T>(this Task<T> task) {
            return task.ContinueWith(e => e, TaskContinuationOptions.ExecuteSynchronously);
        }

        ///<summary>A task that runs to completion (with the given task as the result) when the given task either runs to completion, faults, or cancels.</summary>
        public static Task<Task> AnyTypeOfCompletion(this Task task) {
            return task.ContinueWith(e => e, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// The eventual result of evaluating a function after a task runs to completion.
        /// The projection is evaluated in the same synchronization context as the caller.
        /// Cancellation and exceptions are propagated.
        /// </summary>
        public static async Task<T> Select<T>(this Task task, Func<T> projection) {
            if (task == null) throw new ArgumentNullException("task");
            if (projection == null) throw new ArgumentNullException("projection");
            await task;
            return projection();
        }

        /// <summary>
        /// The eventual result of applying a projection to the task's result.
        /// The projection is evaluated in the same synchronization context as the caller.
        /// Cancellation and exceptions are propagated.
        /// </summary>
        public static async Task<TOut> Select<TIn, TOut>(this Task<TIn> task, Func<TIn, TOut> projection) {
            if (task == null) throw new ArgumentNullException("task");
            if (projection == null) throw new ArgumentNullException("projection");
            return projection(await task);
        }

        /// <summary>
        /// Replaces a task's eventual cancellation with the result of evaluating a function.
        /// If the task runs to completion or faults, then the result is propagated without evaluating the function.
        /// The function is evaluated in the same synchronization context as the caller.
        /// </summary>
        public static async Task<T> SelectWhenCancelled<T>(this Task<T> task, Func<T> cancelledProjection) {
            if (task == null) throw new ArgumentNullException("task");
            if (cancelledProjection == null) throw new ArgumentNullException("cancelledProjection");
            await task.AnyTypeOfCompletion();
            if (task.IsCanceled) return cancelledProjection();
            return await task;
        }

        /// <summary>
        /// Replaces a task's eventual cancellation with the result of evaluating a function.
        /// If the task runs to completion or faults, then the result is propagated without evaluating the function.
        /// The function is evaluated in the same synchronization context as the caller.
        /// </summary>
        public static async Task SelectWhenCancelled(this Task task, Action cancelledProjection) {
            if (task == null) throw new ArgumentNullException("task");
            if (cancelledProjection == null) throw new ArgumentNullException("cancelledProjection");
            await task.AnyTypeOfCompletion();
            if (task.IsCanceled) {
                cancelledProjection();
            } else {
                await task;
            }
        }

        public static Task<T> CancelExceptionToCancelled<T>(this Task<T> task) {
            return task.ContinueWith(e => {
                if (e.IsFaulted && e.Exception.Collapse() is OperationCanceledException) {
                    return CancelledTaskT<T>();
                }
                return e;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        ///<summary>Returns a task that faults if the underlying task is cancelled, but otherwise has the same result.</summary>
        public static Task WithFaultyCancellation(this Task task) {
            var tcs = new TaskCompletionSource();
            task.ContinueWith(t => {
                if (t.IsFaulted) tcs.SetException(t.Exception);
                else if (t.IsCanceled) tcs.SetException(new TaskCanceledException());
                else tcs.SetRanToCompletion();
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
        ///<summary>Returns a task that faults if the underlying task is cancelled, but otherwise has the same result.</summary>
        public static Task<T> WithFaultyCancellation<T>(this Task<T> task) {
            var tcs = new TaskCompletionSource<T>();
            task.ContinueWith(t => {
                if (t.IsFaulted) tcs.SetException(t.Exception);
                else if (t.IsCanceled) tcs.SetException(new TaskCanceledException());
                else tcs.SetResult(t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        /// <summary>
        /// Returns a task whose result is the same as the given task, unless it is cancelled, in which case it is the result of the given function.
        /// </summary>
        public static async Task<TIn> ProjectFaulted<TIn, TCaughtEx>(this Task<TIn> task, Func<TCaughtEx, TIn> faultProjection) where TCaughtEx : Exception {
            if (task == null) throw new ArgumentNullException("task");
            if (faultProjection == null) throw new ArgumentNullException("faultProjection");
            await task.AnyTypeOfCompletion();
            if (task.IsFaulted) {
                var ex = task.Exception.Collapse() as TCaughtEx;
                if (ex != null) return faultProjection(ex);
            }
            return await task;
        }

        public static async Task IgnoreCancelled(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            await task.AnyTypeOfCompletion();
            if (task.IsFaulted) await task;
        }
        public static bool IsRanToCompletion(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            return task.Status == TaskStatus.RanToCompletion;
        }

        ///<summary>
        ///Indicates that the task failing or cancelling is expected, and that nothing should be done.
        ///Handles exceptions propagating out of the task with a no-op, so that they are not considered unhandled.
        ///</summary>
        public static void IgnoreAnyException(this Task task) {
            if (task == null) throw new ArgumentNullException("task");

            // accessing the exception ensures it is considered observed
            task.ContinueWith(t => t.IsFaulted ? t.Exception : null, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Eventually determines if a task completed succesfully (true) or was cancelled (false).
        /// If the task faults, the exception is propagated.
        /// </summary>
        public static Task<bool> CompletionNotCancelledAsync(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            return task.Select(() => true).SelectWhenCancelled(() => false);
        }

        public static async Task AsTask(this IAwaitable awaitable) {
            if (awaitable == null) throw new ArgumentException("awaitable");
            await awaitable;
        }
        public static async Task<T> AsTask<T>(this IAwaitable<T> awaitable) {
            if (awaitable == null) throw new ArgumentException("awaitable");
            return await awaitable;
        }
        public static IAwaitable<T> AsIAwaitable<T>(this Task<T> task) {
            if (task == null) throw new ArgumentException("task");
            return new AnonymousAwaitable<T>(() => {
                var awaiter = task.GetAwaiter();
                return new AnonymousAwaiter<T>(() => awaiter.IsCompleted, awaiter.OnCompleted, awaiter.GetResult);
            });
        }
        public static IAwaitable AsIAwaitable(this Task task) {
            if (task == null) throw new ArgumentException("task");
            return new AnonymousAwaitable(() => {
                var awaiter = task.GetAwaiter();
                return new AnonymousAwaiter(() => awaiter.IsCompleted, awaiter.OnCompleted, awaiter.GetResult);
            });
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
