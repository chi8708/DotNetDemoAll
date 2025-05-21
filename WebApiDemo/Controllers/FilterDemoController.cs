using CNet.Common;
using Com.Mlq.SM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NPOI.SS.Formula.Functions;
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
            string pri = "00F45D758A60BA0BD8D3757AC58BDC846F50C4510F1287ADAA663E1E6065CBD536";
            string data = "04c16454733c77a96fd71924a91515bf84f135ba858c8483a929c0bea4c22d8565c36afefb6af69f1203953108a46c789c493b497f54c5101a923ccb1f3ae702c3b7f7d58c2c6a74ebed669706fdb89c0c3582148513729f5711cf4f240af16483bf5e60f6653b76fe3adbe10a48f55b00001e41dbf3d09d23a9cbfaf5d9ced23a76eee84a0e283d44bac1611fa6a74a8722b867728ef21eece34bc51c418ba25d";
            var deDate=SM2Utils.Decrypt(pri,data);
            SM2Utils.Main();

            var deDate2 = SM2Crypto.Decrypt(pri, data);
            SM2Crypto.Main();

            return "Test2";
        }
    }
}
