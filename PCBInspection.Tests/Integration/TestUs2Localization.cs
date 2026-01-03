using NUnit.Framework;
using OpenCvSharp;
using PCBInspection.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCBInspection.Tests.Integration
{
	[TestFixture]
	public class TestUs2Localization
	{
		[Test] public void LocalizationAndMeasurement_PrecisionTest_T027()
		{
			var fixtures = Path.Combine(TestContext.CurrentContext.WorkDirectory, "fixtures");
			Directory.CreateDirectory(fixtures);

			// Create deterministic fixtures
			var chessPath   = Path.Combine(fixtures, "calib_chess.png");
			var patternCols = 7;
			var patternRows = 5;
			var squarePx    = 20; // 20 px per square => pixelsPerMm later set by ComputeCalibration / squareSize
			FixtureGenerator.CreateChessboard(chessPath, patternCols, patternRows, squarePx);

			// Create synthetic PCB with two rectangles at known pixels
			var pcbPath = Path.Combine(fixtures, "pcb_two_components.png");

			// components at centers 100 and 220 px on X (same as unit tests)
			FixtureGenerator.CreatePcbWithTwoComponents(pcbPath);

			// 使用手動設定校正值確保測試一致性
			// 10 px/mm => 40 px width = 4.0 mm
			Calibration.SetManualScale(10.0);

			// Validate calibration mapping at known point (100px,100px) -> mm = 100 / 10 = 10 mm
			(double xm, double ym) = Calibration.PixelToMm(100, 100);
			Assert.AreEqual(10.0, xm, 0.01);
			Assert.AreEqual(10.0, ym, 0.01);

			// 直接讀取 PCB 圖片進行定位測試
			using(Mat pcbImage = Cv2.ImRead(pcbPath))
			{
				List<Component> comps = Localization.DetectComponents(pcbImage, new LocalizationOptions { MinArea = 50 }).OrderBy(c => c.CenterX_Px).ToList();

				// Ensure components detected
				Assert.IsNotNull(comps);
				Assert.IsTrue(comps.Count >= 2, $"Expected at least 2 components, got {comps.Count}");

				// 取前兩個元件
				Component a = comps[0];
				Component b = comps[1];

				// 計算尺寸（mm）- 使用 Max 取較長邊
				(double aWmm, double aHmm) = Measurement.ComponentSizeMm(a);
				(double bWmm, double bHmm) = Measurement.ComponentSizeMm(b);
				double aMaxDim = Math.Max(aWmm, aHmm);
				double bMaxDim = Math.Max(bWmm, bHmm);

				// 較長邊 in px = 40 px -> with 10 px/mm => 4.0 mm expected
				// 由於形態學操作會輕微侵蝕邊緣，容許 ±0.5 mm 誤差
				Assert.AreEqual(4.0, aMaxDim, 0.5, $"Component A max dimension: {aMaxDim}");
				Assert.AreEqual(4.0, bMaxDim, 0.5, $"Component B max dimension: {bMaxDim}");

				// center distance: centers at 100 and 220 px -> dx=120 px -> 12.0 mm
				double centerDist = Measurement.DistanceBetweenCentersMm(a, b);
				Assert.AreEqual(12.0, centerDist, 0.5, $"Center distance: {centerDist}"); // precision tolerance ±0.5 mm
			}
		}
	}
}