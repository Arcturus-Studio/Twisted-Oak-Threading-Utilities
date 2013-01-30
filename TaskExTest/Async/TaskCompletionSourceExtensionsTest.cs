using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;
using System.Threading.Tasks;

[TestClass]
public class TaskCompletionSourceExtensionsTest {
    [TestMethod]
    public void SetFromCompletedTaskVoid() {
        var cex = new Exception[] { new InvalidEnumArgumentException(), new InvalidProgramException() };

        var r1 = new TaskCompletionSource();
        r1.SetFromCompletedTask(Tasks.RanToCompletion());
        r1.Task.AssertRanToCompletion();

        var r2 = new TaskCompletionSource();
        r2.SetFromCompletedTask(Tasks.Cancelled());
        r2.Task.AssertCancelled();

        var r3 = new TaskCompletionSource();
        r3.SetFromCompletedTask(Tasks.Faulted(new InvalidEnumArgumentException()));
        r3.Task.AssertFailed<InvalidEnumArgumentException>();

        var r4 = new TaskCompletionSource();
        r4.SetFromCompletedTask(Tasks.Faulted(cex));
        r4.Task.AssertFailed<AggregateException>().InnerExceptions.AssertSequenceEquals(cex);

        // failed transitions
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.RanToCompletion()));
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.Cancelled()));
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.Faulted(new InvalidEnumArgumentException())));
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.Faulted(cex)));
    }
    [TestMethod]
    public void SetFromCompletedTaskGeneric() {
        var cex = new Exception[] { new InvalidEnumArgumentException(), new InvalidProgramException() };

        var r1 = new TaskCompletionSource<int>();
        r1.SetFromCompletedTask(Tasks.RanToCompletion(1));
        r1.Task.AssertRanToCompletion();

        var r2 = new TaskCompletionSource<int>();
        r2.SetFromCompletedTask(Tasks.Cancelled<int>());
        r2.Task.AssertCancelled();

        var r3 = new TaskCompletionSource<int>();
        r3.SetFromCompletedTask(Tasks.Faulted<int>(new InvalidEnumArgumentException()));
        r3.Task.AssertFailed<InvalidEnumArgumentException>();

        var r4 = new TaskCompletionSource<int>();
        r4.SetFromCompletedTask(Tasks.Faulted<int>(cex));
        r4.Task.AssertFailed<AggregateException>().InnerExceptions.AssertSequenceEquals(cex);

        // failed transitions
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.RanToCompletion(1)));
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.Cancelled<int>()));
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.Faulted<int>(new InvalidEnumArgumentException())));
        Util.ExpectException<InvalidOperationException>(() => r1.SetFromCompletedTask(Tasks.Faulted<int>(cex)));
    }

    [TestMethod]
    public void TrySetFromCompletedTaskVoid() {
        var cex = new Exception[] { new InvalidEnumArgumentException(), new InvalidProgramException() };

        var r1 = new TaskCompletionSource();
        r1.TrySetFromCompletedTask(Tasks.RanToCompletion()).AssertIsTrue();
        r1.Task.AssertRanToCompletion();

        var r2 = new TaskCompletionSource();
        r2.TrySetFromCompletedTask(Tasks.Cancelled()).AssertIsTrue();
        r2.Task.AssertCancelled();

        var r3 = new TaskCompletionSource();
        r3.TrySetFromCompletedTask(Tasks.Faulted(new InvalidEnumArgumentException())).AssertIsTrue();
        r3.Task.AssertFailed<InvalidEnumArgumentException>();

        var r4 = new TaskCompletionSource();
        r4.TrySetFromCompletedTask(Tasks.Faulted(cex)).AssertIsTrue();
        r4.Task.AssertFailed<AggregateException>().InnerExceptions.AssertSequenceEquals(cex);

        // failed transitions
        r1.TrySetFromCompletedTask(Tasks.RanToCompletion()).AssertIsFalse();
        r1.TrySetFromCompletedTask(Tasks.Cancelled()).AssertIsFalse();
        r1.TrySetFromCompletedTask(Tasks.Faulted(new InvalidEnumArgumentException())).AssertIsFalse();
        r1.TrySetFromCompletedTask(Tasks.Faulted(cex)).AssertIsFalse();
    }
    [TestMethod]
    public void TrySetFromCompletedTaskGeneric() {
        var cex = new Exception[] { new InvalidEnumArgumentException(), new InvalidProgramException() };

        var r1 = new TaskCompletionSource<int>();
        r1.TrySetFromCompletedTask(Tasks.RanToCompletion(1)).AssertIsTrue();
        r1.Task.AssertRanToCompletion();

        var r2 = new TaskCompletionSource<int>();
        r2.TrySetFromCompletedTask(Tasks.Cancelled<int>()).AssertIsTrue();
        r2.Task.AssertCancelled();

        var r3 = new TaskCompletionSource<int>();
        r3.TrySetFromCompletedTask(Tasks.Faulted<int>(new InvalidEnumArgumentException())).AssertIsTrue();
        r3.Task.AssertFailed<InvalidEnumArgumentException>();

        var r4 = new TaskCompletionSource<int>();
        r4.TrySetFromCompletedTask(Tasks.Faulted<int>(cex)).AssertIsTrue();
        r4.Task.AssertFailed<AggregateException>().InnerExceptions.AssertSequenceEquals(cex);

        // failed transitions
        r1.TrySetFromCompletedTask(Tasks.RanToCompletion(1)).AssertIsFalse();
        r1.TrySetFromCompletedTask(Tasks.Cancelled<int>()).AssertIsFalse();
        r1.TrySetFromCompletedTask(Tasks.Faulted<int>(new InvalidEnumArgumentException())).AssertIsFalse();
        r1.TrySetFromCompletedTask(Tasks.Faulted<int>(cex)).AssertIsFalse();
    }
    [TestMethod]
    public void EventuallySetFromTaskVoid() {
        var r1 = new TaskCompletionSource();
        var r2 = new TaskCompletionSource();
        var t3 = r1.EventuallySetFromTask(r2.Task);
        r1.Task.AssertNotCompleted();
        t3.AssertNotCompleted();

        r2.SetRanToCompletion();
        t3.AssertRanToCompletion();
        r1.Task.AssertRanToCompletion();

        r1.EventuallySetFromTask(r2.Task).AssertFailed<InvalidOperationException>();
    }
    [TestMethod]
    public void EventuallySetFromTaskGeneric() {
        var r1 = new TaskCompletionSource<int>();
        var r2 = new TaskCompletionSource<int>();
        var t3 = r1.EventuallySetFromTask(r2.Task);
        r1.Task.AssertNotCompleted();
        t3.AssertNotCompleted();

        r2.SetResult(1);
        t3.AssertRanToCompletion();
        r1.Task.AssertRanToCompletion().AssertEquals(1);
        
        r1.EventuallySetFromTask(r2.Task).AssertFailed<InvalidOperationException>();
    }
    [TestMethod]
    public void EventuallyTrySetFromTaskVoid() {
        var r1 = new TaskCompletionSource();
        var r2 = new TaskCompletionSource();
        var t3 = r1.EventuallyTrySetFromTask(r2.Task);
        r1.Task.AssertNotCompleted();
        t3.AssertNotCompleted();

        r2.TrySetRanToCompletion();
        t3.AssertRanToCompletion().AssertIsTrue();
        r1.Task.AssertRanToCompletion();

        r1.EventuallyTrySetFromTask(r2.Task).AssertRanToCompletion().AssertIsFalse();
    }
    [TestMethod]
    public void EventuallyTrySetFromTaskGeneric() {
        var r1 = new TaskCompletionSource<int>();
        var r2 = new TaskCompletionSource<int>();
        var t3 = r1.EventuallyTrySetFromTask(r2.Task);
        r1.Task.AssertNotCompleted();
        t3.AssertNotCompleted();

        r2.TrySetResult(1);
        t3.AssertRanToCompletion().AssertIsTrue();
        r1.Task.AssertRanToCompletion().AssertEquals(1);

        r1.EventuallyTrySetFromTask(r2.Task).AssertRanToCompletion().AssertIsFalse();
    }
}
