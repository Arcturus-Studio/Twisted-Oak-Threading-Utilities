using System;
using System.Linq;
using TwistedOak.Element.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using TwistedOak.Util.TaskEx;

[TestClass]
public class ExceptionUtilTest {
    [TestMethod]
    public void Collapse() {
        var ax1 = new AggregateException();
        var cx1 = ax1.Collapse();
        Assert.IsTrue(cx1 is AggregateException && ((AggregateException)cx1).InnerExceptions.Count == 0);

        var ex2 = new Exception();
        var ax2 = new AggregateException(ex2);
        Assert.IsTrue(ax2.Collapse() == ex2);

        var ax3 = new AggregateException(new Exception(), new Exception());
        var cx3 = ax3.Collapse();
        Assert.IsTrue(cx3 is AggregateException && ((AggregateException)cx3).InnerExceptions.SequenceEqual(ax3.InnerExceptions));

        var ex4 = new Exception();
        var ax4 = new AggregateException(new AggregateException(), new AggregateException(ex4));
        Assert.IsTrue(ax4.Collapse() == ex4);

        var ex5A = new Exception();
        var ex5B = new Exception();
        var ax5 = new AggregateException(new AggregateException(ex5A), new AggregateException(ex5B));
        var cx5 = ax5.Collapse();
        Assert.IsTrue(cx5 is AggregateException && ((AggregateException)cx5).InnerExceptions.SequenceEqual(new[] { ex5A, ex5B }));

        var ex6 = new Exception();
        var ax6 = new AggregateException(new AggregateException(ex6), new AggregateException(ex6));
        Assert.IsTrue(ax6.Collapse() == ex6);
    }
}
