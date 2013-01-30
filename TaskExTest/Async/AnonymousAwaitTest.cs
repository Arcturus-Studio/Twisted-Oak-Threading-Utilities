using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;
using System.Threading.Tasks;

[TestClass]
public class AnonymousAwaitableTest {
    [TestMethod]
    public void AwaitableAwaiterVoid() {
        var n = 0;
        var r = new AnonymousAwaitable(
            () => new AnonymousAwaiter(
                () => { n = 1; return true; },
                a => { n = 2; a(); },
                () => n = 3));

        var x = r.GetAwaiter();
        n.AssertEquals(0);

        x.IsCompleted.AssertIsTrue();
        n.AssertEquals(1);

        x.OnCompleted(() => { });
        n.AssertEquals(2);

        x.GetResult();
        n.AssertEquals(3);
    }
    [TestMethod]
    public void AwaitableAwaiterT() {
        var n = 0;
        var r = new AnonymousAwaitable<int>(
            () => new AnonymousAwaiter<int>(
                () => { n = 1; return true; },
                a => { n = 2; a(); },
                () => n = 3));

        var x = r.GetAwaiter();
        n.AssertEquals(0);
        
        x.IsCompleted.AssertIsTrue();
        n.AssertEquals(1);
        
        x.OnCompleted(() => { });
        n.AssertEquals(2);
        
        x.GetResult();
        n.AssertEquals(3);
    }
}
