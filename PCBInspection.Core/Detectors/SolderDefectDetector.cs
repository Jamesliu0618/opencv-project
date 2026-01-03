namespace PCBInspection.Core.Detectors
{
    using PCBInspection.Core.Models;
    using OpenCvSharp;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 焊點瑕疵偵測器 - 使用形態學與輪廓分析偵測冷焊、虛焊、漏焊、短路
    /// </summary>
    public class SolderDefectDetector : IDefectDetector
    {
        public string Name => "SolderDefectDetector";

        private readonly double _minSolderArea;
        private readonly double _maxSolderArea;
        private readonly int _morphKernel;

        public SolderDefectDetector(double minArea = 50, double maxArea = 5000, int morphKernel = 3)
        {
            _minSolderArea = minArea;
            _maxSolderArea = maxArea;
            _morphKernel = morphKernel;
        }

        public List<Defect> Detect(Mat image, List<Component> components = null)
        {
            var defects = new List<Defect>();
            if (image == null || image.Empty()) return defects;

            using (var gray = new Mat())
            using (var thresh = new Mat())
            {
                if (image.Channels() == 3)
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                else
                    image.CopyTo(gray);

                // 使用自適應閾值偵測焊點區域
                Cv2.AdaptiveThreshold(gray, thresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 5);

                // 形態學開運算去除雜訊
                using (var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(_morphKernel, _morphKernel)))
                {
                    Cv2.MorphologyEx(thresh, thresh, MorphTypes.Open, kernel);
                }

                Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                int idx = 0;
                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    if (area < _minSolderArea || area > _maxSolderArea) continue;

                    var rect = Cv2.BoundingRect(contour);
                    var circularity = CalculateCircularity(contour, area);

                    // 根據形態分析判斷瑕疵類型
                    var defectType = AnalyzeSolderDefect(area, circularity, rect);
                    if (defectType != null)
                    {
                        defects.Add(new Defect
                        {
                            Id = $"solder_{idx++}",
                            Type = defectType.Value.Item1,
                            Severity = defectType.Value.Item2,
                            Confidence = circularity,
                            BoundingBox = new int[] { rect.X, rect.Y, rect.Width, rect.Height }
                        });
                    }
                }
            }

            return defects;
        }

        private double CalculateCircularity(Point[] contour, double area)
        {
            var perimeter = Cv2.ArcLength(contour, true);
            if (perimeter == 0) return 0;
            return 4 * Math.PI * area / (perimeter * perimeter);
        }

        private (string, int)? AnalyzeSolderDefect(double area, double circularity, Rect rect)
        {
            // 高圓形度 (>0.7) 可能是正常焊點，跳過
            if (circularity > 0.7) return null;

            // 低圓形度 + 大面積 => 可能短路 (severity 4-5)
            if (circularity < 0.3 && area > 1000)
                return ("SHORT_CIRCUIT", 5);

            // 長條形 (aspect ratio > 3) => 可能橋接
            var aspectRatio = (double)rect.Width / Math.Max(rect.Height, 1);
            if (aspectRatio > 3 || aspectRatio < 0.33)
                return ("BRIDGE", 4);

            // 小面積 + 低圓形度 => 冷焊/虛焊
            if (area < 200 && circularity < 0.5)
                return ("COLD_SOLDER", 3);

            // 中等異常
            if (circularity < 0.5)
                return ("SOLDER_ANOMALY", 2);

            return null;
        }
    }
}
