﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TwistedOak.Element.Util;

namespace TwistedOak.Util.TaskEx {
    public static class TaskEx {
        private static readonly Task CachedCompletedTask = Task.FromResult(true);
        private static readonly Task CachedCancelledTask = CancelledTaskT<bool>();

        ///<summary>A task that has run to completion.</summary>
        public static Task CompletedTask { get { return CachedCompletedTask; } }
        ///<summary>A task that has been cancelled.</summary>
        public static Task CancelledTask { get { return CachedCancelledTask; } }
        ///<summary>Returns a task that has faulted with the given exception.</summary>
        public static Task FaultedTask(Exception ex) {
            return FaultedTaskT<bool>(ex);
        }

        ///<summary>Returns a typed task that has been cancelled.</summary>
        public static Task<T> CancelledTaskT<T>() {
            var t = new TaskCompletionSource<T>();
            t.SetCanceled();
            return t.Task;
        }
        ///<summary>Returns a typed task that has fauled with the given exception.</summary>
        public static Task<T> FaultedTaskT<T>(Exception ex) {
            var t = new TaskCompletionSource<T>();
            t.SetException(ex);
            return t.Task;
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
        /// <summary>Sets the task source based on the result, fault or cancellation of the given finished task.</summary>
        public static void SetFromFinishedTask(this TaskCompletionSource source, Task task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            switch (task.Status) {
                case TaskStatus.RanToCompletion: source.SetRanToCompletion(); break;
                case TaskStatus.Faulted: source.SetException(task.Exception.Collapse()); break;
                case TaskStatus.Canceled: source.SetCanceled(); break;
                default: throw new ArgumentException("Task not finished.");
            }
        }
        /// <summary>Sets the task source based on the result, fault or cancellation of the given finished task.</summary>
        public static void SetFromFinishedTask<T>(this TaskCompletionSource<T> source, Task<T> task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            switch (task.Status) {
                case TaskStatus.RanToCompletion: source.SetResult(task.Result); break;
                case TaskStatus.Faulted: source.SetException(task.Exception.Collapse()); break;
                case TaskStatus.Canceled: source.SetCanceled(); break;
                default: throw new ArgumentException("Task not finished.");
            }
        }
        /// <summary>Tries to set the task source based on the result, fault or cancellation of the given finished task.</summary>
        public static bool TrySetFromFinishedTask(this TaskCompletionSource source, Task task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            switch (task.Status) {
                case TaskStatus.RanToCompletion: return source.TrySetRanToCompletion();
                case TaskStatus.Faulted: return source.TrySetException(task.Exception.Collapse());
                case TaskStatus.Canceled: return source.TrySetCanceled();
                default: throw new ArgumentException("Task not finished.");
            }
        }
        /// <summary>Tries to set the task source based on the result, fault or cancellation of the given finished task.</summary>
        public static bool TrySetFromFinishedTask<T>(this TaskCompletionSource<T> source, Task<T> task) {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");
            switch (task.Status) {
                case TaskStatus.RanToCompletion: return source.TrySetResult(task.Result);
                case TaskStatus.Faulted: return source.TrySetException(task.Exception.Collapse());
                case TaskStatus.Canceled: return source.TrySetCanceled();
                default: throw new ArgumentException("Task not finished.");
            }
        }

        /// <summary>
        /// Returns a task whose result is obtained by projecting the result of the given task.
        /// Cancellation and exceptions are propagated.
        /// </summary>
        public static async Task<T> Select<T>(this Task task, Func<T> projection, CancellationToken ct = default(CancellationToken)) {
            if (task == null) throw new ArgumentNullException("task");
            if (projection == null) throw new ArgumentNullException("projection");
            await task.CancellableAwait(ct);
            return projection();
        }
        /// <summary>
        /// Returns a task whose result is obtained by projecting the result of the given task.
        /// Cancellation and exceptions are propagated.
        /// </summary>
        public static async Task<TOut> Select<TIn, TOut>(this Task<TIn> task, Func<TIn, TOut> projection) {
            if (task == null) throw new ArgumentNullException("task");
            if (projection == null) throw new ArgumentNullException("projection");
            return projection(await task);
        }

        /// <summary>
        /// Returns a task whose result is the same as the given task, unless it is cancelled, in which case it is the result of the given function.
        /// </summary>
        public static async Task<T> ProjectCancelled<T>(this Task<T> task, Func<T> cancelledProjection) {
            if (task == null) throw new ArgumentNullException("task");
            if (cancelledProjection == null) throw new ArgumentNullException("cancelledProjection");
            await task.AnyTypeOfCompletion();
            if (task.IsCanceled) return cancelledProjection();
            return await task;
        }
        /// <summary>
        /// Returns a task whose result is the same as the given task, unless it is cancelled, in which case it is the result of the given action.
        /// </summary>
        public static async Task ProjectCancelled(this Task task, Action cancelledProjection) {
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
                    return TaskEx.CancelledTaskT<T>();
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

        public static Task<bool> TrySetFromTaskAsync<T>(this TaskCompletionSource<T> source, Task<T> task) {
            return task.ContinueWith(t => TrySetFromFinishedTask(source, t));
        }
        public static Task SetFromTaskAsync<T>(this TaskCompletionSource<T> source, Task<T> task) {
            return task.ContinueWith(t => SetFromFinishedTask(source, t));
        }
        public static Task SetFromTaskAsync(this TaskCompletionSource source, Task task) {
            return task.ContinueWith(t => SetFromFinishedTask(source, t));
        }
        public static Task<bool> TrySetFromTaskAsync(this TaskCompletionSource source, Task task) {
            return task.ContinueWith(t => TrySetFromFinishedTask(source, t));
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

        ///<summary>Asynchronously dispatches an action to the synchronization context.</summary>
        public static Task PostAction(this SynchronizationContext context, Action action) {
            if (context == null) throw new ArgumentNullException("context");
            if (action == null) throw new ArgumentNullException("action");
            var t = new TaskCompletionSource();
            context.Post(x => t.SetFromFinishedTask(action.ExecuteIntoTask()), null);
            return t.Task;
        }
        ///<summary>Asynchronously dispatches an action to the synchronization context.</summary>
        public static Task<T> PostFunc<T>(this SynchronizationContext context, Func<T> func) {
            if (context == null) throw new ArgumentNullException("context");
            if (func == null) throw new ArgumentNullException("func");
            var t = new TaskCompletionSource<T>();
            context.Post(x => t.SetFromFinishedTask(func.EvalIntoTask()), null);
            return t.Task;
        }

        ///<summary>Returns a task that completes succesfully when the underlying task finishes (whether or not it runs to completion, faults, or cancels).</summary>
        public static Task<Task<T>> AnyTypeOfCompletion<T>(this Task<T> task) {
            return task.ContinueWith(e => e, TaskContinuationOptions.ExecuteSynchronously);
        }
        ///<summary>Returns a task that completes succesfully when the underlying task finishes (whether or not it runs to completion, faults, or cancels).</summary>
        public static Task<Task> AnyTypeOfCompletion(this Task task) {
            return task.ContinueWith(e => e, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Eventually determines if a task completed succesfully (true) or was cancelled (false).
        /// If the task faults, the exception is propagated.
        /// </summary>
        public static Task<bool> CompletionNotCancelledAsync(this Task task) {
            if (task == null) throw new ArgumentNullException("task");
            return task.Select(() => true).ProjectCancelled(() => false);
        }

        ///<summary>Returns an awaitable object that, when awaited, resumes execution within the given synchronization context.</summary>        
        ///<param name="context">The synchronization context to enter.</param>
        ///<param name="forceReentry">Determines if awaiting the current synchronization context results in re-posting to the context or continuing synchronously.</param>
        ///<param name="ct">
        ///Succesful entrance is cancelled when the token is cancelled. 
        ///Determines if a cancellation exception is thrown when the result is accessed.
        ///</param>
        public static IAwaitable AwaitableEntrance(this SynchronizationContext context, CancellationToken ct = default(CancellationToken), bool forceReentry = true) {
            if (context == null) throw new ArgumentNullException("context");
            return new AnonymousAwaitable(() => new AnonymousAwaiter(
                () => !forceReentry && SynchronizationContext.Current == context,
                continuation => context.Post(x => continuation(), null),
                ct.ThrowIfCancellationRequested));
        }
        ///<summary>Returns an awaitable object that, when awaited, resumes execution by posting to the given synchronization context.</summary>        
        public static IAwaiter GetAwaiter(this SynchronizationContext context) {
            if (context == null) throw new ArgumentNullException("context");
            return context.AwaitableEntrance().GetAwaiter();
        }
        ///<summary>Returns an awaitable object that checks a cancellation token, throwing if the token is cancelled, when its result is queried.</summary>        
        public static IAwaitable CancellableAwait(this Task task, CancellationToken ct) {
            if (task == null) throw new ArgumentNullException("task");
            if (!ct.CanBeCanceled) return task.AsIAwaitable();

            var r = new TaskCompletionSource();
            ct.Register(() => r.TrySetRanToCompletion());
            task.ContinueWith(self => r.TrySetRanToCompletion());

            var context = SynchronizationContext.Current ?? new SynchronizationContext();
            return new AnonymousAwaitable(() => new AnonymousAwaiter(
                () => ct.IsCancellationRequested || task.IsCompleted,
                continuation => r.Task.ContinueWith(
                    self => context.Post(x => continuation(), null),
                    TaskContinuationOptions.ExecuteSynchronously),
                () => {
                    ct.ThrowIfCancellationRequested();
                    if (task.IsCanceled) throw new TaskCanceledException();
                    if (task.IsFaulted) throw task.Exception.Collapse();
                }));
        }
        ///<summary>Returns an awaitable object that checks a cancellation token, throwing if the token is cancelled, when its result is queried.</summary>        
        public static IAwaitable<T> CancellableAwait<T>(this Task<T> task, CancellationToken ct) {
            if (task == null) throw new ArgumentNullException("task");
            if (!ct.CanBeCanceled) return task.AsIAwaitable();

            var r = new TaskCompletionSource();
            ct.Register(() => r.TrySetRanToCompletion());
            task.ContinueWith(self => r.TrySetRanToCompletion());

            var context = SynchronizationContext.Current ?? new SynchronizationContext();
            return new AnonymousAwaitable<T>(() => new AnonymousAwaiter<T>(
                () => ct.IsCancellationRequested || task.IsCompleted,
                continuation => r.Task.ContinueWith(
                    self => context.Post(x => continuation(), null),
                    TaskContinuationOptions.ExecuteSynchronously),
                () => {
                    ct.ThrowIfCancellationRequested();
                    if (task.IsCanceled) throw new TaskCanceledException();
                    if (task.IsFaulted) throw task.Exception.Collapse();
                    return task.Result;
                }));
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

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) {
            return Task.WhenAll(tasks);
        }
        public static Task WhenAll(this IEnumerable<Task> tasks) {
            return Task.WhenAll(tasks);
        }
        public static Task<Task<T>> WhenAny<T>(this IEnumerable<Task<T>> tasks) {
            return Task.WhenAny(tasks);
        }
        public static Task<Task> WhenAny(this IEnumerable<Task> tasks) {
            return Task.WhenAny(tasks);
        }
    }
}
