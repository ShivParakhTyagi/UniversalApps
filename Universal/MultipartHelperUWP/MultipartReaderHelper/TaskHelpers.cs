using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MultipartHelperUWP.MultipartReaderHelper
{
    public static class TaskHelpers
    {
        private static readonly Task _defaultCompleted = (Task)Task.FromResult<TaskHelpers.AsyncVoid>(new TaskHelpers.AsyncVoid());
        private static readonly Task<object> _completedTaskReturningNull = Task.FromResult<object>((object)null);

        internal static Task Canceled()
        {
            return (Task)TaskHelpers.CancelCache<TaskHelpers.AsyncVoid>.Canceled;
        }

        internal static Task<TResult> Canceled<TResult>()
        {
            return TaskHelpers.CancelCache<TResult>.Canceled;
        }

        internal static Task Completed()
        {
            return TaskHelpers._defaultCompleted;
        }

        internal static Task FromError(Exception exception)
        {
            return (Task)TaskHelpers.FromError<TaskHelpers.AsyncVoid>(exception);
        }

        internal static Task<TResult> FromError<TResult>(Exception exception)
        {
            TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetException(exception);
            return completionSource.Task;
        }

        internal static Task<object> NullResult()
        {
            return TaskHelpers._completedTaskReturningNull;
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct AsyncVoid
        {
        }

        private static class CancelCache<TResult>
        {
            public static readonly Task<TResult> Canceled = TaskHelpers.CancelCache<TResult>.GetCancelledTask();

            private static Task<TResult> GetCancelledTask()
            {
                TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();
                completionSource.SetCanceled();
                return completionSource.Task;
            }
        }
    }
}