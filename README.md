Twisted Oak Threading Utilities
===============================

Contains a variety of utility classes and methods:

- Factory methods for tasks (Tasks.Faulted, Tasks.RanToCompletion, Task.FromEvaluation, ...)
- A non-generic TaskCompletionSource (with SetRanToCompletion instead of SetResult)
- Extension methods to set task completion sources based on existing tasks (EventuallySetFromTask, TrySetFromCompletedTask, ...)
- Extension methods for manipulating tasks (Select, WithCanceledExceptionToCancellation, HandleCancelled, AnyTypeOfCompletion, ...)
- A super-simple lock that can be acquired only one time, and never released, to determine things like the winners of races
- A synchronization context that wraps other contexts to run methods one by one
- Interfaces for anonymous awaitable types
- Anonymous implementation classes for synchronization contexts and the custom awaitable types
- Extension methods for synchronization contexts (makes them awaitable, PostAction, PostFunc, ...)
- A throttle that runs actions one by one, discarding queued actions when fresher ones are given

A code map of the library:

![code map of library](http://i.imgur.com/fZh2cB2.png)
