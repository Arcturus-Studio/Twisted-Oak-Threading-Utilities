using System;
using System.Threading;
using System.Threading.Tasks;

namespace TwistedOak.Util.TaskEx {
    ///<summary>Contains extension methods for working with synchronization contexts.</summary>
    public static class SynchronizationContextExtensions {
        ///<summary>Posts an action to the synchronization context, exposing its eventual completion as a task.</summary>
        public static Task PostAction(this SynchronizationContext context, Action action) {
            if (context == null) throw new ArgumentNullException("context");
            if (action == null) throw new ArgumentNullException("action");
            var t = new TaskCompletionSource();
            context.Post(x => t.SetFromCompletedTask(action.ExecuteIntoTask()), null);
            return t.Task;
        }

        ///<summary>Posts a function to the synchronization context, exposing its eventual result as a task.</summary>
        public static Task<T> PostFunc<T>(this SynchronizationContext context, Func<T> func) {
            if (context == null) throw new ArgumentNullException("context");
            if (func == null) throw new ArgumentNullException("func");
            var t = new TaskCompletionSource<T>();
            context.Post(x => t.SetFromCompletedTask(func.EvalIntoTask()), null);
            return t.Task;
        }
        
        ///<summary>An awaitable object that, when awaited, resumes execution within the given synchronization context.</summary>        
        ///<param name="context">The synchronization context to enter.</param>
        ///<param name="forceReentry">Determines if awaiting the current synchronization context results in re-posting to the context or continuing synchronously.</param>
        public static IAwaitable AwaitableEntrance(this SynchronizationContext context, bool forceReentry = true) {
            if (context == null) throw new ArgumentNullException("context");
            return new AnonymousAwaitable(() => new AnonymousAwaiter(
                () => !forceReentry && SynchronizationContext.Current == context,
                continuation => context.Post(x => continuation(), null),
                () => { }));
        }

        ///<summary>
        ///An awaiter that resumes execution by posting to the given synchronization context.
        ///If execution is already in the given context, it will re-enter it anyways.
        ///</summary>
        ///<remarks>Makes synchronization contexts awaitable.</remarks>
        public static IAwaiter GetAwaiter(this SynchronizationContext context) {
            if (context == null) throw new ArgumentNullException("context");
            return new AnonymousAwaiter(
                () => false, // always re-enter
                continuation => context.Post(x => continuation(), null), // resume in context
                () => { }); // no way to fail
        }
    }
}