using System;
using System.Diagnostics;

namespace TwistedOak.Util.TaskEx {
    /// <summary>A custom generic IAwaiter, implemented with delegates passed to the constructor.</summary>
    [DebuggerStepThrough]
    public sealed class AnonymousAwaiter<T> : IAwaiter<T> {
        private readonly Func<bool> _isCompleted;
        private readonly Action<Action> _onCompleted;
        private readonly Func<T> _getResult;
        public AnonymousAwaiter(Func<bool> isCompleted, Action<Action> onCompleted, Func<T> getResult) {
            if (isCompleted == null) throw new ArgumentNullException("isCompleted");
            if (onCompleted == null) throw new ArgumentNullException("onCompleted");
            if (getResult == null) throw new ArgumentNullException("getResult");
            this._isCompleted = isCompleted;
            this._onCompleted = onCompleted;
            this._getResult = getResult;
        }
        public bool IsCompleted { get { return _isCompleted(); } }
        public void OnCompleted(Action continuation) { _onCompleted(continuation); }
        public T GetResult() { return _getResult(); }
    }
    /// <summary>A custom void IAwaiter, implemented with delegates passed to the constructor.</summary>
    [DebuggerStepThrough]
    public sealed class AnonymousAwaiter : IAwaiter {
        private readonly Func<bool> _isCompleted;
        private readonly Action<Action> _onCompleted;
        private readonly Action _getResult;
        public AnonymousAwaiter(Func<bool> isCompleted, Action<Action> onCompleted, Action getResult) {
            if (isCompleted == null) throw new ArgumentNullException("isCompleted");
            if (onCompleted == null) throw new ArgumentNullException("onCompleted");
            if (getResult == null) throw new ArgumentNullException("getResult");
            this._isCompleted = isCompleted;
            this._onCompleted = onCompleted;
            this._getResult = getResult;
        }
        public bool IsCompleted { get { return _isCompleted(); } }
        public void OnCompleted(Action continuation) { _onCompleted(continuation); }
        public void GetResult() { _getResult(); }
    }
}
