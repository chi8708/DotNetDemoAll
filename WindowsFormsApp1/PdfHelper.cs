using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

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
                    Font font = new Font(basefont, 9);
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


                 stamper.Close();
                
            }
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
