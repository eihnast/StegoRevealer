using System.Collections.Concurrent;
using System.Diagnostics;

namespace StegoRevealer.Utils.DataPreparer.Lib.TaskPool;

public class TaskPool
{
    public int TasksLimit { get; init; }
    public bool ConsiderRealRunningTasksOnly { get; init; }

    private const long MemoryLimit = 8L * 1024 * 1024 * 1024;

    private object _tasksLock = new object();
    private object _tasksCheckLock = new object();

    private HashSet<IPooledTask> _currentTasks = new();
    private ConcurrentQueue<IPooledTask> _tasksQueue = new();

    private int _realRunnedTasksCount => _currentTasks.Where(t => t.GetStatus() == TaskStatus.Running).Count();

    private bool _checkMemoryLimit = false;
    public bool IsMemoryLimitReached => _checkMemoryLimit ? Process.GetCurrentProcess().WorkingSet64 >= MemoryLimit : false;
    public bool IsCompleted => _tasksQueue.IsEmpty && _currentTasks.Count == 0;


    public TaskPool() : this(Environment.ProcessorCount, false) { }
    public TaskPool(int tasksLimit) : this(tasksLimit, false) { }
    public TaskPool(bool considerOnlyRunningTasks) : this(Environment.ProcessorCount, considerOnlyRunningTasks) { }
    public TaskPool(int tasksLimit, bool considerOnlyRunningTasks)
    {
        TasksLimit = tasksLimit;
        ConsiderRealRunningTasksOnly = considerOnlyRunningTasks;
    }


    public Task AddAsync(Func<Task> task)
    {
        lock (_tasksLock)
        {
            var pooledTask = new PooledTask
            {
                TaskCreator = task,
                Awaiter = new TaskCompletionSource<object?>()
            };
            _tasksQueue.Enqueue(pooledTask);
            CheckQueue();

            return pooledTask.Awaiter.Task;
        }
    }

    public Task<T> AddAsync<T>(Func<Task<T>> task)
    {
        lock (_tasksLock)
        {
            var pooledTask = new PooledTask<T>
            {
                TaskCreator = task,
                Awaiter = new TaskCompletionSource<T>()
            };
            _tasksQueue.Enqueue(pooledTask);
            CheckQueue();

            return pooledTask.Awaiter.Task;
        }
    }

    private async Task Execute(IPooledTask task)
    {
        await task.ExecuteAsync();

        // Завершение
        lock (_tasksLock)
        {
            _currentTasks.Remove(task);
            CheckQueue();
        }
    }

    private void CheckQueue()
    {
        lock (_tasksCheckLock)
        {
            while (!_tasksQueue.IsEmpty && (ConsiderRealRunningTasksOnly ? _realRunnedTasksCount : _currentTasks.Count) < TasksLimit && !IsMemoryLimitReached)
            {
                if (!_tasksQueue.TryDequeue(out var task))
                    continue;

                _currentTasks.Add(task);
                _ = Execute(task);  // Запускаем без ожидания
            }
        }
    }
}
