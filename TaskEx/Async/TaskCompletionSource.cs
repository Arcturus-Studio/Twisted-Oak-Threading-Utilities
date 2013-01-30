using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TwistedOak.Util.TaskEx {
    ///<summary>The producer side of a System.Threading.Tasks.Task unbound to a delegate, providing access to the consumer side through a Task property.</summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class TaskCompletionSource {
        private readonly TaskCompletionSource<bool> _source = new TaskCompletionSource<bool>();
        ///<summary>Gets the task created and controlled by this task completion source.</summary>
        public Task Task { get { return _source.Task; } }
        
        ///<summary>Transitions the underlying task into the faulted state.</summary>
        public void SetException(Exception exception) { _source.SetException(exception); }
        ///<summary>Transitions the underlying task into the faulted state.</summary>
        public void SetException(IEnumerable<Exception> exceptions) { _source.SetException(exceptions); }
        ///<summary>Transitions the underlying task into the canceled state.</summary>
        public void SetCanceled() { _source.SetCanceled(); }
        ///<summary>Transitions the underlying task into the ran-to-completion state.</summary>
        public void SetRanToCompletion() { _source.SetResult(true); }

        ///<summary>Attempts to transtion the underlying task into the faulted state.</summary>
        public bool TrySetException(Exception exception) { return _source.TrySetException(exception); }
        ///<summary>Attempts to transtion the underlying task into the faulted state.</summary>
        public bool TrySetException(IEnumerable<Exception> exceptions) { return _source.TrySetException(exceptions); }
        ///<summary>Attempts to transtion the underlying task into the canceled state.</summary>
        public bool TrySetCanceled() { return _source.TrySetCanceled(); }
        ///<summary>Attempts to transtion the underlying task into the ran-to-completion state.</summary>
        public bool TrySetRanToCompletion() { return _source.TrySetResult(true); }
        
        ///<summary>Returns a string that represents the task completion source.</summary>
        public override string ToString() { return _source.ToString(); }
    }
}
