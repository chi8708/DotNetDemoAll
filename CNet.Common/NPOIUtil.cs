using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNet.Common
{
    public class NPOIUtil
    {

           /// 导出数据到excel
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        public static byte[] DataTableExcel(List<ExcelModel> list)
        {
            //创建EXCEL工作薄
            IWorkbook workBook = new XSSFWorkbook();
           
            //创建sheet文件表
            foreach (var item in list)
            {
                var sheetName = item.SheetName;
                var dataTable = item.DataTable;
                var headerList = item.Headers;
                ISheet sheet = workBook.CreateSheet(sheetName);
                #region 创建Excel表头
                //创建表头
                IRow header = sheet.CreateRow(0);
                CreateHearder(header, headerList);

          
                #endregion
                #region 填充Excel单元格中的数据
                //给工作薄中非表头填充数据，遍历行数据并进行创建和填充表格
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    IRow row = sheet.CreateRow(i + 1);//表示从整张数据表的第二行开始创建并填充数据，第一行已经创建。
                    //cts 修改
                    for (int j = 0; j < headerList.Count; j++)
                    {
                        ICell cell = row.CreateCell(j);
                        var colName = headerList[j].ColName;
                        string value= dataTable.Rows[i][colName].ToString();
                        cell.SetCellValue(value);
                    }
                    //for (int j = 0; j < dataTable.Columns.Count; j++)//遍历并创建每个单元格cell，将行数据填充在创建的单元格中。
                    //{
                    //    //将数据读到cell单元格中
                    //    ICell cell = row.CreateCell(j);
                    //    cell.SetCellValue(dataTable.Rows[i][j].ToString());//对数据为null的情况进行处理
                    //}
                }
                #endregion

                AutoColumnWidth(sheet, item.Headers.Count-1);
            }
         



            #region 工作流创建Excel文件
            //工作流写入，通过流的方式进行创建生成文件
            MemoryStream stream = new MemoryStream();
            workBook.Write(stream);
            byte[] buffer = stream.ToArray();
            stream.Dispose();
            return buffer;

            //cts 注释
            //using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            //{
            //    try
            //    {
            //        fs.Write(buffer, 0, buffer.Length);
            //        fs.Flush();
            //    }
            //    catch
            //    {
            //        //异常不做任何处理，好处是让客户感觉没有问题，缺点是不利于查找程序的问题，需要日志文件跟踪。
            //    }
            //    finally
            //    {
            //        fs.Dispose();//出现异常时，手动释放fs写对象
            //        stream.Dispose();//出现异常时，手动释放stream流对象，防止卡死的现象
            //    }
            //}
            #endregion
        }

        //创建表头
        private static void CreateHearder(IRow header, List<ExcelModelHeader> headers)
        {
            for (int i = 0; i <headers.Count; i++)
            {
                ICell cell = header.CreateCell(i);
                cell.SetCellValue(headers[i].Value);
                
            }
        }

        //宽度自适应
        public static void AutoColumnWidth(ISheet sheet, int cols)
        {
            for (int col = 0; col <= cols; col++)
            {
                sheet.AutoSizeColumn(col);//自适应宽度，但是其实还是比实际文本要宽
                int columnWidth = sheet.GetColumnWidth(col) / 256;//获取当前列宽度
                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    ICell cell = row.GetCell(col);
                    int contextLength = Encoding.UTF8.GetBytes(cell.ToString()).Length;//获取当前单元格的内容宽度
                    columnWidth = columnWidth < contextLength ? contextLength : columnWidth;

                }
                sheet.SetColumnWidth(col, columnWidth * 200);//

            }
        }



        /// <summary>
        /// 导出数据到excel 支持xls xlsx
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="headers"></param>
        /// <param name="isXlsx"></param>
        /// <returns></returns>
        public static MemoryStream ExportToExcel(DataTable dt, List<ExcelModelHeader> headers, bool isXlsx = false)
        {

            IWorkbook workbook;
            if (isXlsx)
                workbook = new XSSFWorkbook(); // .xlsx
            else
                workbook = new HSSFWorkbook(); // .xls

            ISheet sheet = workbook.CreateSheet("Sheet1");

            // 写入表头
            IRow headerRow = sheet.CreateRow(0);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                headerRow.CreateCell(i).SetCellValue(headers.First(p => p.ColName == dt.Columns[i].ColumnName).Value);
            }


            // 写入数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            // 返回文件
            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0; // 重置流位置
            return ms;
            //Response.Clear();
            //Response.ContentType = isXlsx
            //    ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            //    : "application/vnd.ms-excel";
            //Response.AddHeader("content-disposition", string.Format("attachment; filename=Export.{0}", isXlsx ? "xlsx" : "xls"));
            //Response.BinaryWrite(ms.ToArray());
            //Response.End();
        }


        #region xls导入

        /// <summary>
        /// 读取excel,将数据Excel数据源转化为datatable类型  
        /// 默认第一行为标头  
        /// </summary>  
        /// <param name="strFileName">excel文档路径</param>  
        /// <returns></returns>  
        public static DataTable Import(string strFileName, string FileType)
        {
            IWorkbook hssfworkbook;
            DataTable dt = new DataTable();
            //HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))//数据读取
            {
                ////XSSFWorkbook 适用XLSX格式，HSSFWorkbook 适用XLS格式
                //不同格式excle判断
                hssfworkbook = new HSSFWorkbook(file);
                //if (FileType == "xls")
                //{
                //    hssfworkbook = new HSSFWorkbook(file);
                //}
                //else
                //{
                //    //hssfworkbook = new XSSFWorkbook(file);
                //}
            }
            ISheet sheet = hssfworkbook.GetSheetAt(0);
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;

            for (int j = 0; j < cellCount; j++)
            {
                ICell cell = headerRow.GetCell(j);
                dt.Columns.Add(cell.ToString());
            }

            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                DataRow dataRow = dt.NewRow();

                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    if (row.GetCell(j) != null)
                        dataRow[j] = row.GetCell(j).ToString();
                }

                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 将stream数据源转化为datatable类型  
        /// 默认第一行为标头  
        /// </summary>  
        /// <param name="strFileName"></param>  
        /// <returns></returns>  
        public static DataTable ImportStream(Stream stream)
        {
            IWorkbook hssfworkbook;
            DataTable dt = new DataTable();
            using (Stream file = stream)
            {
                hssfworkbook = new HSSFWorkbook(file);
            }
            ISheet sheet = hssfworkbook.GetSheetAt(0);
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;

            for (int j = 0; j < cellCount; j++)
            {
                ICell cell = headerRow.GetCell(j);
                dt.Columns.Add(cell.ToString());
            }
            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                DataRow dataRow = dt.NewRow();
                var cell = row.GetCell(row.FirstCellNum).ToString();
                if (cell != "") //过滤空行
                {
                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {

                        if (row.GetCell(j) != null)
                        {
                            dataRow[j] = row.GetCell(j).ToString();
                        }
                    }
                    dt.Rows.Add(dataRow);
                }
            }
            return dt;
        }
        #endregion


    }

    public class ExcelModel 
    {
        public DataTable DataTable { get; set; }

        public string SheetName { get; set; }

        public List<ExcelModelHeader> Headers { get; set; }

      
    }

    public class ExcelModelHeader
    {

        /// <summary>
        /// 列名
        /// </summary>
        public string ColName { get; set; }
        /// <summary>
        /// 列值
        /// </summary>
        public string Value { get; set; }

    }




}
