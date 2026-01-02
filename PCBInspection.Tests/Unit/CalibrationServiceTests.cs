using NUnit.Framework;
using System.IO;
using PCBInspection.Core.Services;
using PCBInspection.Core;

namespace PCBInspection.Tests.Unit
{
    [TestFixture]
    public class CalibrationServiceTests
    {
        [Test]
        public void SaveAndLoadCalibration_WorksAndAppliesScale()
        {
            var tmp = Path.Combine(TestContext.CurrentContext.WorkDirectory, "temp_calib.json");
            if (File.Exists(tmp)) File.Delete(tmp);

            // set manual scale then save
            Calibration.SetManualScale(12.5);
            CalibrationService.SaveCalibration(tmp);
            Assert.IsTrue(File.Exists(tmp));

            // modify scale to something else and load
            Calibration.SetManualScale(1.0);
            var meta = CalibrationService.LoadCalibration(tmp);
            Assert.AreEqual(12.5, meta.PixelsPerMm, 1e-6);

            // ensure runtime calibration updated
            var (xmm, _) = Calibration.PixelToMm(125, 0); // 125 px at 12.5 px/mm => 10 mm
            Assert.AreEqual(10.0, xmm, 1e-3);
        }
    }
}