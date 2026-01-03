using NUnit.Framework;
using OpenCvSharp;
using PCBInspection.Core;
using System;
using System.Linq;

namespace PCBInspection.Tests.Unit
{
	[TestFixture]
	public class LocalizationTuningTests
	{
		[Test] public void DetectRotatedComponent_WithMoments_ReturnsCentroidCloseToTrue()
		{
			var img = new Mat(new Size(400, 300), MatType.CV_8UC3, Scalar.White);

			// draw rotated rectangle
			var box    = new RotatedRect(new Point2f(200, 150), new Size2f(80, 40), 30);
			var pts    = box.Points();
			var ptsInt = pts.Select(p => new Point((int)Math.Round(p.X), (int)Math.Round(p.Y))).ToArray();
			Cv2.FillConvexPoly(img, ptsInt, Scalar.Black);
			Calibration.SetManualScale(10.0); // 10 px/mm for mm checks
			var opts  = new LocalizationOptions { MinArea = 100, UseMomentsForCentroid = true };
			var comps = Localization.DetectComponents(img, opts);
			Assert.IsTrue(comps.Count >= 1);
			var c = comps[0];
			Assert.AreEqual(200, (int)Math.Round(c.CenterX_Px));
			Assert.AreEqual(150, (int)Math.Round(c.CenterY_Px));
		}

		[Test] public void DetectComponents_WithNoise_ClosingRecoversComponents()
		{
			var img = new Mat(new Size(400, 300), MatType.CV_8UC3, Scalar.White);

			// draw two rectangles with small gaps (simulate noise holes)
			var r1 = new Rect(80,  90, 40, 20);
			var r2 = new Rect(200, 90, 40, 20);
			Cv2.Rectangle(img, r1, Scalar.Black, -1);
			Cv2.Rectangle(img, r2, Scalar.Black, -1);

			// add small white speckles inside to create holes
			Cv2.Circle(img, new Point(90,  100), 2, Scalar.White, -1);
			Cv2.Circle(img, new Point(210, 100), 2, Scalar.White, -1);
			var optsNoMorph = new LocalizationOptions { MinArea = 10, UseMorphClose = false };
			var noClose     = Localization.DetectComponents(img, optsNoMorph);
			Assert.IsTrue(noClose.Count >= 2); // still find two but centers may be off
			var optsClose = new LocalizationOptions { MinArea = 10, UseMorphClose = true, MorphKernel = 5 };
			var withClose = Localization.DetectComponents(img, optsClose);
			Assert.IsTrue(withClose.Count >= 2);
		}
	}
}