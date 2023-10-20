using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiDemo.Filter
{
  
    public class Action1Attribute: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}
