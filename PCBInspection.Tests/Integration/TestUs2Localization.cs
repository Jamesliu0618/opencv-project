using NUnit.Framework;
using System.IO;
using PCBInspection.Core;
using PCBInspection.Drivers;
using System.Linq;

namespace PCBInspection.Tests.Integration
{
    [TestFixture]
    public class TestUs2Localization
    {
        [Test]
        public void LocalizationAndMeasurement_PrecisionTest_T027()
        {
            var fixtures = Path.Combine(TestContext.CurrentContext.WorkDirectory, "fixtures");
            Directory.CreateDirectory(fixtures);

            // Create deterministic fixtures
            var chessPath = Path.Combine(fixtures, "calib_chess.png");
            var patternCols = 7;
            var patternRows = 5;
            var squarePx = 20; // 20 px per square => pixelsPerMm later set by ComputeCalibration / squareSize
            FixtureGenerator.CreateChessboard(chessPath, patternCols, patternRows, squarePx);

            // Create synthetic PCB with two rectangles at known pixels
            var pcbPath = Path.Combine(fixtures, "pcb_two_components.png");
            // components at centers 100 and 220 px on X (same as unit tests)
            FixtureGenerator.CreatePcbWithTwoComponents(pcbPath);

            // Compute calibration: since squarePx = 20 px and we declare squareSizeMm = 2.0 mm
            // pixelsPerMm expected = 20 / 2.0 = 10 px/mm
            var squareSizeMm = 2.0;
            Calibration.ComputeCalibration(new[] { chessPath }, patternCols, patternRows, squareSizeMm);

            // Validate calibration mapping at known point (100px,100px) -> mm = 100 / 10 = 10 mm
            var (xm, ym) = Calibration.PixelToMm(100, 100);
            Assert.AreEqual(10.0, xm, 0.01);
            Assert.AreEqual(10.0, ym, 0.01);

            // Run pipeline using mock camera reading from fixtures
            var artifacts = Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts");
            Directory.CreateDirectory(artifacts);
            var mockCamera = new MockCamera(fixtures);
            var mockIoLog = Path.Combine(artifacts, "mock_io.log");
            if (File.Exists(mockIoLog)) File.Delete(mockIoLog);
            var mockIo = new MockIo(mockIoLog);
            mockIo.Initialize();

            var pipeline = new Pipeline(mockCamera, mockIo, artifacts);
            var result = pipeline.RunOnce("us2-test-pcb");

            // Ensure components detected
            Assert.IsNotNull(result.Components);
            Assert.IsTrue(result.Components.Count >= 2);

            // Validate measurement accuracy for first two components
            var comps = result.Components.Take(2).ToArray();
            var a = comps[0];
            var b = comps[1];

            // widths in px = 40 px -> with 10 px/mm => 4.0 mm expected
            Assert.AreEqual(4.0, a.WidthMm, 0.05); // measurement error tolerance ±0.05 mm
            Assert.AreEqual(4.0, b.WidthMm, 0.05);

            // center distance: centers at 100 and 220 px -> dx=120 px -> 12.0 mm
            var centerDist = Measurement.DistanceBetweenCentersMm(a, b);
            Assert.AreEqual(12.0, centerDist, 0.1); // precision tolerance ±0.1 mm
        }
    }
}