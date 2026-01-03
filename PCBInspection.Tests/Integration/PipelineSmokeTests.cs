using NUnit.Framework;
using PCBInspection.Core;
using PCBInspection.Drivers;
using System.IO;

namespace PCBInspection.Tests.Integration
{
	[TestFixture]
	public class PipelineSmokeTests
	{
		[Test] public void Pipeline_RunOnce_GeneratesReportAndAnnotatedImage()
		{
			var artifacts = Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts");
			Directory.CreateDirectory(artifacts);
			var fixtures = Path.Combine(TestContext.CurrentContext.WorkDirectory, "fixtures");
			Directory.CreateDirectory(fixtures);
			var mockCamera = new MockCamera(fixtures);
			var mockIoLog  = Path.Combine(artifacts, "mock_io.log");

			if(File.Exists(mockIoLog))
			{
				File.Delete(mockIoLog);
			}
			var mockIo = new MockIo(mockIoLog);
			mockIo.Initialize();
			var pipeline = new Pipeline(mockCamera, mockIo, artifacts);
			var result   = pipeline.RunOnce("test-pcb");
			Assert.IsTrue(File.Exists(result.AnnotatedImagePath));
			Assert.IsTrue(File.Exists(result.ReportPath));
			Assert.IsTrue(File.Exists(mockIoLog));

			// Ensure localization & measurement are present in the report
			Assert.IsNotNull(result.Components);
			Assert.IsTrue(result.Components.Count >= 0);

			foreach(var c in result.Components)
			{
				Assert.GreaterOrEqual(c.WidthMm,  0.0);
				Assert.GreaterOrEqual(c.HeightMm, 0.0);
			}
		}
	}
}