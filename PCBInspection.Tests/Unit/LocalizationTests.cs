using NUnit.Framework;
using OpenCvSharp;
using PCBInspection.Core;
using System.Linq;

namespace PCBInspection.Tests.Unit
{
    [TestFixture]
    public class LocalizationTests
    {
        [Test]
        public void DetectComponents_TwoRectangles_ReturnsTwoComponentsWithCorrectPositions()
        {
            // synthetic image 500x200
            var img = new Mat(new Size(500, 200), MatType.CV_8UC3, Scalar.All(255));

            // draw two filled rectangles
            var rect1 = new Rect(80, 90, 40, 20); // center at (100,100)
            var rect2 = new Rect(200, 90, 40, 20); // center at (220,100)
            Cv2.Rectangle(img, rect1, Scalar.Black, -1);
            Cv2.Rectangle(img, rect2, Scalar.Black, -1);

            Calibration.SetManualScale(10.0); // 10 px per mm

            var comps = Localization.DetectComponents(img, new LocalizationOptions { MinArea = 10 });
            Assert.AreEqual(2, comps.Count);

            var c0 = comps[0];
            var c1 = comps[1];

            Assert.AreEqual(100, (int)System.Math.Round(c0.CenterX_Px));
            Assert.AreEqual(100, (int)System.Math.Round(c0.CenterY_Px));

            Assert.AreEqual(220, (int)System.Math.Round(c1.CenterX_Px));
            Assert.AreEqual(100, (int)System.Math.Round(c1.CenterY_Px));

            // mm coordinates
            var (x0mm, y0mm) = Calibration.PixelToMm(c0.CenterX_Px, c0.CenterY_Px);
            Assert.AreEqual(10.0, x0mm, 0.1); // 100px / 10px per mm = 10mm
            Assert.AreEqual(10.0, y0mm, 0.1); // 100px /10 = 10mm
        }
    }
}