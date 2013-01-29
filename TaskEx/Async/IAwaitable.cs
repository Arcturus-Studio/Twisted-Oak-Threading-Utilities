namespace TwistedOak.Util.TaskEx {
    public interface IAwaitable<out T> {
        IAwaiter<T> GetAwaiter();
    }
    public interface IAwaitable {
        IAwaiter GetAwaiter();
    }
}
