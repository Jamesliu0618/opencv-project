using System.Collections.Generic;
using OpenCvSharp;
using System.Linq;

namespace PCBInspection.Core
{
    public class Component
    {
        public string Id { get; set; }
        public double CenterX_Px { get; set; }
        public double CenterY_Px { get; set; }
        public double CenterX_Mm { get; set; }
        public double CenterY_Mm { get; set; }
        public double AngleDeg { get; set; }
        public double SizeW_Px { get; set; }
        public double SizeH_Px { get; set; }
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
    }

    public class LocalizationOptions
    {
        [System.ComponentModel.DisplayName("最小面積 (px)")]
        [System.ComponentModel.Description("過濾小於此面積的輪廓。\n單位: 像素 (Pixel)")]
        public int MinArea { get; set; } = 50;

        [System.ComponentModel.DisplayName("高斯模糊核 (px)")]
        [System.ComponentModel.Description("預處理的模糊半徑，必須為奇數。\n單位: 像素 (Pixel)")]
        public int BlurKernel { get; set; } = 5; // must be odd

        [System.ComponentModel.DisplayName("自適應區塊大小 (px)")]
        [System.ComponentModel.Description("自適應閾值的區塊大小，必須為奇數。\n單位: 像素 (Pixel)")]
        public int AdaptiveBlockSize { get; set; } = 15; // must be odd

        [System.ComponentModel.DisplayName("自適應常數 C")]
        [System.ComponentModel.Description("自適應閾值的常數，值越大閾值越低 (越不易被選中)。")]
        public int AdaptiveC { get; set; } = 7;

        [System.ComponentModel.DisplayName("形態學核大小 (px)")]
        [System.ComponentModel.Description("形態學操作的結構元素大小。\n單位: 像素 (Pixel)")]
        public int MorphKernel { get; set; } = 3;

        [System.ComponentModel.DisplayName("使用閉運算")]
        [System.ComponentModel.Description("是否使用形態學閉運算來連接斷裂的輪廓。\nTrue: 啟用, False: 停用")]
        public bool UseMorphClose { get; set; } = true;

        [System.ComponentModel.DisplayName("使用矩計算中心")]
        [System.ComponentModel.Description("使用影像矩 (Moments) 計算更精確的質心。\nTrue: 使用矩, False: 使用邊界框中心")]
        public bool UseMomentsForCentroid { get; set; } = true;
    }

    public static class Localization
    {
        public static List<Component> DetectComponents(Mat image, LocalizationOptions opts = null)
        {
            if (opts == null) opts = new LocalizationOptions();
            var components = new List<Component>();
            Mat gray = null;
            try
            {
                if (image.Channels() == 3)
                {
                    gray = new Mat();
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                }
                else
                {
                    gray = image.Clone();
                }

                var blurSize = (opts.BlurKernel % 2 == 1) ? opts.BlurKernel : opts.BlurKernel + 1;
                Cv2.GaussianBlur(gray, gray, new Size(blurSize, blurSize), 0);

                var block = (opts.AdaptiveBlockSize % 2 == 1) ? opts.AdaptiveBlockSize : opts.AdaptiveBlockSize + 1;
                Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, block, opts.AdaptiveC);

                if (opts.UseMorphClose)
                {
                    var morphSize = opts.MorphKernel > 0 ? opts.MorphKernel : 3;
                    var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(morphSize, morphSize));
                    Cv2.MorphologyEx(gray, gray, MorphTypes.Close, kernel);
                }

                Cv2.FindContours(gray, out var contours, out var hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                int idx = 0;
                foreach (var c in contours)
                {
                    var area = Cv2.ContourArea(c);
                    if (area < opts.MinArea) continue;

                    var rect = Cv2.MinAreaRect(c);
                    Point2f center;
                    double angle = rect.Angle;
                    Size2f size = rect.Size;

                    if (opts.UseMomentsForCentroid)
                    {
                        var m = Cv2.Moments(c);
                        if (m.M00 != 0)
                        {
                            center = new Point2f((float)(m.M10 / m.M00), (float)(m.M01 / m.M00));
                        }
                        else
                        {
                            center = rect.Center;
                        }
                    }
                    else
                    {
                        center = rect.Center;
                    }

                    var (cxMm, cyMm) = Calibration.PixelToMm(center.X, center.Y);

                    components.Add(new Component
                    {
                        Id = "cmp_" + (idx++),
                        CenterX_Px = center.X,
                        CenterY_Px = center.Y,
                        CenterX_Mm = cxMm,
                        CenterY_Mm = cyMm,
                        AngleDeg = angle,
                        SizeW_Px = size.Width,
                        SizeH_Px = size.Height
                    });
                }
            }
            finally
            {
                gray?.Dispose();
            }

            return components.OrderBy(c => c.CenterX_Px).ThenBy(c => c.CenterY_Px).ToList();
        }
    }
}