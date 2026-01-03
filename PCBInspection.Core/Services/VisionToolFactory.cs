using System;
using System.Collections.Generic;
using OpenCvSharp;
using PCBInspection.Core.Tools;
using PCBInspection.Core.Models;

namespace PCBInspection.Core.Services
{
    public static class VisionToolFactory
    {
        // 定義工具執行的委派簽名
        // 回傳: (是否成功, 處理後的影像, 瑕疵列表)
        // 注意: 這裡我們擴充了回傳值，多了一個 Mat 用於顯示中間結果 pipeline
        public delegate (bool IsOk, Mat ResultImage, List<Defect> Defects) VisionAction(Mat input, object param);

        public class ToolDefinition
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public object DefaultParameters { get; set; }
            public VisionAction Action { get; set; }
        }

        public static List<ToolDefinition> GetAllTools()
        {
            var tools = new List<ToolDefinition>();

            // --- Image Processing ---

            tools.Add(new ToolDefinition
            {
                Name = "灰階化 (Grayscale)",
                Category = "01. 預處理",
                DefaultParameters = new GrayscaleParameters(),
                Action = (img, p) =>
                {
                    var result = new Mat();
                    if (img.Channels() == 3)
                        Cv2.CvtColor(img, result, ColorConversionCodes.BGR2GRAY);
                    else
                        img.CopyTo(result);
                    return (true, result, new List<Defect>());
                }
            });

            tools.Add(new ToolDefinition
            {
                Name = "濾波模糊 (Blur)",
                Category = "01. 預處理",
                DefaultParameters = new BlurParameters(),
                Action = (img, p) =>
                {
                    var pp = (BlurParameters)p;
                    var result = new Mat();
                    var k = pp.KernelSize % 2 == 1 ? pp.KernelSize : pp.KernelSize + 1;
                    
                    switch (pp.Type)
                    {
                        case BlurParameters.BlurType.Gaussian:
                            Cv2.GaussianBlur(img, result, new Size(k, k), pp.SigmaX);
                            break;
                        case BlurParameters.BlurType.Median:
                            Cv2.MedianBlur(img, result, k);
                            break;
                        case BlurParameters.BlurType.Box:
                            Cv2.Blur(img, result, new Size(k, k));
                            break;
                    }
                    return (true, result, new List<Defect>());
                }
            });

            tools.Add(new ToolDefinition
            {
                Name = "二值化 (Threshold)",
                Category = "01. 預處理",
                DefaultParameters = new ThresholdParameters(),
                Action = (img, p) =>
                {
                    var pp = (ThresholdParameters)p;
                    var result = new Mat();
                    var gray = new Mat();
                    
                    // 確保輸入為灰階
                    if (img.Channels() == 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
                    else img.CopyTo(gray);

                    ThresholdTypes type = ThresholdTypes.Binary;
                    if (pp.Method == ThresholdParameters.ThreshMethod.BinaryInv) type = ThresholdTypes.BinaryInv;
                    else if (pp.Method == ThresholdParameters.ThreshMethod.Otsu) type = ThresholdTypes.Otsu | ThresholdTypes.Binary;
                    else if (pp.Method == ThresholdParameters.ThreshMethod.ToZero) type = ThresholdTypes.Tozero;

                    Cv2.Threshold(gray, result, pp.Threshold, pp.MaxVal, type);
                    gray.Dispose();
                    return (true, result, new List<Defect>());
                }
            });

            tools.Add(new ToolDefinition
            {
                Name = "形態學 (Morphology)",
                Category = "01. 預處理",
                DefaultParameters = new MorphologyParameters(),
                Action = (img, p) =>
                {
                    var pp = (MorphologyParameters)p;
                    var result = new Mat();
                    using(var kernel = Cv2.GetStructuringElement(pp.Shape, new Size(pp.KernelSize, pp.KernelSize)))
                    {
                        MorphTypes op = MorphTypes.Open; 
                        // Map enum
                        if (pp.Operation == MorphologyParameters.MorphOp.Erode) op = MorphTypes.Erode;
                        else if (pp.Operation == MorphologyParameters.MorphOp.Dilate) op = MorphTypes.Dilate;
                        else if (pp.Operation == MorphologyParameters.MorphOp.Close) op = MorphTypes.Close;
                        else if (pp.Operation == MorphologyParameters.MorphOp.Gradient) op = MorphTypes.Gradient;

                        Cv2.MorphologyEx(img, result, op, kernel, iterations: pp.Iterations);
                    }
                    return (true, result, new List<Defect>());
                }
            });

            // --- Feature Extraction ---

            tools.Add(new ToolDefinition
            {
                Name = "邊緣偵測 (Edge)",
                Category = "02. 特徵提取",
                DefaultParameters = new EdgeDetectionParameters(),
                Action = (img, p) =>
                {
                    var pp = (EdgeDetectionParameters)p;
                    var result = new Mat();
                    var gray = new Mat();
                    if (img.Channels() == 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
                    else img.CopyTo(gray);

                    if (pp.Method == EdgeDetectionParameters.EdgeMethod.Canny)
                    {
                        Cv2.Canny(gray, result, pp.Threshold1, pp.Threshold2);
                    }
                    else
                    {
                        Cv2.Sobel(gray, result, MatType.CV_8U, (int)pp.Threshold1, (int)pp.Threshold2, ksize: pp.ApertureSize);
                    }
                    gray.Dispose();
                    return (true, result, new List<Defect>());
                }
            });

            return tools;
        }
    }
}
