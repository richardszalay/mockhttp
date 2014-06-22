using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichardSzalay.MockHttp
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
