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
		[Category("01. 匹配設定")]
		[DisplayName("匹配閾值")] [Description("匹配分數閾值 (0-1 for Normed methods)")]
		public double MatchThreshold { get; set; } = 0.8;

		/// <summary>最大匹配數量 (設為 0 表示僅尋找最佳匹配)</summary>
		[Category("01. 匹配設定")]
		[DisplayName("最大匹配數量")] [Description("要尋找的最大匹配數量。設為 0 或 1 僅尋找最佳匹配。")]
		public int MaxMatches { get; set; } = 1;

		// ===== 來源影像 ROI =====

		/// <summary>來源 ROI 編號 (優先使用，設為 0 則使用手動座標)</summary>
		[Category("02. 來源 ROI")]
		[DisplayName("來源 ROI 編號")] [Description("使用指定編號的 ROI 作為搜尋區域 (0 = 不使用 ROI 或使用手動座標)")]
		public int SourceRoiIndex { get; set; } = 0;

		/// <summary>是否使用手動來源 ROI</summary>
		[Category("02. 來源 ROI")]
		[DisplayName("啟用手動 ROI")] [Description("若 ROI 編號為 0，可手動輸入搜尋區域座標")]
		public bool UseManualSourceRoi { get; set; } = false;

		/// <summary>來源 ROI 左上角 X</summary>
		[Category("02. 來源 ROI")]
		[DisplayName("手動 ROI X")] [Description("搜尋區域左上角 X 座標")]
		public int SourceRoiX { get; set; } = 0;

		/// <summary>來源 ROI 左上角 Y</summary>
		[Category("02. 來源 ROI")]
		[DisplayName("手動 ROI Y")] [Description("搜尋區域左上角 Y 座標")]
		public int SourceRoiY { get; set; } = 0;

		/// <summary>來源 ROI 寬度</summary>
		[Category("02. 來源 ROI")]
		[DisplayName("手動 ROI 寬度")] [Description("搜尋區域寬度 (0 = 使用全圖)")]
		public int SourceRoiWidth { get; set; } = 0;

		/// <summary>來源 ROI 高度</summary>
		[Category("02. 來源 ROI")]
		[DisplayName("手動 ROI 高度")] [Description("搜尋區域高度 (0 = 使用全圖)")]
		public int SourceRoiHeight { get; set; } = 0;

		// ===== 模板 ROI =====

		/// <summary>模板 ROI 編號 (優先使用，設為 0 則使用手動座標)</summary>
		[Category("03. 模板 ROI")]
		[DisplayName("模板 ROI 編號")] [Description("使用指定編號的 ROI 擷取模板 (0 = 使用完整模板或手動座標)")]
		public int TemplateRoiIndex { get; set; } = 0;

		/// <summary>是否使用手動模板 ROI</summary>
		[Category("03. 模板 ROI")]
		[DisplayName("啟用模板 ROI")] [Description("僅使用模板影像的指定區域")]
		public bool UseManualTemplateRoi { get; set; } = false;

		/// <summary>模板 ROI 左上角 X</summary>
		[Category("03. 模板 ROI")]
		[DisplayName("模板 ROI X")] [Description("模板擷取區域左上角 X 座標")]
		public int TemplateRoiX { get; set; } = 0;

		/// <summary>模板 ROI 左上角 Y</summary>
		[Category("03. 模板 ROI")]
		[DisplayName("模板 ROI Y")] [Description("模板擷取區域左上角 Y 座標")]
		public int TemplateRoiY { get; set; } = 0;

		/// <summary>模板 ROI 寬度</summary>
		[Category("03. 模板 ROI")]
		[DisplayName("模板 ROI 寬度")] [Description("模板擷取區域寬度")]
		public int TemplateRoiWidth { get; set; } = 0;

		/// <summary>模板 ROI 高度</summary>
		[Category("03. 模板 ROI")]
		[DisplayName("模板 ROI 高度")] [Description("模板擷取區域高度")]
		public int TemplateRoiHeight { get; set; } = 0;
	}
}
