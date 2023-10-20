using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiDemo.Filter;

namespace WebApiDemo.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FilterDemoController : ControllerBase
    {
        [HttpGet]
        public string Test1() 
        {
            string s = System.IO.File.ReadAllText("D:/11.txt");
            return s;
        }


        [HttpGet]
        [Action1]
        public string Test2()
        {
            return "Test2";
        }
    }
}
