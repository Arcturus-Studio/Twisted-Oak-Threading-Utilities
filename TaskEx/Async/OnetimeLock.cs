using System.Threading;

namespace TwistedOak.Util.TaskEx {
    ///<summary>A thread-safe lock that can only be acquired once and never released.</summary>
    public sealed class OnetimeLock {
        private int _state;
        ///<summary>Returns true exactly once, for the first caller. Thread safe.</summary>
        public bool TryAcquire() {
            return Interlocked.Exchange(ref _state, 1) == 0;
        }
        ///<summary>
        ///Determines if the lock has been acquired.
        ///Volatile result if the lock has not been acquired.
        ///</summary>
        public bool IsAcquired() {
            return _state != 0;
        }
    }
}
