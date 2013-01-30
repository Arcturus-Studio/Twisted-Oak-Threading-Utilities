using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Threading;

[TestClass]
public class DiscardRedundantWorkThrottleTest {
    [TestMethod]
    public void SetNext() {
        var r = new DiscardRedundantWorkThrottle();
        var ax = new ArgumentException();
        var cx = new OperationCanceledException();

        // results are propagated
        var n = 0;
        r.SetNextToAction(() => n++).AssertRanToCompletion();
        n.AssertEquals(1);
        r.SetNextToFunction(() => 2).AssertRanToCompletion().AssertEquals(2);
        r.SetNextToAsyncFunction(Tasks.RanToCompletion).AssertRanToCompletion();
        r.SetNextToAsyncFunction(() => Task.FromResult(3)).AssertRanToCompletion().AssertEquals(3);

        // faulted tasks are propagated
        r.SetNextToAsyncFunction(() => Tasks.Faulted(ax)).AssertFailed<ArgumentException>();
        r.SetNextToAsyncFunction(() => Tasks.Faulted<int>(ax)).AssertFailed<ArgumentException>();

        // cancelled tasks are propagated
        r.SetNextToAsyncFunction(Tasks.Cancelled).AssertCancelled();
        r.SetNextToAsyncFunction(Tasks.Cancelled<int>).AssertCancelled();

        // thrown cancellation exceptions indicate cancellation
        r.SetNextToAsyncFunction(() => { throw cx; }).AssertCancelled();
        r.SetNextToAsyncFunction<int>(() => { throw cx; }).AssertCancelled();
        r.SetNextToAction(() => { throw cx; }).AssertCancelled();
        r.SetNextToFunction<int>(() => { throw cx; }).AssertCancelled();

        // thrown exceptions are propagated
        r.SetNextToAsyncFunction(() => { throw ax; }).AssertFailed<ArgumentException>();
        r.SetNextToAsyncFunction<int>(() => { throw ax; }).AssertFailed<ArgumentException>();
        r.SetNextToAction(() => { throw ax; }).AssertFailed<ArgumentException>();
        r.SetNextToFunction<int>(() => { throw ax; }).AssertFailed<ArgumentException>();
    }

    [TestMethod]
    public void NoOverlap() {
        var r = new DiscardRedundantWorkThrottle();
        var n = 0;
        Enumerable.Range(0, 5)
                  .Select(async e => await Task.Factory.StartNew(
                      () => {
                          for (var i = 0; i < 1000; i++) {
                              r.SetNextToAction(() => {
                                  Interlocked.Increment(ref n).AssertEquals(1);
                                  Interlocked.Decrement(ref n).AssertEquals(0);
                              });
                          }
                      },
                      TaskCreationOptions.LongRunning))
                  .WhenAll()
                  .AssertRanToCompletion();
    }

    [TestMethod]
    public void WaitsForAsyncAndCancels() {
        var r = new DiscardRedundantWorkThrottle();
        var h = new TaskCompletionSource();
        
        var t1 = r.SetNextToAsyncFunction(() => h.Task);
        var t2 = r.SetNextToAction(() => { });
        new[] { t1, t2 }.WhenAny().AssertNotCompleted();
        
        var t3 = r.SetNextToAction(() => { });
        t2.AssertCancelled();
        new[] {t1, t3}.WhenAny().AssertNotCompleted();
        
        h.SetRanToCompletion();
        t1.AssertRanToCompletion();
        t3.AssertRanToCompletion();
    }

    [TestMethod]
    public void WorksWithReentrancy() {
        for (var i = 0; i < 25; i++) {
            var r = new DiscardRedundantWorkThrottle();
            var h = new TaskCompletionSource();
            Task t = null;
            r.SetNextToAction(() => t = r.SetNextToAsyncFunction(() => h.Task)).AssertRanToCompletion();
            t.AssertNotCompleted(timeout: TimeSpan.FromMilliseconds(5));
            h.SetRanToCompletion();
            t.AssertRanToCompletion();
        }
    }
}
