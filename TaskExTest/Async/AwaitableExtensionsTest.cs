using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;
using System.Threading.Tasks;

[TestClass]
public class AwaitableExtensionsTest {
    [TestMethod]
    public void TaskToAwaitConversionCompletion() {
        var r = new TaskCompletionSource<int>();
        var rt = r.Task.AsIAwaitable().AsTask();
        var rv = ((Task)r.Task).AsIAwaitable().AsTask();
        
        Task.WhenAny(rv, rt).AssertNotCompleted();
        r.SetResult(1);

        // worked before
        rt.AssertRanToCompletion().AssertEquals(1);
        rv.AssertRanToCompletion();

        // works after
        r.Task.AsIAwaitable().AsTask().AssertRanToCompletion().AssertEquals(1);
        ((Task)r.Task).AsIAwaitable().AsTask().AssertRanToCompletion();
    }
    [TestMethod]
    public void TaskToAwaitConversionCancel() {
        var r = new TaskCompletionSource<int>();
        var rt = r.Task.AsIAwaitable().AsTask();
        var rv = ((Task)r.Task).AsIAwaitable().AsTask();

        Task.WhenAny(rv, rt).AssertNotCompleted();
        r.SetCanceled();

        // worked before
        rt.AssertCancelled();
        rv.AssertCancelled();

        // works after
        r.Task.AsIAwaitable().AsTask().AssertCancelled();
        ((Task)r.Task).AsIAwaitable().AsTask().AssertCancelled();
    }
    [TestMethod]
    public void TaskToAwaitConversionFault() {
        var r = new TaskCompletionSource<int>();
        var rt = r.Task.AsIAwaitable().AsTask();
        var rv = ((Task)r.Task).AsIAwaitable().AsTask();

        Task.WhenAny(rv, rt).AssertNotCompleted();
        var ex = new InvalidOperationException();
        r.SetException(ex);

        // worked before
        rt.AssertFailed<InvalidOperationException>().AssertEquals(ex);
        rv.AssertFailed<InvalidOperationException>().AssertEquals(ex);

        // works after
        r.Task.AsIAwaitable().AsTask().AssertFailed<InvalidOperationException>().AssertEquals(ex);
        ((Task)r.Task).AsIAwaitable().AsTask().AssertFailed<InvalidOperationException>().AssertEquals(ex);
    }
}
