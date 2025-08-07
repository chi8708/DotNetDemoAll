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
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


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


            //===不含税计算方式===

            //1、税额 = 金额（不含）*税率

            //2、合计金额 = 金额（不含）

            //3、价税合计 = 金额+税额




            //===含税计算方式===

            //1、税额 = 金额（不含）*税率 = 金额（含）* 税率/（1+税率）= 合计税额

            //2、合计金额 = 金额（含）/（1+税率）

            //3、价税合计 = 金额（含）


            //模板使用Adobe Acrobat制作，工具→准备表单→文本域
            string filename = "E:\\Study\\DotNet\\DotNetDemoAll\\WindowsFormsApp1\\invoiceTemplate.pdf";
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
            var remark = "";

             remark += "购方开户银行:测试银行; ";
             remark += "银行账号:12345645645645456456;\r\n";
             remark += string.Format("被红冲蓝字数电票号码：{0} 红字发票信息确认单编号：{1};\r\n", "12345678901234567890", "123123123123123123");

            new PdfHelper().FillForm(filename, outfilename, new Dictionary<string, string>() { 
                {"buyercode","xxxxxx111" },
                { "buyername","测试测试测测试测试"},
                //{"sallercode","weriweuriwu111"},
                //{"sallername","测试测试测试测试测试测试测试"},
                { "invoiceno","123456789012345678901"},
                { "invoicedate",DateTime.Now.ToString("yyyy年MM月dd日")},
                { "totalPrice",detail.Sum(p=>string.IsNullOrWhiteSpace(p.Total)?0m:Convert.ToDecimal(p.Total)).ToString("0.00")},
                { "totalTax","¥"+1700.ToString("0.00")},
                 { "totalPriceTaxtDX",MoneyToChinese.ConvertToChinese(Convert.ToDecimal(17000.ToString("0.00")))},
                 { "totalPriceTaxt",170000.ToString("0.00")},
                 { "Remark",remark},
                 { "bwPayee","张小三"},
                 { "bwChecker","李小二"},
                 { "bwInvoiceDrawer","王五"}
            }, detail);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //// 设置二维码内容的字段
            string version = "01";
            string invoiceType = "32";
            string invoiceCode = ""; // 可以为空
            string invoiceNumber = "24532000000093727413";
            string totalAmount = "94.50";
            string issueDate = "20241023";
            string checkCode = ""; // 可以为空

            //// 拼接内容，CRC 校验值稍后生成
            string qrContent = $"{version},{invoiceType},{invoiceCode},{invoiceNumber},{totalAmount},{issueDate},{checkCode}";

            // 计算 CRC 校验码
            string crc = QRCodeHelper.CalculateCRC16Minim(qrContent);
            qrContent += $",{crc}";

            //// 生成二维码并添加浅黄色的“税”字
            //QRCodeHelper.GenerateQRCodeWithText(qrContent, "税");
            //Console.WriteLine("二维码生成成功！");

            // 生成二维码并添加“税”字
            Bitmap qrImage = QRCodeHelper.GenerateQRCodeWithText(qrContent, "税");

            // 将二维码调整为 20mm x 20mm
            Bitmap resizedQrImage = QRCodeHelper.ResizeImage(qrImage, 20, 20);
        }
    }
}
