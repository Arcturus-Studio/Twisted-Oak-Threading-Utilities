using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;

[TestClass]
public class TaskCompletionSourceTest {
    [TestMethod]
    public void SourceTransitions() {
        new TaskCompletionSource().Task.AssertNotCompleted();
        var cex = new Exception[] { new InvalidEnumArgumentException(), new InvalidProgramException() };

        var r1 = new TaskCompletionSource();
        r1.SetRanToCompletion();
        r1.Task.AssertRanToCompletion();

        var r2 = new TaskCompletionSource();
        r2.TrySetRanToCompletion().AssertIsTrue();
        r2.Task.AssertRanToCompletion();

        var r3 = new TaskCompletionSource();
        r3.SetCanceled();
        r3.Task.AssertCancelled();

        var r4 = new TaskCompletionSource();
        r4.TrySetCanceled().AssertIsTrue();
        r4.Task.AssertCancelled();

        var r5 = new TaskCompletionSource();
        r5.SetException(new InvalidEnumArgumentException());
        r5.Task.AssertFailed<InvalidEnumArgumentException>();

        var r6 = new TaskCompletionSource();
        r6.SetException(cex);
        r6.Task.AssertFailed<AggregateException>().InnerExceptions.AssertSequenceEquals(cex);

        var r7 = new TaskCompletionSource();
        r7.TrySetException(new InvalidEnumArgumentException()).AssertIsTrue();
        r7.Task.AssertFailed<InvalidEnumArgumentException>();

        // failed transitions
        Util.ExpectException<InvalidOperationException>(r1.SetRanToCompletion);
        Util.ExpectException<InvalidOperationException>(r1.SetCanceled);
        Util.ExpectException<InvalidOperationException>(() => r1.SetException(new Exception()));
        Util.ExpectException<InvalidOperationException>(() => r1.SetException(cex));
        r1.TrySetRanToCompletion().AssertIsFalse();
        r1.TrySetCanceled().AssertIsFalse();
        r1.TrySetException(new Exception()).AssertIsFalse();
        r1.TrySetException(cex).AssertIsFalse();
    }
}
