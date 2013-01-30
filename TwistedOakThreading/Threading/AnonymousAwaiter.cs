using System;
using System.Diagnostics;

namespace TwistedOak.Threading {
    /// <summary>A custom generic IAwaiter, implemented with delegates passed to the constructor.</summary>
    [DebuggerStepThrough]
    public sealed class AnonymousAwaiter<T> : IAwaiter<T> {
        private readonly Func<bool> _isCompleted;
        private readonly Action<Action> _onCompleted;
        private readonly Func<T> _getResult;
        ///<summary>Creates an awaiter based on the given delegates.</summary>
        public AnonymousAwaiter(Func<bool> isCompleted, Action<Action> onCompleted, Func<T> getResult) {
            if (isCompleted == null) throw new ArgumentNullException("isCompleted");
            if (onCompleted == null) throw new ArgumentNullException("onCompleted");
            if (getResult == null) throw new ArgumentNullException("getResult");
            this._isCompleted = isCompleted;
            this._onCompleted = onCompleted;
            this._getResult = getResult;
        }
        ///<summary>Determines if OnCompleted needs to be called or not, in order to access the result.</summary>
        public bool IsCompleted { get { return _isCompleted(); } }
        ///<summary>Registers a callback to run when the awaited thing has completed, or to run immediately if it has already completed.</summary>
        public void OnCompleted(Action continuation) { _onCompleted(continuation); }
        ///<summary>Gets the awaited result, rethrowing any exceptions.</summary>
        public T GetResult() { return _getResult(); }
    }
    /// <summary>A custom void IAwaiter, implemented with delegates passed to the constructor.</summary>
    [DebuggerStepThrough]
    public sealed class AnonymousAwaiter : IAwaiter {
        private readonly Func<bool> _isCompleted;
        private readonly Action<Action> _onCompleted;
        private readonly Action _getResult;
        ///<summary>Creates an awaiter based on the given delegates.</summary>
        public AnonymousAwaiter(Func<bool> isCompleted, Action<Action> onCompleted, Action getResult) {
            if (isCompleted == null) throw new ArgumentNullException("isCompleted");
            if (onCompleted == null) throw new ArgumentNullException("onCompleted");
            if (getResult == null) throw new ArgumentNullException("getResult");
            this._isCompleted = isCompleted;
            this._onCompleted = onCompleted;
            this._getResult = getResult;
        }
        ///<summary>Determines if OnCompleted needs to be called or not, in order to access the result.</summary>
        public bool IsCompleted { get { return _isCompleted(); } }
        ///<summary>Registers a callback to run when the awaited thing has completed, or to run immediately if it has already completed.</summary>
        public void OnCompleted(Action continuation) { _onCompleted(continuation); }
        ///<summary>Rethrows any exception from the awaited operation.</summary>
        public void GetResult() { _getResult(); }
    }
}
