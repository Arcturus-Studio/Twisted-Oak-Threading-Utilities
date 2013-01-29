namespace TwistedOak.Util.TaskEx {
    ///<summary>Can be awaited, in order to get a value.</summary>
    public interface IAwaitable<out T> {
        IAwaiter<T> GetAwaiter();
    }
    ///<summary>Can be awaited, without getting a value.</summary>
    public interface IAwaitable {
        IAwaiter GetAwaiter();
    }
}
