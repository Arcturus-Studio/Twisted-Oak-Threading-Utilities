using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;

[TestClass]
public class OnetimeLockTest {
    [TestMethod]
    public void TryAcquire() {
        // TryAcquired acquires the lock once
        var r = new OnetimeLock();
        Assert.IsTrue(r.TryAcquire());
        Assert.IsTrue(!r.TryAcquire());
        Assert.IsTrue(!r.TryAcquire());
    }
    [TestMethod]
    public void IsAcquired() {
        // IsAcquired determines if the lock is acquired
        var r = new OnetimeLock();
        Assert.IsTrue(!r.IsAcquired());
        Assert.IsTrue(r.TryAcquire());
        Assert.IsTrue(r.IsAcquired());
    }
    [TestMethod]
    public void TryAcquire_Race() {
        // racing TryAcquires: only one wins
        var r = new OnetimeLock();
        var x = Task.WhenAll(
            Enumerable.Range(0, 10).Select(
                e => Task.Factory.StartNew(
                    () => r.TryAcquire(),
                    TaskCreationOptions.LongRunning))).AssertRanToCompletion();
        Assert.IsTrue(x.Where(e => e).Count() == 1);
        Assert.IsTrue(r.IsAcquired());
    }
}
