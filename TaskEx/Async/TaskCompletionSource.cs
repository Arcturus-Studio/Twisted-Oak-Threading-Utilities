using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TwistedOak.Util.TaskEx {
    [DebuggerDisplay("{ToString()}")]
    public class TaskCompletionSource {
        private readonly TaskCompletionSource<bool> _source = new TaskCompletionSource<bool>();
        public Task Task { get { return _source.Task; } }
        public void SetException(Exception exception) { _source.SetException(exception); }
        public void SetCanceled() { _source.SetCanceled(); }
        public void SetRanToCompletion() { _source.SetResult(true); }
        public bool TrySetException(Exception exception) { return _source.TrySetException(exception); }
        public bool TrySetCanceled() { return _source.TrySetCanceled(); }
        public bool TrySetRanToCompletion() { return _source.TrySetResult(true); }
        public override string ToString() { return _source.ToString(); }
    }
}
