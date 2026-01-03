using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>通用缺陷檢測參數</summary>
	public class DefectDetectionParameters
	{
		/// <summary>缺陷分類類型</summary>
		public enum DefectType
		{
			/// <summary>不限類型</summary>
			Any,
			/// <summary>刮傷</summary>
			Scratch,
			/// <summary>污點</summary>
			Stain,
			/// <summary>凹陷</summary>
			Dent,
			/// <summary>缺件 (漏打)</summary>
			Missing,
			/// <summary>多件 (多餘物件)</summary>
			Extra,
		}

		/// <summary>缺陷檢測運算模式</summary>
		public enum DetectionMode
		{
			/// <summary>模板差異比對 (最常用)</summary>
			TemplateDiff,
			/// <summary>基於邊緣的異常檢測</summary>
			EdgeBased,
			/// <summary>基於色彩的異常檢測</summary>
			ColorBased,
		}

		/// <summary>選擇缺陷檢測模式</summary>
		[DisplayName("檢測模式")] [Description("缺陷檢測的演算法模式:\\n- TemplateDiff: 與參考影像差異比對\\n- EdgeBased: 邊緣異常檢測\\n- ColorBased: 色彩異常檢測")]
		public DetectionMode Mode { get; set; } = DetectionMode.TemplateDiff;

		/// <summary>存放無缺陷標準樣本影像的路徑</summary>
		[DisplayName("參考樣本路徑")] [Description("無缺陷的標準參考影像路徑")]
		public string ReferenceSamplePath { get; set; } = "";

		/// <summary>最小缺陷判定面積 (px2)</summary>
		[DisplayName("最小缺陷面積 (px²)")] [Description("過濾小於此面積的缺陷")]
		public double MinDefectArea { get; set; } = 50;

		/// <summary>最大缺陷判定面積 (px2)，設為 0 表示不限制</summary>
		[DisplayName("最大缺陷面積 (px²)")] [Description("過濾大於此面積的缺陷。設為 0 表示不限制。")]
		public double MaxDefectArea { get; set; } = 0;

		/// <summary>影像差異比對的靈敏度閾值 (0~255)</summary>
		[DisplayName("差異閾值 (0-255)")] [Description("差異影像的二值化閾值")]
		public double DifferenceThreshold { get; set; } = 30;

		/// <summary>是否在輸出影像上高亮框選缺陷</summary>
		[DisplayName("高亮顯示缺陷")] [Description("在輸出影像上以紅色框標示缺陷位置")]
		public bool HighlightDefects { get; set; } = true;

		/// <summary>欲檢測的缺陷類型過濾</summary>
		[DisplayName("缺陷類型標註")] [Description("指定要檢測的缺陷類型")]
		public DefectType TypeFilter { get; set; } = DefectType.Any;

		/// <summary>是否匯出缺陷統計報告至 Log</summary>
		[DisplayName("輸出缺陷報告")] [Description("將缺陷統計資訊輸出到 Log")]
		public bool OutputDefectReport { get; set; } = true;

		/// <summary>是否將缺陷清單匯出為 CSV 檔案</summary>
		[DisplayName("缺陷座標輸出到 CSV")] [Description("將所有缺陷的座標與資訊輸出為 CSV")]
		public bool OutputToCsv { get; set; } = false;

		/// <summary>缺陷報告 CSV 的儲存路徑</summary>
		[DisplayName("CSV 輸出路徑")] [Description("CSV 檔案的儲存路徑")]
		public string CsvOutputPath { get; set; } = "defect_report.csv";
	}
}
