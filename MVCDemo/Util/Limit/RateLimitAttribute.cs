using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MVCDemo.Util.Limit
{
    /// <summary>
    ///  基于内存的计数器滑动窗口限流：每个用户在指定时间窗口内只能发起一定数量的请求。
    /// </summary>
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requests = new ConcurrentDictionary<string, Queue<DateTime>>();
        private static readonly object _lock = new object();

        public int MaxRequests { get; set; } = 100;
        public int TimeWindowSeconds { get; set; } = 60;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var clientId = GetClientIdentifier(filterContext.HttpContext);

            lock (_lock)
            {
                if (!_requests.ContainsKey(clientId))
                    _requests[clientId] = new Queue<DateTime>();

                var queue = _requests[clientId];
                var now = DateTime.Now;

                // 清理过期请求
                while (queue.Count > 0 && (now - queue.Peek()).TotalSeconds > TimeWindowSeconds)
                    queue.Dequeue();

                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };

                if (queue.Count >= MaxRequests)
                {
                    //filterContext.Result = new HttpStatusCodeResult(429, "Too Many Requests");
                    filterContext.Result = new Microsoft.AspNetCore.Mvc.JsonResult(new
                    {
                        state = -1,
                        msg = "请求过于频繁，请稍后再试"
                    }, jsonOptions);
                    return;
                }

                queue.Enqueue(now);
            }

            base.OnActionExecuting(filterContext);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // 可以基于IP、用户ID等组合
            //return context.Request.UserHostAddress + "_" + (context.User?.Identity?.Name ?? "anonymous");
            return context.Request.Host + "_" + (context.User?.Identity?.Name ?? "anonymous");

        }
    }
}
