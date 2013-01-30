using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;
using TwistedOak.Element.Util;

[TestClass]
public class TaskExTest {
    [TestMethod]
    public void CompletedCancelledFaultedTask() {
        Assert.IsTrue(TaskEx.CompletedTask.Status == TaskStatus.RanToCompletion);
        
        Assert.IsTrue(TaskEx.CancelledTask.Status == TaskStatus.Canceled);
        
        Assert.IsTrue(TaskEx.CancelledTaskT<int>().Status == TaskStatus.Canceled);
        
        var ex = new ArgumentException();
        Assert.IsTrue(TaskEx.FaultedTask(ex).Status == TaskStatus.Faulted);
        Assert.IsTrue(TaskEx.FaultedTask(ex).Exception.InnerExceptions.Single() == ex);
        
        Assert.IsTrue(TaskEx.FaultedTaskT<int>(ex).Status == TaskStatus.Faulted);
        Assert.IsTrue(TaskEx.FaultedTaskT<int>(ex).Exception.InnerExceptions.Single() == ex);
    }

    [TestMethod]
    public void WithFaultyCancellation() {
        TaskEx.CompletedTask.WithFaultyCancellation().AssertRanToCompletion();
        Task.FromResult(1).WithFaultyCancellation().AssertRanToCompletion();
        TaskEx.CancelledTask.WithFaultyCancellation().AssertFailed<TaskCanceledException>();
        TaskEx.CancelledTaskT<int>().WithFaultyCancellation().AssertFailed<TaskCanceledException>();
        TaskEx.FaultedTask(new ArgumentOutOfRangeException()).WithFaultyCancellation().AssertFailed<ArgumentOutOfRangeException>();
        TaskEx.FaultedTaskT<int>(new ArgumentOutOfRangeException()).WithFaultyCancellation().AssertFailed<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void ContextAwait() {
        var context1 = new ExclusiveSynchronizationContext();
        var context2 = new ExclusiveSynchronizationContext();
        Assert.IsTrue(context1 != context2);
        Util.AsyncTest(async () => {
            Assert.IsTrue(SynchronizationContext.Current != context1);
            await context1;
            Assert.IsTrue(SynchronizationContext.Current == context1);
            await context2;
            Assert.IsTrue(SynchronizationContext.Current == context2);

            // force re-entry
            var t = new TaskCompletionSource();
            context2.Post(x => t.Task.Wait(), null);
            var tin = new Func<Task>(async () => { await context2.AwaitableEntrance(forceReentry: false); }).Invoke();
            await context1;
            var tforce = new Func<Task>(async () => { await context2.AwaitableEntrance(forceReentry: true); }).Invoke();
            tin.AssertRanToCompletion();
            tforce.AssertNotCompleted();
            t.SetRanToCompletion();
            tforce.AssertRanToCompletion();
        });
    }
    [TestMethod]
    public void EvalIntoTask() {
        var t1 = TaskEx.EvalIntoTask(() => 1);
        var t2 = TaskEx.EvalIntoTask<int>(() => { throw new TaskCanceledException(); });
        var t3 = TaskEx.EvalIntoTask<int>(() => { throw new InvalidOperationException(); });
        Assert.IsTrue(t1.IsRanToCompletion() && t1.Result == 1);
        Assert.IsTrue(t2.IsCanceled);
        Assert.IsTrue(t3.IsFaulted && t3.Exception.InnerExceptions.Single() is InvalidOperationException);
    }
    [TestMethod]
    public void ExecuteIntoTask() {
        var t1 = TaskEx.ExecuteIntoTask(() => { });
        var t2 = TaskEx.ExecuteIntoTask(() => { throw new TaskCanceledException(); });
        var t3 = TaskEx.ExecuteIntoTask(() => { throw new InvalidOperationException(); });
        Assert.IsTrue(t1.IsRanToCompletion());
        Assert.IsTrue(t2.IsCanceled);
        Assert.IsTrue(t3.IsFaulted && t3.Exception.InnerExceptions.Single() is InvalidOperationException);
    }
    [TestMethod]
    public void TrySetFromFinishedTask() {
        var ts = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource<int>()).ToArray();
        Assert.IsTrue(ts[0].TrySetFromCompletedTask(Task.FromResult(1)));
        Assert.IsTrue(ts[0].Task.IsRanToCompletion() && ts[0].Task.Result == 1);

        Util.ExpectException<ArgumentException>(() => ts[0].TrySetFromCompletedTask(Task.Delay(10).ContinueWith(e => 5)));
        Assert.IsTrue(!ts[0].TrySetFromCompletedTask(Task.FromResult(1)));
        Assert.IsTrue(!ts[0].TrySetFromCompletedTask(TaskEx.CancelledTaskT<int>()));
        Assert.IsTrue(!ts[0].TrySetFromCompletedTask(TaskEx.FaultedTaskT<int>(new ArgumentException())));

        Assert.IsTrue(ts[1].TrySetFromCompletedTask(TaskEx.CancelledTaskT<int>()));
        Assert.IsTrue(ts[1].Task.IsCanceled);

        Assert.IsTrue(ts[2].TrySetFromCompletedTask(TaskEx.FaultedTaskT<int>(new InvalidOperationException())));
        Assert.IsTrue(ts[2].Task.IsFaulted && ts[2].Task.Exception.InnerExceptions.Single() is InvalidOperationException);

        var vs = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource()).ToArray();
        Assert.IsTrue(vs[0].TrySetFromCompletedTask(Task.FromResult(1)));
        Assert.IsTrue(vs[0].Task.IsRanToCompletion());

        Util.ExpectException<ArgumentException>(() => vs[0].TrySetFromCompletedTask(Task.Delay(10).ContinueWith(e => 5)));
        Assert.IsTrue(!vs[0].TrySetFromCompletedTask(Task.FromResult(1)));
        Assert.IsTrue(!vs[0].TrySetFromCompletedTask(TaskEx.CancelledTask));
        Assert.IsTrue(!vs[0].TrySetFromCompletedTask(TaskEx.FaultedTask(new ArgumentException())));

        Assert.IsTrue(vs[1].TrySetFromCompletedTask(TaskEx.CancelledTask));
        Assert.IsTrue(vs[1].Task.IsCanceled);

        Assert.IsTrue(vs[2].TrySetFromCompletedTask(TaskEx.FaultedTask(new InvalidOperationException())));
        Assert.IsTrue(vs[2].Task.IsFaulted && vs[2].Task.Exception.InnerExceptions.Single() is InvalidOperationException);
    }

    [TestMethod]
    public void SetFromFinishedTask() {
        var ts = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource<int>()).ToArray();
        ts[0].SetFromCompletedTask(Task.FromResult(1));
        Assert.IsTrue(ts[0].Task.IsRanToCompletion() && ts[0].Task.Result == 1);

        Util.ExpectException<ArgumentException>(() => ts[0].SetFromCompletedTask(Task.Delay(10).ContinueWith(e => 5)));
        Util.ExpectException<InvalidOperationException>(() => ts[0].SetFromCompletedTask(Task.FromResult(1)));
        Util.ExpectException<InvalidOperationException>(() => ts[0].SetFromCompletedTask(TaskEx.CancelledTaskT<int>()));
        Util.ExpectException<InvalidOperationException>(() => ts[0].SetFromCompletedTask(TaskEx.FaultedTaskT<int>(new ArgumentException())));

        ts[1].SetFromCompletedTask(TaskEx.CancelledTaskT<int>());
        Assert.IsTrue(ts[1].Task.IsCanceled);

        ts[2].SetFromCompletedTask(TaskEx.FaultedTaskT<int>(new InvalidOperationException()));
        Assert.IsTrue(ts[2].Task.IsFaulted && ts[2].Task.Exception.InnerExceptions.Single() is InvalidOperationException);

        var vs = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource()).ToArray();
        vs[0].SetFromCompletedTask(Task.FromResult(1));
        Assert.IsTrue(vs[0].Task.IsRanToCompletion());

        Util.ExpectException<ArgumentException>(() => vs[0].SetFromCompletedTask(Task.Delay(10).ContinueWith(e => 5)));
        Util.ExpectException<InvalidOperationException>(() => vs[0].SetFromCompletedTask(Task.FromResult(1)));
        Util.ExpectException<InvalidOperationException>(() => vs[0].SetFromCompletedTask(TaskEx.CancelledTask));
        Util.ExpectException<InvalidOperationException>(() => vs[0].SetFromCompletedTask(TaskEx.FaultedTask(new ArgumentException())));

        vs[1].SetFromCompletedTask(TaskEx.CancelledTask);
        Assert.IsTrue(vs[1].Task.IsCanceled);

        vs[2].SetFromCompletedTask(TaskEx.FaultedTask(new InvalidOperationException()));
        Assert.IsTrue(vs[2].Task.IsFaulted && vs[2].Task.Exception.InnerExceptions.Single() is InvalidOperationException);
    }

    [TestMethod]
    public void TrySetFromTaskAsync() {
        var ts = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource<int>()).ToArray();
        Assert.IsTrue(ts[0].EventuallyTrySetFromTask(Task.Delay(10).ContinueWith(e => 5)).AssertRanToCompletion());
        Assert.IsTrue(ts[0].Task.AssertRanToCompletion() == 5);

        Assert.IsTrue(!ts[0].EventuallyTrySetFromTask(Task.Delay(10).ContinueWith(e => 5)).AssertRanToCompletion());
        Assert.IsTrue(!ts[0].EventuallyTrySetFromTask(Task.FromResult(1)).AssertRanToCompletion());
        Assert.IsTrue(!ts[0].EventuallyTrySetFromTask(TaskEx.CancelledTaskT<int>()).AssertRanToCompletion());
        Assert.IsTrue(!ts[0].EventuallyTrySetFromTask(TaskEx.FaultedTaskT<int>(new ArgumentException())).AssertRanToCompletion());

        Assert.IsTrue(ts[1].EventuallyTrySetFromTask(TaskEx.CancelledTaskT<int>()).AssertRanToCompletion());
        ts[1].Task.AssertCancelled();

        Assert.IsTrue(ts[2].EventuallyTrySetFromTask(TaskEx.FaultedTaskT<int>(new InvalidOperationException())).AssertRanToCompletion());
        ts[2].Task.AssertFailed<InvalidOperationException>();

        var vs = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource()).ToArray();
        Assert.IsTrue(vs[0].EventuallyTrySetFromTask(Task.Delay(10).ContinueWith(e => 5)).AssertRanToCompletion());
        vs[0].Task.AssertRanToCompletion();

        Assert.IsTrue(!vs[0].EventuallyTrySetFromTask(Task.Delay(10).ContinueWith(e => 5)).AssertRanToCompletion());
        Assert.IsTrue(!vs[0].EventuallyTrySetFromTask(Task.FromResult(1)).AssertRanToCompletion());
        Assert.IsTrue(!vs[0].EventuallyTrySetFromTask(TaskEx.CancelledTaskT<int>()).AssertRanToCompletion());
        Assert.IsTrue(!vs[0].EventuallyTrySetFromTask(TaskEx.FaultedTaskT<int>(new ArgumentException())).AssertRanToCompletion());

        Assert.IsTrue(vs[1].EventuallyTrySetFromTask(TaskEx.CancelledTaskT<int>()).AssertRanToCompletion());
        vs[1].Task.AssertCancelled();

        Assert.IsTrue(vs[2].EventuallyTrySetFromTask(TaskEx.FaultedTaskT<int>(new InvalidOperationException())).AssertRanToCompletion());
        vs[2].Task.AssertFailed<InvalidOperationException>();
    }

    [TestMethod]
    public void SetFromTaskAsync() {
        var ts = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource<int>()).ToArray();
        ts[0].EventuallySetFromTask(Task.FromResult(1)).AssertRanToCompletion();
        Assert.IsTrue(ts[0].Task.IsRanToCompletion() && ts[0].Task.Result == 1);

        ts[0].EventuallySetFromTask(Task.Delay(10).ContinueWith(e => 5)).AssertFailed<InvalidOperationException>();
        ts[0].EventuallySetFromTask(Task.FromResult(1)).AssertFailed<InvalidOperationException>();
        ts[0].EventuallySetFromTask(TaskEx.CancelledTaskT<int>()).AssertFailed<InvalidOperationException>();
        ts[0].EventuallySetFromTask(TaskEx.FaultedTaskT<int>(new ArgumentException())).AssertFailed<InvalidOperationException>();

        ts[1].EventuallySetFromTask(TaskEx.CancelledTaskT<int>()).AssertRanToCompletion();
        ts[1].Task.AssertCancelled();

        ts[2].EventuallySetFromTask(TaskEx.FaultedTaskT<int>(new InvalidOperationException())).AssertRanToCompletion();
        ts[2].Task.AssertFailed<InvalidOperationException>();

        var vs = Enumerable.Range(0, 3).Select(e => new TaskCompletionSource()).ToArray();
        vs[0].EventuallySetFromTask(Task.FromResult(1)).AssertRanToCompletion();
        Assert.IsTrue(vs[0].Task.IsRanToCompletion());

        vs[0].EventuallySetFromTask(Task.Delay(10).ContinueWith(e => 5)).AssertFailed<InvalidOperationException>();
        vs[0].EventuallySetFromTask(Task.FromResult(1)).AssertFailed<InvalidOperationException>();
        vs[0].EventuallySetFromTask(TaskEx.CancelledTaskT<int>()).AssertFailed<InvalidOperationException>();
        vs[0].EventuallySetFromTask(TaskEx.FaultedTaskT<int>(new ArgumentException())).AssertFailed<InvalidOperationException>();

        vs[1].EventuallySetFromTask(TaskEx.CancelledTask).AssertRanToCompletion();
        vs[1].Task.AssertCancelled();

        vs[2].EventuallySetFromTask(TaskEx.FaultedTask(new InvalidOperationException())).AssertRanToCompletion();
        vs[2].Task.AssertFailed<InvalidOperationException>();
    }

    [TestMethod]
    public void Select() {
        Assert.IsTrue(TaskEx.CompletedTask.Select(() => 1).AssertRanToCompletion() == 1);
        Assert.IsTrue(Task.FromResult(3).Select(() => 2).AssertRanToCompletion() == 2);
        TaskEx.CancelledTask.Select(() => 1).AssertCancelled();
        TaskEx.FaultedTask(new ArgumentException()).Select(() => 1).AssertFailed<ArgumentException>();

        Assert.IsTrue(Task.FromResult(3).Select(e => e + 1).AssertRanToCompletion() == 4);
        TaskEx.CancelledTaskT<int>().Select(e => e + 1).AssertCancelled();
        TaskEx.FaultedTaskT<int>(new ArgumentException()).Select(e => e + 1).AssertFailed<ArgumentException>();

        TaskEx.CompletedTask.Select<int>(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
        Task.FromResult(1).Select<int>(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
        TaskEx.CompletedTask.Select<int>(() => { throw new TaskCanceledException(); }).AssertCancelled();
        Task.FromResult(1).Select<int>(() => { throw new TaskCanceledException(); }).AssertCancelled();
    }

    [TestMethod]
    public void ProjectCancelled() {
        Assert.IsTrue(Task.FromResult(3).ProjectCancelled(() => 4).AssertRanToCompletion() == 3);
        Assert.IsTrue(TaskEx.CancelledTaskT<int>().ProjectCancelled(() => 4).AssertRanToCompletion() == 4);
        TaskEx.FaultedTaskT<int>(new ArgumentException()).ProjectCancelled(() => 1).AssertFailed<ArgumentException>();

        Assert.IsTrue(Task.FromResult(3).ProjectCancelled<int>(() => { throw new ArgumentException(); }).AssertRanToCompletion() == 3);
        TaskEx.CancelledTaskT<int>().ProjectCancelled<int>(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
        TaskEx.CancelledTaskT<int>().ProjectCancelled<int>(() => { throw new TaskCanceledException(); }).AssertCancelled();
        TaskEx.FaultedTaskT<int>(new ArithmeticException()).ProjectCancelled<int>(() => { throw new ArgumentException(); }).AssertFailed<ArithmeticException>();
    }

    [TestMethod]
    public void ProjectFaulted() {
        Assert.IsTrue(Task.FromResult(3).ProjectFaulted<int, ArgumentException>(e => 4).AssertRanToCompletion() == 3);
        TaskEx.CancelledTaskT<int>().ProjectFaulted<int, ArgumentException>(ex => 4).AssertCancelled();
        Assert.IsTrue(TaskEx.FaultedTaskT<int>(new ArgumentException()).ProjectFaulted<int, ArgumentException>(ex => 1).AssertRanToCompletion() == 1);

        Assert.IsTrue(Task.FromResult(3).ProjectFaulted<int, ArgumentException>(ex => { throw new ArgumentException(); }).AssertRanToCompletion() == 3);
        TaskEx.CancelledTaskT<int>().ProjectFaulted<int, ArgumentException>(ex => { throw new ArithmeticException(); }).AssertCancelled();
        TaskEx.FaultedTaskT<int>(new InvalidOperationException()).ProjectFaulted<int, ArgumentException>(ex => { throw new ArithmeticException(); }).AssertFailed<InvalidOperationException>();
        TaskEx.FaultedTaskT<int>(new ArgumentException()).ProjectFaulted<int, ArgumentException>(ex => { throw new ArithmeticException(); }).AssertFailed<ArithmeticException>();
    }

    [TestMethod]
    public void CompletionNotCancelled() {
        Assert.IsTrue(TaskEx.CompletedTask.CompletionNotCancelledAsync().AssertRanToCompletion() == true);
        Assert.IsTrue(TaskEx.CancelledTask.CompletionNotCancelledAsync().AssertRanToCompletion() == false);
        TaskEx.FaultedTask(new ArgumentException()).CompletionNotCancelledAsync().AssertFailed<ArgumentException>();
        
        var tx = new TaskCompletionSource<int>();
        var ty = tx.Task.CompletionNotCancelledAsync();
        ty.AssertNotCompleted();
        tx.SetCanceled();
        Assert.IsTrue(ty.AssertRanToCompletion() == false);
    }

    [TestMethod]
    public void AnyTypeOfCompletion() {
        Assert.IsTrue(TaskEx.CompletedTask.AnyTypeOfCompletion().AssertRanToCompletion() == TaskEx.CompletedTask);
        var t = Task.FromResult(new object());
        Assert.IsTrue(t.AnyTypeOfCompletion().AssertRanToCompletion() == t);
        Assert.IsTrue(TaskEx.CancelledTask.AnyTypeOfCompletion().AssertRanToCompletion() == TaskEx.CancelledTask);
        var t2 = TaskEx.CancelledTaskT<int>();
        Assert.IsTrue(t2.AnyTypeOfCompletion().AssertRanToCompletion() == t2);
        var t3 = TaskEx.FaultedTaskT<int>(new ArgumentException());
        Assert.IsTrue(t3.AnyTypeOfCompletion().AssertRanToCompletion() == t3);
        var t4 = TaskEx.FaultedTask(new ArgumentException());
        Assert.IsTrue(t4.AnyTypeOfCompletion().AssertRanToCompletion() == t4);

        var tx = new TaskCompletionSource<int>();
        var ty = tx.Task.AnyTypeOfCompletion();
        var tz = (tx.Task as Task).AnyTypeOfCompletion();
        ty.AssertNotCompleted();
        tz.AssertNotCompleted();
        tx.SetCanceled();
        ty.AssertRanToCompletion().AssertCancelled();
        tz.AssertRanToCompletion().AssertCancelled();
    }

    [TestMethod]
    public void IgnoreCancelled() {
        TaskEx.CompletedTask.IgnoreCancelled().AssertRanToCompletion();
        TaskEx.CancelledTask.IgnoreCancelled().AssertRanToCompletion();
        TaskEx.CancelledTaskT<int>().IgnoreCancelled().AssertRanToCompletion();
        TaskEx.FaultedTask(new ArgumentException()).IgnoreCancelled().AssertFailed<ArgumentException>();
        TaskEx.FaultedTaskT<int>(new ArgumentException()).IgnoreCancelled().AssertFailed<ArgumentException>();
    }
    [TestMethod]
    public void PostAction() {
        var c = new SynchronizationContext();
        c.PostAction(() => { }).AssertRanToCompletion();
        c.PostAction(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
        c.PostAction(() => { throw new TaskCanceledException(); }).AssertCancelled();
    }
    [TestMethod]
    public void PostFunc() {
        var c = new SynchronizationContext();
        Assert.IsTrue(c.PostFunc(() => 1).AssertRanToCompletion() == 1);
        c.PostFunc<int>(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
        c.PostFunc<int>(() => { throw new TaskCanceledException(); }).AssertCancelled();
    }

    [TestMethod]
    public void AsTask() {
        var r = new TaskCompletionSource();
        var t = new AnonymousAwaitable<int>(
            () => new AnonymousAwaiter<int>(
                () => false,
                async e => { await r.Task; e(); },
                () => 5)).AsTask();
        var tv = new AnonymousAwaitable(
            () => new AnonymousAwaiter(
                () => false,
                async e => { await r.Task; e(); },
                () => { throw new ArithmeticException(); })).AsTask();
        t.AssertNotCompleted();
        tv.AssertNotCompleted();
        r.SetRanToCompletion();
        Assert.IsTrue(t.AssertRanToCompletion() == 5);
        tv.AssertFailed<ArithmeticException>();
    }
    [TestMethod]
    public void AsIAwaitable() {
        Util.AsyncTest(async () => {
            Assert.IsTrue(await Task.FromResult(1).AsIAwaitable() == 1);
            Assert.IsTrue(await Task.Delay(5).ContinueWith(e => 2).AsIAwaitable() == 2);
            await Task.Delay(5).AsIAwaitable();
        });
    }
}
