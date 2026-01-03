using NUnit.Framework;
using PCBInspection.Core;

namespace PCBInspection.Tests.Unit
{
	[TestFixture]
	public class CalibrationTests
	{
		[Test] public void GetPixelToMm_Default_ReturnsMatrix()
		{
			var m = Calibration.GetPixelToMm();
			Assert.IsNotNull(m);
			Assert.AreEqual(3, m.GetLength(0));
			Assert.AreEqual(3, m.GetLength(1));
		}
	}
}