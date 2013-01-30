using System;
using System.Threading.Tasks;

namespace TwistedOak.Threading {
    ///<summary>Contains extensions methods related to awaitables.</summary>
    public static class AwaitableExtensions {
        ///<summary>A task that completes with the same result as a given awaitable.</summary>
        public static async Task AsTask(this IAwaitable awaitable) {
            if (awaitable == null) throw new ArgumentException("awaitable");
            await awaitable;
        }
        
        ///<summary>A task that completes with the same result as a given awaitable.</summary>
        public static async Task<T> AsTask<T>(this IAwaitable<T> awaitable) {
            if (awaitable == null) throw new ArgumentException("awaitable");
            return await awaitable;
        }
        
        ///<summary>An awaitable that completes with the same result as a given task.</summary>
        public static IAwaitable<T> AsIAwaitable<T>(this Task<T> task) {
            if (task == null) throw new ArgumentException("task");
            return new AnonymousAwaitable<T>(() => {
                var awaiter = task.GetAwaiter();
                return new AnonymousAwaiter<T>(() => awaiter.IsCompleted, awaiter.OnCompleted, awaiter.GetResult);
            });
        }
        
        ///<summary>An awaitable that completes with the same result as a given task.</summary>
        public static IAwaitable AsIAwaitable(this Task task) {
            if (task == null) throw new ArgumentException("task");
            return new AnonymousAwaitable(() => {
                var awaiter = task.GetAwaiter();
                return new AnonymousAwaiter(() => awaiter.IsCompleted, awaiter.OnCompleted, awaiter.GetResult);
            });
        }
    }
}