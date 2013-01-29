using System.Runtime.CompilerServices;

namespace TwistedOak.Util.TaskEx {
    ///<summary>Handles awaiting, in order to get a value.</summary>
    public interface IAwaiter<out T> : INotifyCompletion {
        bool IsCompleted { get; }
        T GetResult();
    }
    ///<summary>Handles awaiting, without getting a value.</summary>
    public interface IAwaiter : INotifyCompletion {
        bool IsCompleted { get; }
        void GetResult();
    }
}
