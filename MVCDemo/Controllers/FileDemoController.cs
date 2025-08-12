using CNet.Common;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace MVCDemo.Controllers
{
    public class FileDemoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// excel模板下载
        /// </summary>
        /// <returns></returns>
        public FileResult DownloadTemplate()
        {
           
            var dt = new DataTable();
            var excelModel = new List<ExcelModel>{
                new ExcelModel(){
                    DataTable=dt,
                    SheetName="sheet1",
                    Headers=new List<ExcelModelHeader>
                    {
                        new ExcelModelHeader(){ ColName="col1",Value="字段1"},
                        new ExcelModelHeader(){ ColName="col1",Value="字段2"},
                    }
                }
            };
            //var reportByte = NPOIUtil.DataTableExcel(excelModel, false);//仅支持xls
            var reportByte = NPOIUtil.ExportToExcel(dt, excelModel.First().Headers);
            var fileName = string.Format("模板");
            return File(reportByte.ToArray(), "application/vnd.ms-excel", fileName + ".xls");
        }
    }
}
