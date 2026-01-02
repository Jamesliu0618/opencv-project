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
        public int MinArea { get; set; } = 50;
        public int BlurKernel { get; set; } = 5; // must be odd
        public int AdaptiveBlockSize { get; set; } = 15; // must be odd
        public int AdaptiveC { get; set; } = 7;
        public int MorphKernel { get; set; } = 3;
        public bool UseMorphClose { get; set; } = true;
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