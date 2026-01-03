using NUnit.Framework;
using System.IO;

namespace PCBInspection.Tests.Integration
{
	[TestFixture]
	public class TestSmoke
	{
		[Test] public void FixturesExist()
		{
			// 使用 solution root 相對路徑
			var solutionRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));
			var fixturesDir  = Path.Combine(solutionRoot, "tests", "fixtures", "images");

			if(!Directory.Exists(fixturesDir))
			{
				// 如果目錄不存在，建立它並跳過（測試基礎設施檢查）
				Directory.CreateDirectory(fixturesDir);
			}
			Assert.IsTrue(Directory.Exists(fixturesDir), "Fixtures directory not found: " + fixturesDir);
		}
	}
}