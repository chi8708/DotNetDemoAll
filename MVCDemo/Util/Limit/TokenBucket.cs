using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace MVCDemo.Util.Limit
{
    /// <summary>
    /// 方法1 令牌桶
    /// </summary>
    public class TokenBucket
    {
        private readonly int _capacity;        // 桶的最大容量
        private readonly double _refillRate;   // 令牌填充速率（每秒）
        private double _tokens;                // 当前令牌数量
        private DateTime _lastRefill;          // 上次填充时间
        private readonly object _lock = new object();

        /// <summary>
        /// 初始化令牌桶
        /// </summary>
        /// <param name="capacity">桶容量（最多存储多少个令牌）</param>
        /// <param name="refillRate">填充速率（每秒产生多少个令牌）</param>
        public TokenBucket(int capacity, double refillRate)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _tokens = capacity;  // 初始时桶是满的
            _lastRefill = DateTime.UtcNow;
        }

        /// <summary>
        /// 尝试消耗令牌
        /// </summary>
        /// <param name="tokensRequested">需要的令牌数量</param>
        /// <returns>是否成功获取令牌</returns>
        public bool TryConsume(int tokensRequested = 1)
        {
            lock (_lock)
            {
                // 先补充令牌
                Refill();

                // 检查令牌是否足够
                if (_tokens >= tokensRequested)
                {
                    _tokens -= tokensRequested;  // 消耗令牌
                    return true;                 // 允许请求
                }

                return false;  // 令牌不足，拒绝请求
            }
        }

        /// <summary>
        /// 补充令牌
        /// </summary>
        private void Refill()
        {
            var now = DateTime.UtcNow;
            var timePassed = (now - _lastRefill).TotalSeconds;  // 距离上次填充过了多少秒

            if (timePassed > 0)
            {
                // 计算应该添加的令牌数 = 时间间隔 × 填充速率
                var tokensToAdd = timePassed * _refillRate;

                // 更新令牌数量，但不能超过桶容量
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefill = now;
            }
        }

        /// <summary>
        /// 获取当前令牌数量（用于监控）
        /// </summary>
        public double CurrentTokens
        {
            get
            {
                lock (_lock)
                {
                    Refill();
                    return _tokens;
                }
            }
        }

        /// <summary>
        /// 等待直到有足够令牌（异步版本）
        /// </summary>
        public async Task<bool> TryConsumeAsync(int tokensRequested = 1, TimeSpan timeout = default)
        {
            var endTime = DateTime.UtcNow + (timeout == default ? TimeSpan.FromSeconds(30) : timeout);

            while (DateTime.UtcNow < endTime)
            {
                if (TryConsume(tokensRequested))
                    return true;

                // 等待一小段时间再重试
                await Task.Delay(100);
            }

            return false;  // 超时
        }
    }


    /// <summary>
    /// 方法2   // 按用户限流[TokenBucketRateLimit(Capacity = 10, RefillRate = 1.0, BucketKey = "user")]
    /// </summary>
    public class TokenBucketRateLimitAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, TokenBucket> _buckets =
            new ConcurrentDictionary<string, TokenBucket>();

        public int Capacity { get; set; } = 10;           // 桶容量
        public double RefillRate { get; set; } = 1.0;     // 每秒填充速率
        public int TokensRequired { get; set; } = 1;      // 每次请求需要的令牌数
        public string BucketKey { get; set; } = "default"; // 令牌桶标识

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var key = GetBucketKey(filterContext);
            var bucket = _buckets.GetOrAdd(key, k => new TokenBucket(Capacity, RefillRate));

            if (!bucket.TryConsume(TokensRequired))
            {
                filterContext.Result = new JsonResult(new
                {
                    success = false,
                    message = "请求过于频繁，请稍后再试",
                    currentTokens = Math.Floor(bucket.CurrentTokens)  // 显示当前令牌数
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
}
