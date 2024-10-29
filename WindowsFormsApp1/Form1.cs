using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InvoiceDemo;
using iTextSharp.text;
using iTextSharp.text.pdf;
using static iTextSharp.text.pdf.codec.TiffWriter;


using System.Reflection;
using System.Web;


namespace WindowsFormsApp1
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            WindowsFormsApp1.Form1.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {


            Document document = new Document(pageSize: new Rectangle(0,0,210,140), marginLeft: 7, marginRight: 7, marginBottom: 7, marginTop: 7);


            //模板使用Adobe Acrobat制作，工具→准备表单→文本域
            string filename = "E:\\项目资料\\电子发票-乐企\\开发\\invoiceTemplate.pdf";
             string outfilename = @"E:\项目资料\电子发票-乐企\开发\invoiceTemplate1.pdf";
           string imgsrc = "C:\\Users\\Administrator\\Desktop\\Assets\\TestPic.jpg";
            // 初始化PdfReader
            //PdfReader pdfReader = new PdfReader(filename);
            /*//PdfDocument document = new PdfDocument();*/

            // 创建PdfStamper
            //PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(outfilename, FileMode.Create));
            //var forms = pdfStamper.AcroFields;
            //forms.SetField("fill_1", "fill_1");
            //forms.SetField("fill_2", "fill_2");
            //forms.SetField("@BuyerTax", "xxxxxx");
            // AcroFields.Item item = forms.GetFieldItem("@BuyerTax"); 
            /*float x =1/ (float)0.352778;
            double h = 0.352778 * 140;
            double w = 0.352778 * 210;
            //计量单位pt,1pt=0.352777mm
            // 添加文本到第一页
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(outfilename, FileMode.Create));
            //iTextSharp.text.Rectangle rect = pdfReader.GetPageSize(1);
            //Font font=new Font(iTextSharp.text.Font.FontFamily.s)
            Phrase p1 = new Phrase("cccslk时代峰峻圣诞快乐cc");
          ColumnText.ShowTextAligned(pdfStamper.GetUnderContent(1), Element.DIV, p1, (float)19*x, (float)103.5*x, 0);*/
            // 关闭PdfStamper
            //pdfStamper.Close();
            //pdfReader.Close();
            var detail = new List<detailmodel>() { new detailmodel() { Name= "上了对方极乐世界sgsgggg",Model= "收到了放假", Num="10", Price="10.006661", Total="10000.00", TaxAmount="1701.00", TaxRate="17%", Unit="x" },
            new detailmodel() { Name= "上了对方极乐世界",Model= "收到了放", Num="10", Price="100.00", Total="1000.00", TaxAmount="170.00", TaxRate="17%", Unit="x" },
            new detailmodel() { Name= "sfjsdljflsdfjksldfjslkfjslfjkslfj",Model= "2394820938402" }
            };
            new PdfHelper().FillForm(filename, outfilename, new Dictionary<string, string>() { 
                {"buyercode","xxxxxx111" },
                { "buyername","测试测试测测试测试测测试测试测测试测试测测试测试测测试测试测测试测试测测试测试测111"},
                //{"sallercode","weriweuriwu111"},
                //{"sallername","测试测试测试测试测试测试测试"},
                { "invoiceno","123456789012345678901"},
                { "invoicedate",DateTime.Now.ToString("yyyy年MM月dd日")},
                { "totalPrize",detail.Sum(p=>string.IsNullOrWhiteSpace(p.Total)?0m:Convert.ToDecimal(p.Total)).ToString("0.00")}
            }, detail);


        }


    }
}
