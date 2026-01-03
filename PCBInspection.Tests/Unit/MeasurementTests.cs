using NUnit.Framework;
using PCBInspection.Core;

namespace PCBInspection.Tests.Unit
{
	[TestFixture]
	public class MeasurementTests
	{
		[Test] public void ComputeMeasurements_FromSyntheticComponents_ReturnsExpectedMmValues()
		{
			// create two synthetic components
			var a = new Component { CenterX_Px = 100, CenterY_Px = 100, SizeW_Px = 40, SizeH_Px = 20 };
			var b = new Component { CenterX_Px = 220, CenterY_Px = 100, SizeW_Px = 40, SizeH_Px = 20 };
			Calibration.SetManualScale(10.0); // 10 px per mm
			(double aw, double ah) = Measurement.ComponentSizeMm(a);
			Assert.AreEqual(4.0, aw, 0.01);
			Assert.AreEqual(2.0, ah, 0.01);
			double centerDist = Measurement.DistanceBetweenCentersMm(a, b);
			Assert.AreEqual(12.0, centerDist, 0.01);
			double gap = Measurement.GapBetweenComponentsAlongXmm(a, b);
			Assert.AreEqual(8.0, gap, 0.01);
		}
	}
}