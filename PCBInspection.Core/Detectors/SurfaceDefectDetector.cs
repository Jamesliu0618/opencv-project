namespace PCBInspection.Core.Detectors
{
    using PCBInspection.Core.Models;
    using OpenCvSharp;
    using System.Collections.Generic;

    /// <summary>
    /// 表面瑕疵偵測器 - 使用形態學操作偵測刮痕、污染、氧化
    /// </summary>
    public class SurfaceDefectDetector : IDefectDetector
    {
        public string Name => "SurfaceDefectDetector";

        private readonly int _blurSize;
        private readonly double _thresholdValue;
        private readonly int _minDefectArea;

        public SurfaceDefectDetector(int blurSize = 5, double threshold = 30, int minArea = 100)
        {
            _blurSize = blurSize;
            _thresholdValue = threshold;
            _minDefectArea = minArea;
        }

        public List<Defect> Detect(Mat image, List<Component> components = null)
        {
            var defects = new List<Defect>();
            if (image == null || image.Empty()) return defects;

            using (var gray = new Mat())
            using (var blurred = new Mat())
            using (var diff = new Mat())
            using (var thresh = new Mat())
            {
                if (image.Channels() == 3)
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                else
                    image.CopyTo(gray);

                // 使用中值濾波去除椒鹽雜訊
                Cv2.MedianBlur(gray, blurred, _blurSize);

                // 計算高通濾波結果以偵測表面紋理異常
                Cv2.Subtract(gray, blurred, diff);
                Cv2.ConvertScaleAbs(diff, diff);

                // 閾值分割
                Cv2.Threshold(diff, thresh, _thresholdValue, 255, ThresholdTypes.Binary);

                // 形態學閉運算連接相鄰瑕疵
                using (var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
                {
                    Cv2.MorphologyEx(thresh, thresh, MorphTypes.Close, kernel);
                }

                Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                int idx = 0;
                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    if (area < _minDefectArea) continue;

                    var rect = Cv2.BoundingRect(contour);
                    var defectInfo = ClassifySurfaceDefect(contour, area, rect);

                    defects.Add(new Defect
                    {
                        Id = $"surface_{idx++}",
                        Type = defectInfo.Item1,
                        Severity = defectInfo.Item2,
                        Confidence = 0.8,
                        BoundingBox = new int[] { rect.X, rect.Y, rect.Width, rect.Height }
                    });
                }
            }

            return defects;
        }

        private (string, int) ClassifySurfaceDefect(Point[] contour, double area, Rect rect)
        {
            var aspectRatio = (double)rect.Width / System.Math.Max(rect.Height, 1);

            // 長條形 (高 aspect ratio) => 刮痕
            if (aspectRatio > 5 || aspectRatio < 0.2)
                return ("SCRATCH", 3);

            // 大面積塊狀 => 污染
            if (area > 500)
                return ("CONTAMINATION", 3);

            // 小面積斑點 => 氧化/腐蝕
            return ("OXIDATION", 2);
        }
    }
}
