using System.Runtime.CompilerServices;

namespace TwistedOak.Threading {
    ///<summary>Handles awaiting, in order to get a value.</summary>
    public interface IAwaiter<out T> : INotifyCompletion {
        ///<summary>Determines if OnCompleted needs to be called or not, in order to access the result.</summary>
        bool IsCompleted { get; }
        ///<summary>Gets the awaited result, rethrowing any exceptions.</summary>
        T GetResult();
    }
    ///<summary>Handles awaiting, without getting a value.</summary>
    public interface IAwaiter : INotifyCompletion {
        ///<summary>Determines if OnCompleted needs to be called or not, in order to access the result.</summary>
        bool IsCompleted { get; }
        ///<summary>Rethrows any exception from the awaited operation.</summary>
        void GetResult();
    }
}
