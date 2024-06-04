using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using FaceRecognitionDotNet;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using SixLabors.ImageSharp.Processing.Processors.Drawing;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing.Processors;


namespace LipstickApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LipstickController : ControllerBase
    {
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        private readonly string _resultFolder = Path.Combine(Directory.GetCurrentDirectory(), "results");
        private readonly FaceRecognition _faceRecognition;

        public LipstickController()
        {
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
            if (!Directory.Exists(_resultFolder))
            {
                Directory.CreateDirectory(_resultFolder);
            }

            _faceRecognition = FaceRecognition.Create("models"); // 确保下载并放置模型文件
        }

        [HttpGet]
        public async Task<string> Test() 
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "assets\\img\\face.jpg");
            var resultImagePath = ApplyLipstickColor(filePath, "CC6284");

            return resultImagePath;

        }


        [HttpPost("applyLipstick")]
        public async Task<IActionResult> ApplyLipstick([FromForm] IFormFile file, [FromForm] string lipColor)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var filePath = Path.Combine(_uploadFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var resultImagePath = ApplyLipstickColor(filePath, lipColor);

            return Ok(new { resultImagePath });
        }

        private string ApplyLipstickColor(string imagePath, string lipColor)
        {
            var image = FaceRecognition.LoadImageFile(imagePath);
            var faceLocations = _faceRecognition.FaceLocations(image).ToArray();
            var faceLandmarks = _faceRecognition.FaceLandmark(image, faceLocations).ToArray();

            var color = Color.ParseHex(lipColor).WithAlpha(0.5f);// 设置颜色透明度以保留唇纹细节
            using (Image<Rgba32> img = Image.Load<Rgba32>(imagePath))
            {
                foreach (var landmarks in faceLandmarks)
                {
                    if (landmarks.TryGetValue(FacePart.TopLip, out var topLip) && landmarks.TryGetValue(FacePart.BottomLip, out var bottomLip))
                    {
                        var lipPoints = topLip.Concat(bottomLip).Select(point => new SixLabors.ImageSharp.PointF(point.Point.X, point.Point.Y)).ToArray();
                        //img.Mutate(ctx => ctx.FillPolygon(color, lipPoints)); //直接填充很僵硬

                        // 创建一个渐变填充笔刷
                        var centerPoint = new SixLabors.ImageSharp.PointF(
                            lipPoints.Average(p => p.X),
                            lipPoints.Average(p => p.Y)
                        );

                        var brush = new RadialGradientBrush(centerPoint, 50, GradientRepetitionMode.None, new ColorStop(0, color), new ColorStop(1, Color.Transparent));

                            img.Mutate(ctx =>
                            {
                            // 应用渐变填充
                            ctx.FillPolygon(brush, lipPoints);


                            // 混合模式
                            ctx.FillPolygon(new DrawingOptions { GraphicsOptions = new GraphicsOptions { ColorBlendingMode = PixelColorBlendingMode.Overlay } }, color, lipPoints);

                            // 混合模式
                            //ctx.DrawPolygon(new DrawingOptions { GraphicsOptions = new GraphicsOptions { BlendPercentage = 0.5f, ColorBlendingMode = PixelColorBlendingMode.Overlay } }, color,1, lipPoints);

                            // 边缘平滑
                            ctx.GaussianBlur(1f);
                            });
                    }
                }

                var resultImagePath = Path.Combine(_resultFolder, Path.GetFileName(imagePath));
                img.Save(resultImagePath);
                return resultImagePath;
            }
        }
    }
}
