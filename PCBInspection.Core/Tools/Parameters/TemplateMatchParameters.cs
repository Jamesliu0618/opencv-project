using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>模板匹配參數</summary>
	public class TemplateMatchParameters
	{
		/// <summary>匹配演算法類型</summary>
		public enum MatchMethod
		{
			/// <summary>平方差匹配</summary>
			SqDiff,
			/// <summary>正規化平方差匹配</summary>
			SqDiffNormed,
			/// <summary>相關性匹配</summary>
			CCorr,
			/// <summary>正規化相關性匹配</summary>
			CCorrNormed,
			/// <summary>相關係數匹配</summary>
			CCoeff,
			/// <summary>正規化相關係數匹配 (推薦)</summary>
			CCoeffNormed,
		}

		/// <summary>選擇匹配演算法</summary>
		[DisplayName("匹配方法")] [Description("- SqDiff: 平方差 (越小越好)\n- CCorr: 相關性 (越大越好)\n- CCoeff: 相關係數 (越大越好)\n- *Normed: 正規化版本")]
		public MatchMethod Method { get; set; } = MatchMethod.CCoeffNormed;

		/// <summary>模板影像之磁碟路徑</summary>
		[DisplayName("模板路徑")] [Description("模板影像檔案的完整路徑")]
		public string TemplatePath { get; set; } = "";

		/// <summary>匹配確信度閾值 (適用於 Normed 方法，範圍 0-1)</summary>
		[DisplayName("匹配閾值")] [Description("匹配分數閾值 (0-1 for Normed methods)")]
		public double MatchThreshold { get; set; } = 0.8;
	}
}
