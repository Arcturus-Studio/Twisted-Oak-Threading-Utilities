using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;
using System.Threading;
using System.Linq;

internal static class Util {
    private static void TestWait(this Task t, TimeSpan timeout) {
        var r = new TaskCompletionSource();
        t.ContinueWith(e => r.TrySetRanToCompletion());
        Task.Delay(timeout).ContinueWith(e => r.TrySetRanToCompletion());
        try {
            r.Task.Wait();
        } catch (Exception) {
        }
    }
    [DebuggerStepThrough]
    public static void AssertEquals<T>(this T v1, T v2) {
        Assert.AreEqual(actual: v1, expected: v2);
    }
    [DebuggerStepThrough]
    public static void AssertDoesNotEqual<T>(this T v1, T v2) {
        Assert.AreNotEqual(actual: v1, notExpected: v2);
    }
    [DebuggerStepThrough]
    public static void AssertSequenceEquals<T>(this IEnumerable<T> actual, IEnumerable<T> expected) {
        var r1 = actual.ToArray();
        var r2 = expected.ToArray();
        if (!r1.SequenceEqual(r2))
            Assert.Fail("Sequences not equal. Expected: {0}; Actual: {1}", String.Join(",", r2), String.Join(",", r1));
    }
    [DebuggerStepThrough]
    public static void AssertIsTrue(this bool b) {
        Assert.IsTrue(b);
    }
    [DebuggerStepThrough]
    public static void AssertIsFalse(this bool b) {
        Assert.IsFalse(b);
    }
    public static void ExpectException<TExpectedException>(Action action) where TExpectedException : Exception {
        try {
            action();
        } catch (TExpectedException) {
            return;
        }
        throw new InvalidOperationException("Expected an exception.");
    }
    public static Action Ack<T>(Func<T> func) {
        return () => func();
    }
    public static void AsyncTest(Func<Task> test, TimeSpan? timeout = null) {
        RunAsyncAwait(test).AssertRanToCompletion(timeout ?? TimeSpan.FromSeconds(10));
    }
    public static void AssertCancelled(this CancellationToken ct, TimeSpan? timeout = null) {
        var r = new TaskCompletionSource();
        ct.Register(() => r.TrySetRanToCompletion());
        Task.Delay(timeout ?? TimeSpan.FromSeconds(5)).ContinueWith(e => r.TrySetRanToCompletion());
        r.Task.Wait();
        Assert.IsTrue(ct.IsCancellationRequested);
    }
    public static void AssertNotCancelled(this CancellationToken ct, TimeSpan? timeout = null) {
        var r = new TaskCompletionSource();
        ct.Register(() => r.TrySetRanToCompletion());
        Task.Delay(timeout ?? TimeSpan.FromMilliseconds(25)).ContinueWith(e => r.TrySetRanToCompletion());
        r.Task.Wait();
        Assert.IsTrue(!ct.IsCancellationRequested);
    }
    public static void AssertRanToCompletion(this Task t, TimeSpan? timeout = null) {
        t.TestWait(timeout ?? TimeSpan.FromSeconds(5));
        Assert.IsTrue(t.Status == TaskStatus.RanToCompletion);
    }
    public static T AssertRanToCompletion<T>(this Task<T> t, TimeSpan? timeout = null) {
        t.TestWait(timeout ?? TimeSpan.FromSeconds(3));
        Assert.IsTrue(t.Status == TaskStatus.RanToCompletion);
        return t.Result;
    }
    public static TExpectedException AssertFailed<TExpectedException>(this Task t, TimeSpan? timeout = null) where TExpectedException : Exception {
        try {
            t.TestWait(timeout ?? TimeSpan.FromSeconds(5));
            Assert.IsTrue(t.Status == TaskStatus.Faulted);
            t.Wait();
            throw new AssertFailedException("Did not fail when expected.");
        } catch (TExpectedException ex) {
            return ex;
        } catch (AggregateException e) {
            if (e.InnerExceptions.Count == 1) {
                var ex = e.InnerExceptions[0];
                if (ex is TExpectedException) return (TExpectedException)ex;
            }
            throw;
        }
    }
    public static void AssertCancelled(this Task t, TimeSpan? timeout = null) {
        t.TestWait(timeout ?? TimeSpan.FromSeconds(5));
        Assert.IsTrue(t.Status == TaskStatus.Canceled);
    }
    public static Task<T> Timeout<T>(this Task<T> t, TimeSpan? timeout = null) {
        var r = new TaskCompletionSource<T>();
        t.ContinueWith(self => {
            if (t.IsFaulted) r.TrySetException(t.Exception);
            else if (t.IsCanceled) r.TrySetCanceled();
            else if (t.IsCompleted) r.TrySetResult(t.Result);
            else r.TrySetException(new InvalidOperationException("??"));
        });
        Task.Delay(timeout ?? TimeSpan.FromSeconds(1)).ContinueWith(self => r.TrySetException(new AssertFailedException("Timeout")));
        return r.Task;
    }
    public static Task Timeout(this Task t, TimeSpan? timeout = null) {
        var r = new TaskCompletionSource();
        t.ContinueWith(self => {
            if (t.IsFaulted) r.TrySetException(t.Exception);
            else if (t.IsCanceled) r.TrySetCanceled();
            else if (t.IsCompleted) r.TrySetRanToCompletion();
            else r.TrySetException(new InvalidOperationException("??"));
        });
        Task.Delay(timeout ?? TimeSpan.FromSeconds(1)).ContinueWith(self => r.TrySetException(new AssertFailedException("Timeout")));
        return r.Task;
    }
    public static void AssertNotCompleted(this Task t, TimeSpan? timeout = null) {
        t.TestWait(timeout ?? TimeSpan.FromMilliseconds(25));
        Assert.IsTrue(!t.IsCompleted);
        Assert.IsTrue(!t.IsFaulted);
        Assert.IsTrue(!t.IsCanceled);
    }
    public async static Task RunAsync(Action a) {
        var t = new TaskCompletionSource();
        await Task.Run(() => {
            try {
                a();
                t.SetRanToCompletion();
            } catch (Exception ex) {
                t.SetException(ex);
            }
        });
        await t.Task;
    }
    public async static Task RunAsyncAwait(Func<Task> a) {
        var t = new TaskCompletionSource();
        await Task.Run(async () => {
            try {
                await a();
                t.SetRanToCompletion();
            } catch (Exception ex) {
                t.SetException(ex);
            }
        });
        await t.Task;
    }
}
