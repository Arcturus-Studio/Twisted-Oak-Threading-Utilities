namespace TwistedOak.Threading {
    ///<summary>Can be awaited, in order to get a value.</summary>
    public interface IAwaitable<out T> {
        ///<summary>Gets an awaiter to handle awaiting.</summary>
        IAwaiter<T> GetAwaiter();
    }
    ///<summary>Can be awaited, without getting a value.</summary>
    public interface IAwaitable {
        ///<summary>Gets an awaiter to handle awaiting.</summary>
        IAwaiter GetAwaiter();
    }
}
