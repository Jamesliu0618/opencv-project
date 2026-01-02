using NUnit.Framework;
using System.IO;
using PCBInspection.Drivers;
using PCBInspection.Core;

namespace PCBInspection.Tests.Integration
{
    [TestFixture]
    public class PipelineSmokeTests
    {
        [Test]
        public void Pipeline_RunOnce_GeneratesReportAndAnnotatedImage()
        {
            var artifacts = Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts");
            Directory.CreateDirectory(artifacts);
            var fixtures = Path.Combine(TestContext.CurrentContext.WorkDirectory, "fixtures");
            Directory.CreateDirectory(fixtures);

            var mockCamera = new MockCamera(fixtures);
            var mockIoLog = Path.Combine(artifacts, "mock_io.log");
            if (File.Exists(mockIoLog)) File.Delete(mockIoLog);
            var mockIo = new MockIo(mockIoLog);
            mockIo.Initialize();

            var pipeline = new Pipeline(mockCamera, mockIo, artifacts);
            var result = pipeline.RunOnce("test-pcb");

            Assert.IsTrue(File.Exists(result.AnnotatedImagePath));
            Assert.IsTrue(File.Exists(result.ReportPath));
            Assert.IsTrue(File.Exists(mockIoLog));
        }
    }
}