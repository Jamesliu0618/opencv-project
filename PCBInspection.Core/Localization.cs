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

    public static class Localization
    {
        public static List<Component> DetectComponents(Mat image, int minArea = 50)
        {
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

                Cv2.GaussianBlur(gray, gray, new Size(5,5), 0);
                Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 15, 7);

                Cv2.FindContours(gray, out var contours, out var hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                int idx = 0;
                foreach (var c in contours)
                {
                    var area = Cv2.ContourArea(c);
                    if (area < minArea) continue;
                    var rect = Cv2.MinAreaRect(c);
                    var center = rect.Center;
                    var size = rect.Size;
                    var angle = rect.Angle;

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