using System;
using System.Threading;

namespace TwistedOak.Util.TaskEx {
    /// <summary>
    /// A custom synchronization context, implemented with a post delegate passed to the constructor.
    /// Automatically exposes itself as the current synchronization context, when running posted methods.
    /// </summary>
    public sealed class AnonymousSynchronizationContext : SynchronizationContext {
        private readonly Action<Action> _post;
        ///<summary>Creates a custom synchronization context based on the given posting action.</summary>
        /// <param name="post">
        /// Used to run actions posted to the synchronization context.
        /// Actions passed to this method wrap callbacks posted to the created context, and take care of exposing the correct synchronization context.
        /// </param>
        public AnonymousSynchronizationContext(Action<Action> post) {
            if (post == null) throw new ArgumentNullException("post");
            this._post = post;
        }
        public override void Post(SendOrPostCallback d, object state) {
            if (d == null) throw new ArgumentNullException("d");
            _post(() => {
                var c = Current;
                try {
                    SetSynchronizationContext(this);
                    d(state);
                } finally {
                    SetSynchronizationContext(c); // restore surrounding sync context
                }
            });
        }
        public override void Send(SendOrPostCallback d, object state) {
            throw new NotSupportedException();
        }
        public override SynchronizationContext CreateCopy() {
            return this;
        }
    }
}
