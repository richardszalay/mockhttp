namespace RichardSzalay.MockHttp.Extensions;

public static class TaskExtensions
{
    public static Task<T> FromResult<T>(T result)
    {
        TaskCompletionSource<T> tcs = new();

        tcs.SetResult(result);

        return tcs.Task;
    }
}