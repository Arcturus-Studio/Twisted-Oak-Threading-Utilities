using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Util.TaskEx;

[TestClass]
public class TasksTest {
    [TestMethod]
    public void CompletedCancelledFaultedTask() {
        Tasks.RanToCompletion().AssertRanToCompletion();
        Tasks.RanToCompletion(1).AssertRanToCompletion().AssertEquals(1);
        
        Tasks.Cancelled().AssertCancelled();
        Tasks.Cancelled<int>().AssertCancelled();
        
        var ex = new ArgumentException();
        
        Tasks.Faulted(ex).AssertFailed<ArgumentException>().AssertEquals(ex);
        Tasks.Faulted<int>(ex).AssertFailed<ArgumentException>().AssertEquals(ex);
        
        Tasks.Faulted(new[] { ex, ex }).AssertFailed<AggregateException>().InnerExceptions.AssertSequenceEquals(new[] { ex, ex });
        Tasks.Faulted<int>(new[] { ex, ex }).AssertFailed<AggregateException>().InnerExceptions.AssertSequenceEquals(new[] { ex, ex });
    }
    [TestMethod]
    public void FromEvaluationExecution() {
        Tasks.FromEvaluation(() => 1).AssertRanToCompletion().AssertEquals(1);
        Tasks.FromEvaluation<int>(() => { throw new TaskCanceledException(); }).AssertCancelled();
        Tasks.FromEvaluation<int>(() => { throw new InvalidOperationException(); }).AssertFailed<InvalidOperationException>();

        var n = 0;
        Tasks.FromExecution(() => n = 1).AssertRanToCompletion();
        n.AssertEquals(1);
        Tasks.FromExecution(() => { throw new TaskCanceledException(); }).AssertCancelled();
        Tasks.FromExecution(() => { throw new InvalidOperationException(); }).AssertFailed<InvalidOperationException>();
    }
}
