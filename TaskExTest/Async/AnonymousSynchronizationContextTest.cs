using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;

[TestClass]
public class AnonymousSynchronizationContextTest {
    [TestMethod]
    public void AnonymousPost() {
        var n = 0;
        var m = 0;

        SynchronizationContext r = null;
        r = new AnonymousSynchronizationContext(a => {
            n += 1;
            var c = SynchronizationContext.Current;
            a();
            SynchronizationContext.Current.AssertDoesNotEqual(r);
            SynchronizationContext.Current.AssertEquals(c);
        });
        
        n.AssertEquals(0);
        m.AssertEquals(0);
        
        r.Post(x => {
            SynchronizationContext.Current.AssertEquals(r);
            m += 1;
        }, null);
        
        n.AssertEquals(1);
        m.AssertEquals(1);
    }
}
