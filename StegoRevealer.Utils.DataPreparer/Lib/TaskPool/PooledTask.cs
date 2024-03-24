using System.Runtime.CompilerServices;

namespace StegoRevealer.Utils.DataPreparer.Lib.TaskPool;

public interface IPooledTask
{
    public Task ExecuteAsync();

    public TaskStatus? GetStatus();
}

public class PooledTask : IPooledTask
{
    public required Func<Task> TaskCreator { get; init; }

    public required TaskCompletionSource<object?> Awaiter { get; init; }

    private Task? _innerTask = null;
    public TaskStatus? GetStatus() => _innerTask?.Status;

    public async Task ExecuteAsync()
    {
        _innerTask = TaskCreator();
        await _innerTask;
        Awaiter.SetResult(null);
    }
}

public class PooledTask<T> : IPooledTask
{
    public required Func<Task<T>> TaskCreator { get; init; }

    public required TaskCompletionSource<T> Awaiter { get; init; }

    private Task<T>? _innerTask = null;
    public TaskStatus? GetStatus() => _innerTask?.Status;

    public async Task ExecuteAsync()
    {
        _innerTask = TaskCreator();
        var result = await _innerTask;
        Awaiter.SetResult(result);
    }
}
