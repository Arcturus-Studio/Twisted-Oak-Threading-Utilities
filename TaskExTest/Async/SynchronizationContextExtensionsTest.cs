using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;

[TestClass]
public class SynchronizationContextExtensionsTest {
    [TestMethod]
    public void AwaitableSyncContexts() {
        var r = new ExclusiveSynchronizationContext();
        Task.Factory.StartNew(async () => {
            SynchronizationContext.Current.AssertDoesNotEqual(r);
            await r;
            SynchronizationContext.Current.AssertEquals(r);
        }).Unwrap().AssertRanToCompletion();
    }
    [TestMethod]
    public void AwaitableEntrance() {
        var n = 0;
        var r = new AnonymousSynchronizationContext(a => {
            n += 1;
            a();
        });
        Task.Factory.StartNew(async () => {
            SynchronizationContext.Current.AssertDoesNotEqual(r);
            n.AssertEquals(0);

            await r.AwaitableEntrance(forceReentry: true);
            SynchronizationContext.Current.AssertEquals(r);
            n.AssertEquals(1);

            await r.AwaitableEntrance(forceReentry: true);
            SynchronizationContext.Current.AssertEquals(r);
            n.AssertEquals(2);

            await r.AwaitableEntrance(forceReentry: false);
            SynchronizationContext.Current.AssertEquals(r);
            n.AssertEquals(2);

            await new SynchronizationContext();
            await r.AwaitableEntrance(forceReentry: false);
            SynchronizationContext.Current.AssertEquals(r);
            n.AssertEquals(3);
        }).Unwrap().AssertRanToCompletion();
    }
    [TestMethod]
    public void Post() {
        var r = new SynchronizationContext();
        
        var n = 0;
        r.PostAction(() => n = 1).AssertRanToCompletion();
        n.AssertEquals(1);
        
        r.PostAction(() => { throw new InvalidOperationException(); }).AssertFailed<InvalidOperationException>();
        r.PostFunc(() => 2).AssertRanToCompletion().AssertEquals(2);
        r.PostFunc<int>(() => { throw new InvalidOperationException(); }).AssertFailed<InvalidOperationException>();
    }
}
