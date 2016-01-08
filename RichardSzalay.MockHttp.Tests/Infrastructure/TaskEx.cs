using System.Threading.Tasks;

namespace RichardSzalay.MockHttp.Tests.Infrastructure
{
    class TaskEx
    {
        public static Task<T> FromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }
    }
}
