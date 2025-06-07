using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
/// <summary>
/// 漏桶算法实现
/// 特点：以固定速率处理请求，超出容量的请求被丢弃
/// </summary>
public class LeakyBucket
{
    private readonly int _capacity;              // 桶容量
    private readonly double _leakRate;           // 漏出速率（每秒）
    private readonly Queue<DateTime> _requests;  // 请求队列
    private DateTime _lastLeak;                  // 上次漏出时间
    private readonly object _lock = new object();

    /// <summary>
    /// 创建漏桶
    /// </summary>
    /// <param name="capacity">桶容量（最多排队多少个请求）</param>
    /// <param name="leakRate">漏出速率（每秒处理多少个请求）</param>
    public LeakyBucket(int capacity, double leakRate)
    {
        _capacity = capacity;
        _leakRate = leakRate;
        _requests = new Queue<DateTime>();
        _lastLeak = DateTime.UtcNow;
    }

    /// <summary>
    /// 尝试添加请求到桶中
    /// </summary>
    /// <returns>true表示请求被接受，false表示桶满被拒绝</returns>
    public bool TryAddRequest()
    {
        lock (_lock)
        {
            // 先漏出已处理的请求
            LeakRequests();

            // 检查桶是否已满
            if (_requests.Count >= _capacity)
            {
                return false; // 桶满，拒绝请求
            }

            // 添加新请求
            _requests.Enqueue(DateTime.UtcNow);
            return true; // 请求被接受
        }
    }

    /// <summary>
    /// 漏出请求（模拟以固定速率处理）
    /// </summary>
    private void LeakRequests()
    {
        var now = DateTime.UtcNow;
        var timePassed = (now - _lastLeak).TotalSeconds;

        if (timePassed <= 0) return;

        // 计算在这段时间内应该处理多少个请求
        var requestsToLeak = (int)(timePassed * _leakRate);

        // 漏出请求
        for (int i = 0; i < requestsToLeak && _requests.Count > 0; i++)
        {
            _requests.Dequeue();
        }

        _lastLeak = now;
    }

    /// <summary>
    /// 获取当前桶中的请求数量
    /// </summary>
    public int CurrentRequests
    {
        get
        {
            lock (_lock)
            {
                LeakRequests();
                return _requests.Count;
            }
        }
    }

    /// <summary>
    /// 获取桶的使用率（0-1之间）
    /// </summary>
    public double Usage
    {
        get { return (double)CurrentRequests / _capacity; }
    }

    /// <summary>
    /// 估算请求需要等待的时间
    /// </summary>
    public TimeSpan EstimateWaitTime()
    {
        lock (_lock)
        {
            LeakRequests();
            if (_requests.Count == 0) return TimeSpan.Zero;

            // 按当前漏出速率计算等待时间
            var waitSeconds = _requests.Count / _leakRate;
            return TimeSpan.FromSeconds(waitSeconds);
        }
    }
}

/// <summary>
/// 异步漏桶实现（支持等待处理）
/// </summary>
public class AsyncLeakyBucket
{
    private readonly int _capacity;
    private readonly double _leakRate;
    private readonly Queue<TaskCompletionSource<bool>> _waitingRequests;
    private readonly Timer _leakTimer;
    private readonly object _lock = new object();

    public AsyncLeakyBucket(int capacity, double leakRate)
    {
        _capacity = capacity;
        _leakRate = leakRate;
        _waitingRequests = new Queue<TaskCompletionSource<bool>>();

        // 定时器以固定间隔漏出请求
        var interval = TimeSpan.FromMilliseconds(1000.0 / leakRate);
        _leakTimer = new Timer(ProcessRequest, null, interval, interval);
    }

    /// <summary>
    /// 异步请求处理（会等待直到被处理）
    /// </summary>
    public Task<bool> RequestAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_waitingRequests.Count >= _capacity)
            {
                return Task.FromResult(false); // 桶满，拒绝请求
            }

            var tcs = new TaskCompletionSource<bool>();
            _waitingRequests.Enqueue(tcs);

            // 支持取消
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => tcs.TrySetCanceled());
            }

            return tcs.Task;
        }
    }

    /// <summary>
    /// 定时处理请求
    /// </summary>
    private void ProcessRequest(object state)
    {
        TaskCompletionSource<bool> request = null;

        lock (_lock)
        {
            if (_waitingRequests.Count > 0)
            {
                request = _waitingRequests.Dequeue();
            }
        }

        // 在锁外完成任务，避免死锁
        request?.TrySetResult(true);
    }

    public int CurrentRequests
    {
        get
        {
            lock (_lock)
            {
                return _waitingRequests.Count;
            }
        }
    }

    public void Dispose()
    {
        _leakTimer?.Dispose();

        lock (_lock)
        {
            // 取消所有等待的请求
            while (_waitingRequests.Count > 0)
            {
                var request = _waitingRequests.Dequeue();
                request.TrySetCanceled();
            }
        }
    }
}

/// <summary>
/// 漏桶限流特性
/// </summary>
public class LeakyBucketRateLimitAttribute : ActionFilterAttribute
{
    private static readonly ConcurrentDictionary<string, LeakyBucket> _buckets =
        new ConcurrentDictionary<string, LeakyBucket>();

    public int Capacity { get; set; } = 10;        // 桶容量
    public double LeakRate { get; set; } = 2.0;    // 漏出速率（每秒）
    public string BucketKey { get; set; } = "default";

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var key = GetBucketKey(filterContext);
        var bucket = _buckets.GetOrAdd(key, k => new LeakyBucket(Capacity, LeakRate));

        if (!bucket.TryAddRequest())
        {
            // 桶满，拒绝请求
            filterContext.Result = new JsonResult(new
            {
                success = false,
                message = "系统繁忙，请稍后再试",
                currentRequests = bucket.CurrentRequests,
                capacity = Capacity,
                estimatedWaitTime = bucket.EstimateWaitTime().TotalSeconds
            });
            filterContext.HttpContext.Response.StatusCode = 429;
            return;
        }

        base.OnActionExecuting(filterContext);
    }

    private string GetBucketKey(ActionExecutingContext context)
    {
        if (BucketKey != "default")
            return BucketKey;

        var ip = context.HttpContext.Connection.RemoteIpAddress;
        var action = context.ActionDescriptor.DisplayName;
        return $"{ip}_{action}";
    }
}

/// <summary>
/// 全局漏桶管理器
/// </summary>
public static class LeakyBucketManager
{
    private static readonly ConcurrentDictionary<string, LeakyBucket> _buckets =
        new ConcurrentDictionary<string, LeakyBucket>();

    /// <summary>
    /// 获取或创建漏桶
    /// </summary>
    public static LeakyBucket GetOrCreateBucket(string key, int capacity = 10, double leakRate = 2.0)
    {
        return _buckets.GetOrAdd(key, k => new LeakyBucket(capacity, leakRate));
    }

    /// <summary>
    /// 尝试处理请求
    /// </summary>
    public static bool TryProcessRequest(string key, int capacity = 10, double leakRate = 2.0)
    {
        var bucket = GetOrCreateBucket(key, capacity, leakRate);
        return bucket.TryAddRequest();
    }

    /// <summary>
    /// 获取所有桶的状态
    /// </summary>
    public static Dictionary<string, object> GetAllBucketsStatus()
    {
        var status = new Dictionary<string, object>();
        foreach (var kvp in _buckets)
        {
            var bucket = kvp.Value;
            status[kvp.Key] = new
            {
                currentRequests = bucket.CurrentRequests,
                usage = bucket.Usage,
                estimatedWaitTime = bucket.EstimateWaitTime().TotalSeconds
            };
        }
        return status;
    }

    /// <summary>
    /// 清理空闲的桶
    /// </summary>
    public static void CleanupIdleBuckets()
    {
        var keysToRemove = new List<string>();
        foreach (var kvp in _buckets)
        {
            if (kvp.Value.CurrentRequests == 0)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _buckets.TryRemove(key, out _);
        }
    }
}