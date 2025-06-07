using System.Collections.Concurrent;

namespace MVCDemo.Util.Limit
{
    /// <summary>
    /// 滑动窗口限流：允许在指定时间窗口内的请求数量，适用于高并发场景。
    /// </summary>
    public class SlidingWindowRateLimit
    {
        private static readonly ConcurrentDictionary<string, SlidingWindow> _windows =
            new ConcurrentDictionary<string, SlidingWindow>();

        public static bool IsAllowed(string key, int maxRequests, TimeSpan window)
        {
            var slidingWindow = _windows.GetOrAdd(key, k => new SlidingWindow());
            return slidingWindow.IsAllowed(maxRequests, window);
        }

        // 清理过期的窗口（可选，用于内存管理）
        public static void CleanupExpiredWindows()
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in _windows)
            {
                if (kvp.Value.IsExpired(TimeSpan.FromMinutes(10))) // 10分钟无活动就清理
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _windows.TryRemove(key, out _);
            }
        }

        private class SlidingWindow
        {
            private readonly Queue<DateTime> _timestamps = new Queue<DateTime>();
            private readonly object _lock = new object();
            private DateTime _lastActivity = DateTime.UtcNow;

            public bool IsAllowed(int maxRequests, TimeSpan window)
            {
                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    _lastActivity = now;
                    var cutoff = now - window;

                    // 移除窗口外的旧请求
                    while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
                        _timestamps.Dequeue();

                    // 检查是否超过限制
                    if (_timestamps.Count >= maxRequests)
                        return false;

                    // 记录新请求
                    _timestamps.Enqueue(now);
                    return true;
                }
            }

            public bool IsExpired(TimeSpan timeout)
            {
                return DateTime.UtcNow - _lastActivity > timeout;
            }
        }
    }
}
