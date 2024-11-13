using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class QRCodeHelper
    {
        // 计算 CRC16_MINIM 校验码
        public static string CalculateCRC16Minim(string input)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in Encoding.ASCII.GetBytes(input))
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    bool bit = (crc & 1) == 1;
                    crc >>= 1;
                    if (bit) crc ^= 0x8408;
                }
            }
            return crc.ToString("X4"); // 转换为4位16进制字符串
        }

        // 生成二维码并在中心添加浅黄色“税”字
        public static  void GenerateQRCodeWithText_Old(string content, string centerText)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                using (Bitmap qrImage = qrCode.GetGraphic(20))
                {
                    using (Graphics graphics = Graphics.FromImage(qrImage))
                    {
                        // 计算矩形区域大小并设置字体样式
                        float mmToPixel = 3.78f; // 转换系数
                        float rectSize = 5.36f * mmToPixel; // 矩形大小 (5.36mm x 5.36mm)
                        RectangleF rect = new RectangleF(
                            (qrImage.Width - rectSize) / 2,
                            (qrImage.Height - rectSize) / 2,
                            rectSize, rectSize);

                        // 设置字体、颜色和对齐方式
                        Font font = new Font("宋体", 9, FontStyle.Regular); // 9pt 宋体
                        SolidBrush brush = new SolidBrush(Color.FromArgb(250, 239, 173)); // 浅黄色
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        // 绘制“税”字在二维码中心
                        graphics.DrawString(centerText, font, brush, rect, format);
                    }

                    // 保存二维码图像
                    qrImage.Save("TaxQRCode.png");
                    Console.WriteLine("二维码已保存为 TaxQRCode.png");
                }
            }
        }

        // 生成高分辨率的带白色底色和“税”字的二维码
       public static Bitmap GenerateQRCodeWithText(string content, string centerText)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                // 生成高分辨率的二维码图像
                using (Bitmap qrImage = qrCode.GetGraphic(20, Color.Black, Color.White, true))
                {
                    int highResSize = 200; // 设置生成的二维码图像大小为 200x200 像素

                    using (Bitmap highResImage = new Bitmap(highResSize, highResSize))
                    using (Graphics graphics = Graphics.FromImage(highResImage))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        // 绘制二维码到高分辨率图像
                        graphics.DrawImage(qrImage, 0, 0, highResSize, highResSize);

                        // 计算 5.36mm 转换为像素的大小
                        float mmToPixel = 7.5f; // 更高的 DPI 值，1 mm ≈ 7.5 pixels (在高 DPI 下)
                        float rectSize = 5.2f * mmToPixel; // 5.36mm 转换为像素
                        RectangleF rect = new RectangleF(
                            (highResSize - rectSize) / 2,
                            (highResSize - rectSize) / 2,
                            rectSize, rectSize);

                        // 绘制白色矩形作为底色
                        SolidBrush whiteBrush = new SolidBrush(Color.White);
                        graphics.FillRectangle(whiteBrush, rect);

                        // 设置字体、颜色和对齐方式
                        Font font = new Font("宋体", 22, FontStyle.Regular); // 9pt 宋体
                        SolidBrush textBrush = new SolidBrush(Color.FromArgb(250, 239, 173)); // 浅黄色
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        // 在白色矩形中心绘制“税”字
                        graphics.DrawString(centerText, font, textBrush, rect, format);

                        return new Bitmap(highResImage); // 返回带“税”字的高分辨率二维码
                    }
                }
            }
        }

        // 调整图像大小到 20mm x 20mm
        public static Bitmap ResizeImage(Bitmap image, float widthMm, float heightMm)
        {
            float mmToPixel = 3.78f;
            int width = (int)(widthMm * mmToPixel);
            int height = (int)(heightMm * mmToPixel);

            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }
    }
}
