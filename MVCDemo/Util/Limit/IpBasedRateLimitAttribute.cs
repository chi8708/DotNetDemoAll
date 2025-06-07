using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace MVCDemo.Util.Limit
{
    public class IpBasedRateLimitAttribute : ActionFilterAttribute
    {
        private static readonly Dictionary<string, int> _requestCounts = new Dictionary<string, int>();
        private static readonly object _lock = new object();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ip = filterContext.HttpContext.Connection.RemoteIpAddress.ToString();

            lock (_lock)
            {
                if (!_requestCounts.ContainsKey(ip))
                    _requestCounts[ip] = 0;

                _requestCounts[ip]++;

                // 每个IP每分钟重置计数（简单实现）
                if (DateTime.Now.Second == 0)
                    _requestCounts.Clear();

                if (_requestCounts[ip] > 30) // 每分钟最多30次请求
                {
                    filterContext.Result = new ContentResult
                    {
                        Content = "访问过于频繁",
                        ContentType = "text/plain"
                    };
                    filterContext.HttpContext.Response.StatusCode = 429;
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
