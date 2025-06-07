using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MVCDemo.Util.Limit;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MVCDemo.Controllers
{
    public class LimitController : Controller
    {

        //滑动窗口限流：基于时间滑动窗口，平滑处理请求。
        [RateLimit(MaxRequests = 5, TimeWindowSeconds = 60)]
        public IActionResult Index()
        {
            var clientKey = GetClientKey(); 

            //滑动窗口限流：允许在指定时间窗口内的请求数量，适用于高并发场景。
            //if (!SlidingWindowRateLimit.IsAllowed(clientKey, 10, TimeSpan.FromMinutes(1)))
            //{
            //    return new ContentResult
            //    {
            //        Content = @"
            //            <script type='text/javascript'>
            //                alert('请求过于频繁，请稍后再试');
            //                window.history.back();
            //            </script>",
            //        ContentType = "text/html; charset=utf-8"
            //    };
            //}

            return View();
        }


        // 静态令牌桶，每秒生成2个令牌，最多存储10个令牌。// 第1秒：最多10个请求 。之后每秒：最多2个请求
        private static readonly TokenBucket _tokenBucket = new TokenBucket(10, 2.0);

        //令牌桶算法：以固定速率生成令牌，请求需要获取令牌才能处理。
        // 场景1：API接口限流 - 允许突发，平稳补充
        //var apiRateLimit = new TokenBucket(50, 5.0);  // 50个令牌容量，每秒5个
        //// 场景2：支付接口 - 严格限流
        //var paymentLimit = new TokenBucket(3, 0.1);   // 3个令牌容量，每10秒1个
        //// 场景3：文件上传 - 大容量，慢补充
        //var uploadLimit = new TokenBucket(100, 1.0);  // 100个令牌容量，每秒1个
        //// 场景4：抢购活动 - 小容量，快补充
        //var flashSaleLimit = new TokenBucket(10, 2.0); // 10个令牌容量，每秒2个
        
        [TokenBucketRateLimit(Capacity = 10, RefillRate = 2, TokensRequired = 1)]// 普通接口：桶容量10，每秒填充2个令牌，每次消耗1个
        public ActionResult Purchase()
        {
            // 尝试消耗1个令牌
            //if (!_tokenBucket.TryConsume(1))
            //{
            //    Response.StatusCode = 500;
            //    return Json(new { success = false, message = "busing..., 系统繁忙，请稍后再试" });
            //}

            // 有令牌，可以处理请求
            // var result = ProcessPurchase();
            return View("index");
        }


        /// <summary>
        /// 漏洞桶限流：基于漏斗桶算法，平滑处理请求。
        /// </summary>
        /// <returns></returns>

        [LeakyBucketRateLimit(Capacity = 20, LeakRate = 2)]
        public ActionResult LeakyBucket()
        {
            return View("index");
        }


        /// <summary>
        /// 固定时间固定数量限流：在固定时间内允许固定数量的请求。
        /// </summary>
        /// <returns></returns>
        public ActionResult TotalLimit()
        {
            var startTime= TimeSpan.Parse("09:00:00");
            var endTime = TimeSpan.Parse("16:01:00");
            var result= TotalLimitRate.TryAcquireToken2(startTime, endTime);
            if (result.Success)
            {
                return View("index");
            }
            return Json(new { success = false, message = "busing..., 系统繁忙，请稍后再试" });
        }

        private string GetClientKey()
        {
            var ip = Request.Path;
            var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "anonymous";
            return $"{ip}_{userId}";
        }


    }
}
