using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WindowsFormsApp1;

namespace InvoiceDemo
{
    public class PdfHelper
    {
        public void FillForm(string pdfPath, string pdfSavePath, Dictionary<string, string> formFields, List<detailmodel> details)
        {
                var reader = new PdfReader(pdfPath);
                using (var outputStream = new FileStream(pdfSavePath, FileMode.Create,FileAccess.ReadWrite))
                {

                    var stamper = new PdfStamper(reader, outputStream);//itextsharp 5.2.0�ϻᱨ�����ǲ�Ӱ��ʹ��
                    reader.Close();
                    stamper.Writer.CloseStream = false;
                    var form = stamper.AcroFields;
                    form.GenerateAppearances = true;
                    foreach (var field in formFields)
                    {
                        form.SetField(field.Key, field.Value);

                    }
                    form.GenerateAppearances = false;

                    #region ��Ʊ��ϸ
                    //����itextsharp�ĵ�λ���㣬itextsharpʹ�ð���Ϊ��λ��(1cm / 2.54)*72 = 28.3464566928������Ҫ�Ŀ������л���1mm=2.83464566929
                    //����
                    float baseunit = 2.83464566929f;//���ȵ�λ����
                    BaseFont basefont = BaseFont.CreateFont("C:\\Windows\\Fonts\\simsun.ttc,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                    iTextSharp.text.Font font = new iTextSharp.text.Font(basefont, 9);
                    //�������
                    PdfPTable table = new PdfPTable(8);
                    table.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.TotalWidth = 201*baseunit;//����ܿ��
                    table.LockedWidth = true;//�������
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
                        cell.HorizontalAlignment=Element.ALIGN_RIGHT;
                        table.AddCell(cell);

                    }

                    float x = 4.5f * baseunit;//14;
                    float y = 82.5f * baseunit; //235f;
                    table.WriteSelectedRows(0, -1, x, y, stamper.GetOverContent(1));
                //table.WriteSelectedRows(0, -1, x, y, stamper.GetUnderContent(1)); 
                #endregion

                // ��ָ��λ�ò����ά�루���Ͻ� 20mm x 20mm λ�ã�
                var qrImage = GetQRcode();
                iTextSharp.text.Image qrCodeImage = iTextSharp.text.Image.GetInstance(qrImage, ImageFormat.Png);
                qrCodeImage.SetAbsolutePosition(12.5f, 318f); // ����λ�� (X, Y)
               // qrCodeImage.ScaleToFit(56.7f, 56.7f); // ����ά�����ŵ� 20mm x 20mm (1mm �� 2.835pt)
                qrCodeImage.ScaleToFit(67.88f, 67.88f); // ����ά�����ŵ� 20mm x 20mm (1mm �� 2.835pt)
                PdfContentByte pdfContentByte = stamper.GetOverContent(1);
                pdfContentByte.AddImage(qrCodeImage);

                // ����ѹ������
                stamper.Writer.SetFullCompression();
                stamper.Writer.CompressionLevel = PdfStream.BEST_COMPRESSION; // �������ѹ������
                // ��� PDF ������������ƽ�滯�Լ��ٴ�С
                stamper.FormFlattening = true;
                stamper.Close();
                
            }
        }

        private Bitmap GetQRcode() 
        {

            //// ���ö�ά�����ݵ��ֶ�
            string version = "01";
            string invoiceType = "32";
            string invoiceCode = ""; // ����Ϊ��
            string invoiceNumber = "24532000000093727413";
            string totalAmount = "94.50";
            string issueDate = "20241023";
            string checkCode = ""; // ����Ϊ��

            //// ƴ�����ݣ�CRC У��ֵ�Ժ�����
            string qrContent = $"{version},{invoiceType},{invoiceCode},{invoiceNumber},{totalAmount},{issueDate},{checkCode}";

            // ���� CRC У����
            string crc = QRCodeHelper.CalculateCRC16Minim(qrContent);
            qrContent += $",{crc}";

            //// ���ɶ�ά�벢���ǳ��ɫ�ġ�˰����
            //QRCodeHelper.GenerateQRCodeWithText(qrContent, "˰");
            //Console.WriteLine("��ά�����ɳɹ���");

            // ���ɶ�ά�벢��ӡ�˰����
            Bitmap qrImage = QRCodeHelper.GenerateQRCodeWithText(qrContent, "˰");

            return qrImage;
            // ����ά�����Ϊ 20mm x 20mm ��С��ģ������pdf����С��20mm x 20mm
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
