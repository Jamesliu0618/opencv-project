using NUnit.Framework;
using System.IO;
using OpenCvSharp;
using PCBInspection.Core;
using System.Linq;

namespace PCBInspection.Tests.Integration
{
    [TestFixture]
    public class TestUs2TuningMeasurement
    {
        [Test]
        public void RotatedComponents_MeasurementAccuracy()
        {
            var fixtures = Path.Combine(TestContext.CurrentContext.WorkDirectory, "fixtures_tuning");
            Directory.CreateDirectory(fixtures);

            var imgPath = Path.Combine(fixtures, "rotated_two.png");
            using (var img = new Mat(new Size(800, 600), MatType.CV_8UC3, Scalar.White))
            {
                var box1 = new RotatedRect(new Point2f(200, 200), new Size2f(80, 40), 15);
                var box2 = new RotatedRect(new Point2f(400, 200), new Size2f(80, 40), -10);
                var p1 = box1.Points();
                var p2 = box2.Points();
                Cv2.FillConvexPoly(img, p1.Select(p=>new Point((int)System.Math.Round(p.X),(int)System.Math.Round(p.Y))).ToArray(), Scalar.Black);
                Cv2.FillConvexPoly(img, p2.Select(p=>new Point((int)System.Math.Round(p.X),(int)System.Math.Round(p.Y))).ToArray(), Scalar.Black);
                Cv2.ImWrite(imgPath, img);
            }

            // chess calibration
            var chess = Path.Combine(fixtures, "calib_chess.png");
            FixtureGenerator.CreateChessboard(chess, 7, 5, 20);
            Calibration.ComputeCalibration(new[] { chess }, 7, 5, 2.0);

            var imgMat = Cv2.ImRead(imgPath);
            var opts = new LocalizationOptions { MinArea = 100, UseMomentsForCentroid = true, MorphKernel = 5 };
            var comps = Localization.DetectComponents(imgMat, opts);
            Assert.AreEqual(2, comps.Count);

            // width in px approx 80 -> at 10 px/mm => 8 mm
            foreach (var c in comps)
            {
                Assert.AreEqual(8.0, Measurement.ComponentSizeMm(c).Item1, 0.1);
            }
        }
    }
}