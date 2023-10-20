using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiDemo.Filter
{
    public class ActionFilter : IAsyncActionFilter
    {
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int a = 1;
            return Task.CompletedTask;
        }
    }
}
