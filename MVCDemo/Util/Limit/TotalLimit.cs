namespace MVCDemo.Util.Limit
{
    public class TotalLimitRate
    {

        // 静态变量模拟令牌桶
        private static int _remainingTokens = 0;
        private static readonly int _totalTokens = 100; // 总限流人数
        //private static readonly TimeSpan _startTime = TimeSpan.Parse("09:00:00");
        //private static readonly TimeSpan _endTime = TimeSpan.Parse("09:10:00");
        private static DateTime _lastResetTime = DateTime.MinValue;

        // 单一限流方法
        public static (bool Success, string Message) TryAcquireToken(TimeSpan _startTime, TimeSpan _endTime, int _totalTokens)
        {
            // 检查时间窗口并初始化
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            // 如果时间超过9:10或未到9:00，重置令牌并拒绝请求
            if (currentTime < _startTime || currentTime > _endTime)
            {
                if (now.Date != _lastResetTime.Date || currentTime > _endTime)
                {
                    Interlocked.Exchange(ref _remainingTokens, 0); // 重置令牌
                    _lastResetTime = now;
                }
                return (false, "活动未在时间窗口内");
            }

            // 初始化令牌（仅在9:00前后首次触发）
            if (_remainingTokens == 0 && now.Date != _lastResetTime.Date)
            {
                Interlocked.Exchange(ref _remainingTokens, _totalTokens);
                _lastResetTime = now;
            }

            // 原子操作：减少令牌计数
            int tokens = Interlocked.Decrement(ref _remainingTokens);
            if (tokens >= 0)
            {
                return (true, "抢购成功！");
            }

            // 令牌不足，恢复计数（防止负数）
            Interlocked.Increment(ref _remainingTokens);
            return (false, "名额已满，请下次参与");
        }

        private static readonly object _lock = new object();
        // 单一限流方法
        public static (bool Success, string Message) TryAcquireToken2(TimeSpan _startTime, TimeSpan _endTime)
        {
            lock (_lock) // 确保线程安全
            {
                // 检查时间窗口并初始化
                var now = DateTime.Now;
                var currentTime = now.TimeOfDay;

                // 如果时间超过9:10或未到9:00，重置令牌并拒绝请求
                if (currentTime < _startTime || currentTime > _endTime)
                {
                    //if (now.Date != _lastResetTime.Date || currentTime > _endTime)
                    //{
                    //    _remainingTokens = 0; // 重置令牌
                    //    _lastResetTime = now;
                    //}
                    return (true, "其他时间不限流");
                }

                // 初始化令牌（仅在9:00前后首次触发）
                if (_remainingTokens == 0 && now.Date != _lastResetTime.Date)
                {
                    _remainingTokens = _totalTokens;
                    _lastResetTime = now;
                }

                // 检查令牌并减少
                if (_remainingTokens > 0)
                {
                    _remainingTokens--;
                    return (true, "抢购成功！");
                }

                return (false, "名额已满，请下次参与");
            }
        }

    }
}
