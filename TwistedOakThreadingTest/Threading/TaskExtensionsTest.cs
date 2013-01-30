using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwistedOak.Threading;

[TestClass]
public class TaskExtensionsTest {
    [TestMethod]
    public void WithCancellationToTaskCanceledException() {
        Tasks.RanToCompletion().WithCancellationToTaskCanceledException().AssertRanToCompletion();
        Tasks.Cancelled().WithCancellationToTaskCanceledException().AssertFailed<TaskCanceledException>();
        Tasks.Faulted(new ArgumentOutOfRangeException()).WithCancellationToTaskCanceledException().AssertFailed<ArgumentOutOfRangeException>();
        Tasks.Faulted(new OperationCanceledException()).WithCancellationToTaskCanceledException().AssertFailed<OperationCanceledException>();

        Task.FromResult(1).WithCancellationToTaskCanceledException().AssertRanToCompletion();
        Tasks.Cancelled<int>().WithCancellationToTaskCanceledException().AssertFailed<TaskCanceledException>();
        Tasks.Faulted<int>(new ArgumentOutOfRangeException()).WithCancellationToTaskCanceledException().AssertFailed<ArgumentOutOfRangeException>();
        Tasks.Faulted<int>(new OperationCanceledException()).WithCancellationToTaskCanceledException().AssertFailed<OperationCanceledException>();
    }
    [TestMethod]
    public void WithCanceledExceptionToCancellation() {
        Tasks.RanToCompletion().WithCanceledExceptionToCancellation().AssertRanToCompletion();
        Tasks.Cancelled().WithCanceledExceptionToCancellation().AssertCancelled();
        Tasks.Faulted(new ArgumentOutOfRangeException()).WithCanceledExceptionToCancellation().AssertFailed<ArgumentOutOfRangeException>();
        Tasks.Faulted(new OperationCanceledException()).WithCanceledExceptionToCancellation().AssertCancelled();

        Task.FromResult(1).WithCanceledExceptionToCancellation().AssertRanToCompletion();
        Tasks.Cancelled<int>().WithCanceledExceptionToCancellation().AssertCancelled();
        Tasks.Faulted<int>(new ArgumentOutOfRangeException()).WithCanceledExceptionToCancellation().AssertFailed<ArgumentOutOfRangeException>();
        Tasks.Faulted<int>(new OperationCanceledException()).WithCanceledExceptionToCancellation().AssertCancelled();
    }

    [TestMethod]
    public void Select() {
        // void
        Tasks.Cancelled().Select(() => 1).AssertCancelled();
        Tasks.Faulted(new ArgumentException()).Select(() => 1).AssertFailed<ArgumentException>();
        Tasks.RanToCompletion().Select(() => 1).AssertRanToCompletion().AssertEquals(1);
        // wrap inline failure
        Tasks.RanToCompletion().Select<int>(() => { throw new TaskCanceledException(); }).AssertCancelled();
        Tasks.RanToCompletion().Select<int>(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();

        // generic
        Tasks.Cancelled<int>().Select(e => e + 1).AssertCancelled();
        Tasks.Faulted<int>(new ArgumentException()).Select(e => e + 1).AssertFailed<ArgumentException>();
        Task.FromResult(3).Select(e => e + 1).AssertRanToCompletion().AssertEquals(4);
        (from r in Tasks.RanToCompletion(1) select r + 1).AssertRanToCompletion().AssertEquals(2);
        // wrap inline failure
        Tasks.RanToCompletion(1).Select<int, int>(e => { throw new TaskCanceledException(); }).AssertCancelled();
        Tasks.RanToCompletion(1).Select<int, int>(e => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
    }
    [TestMethod]
    public void Where() {
        // void
        Tasks.Cancelled().Select(() => 1).AssertCancelled();
        Tasks.Faulted(new ArgumentException()).Select(() => 1).AssertFailed<ArgumentException>();
        Tasks.RanToCompletion().Select(() => 1).AssertRanToCompletion().AssertEquals(1);
        // wrap inline failure
        Tasks.RanToCompletion().Where(() => { throw new TaskCanceledException(); }).AssertCancelled();
        Tasks.RanToCompletion().Where(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();

        // generic
        Tasks.Cancelled<int>().Where(e => e == 3).AssertCancelled();
        Tasks.Faulted<int>(new ArgumentException()).Where(e => e == 3).AssertFailed<ArgumentException>();
        Task.FromResult(3).Where(e => e == 3).AssertRanToCompletion().AssertEquals(3);
        Task.FromResult(3).Where(e => e == 4).AssertCancelled();
        (from r in Tasks.RanToCompletion(1) where r == 1 select r + 1).AssertRanToCompletion().AssertEquals(2);
        // wrap inline failure
        Tasks.RanToCompletion(1).Where(e => { throw new TaskCanceledException(); }).AssertCancelled();
        Tasks.RanToCompletion(1).Where(e => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
    }

    [TestMethod]
    public void SelectMany() {
        (from e in Tasks.RanToCompletion(1) 
         from e2 in Tasks.RanToCompletion(2) 
         select e2 + e).AssertRanToCompletion()
                       .AssertEquals(3);
    }

    [TestMethod]
    public void SelectCancelled() {
        Task.FromResult(3).SelectCancelled(() => 4).AssertRanToCompletion().AssertEquals(3);
        Tasks.Faulted<int>(new ArgumentException()).SelectCancelled(() => 1).AssertFailed<ArgumentException>();
        Tasks.Cancelled<int>().SelectCancelled(() => 4).AssertRanToCompletion().AssertEquals(4);

        // wrap inline failure
        Task.FromResult(3).SelectCancelled(() => { throw new ArgumentException(); }).AssertRanToCompletion().AssertEquals(3);
        Tasks.Cancelled<int>().SelectCancelled<int>(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
        Tasks.Cancelled<int>().SelectCancelled<int>(() => { throw new TaskCanceledException(); }).AssertCancelled();
    }
    [TestMethod]
    public void HandleCancelled() {
        var n = 0;
        Task.FromResult(3).HandleCancelled(() => n++).AssertRanToCompletion();
        n.AssertEquals(0);
        Tasks.Faulted<int>(new ArgumentException()).HandleCancelled(() => n++).AssertFailed<ArgumentException>();
        n.AssertEquals(0);
        Tasks.Cancelled<int>().HandleCancelled(() => n++).AssertRanToCompletion();
        n.AssertEquals(1);

        // wrap inline failure
        Task.FromResult(3).HandleCancelled(() => { throw new ArgumentException(); }).AssertRanToCompletion();
        Tasks.Cancelled<int>().HandleCancelled(() => { throw new ArgumentException(); }).AssertFailed<ArgumentException>();
        Tasks.Cancelled<int>().HandleCancelled(() => { throw new TaskCanceledException(); }).AssertCancelled();
    }

    [TestMethod]
    public void SelectFaulted() {
        Task.FromResult(3).SelectFaulted<int, ArgumentException>(e => 4).AssertRanToCompletion().AssertEquals(3);
        Tasks.Cancelled<int>().SelectFaulted<int, ArgumentException>(ex => 4).AssertCancelled();
        Tasks.Faulted<int>(new ArgumentException()).SelectFaulted<int, ArgumentException>(ex => 1).AssertRanToCompletion().AssertEquals(1);

        // wrap inline failure
        Task.FromResult(3).SelectFaulted<int, ArgumentException>(ex => { throw new ArgumentException(); }).AssertRanToCompletion().AssertEquals(3);
        Tasks.Cancelled<int>().SelectFaulted<int, ArgumentException>(ex => { throw new ArithmeticException(); }).AssertCancelled();
        Tasks.Faulted<int>(new InvalidOperationException()).SelectFaulted<int, ArgumentException>(ex => { throw new ArithmeticException(); }).AssertFailed<InvalidOperationException>();
        Tasks.Faulted<int>(new ArgumentException()).SelectFaulted<int, ArgumentException>(ex => { throw new ArithmeticException(); }).AssertFailed<ArithmeticException>();
    }
    [TestMethod]
    public void HandleFaulted() {
        var n = 0;
        Tasks.RanToCompletion().HandleFaulted<ArgumentException>(ex => n++).AssertRanToCompletion();
        n.AssertEquals(0);
        Tasks.Cancelled().HandleFaulted<ArgumentException>(ex => n++).AssertCancelled();
        n.AssertEquals(0);
        Tasks.Faulted(new ArgumentException()).HandleFaulted<ArgumentException>(ex => n++).AssertRanToCompletion();
        n.AssertEquals(1);

        // wrap inline failure
        Task.FromResult(3).HandleFaulted<ArgumentException>(ex => { throw new ArgumentException(); }).AssertRanToCompletion();
        Tasks.Cancelled<int>().HandleFaulted<ArgumentException>(ex => { throw new ArithmeticException(); }).AssertCancelled();
        Tasks.Faulted<int>(new InvalidOperationException()).HandleFaulted<ArgumentException>(ex => { throw new ArithmeticException(); }).AssertFailed<InvalidOperationException>();
        Tasks.Faulted<int>(new ArgumentException()).HandleFaulted<ArgumentException>(ex => { throw new ArithmeticException(); }).AssertFailed<ArithmeticException>();
    }

    [TestMethod]
    public void AnyTypeOfCompletion() {
        // void
        new TaskCompletionSource().Task.AnyTypeOfCompletion().AssertNotCompleted();
        Tasks.RanToCompletion().AnyTypeOfCompletion().AssertRanToCompletion().AssertRanToCompletion();
        Tasks.Cancelled().AnyTypeOfCompletion().AssertRanToCompletion().AssertCancelled();
        Tasks.Faulted(new InvalidOperationException()).AnyTypeOfCompletion().AssertRanToCompletion().AssertFailed<InvalidOperationException>();

        // generic
        new TaskCompletionSource<int>().Task.AnyTypeOfCompletion().AssertNotCompleted();
        Tasks.RanToCompletion(1).AnyTypeOfCompletion().AssertRanToCompletion().AssertRanToCompletion().AssertEquals(1);
        Tasks.Cancelled<int>().AnyTypeOfCompletion().AssertRanToCompletion().AssertCancelled();
        Tasks.Faulted<int>(new InvalidOperationException()).AnyTypeOfCompletion().AssertRanToCompletion().AssertFailed<InvalidOperationException>();
    }

    [TestMethod]
    public void IsRanToCompletion() {
        new TaskCompletionSource().Task.IsRanToCompletion().AssertIsFalse();
        Tasks.RanToCompletion().IsRanToCompletion().AssertIsTrue();
        Tasks.Cancelled().IsRanToCompletion().AssertIsFalse();
        Tasks.Faulted(new Exception()).IsRanToCompletion().AssertIsFalse();
    }

    [TestMethod]
    public void ObserveException() {
        Tasks.Faulted(new Exception()).ObserveAnyException();
        GC.Collect();
    }

    [TestMethod]
    public void When() {
        new[] { Task.FromResult(1), Task.FromResult(2) }.WhenAll().AssertRanToCompletion().AssertSequenceEquals(new[] { 1, 2 });
        new[] { Tasks.RanToCompletion(), Tasks.RanToCompletion() }.WhenAll().AssertRanToCompletion();
        new[] { Task.FromResult(1), new TaskCompletionSource<int>().Task }.WhenAny().AssertRanToCompletion().AssertRanToCompletion().AssertEquals(1);
        new[] { Tasks.RanToCompletion(), new TaskCompletionSource().Task }.WhenAny().AssertRanToCompletion();
    }
}
