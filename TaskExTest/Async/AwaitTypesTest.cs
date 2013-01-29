using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;

[TestClass]
public class AwaitTypesTest {
    [TestMethod]
    public void AwaitableAwaiterT() {
        var x = 0;
        var y = 0;
        var L = 0;
        var a = new AnonymousAwaitable<int>(
            () => new AnonymousAwaiter<int>(
                () => { x += 1; return x > 1; },
                cont => { L += 1; cont(); },
                () => { y += 1; if (y > 4) throw new ArgumentException(); y += 1; return y; }));
        Util.AsyncTest(async () => {
            Assert.IsTrue(x == 0);
            Assert.IsTrue(L == 0);
            Assert.IsTrue(y == 0);
            Assert.IsTrue(await a == 2);
            Assert.IsTrue(x == 1);
            Assert.IsTrue(L == 1);
            Assert.IsTrue(y == 2);
            Assert.IsTrue(await a == 4);
            Assert.IsTrue(x == 2);
            Assert.IsTrue(L == 1);
            Assert.IsTrue(y == 4);
            try { await a; Assert.Fail(); } catch (ArgumentException) { }
            Assert.IsTrue(x == 3);
            Assert.IsTrue(L == 1);
            Assert.IsTrue(y == 5);
        });
    }
    [TestMethod]
    public void AwaitableAwaiterVoid() {
        var x = 0;
        var y = 0;
        var L = 0;
        var a = new AnonymousAwaitable(
            () => new AnonymousAwaiter(
                () => { x += 1; return x > 1; },
                cont => { L += 1; cont(); },
                () => { y += 1; if (y > 4) throw new ArgumentException(); y += 1; }));
        Util.AsyncTest(async () => {
            Assert.IsTrue(x == 0);
            Assert.IsTrue(L == 0);
            Assert.IsTrue(y == 0);
            await a;
            Assert.IsTrue(x == 1);
            Assert.IsTrue(L == 1);
            Assert.IsTrue(y == 2);
            await a;
            Assert.IsTrue(x == 2);
            Assert.IsTrue(L == 1);
            Assert.IsTrue(y == 4);
            try { await a; Assert.Fail(); } catch (ArgumentException) { }
            Assert.IsTrue(x == 3);
            Assert.IsTrue(L == 1);
            Assert.IsTrue(y == 5);
        });
    }
}
