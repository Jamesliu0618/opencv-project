using NUnit.Framework;
using System.IO;

namespace PCBInspection.Tests.Integration
{
    [TestFixture]
    public class TestSmoke
    {
        [Test]
        public void FixturesExist()
        {
            var fixturesDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "tests", "fixtures", "images");
            Assert.IsTrue(Directory.Exists(fixturesDir) || Directory.Exists(Path.GetFullPath(fixturesDir)), "Fixtures directory not found: " + fixturesDir);
        }
    }
}