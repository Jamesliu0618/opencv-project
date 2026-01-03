using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>物件特徵分析參數</summary>
	public class ObjectAnalysisParameters
	{
		/// <summary>是否計算物件水平外接矩形</summary>
		[DisplayName("計算外接矩形")] [Description("計算並繪製物件的水平外接矩形")]
		public bool ComputeBoundingRect { get; set; } = true;

		/// <summary>是否計算物件最小旋轉外接矩形</summary>
		[DisplayName("計算最小外接矩形")] [Description("計算並繪製物件的最小旋轉外接矩形")]
		public bool ComputeMinAreaRect { get; set; } = true;

		/// <summary>是否計算物件最小外接圓</summary>
		[DisplayName("計算外接圓")] [Description("計算並繪製物件的最小外接圓")]
		public bool ComputeMinEnclosingCircle { get; set; } = false;

		/// <summary>是否計算物件凸包廓</summary>
		[DisplayName("計算凸包")] [Description("計算並繪製物件的凸包輪廓")]
		public bool ComputeConvexHull { get; set; } = false;

		/// <summary>是否計算圓度、矩形度等細節形狀特徵</summary>
		[DisplayName("計算形狀特徵")] [Description("計算面積、周長、圓度、長寬比、矩形度等形狀特徵")]
		public bool ComputeShapeFeatures { get; set; } = true;

		/// <summary>是否在影像上標註物件序號 (Index)</summary>
		[DisplayName("標註物件編號")] [Description("在每個物件上標註序號")]
		public bool LabelObjectIndex { get; set; } = true;

		/// <summary>是否將分析結果匯出為 DataTable</summary>
		[DisplayName("輸出結果到資料表")] [Description("將分析結果輸出為 DataTable 格式")]
		public bool OutputToDataTable { get; set; } = true;

		/// <summary>面積過濾下界 (px2)</summary>
		[DisplayName("最小面積過濾 (px²)")] [Description("過濾面積小於此值的物件")]
		public double FilterMinArea { get; set; } = 100;

		/// <summary>面積過濾上界 (px2)，設為 0 表示不限制</summary>
		[DisplayName("最大面積過濾 (px²)")] [Description("過濾面積大於此值的物件。設為 0 表示不限制。")]
		public double FilterMaxArea { get; set; } = 0;
	}
}
