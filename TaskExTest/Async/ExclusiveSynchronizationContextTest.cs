using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;

[TestClass]
public class ExclusiveSynchronizationContextTest {
    [TestMethod]
    public void Propagate() {
        new Func<Task>(async () => {
            await new ExclusiveSynchronizationContext();
            throw new InvalidOperationException("test");
        }).Invoke().AssertFailed<InvalidOperationException>();
    }
    [TestMethod]
    public void NoInterference() {
        var r = new ExclusiveSynchronizationContext();
        var n = 0;
        var t = Task.WhenAll(Enumerable.Range(0, 5).Select(async e => await Util.RunAsync(() => {
            for (var i = 0; i < 500; i++) {
                r.Post(z => {
                    n += 1;
                }, null);
            }
            var a = new TaskCompletionSource();
            r.Post(z => a.SetRanToCompletion(), null);
            a.Task.AssertRanToCompletion();
        })));
        t.AssertRanToCompletion(timeout: TimeSpan.FromSeconds(20)); // lots of work, long timeout
        Assert.IsTrue(n == 500 * 5);
    }
    [TestMethod]
    public void NoOverlap() {
        var r = new ExclusiveSynchronizationContext();
        var n = 0;
        for (var i = 0; i < 1000; i++) {
            r.Post(z => {
                Assert.IsTrue(Interlocked.Increment(ref n) == 1);
                Assert.IsTrue(Interlocked.Decrement(ref n) == 0);
            }, null);
        }
        var a = new TaskCompletionSource();
        r.Post(z => a.SetRanToCompletion(), null);
        a.Task.AssertRanToCompletion();
    }
}
