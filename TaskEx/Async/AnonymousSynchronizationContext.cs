using System;
using System.Threading;

namespace TwistedOak.Util.TaskEx {
    public sealed class AnonymousSynchronizationContext : SynchronizationContext {
        private readonly Action<Action> _post;
        public AnonymousSynchronizationContext(Action<Action> post) {
            if (post == null) throw new ArgumentNullException("post");
            this._post = post;
        }
        public override void Post(SendOrPostCallback d, object state) {
            _post(() => {
                var c = Current;
                try {
                    SetSynchronizationContext(this);
                    d(state);
                } finally {
                    SetSynchronizationContext(c);
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
