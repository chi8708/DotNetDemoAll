using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfSharp.Pdf.AcroForms;
using WindowsFormsApp1;

namespace InvoiceDemo
{
    public class PdfHelper
    {
        public void FillForm(string pdfPath, string pdfSavePath, Dictionary<string, string> formFields, List<detailmodel> details)
        {
            var reader = new PdfReader(pdfPath);
            using (var outputStream = new FileStream(pdfSavePath, FileMode.Create, FileAccess.ReadWrite))
            {

                var stamper = new PdfStamper(reader, outputStream);//itextsharp 5.2.0上会报错，但是不影响使用
                reader.Close();
                stamper.Writer.CloseStream = false;

                #region 表单域填充
                var form = stamper.AcroFields;
                form.GenerateAppearances = true;
                stamper.FormFlattening = false;

                BaseFont basefont = BaseFont.CreateFont("C:\\Windows\\Fonts\\simsun.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, true);
                iTextSharp.text.Font font = new iTextSharp.text.Font(basefont, 9f);
                //iTextSharp.text.Font font11 = new iTextSharp.text.Font(basefont, 11f);
                //BaseFont baseFontCOURIER = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                //iTextSharp.text.Font fontCOURIER = new iTextSharp.text.Font(baseFontCOURIER, 12f);  // 设置为粗体 Courier 字体
                foreach (var field in formFields)
                {
                    //直接填充表单域，不设置字体 stamper.FormFlattening=true;后就有问题，宋体无法显示。设置成宋体1后pdf太大
                    //form.SetFieldProperty(field.Key, "textfont", basefont,null);
                    //form.SetField(field.Key, field.Value);

                    var positions = form.GetFieldPositions(field.Key);
                    if (positions != null && positions.Count > 0)
                    {
                        foreach (var position in positions)
                        {
                            var pageNumber = position.page; // 表单域所在的页面
                            var rect = position.position;  // 表单域的位置 (Rectangle)

                            // 使用 Paragraph 替换表单内容
                            PdfContentByte contentByte = stamper.GetOverContent(pageNumber);
                            ColumnText ct = new ColumnText(contentByte);

                            ct.SetSimpleColumn(
                                rect.Left,   // 左边界
                                rect.Bottom, // 底边界
                                rect.Right,  // 右边界
                                rect.Top + 3     // 顶边界
                            );

                            // 设置垂直居中对齐
                            //ct.Alignment = Element.ALIGN_CENTER;  // 水平居中
                            // 创建一个段落
                            Paragraph paragraph = new Paragraph(field.Value, font);
                            if (field.Key == "totalPriceTaxt")
                            {
                                paragraph.Font.Size = 11f;
                            }
                            else if (field.Key == "buyercode")
                            {
                                paragraph.Font.Size = 12f;
                            }
                            ct.AddElement(paragraph);
                            ct.Go(); // 绘制内容
                            paragraph.Font.Size = 9f;
                        }
                    }

                }
                form.GenerateAppearances = false;
                #endregion

                #region 发票明细
                //关于itextsharp的单位换算，itextsharp使用磅作为单位，(1cm / 2.54)*72 = 28.3464566928磅，需要的可以自行换算1mm=2.83464566929
                //宋体
                float baseunit = 2.83464566929f;//长度单位基数

                //创建表格
                PdfPTable table = new PdfPTable(8);
                table.HorizontalAlignment = Element.ALIGN_LEFT;
                table.TotalWidth = 201 * baseunit;//表格总宽度
                table.LockedWidth = true;//锁定宽度
                table.SetWidths(new float[] { 37*baseunit,
                        24*baseunit,
                        12*baseunit,
                        25*baseunit,
                        25*baseunit,
                        26*baseunit,
                        25* baseunit,
                        27*baseunit });

                int cellwidth = 0;
                int padding = 1;
                for (int i = 0; i < details.Count; i++)
                {
                    PdfPCell cell = new PdfPCell(new Paragraph(details[i].Name, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = padding;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph(details[i].Model, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = padding;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph(details[i].Unit, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = padding;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph(details[i].Num, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = padding;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph(details[i].Price, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = padding;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph(details[i].Total, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph(details[i].TaxRate, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = padding;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph(details[i].TaxAmount, font));
                    cell.BorderWidth = cellwidth;
                    cell.Padding = padding;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(cell);

                }

                float x = 4.5f * baseunit;//14;
                float y = 82.5f * baseunit; //235f;
                table.WriteSelectedRows(0, -1, x, y, stamper.GetOverContent(1));
                //table.WriteSelectedRows(0, -1, x, y, stamper.GetUnderContent(1)); 
                #endregion

                // 在指定位置插入二维码（左上角 20mm x 20mm 位置）
                var qrImage = GetQRcode();
                iTextSharp.text.Image qrCodeImage = iTextSharp.text.Image.GetInstance(qrImage, ImageFormat.Png);
                qrCodeImage.SetAbsolutePosition(12.5f, 318f); // 设置位置 (X, Y)
                                                              // qrCodeImage.ScaleToFit(56.7f, 56.7f); // 将二维码缩放到 20mm x 20mm (1mm ≈ 2.835pt)
                qrCodeImage.ScaleToFit(67.88f, 67.88f); // 将二维码缩放到 20mm x 20mm (1mm ≈ 2.835pt)
                PdfContentByte pdfContentByte = stamper.GetOverContent(1);
                pdfContentByte.AddImage(qrCodeImage);

                // 设置压缩级别 不能压缩压缩后部分表单字段会消失
                stamper.Writer.SetFullCompression();
                stamper.Writer.CompressionLevel = PdfStream.BEST_COMPRESSION; // 设置最佳压缩级别
                // 如果 PDF 包含表单，将其平面化以减少大小
                stamper.FormFlattening = true;
                stamper.Close();

            }
        }

        private Bitmap GetQRcode()
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

            return qrImage;
            // 将二维码调整为 20mm x 20mm 缩小后模糊，在pdf中缩小到20mm x 20mm
            //Bitmap resizedQrImage = QRCodeHelper.ResizeImage(qrImage, 20, 20);
            //return resizedQrImage;
        }
    }
    public class detailmodel
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Unit { get; set; }
        public string Num { get; set; }
        public string Price { get; set; }
        public string Total { get; set; }
        public string TaxRate { get; set; }
        public string TaxAmount { get; set; }
    }
}
