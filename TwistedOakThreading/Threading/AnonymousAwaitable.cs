﻿using System;
using System.Diagnostics;

namespace TwistedOak.Threading {
    /// <summary>A custom generic IAwaitable, implemented with delegates passed to the constructor.</summary>
    [DebuggerStepThrough]
    public sealed class AnonymousAwaitable<T> : IAwaitable<T> {
        private readonly Func<IAwaiter<T>> _getAwaiter;
        ///<summary>Creates an awaitable based on the given delegate.</summary>
        public AnonymousAwaitable(Func<IAwaiter<T>> getAwaiter) {
            if (getAwaiter == null) throw new ArgumentNullException("getAwaiter");
            this._getAwaiter = getAwaiter;
        }
        ///<summary>Gets an awaiter to handle awaiting.</summary>
        public IAwaiter<T> GetAwaiter() { return _getAwaiter(); }
    }
    /// <summary>A custom void IAwaitable, implemented with delegates passed to the constructor.</summary>
    [DebuggerStepThrough]
    public sealed class AnonymousAwaitable : IAwaitable {
        private readonly Func<IAwaiter> _getAwaiter;
        ///<summary>Creates an awaitable based on the given delegate.</summary>
        public AnonymousAwaitable(Func<IAwaiter> getAwaiter) {
            if (getAwaiter == null) throw new ArgumentNullException("getAwaiter");
            this._getAwaiter = getAwaiter;
        }
        ///<summary>Gets an awaiter to handle awaiting.</summary>
        public IAwaiter GetAwaiter() { return _getAwaiter(); }
    }
}
