using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiDemo.Filter
{
    //主要有5种filter： Authorization、Resource 、Action、Exception、Result
    //推荐使用异步filter:一般有同步和异步两个版本例 IAsyncExceptionFilter IExceptionFilter

    //全局过滤器
//builder.Services.Configure<MvcOptions>(option => {
//    option.Filters.Add<ExceptionFilter>();
//});
public class ExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            context.Result = new ObjectResult(new
            {
                state = -1,
                msg="请求异常"
            }); ;
            context.ExceptionHandled = true;
           return Task.CompletedTask;
        }
    }
}
