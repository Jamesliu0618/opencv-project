using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>霍夫圓形偵測參數</summary>
	public class HoughCirclesParameters
	{
		/// <summary>結果排序基準</summary>
		public enum SortType
		{
			/// <summary>依據確信度 (累加器數值)</summary>
			Confidence,
			/// <summary>依據半徑由小到大</summary>
			SmallestRadius,
			/// <summary>依據半徑由大到小</summary>
			LargestRadius,
			/// <summary>依據 X 軸位置由左至右</summary>
			XPosition,
		}

		// ===== 效能優化選項 =====

		/// <summary>啟用影像預縮放 (大幅提升效能)</summary>
		[Category("效能優化")]
		[DisplayName("啟用預縮放")] [Description("在偵測前先縮小影像，可大幅提升效能 (建議對 2000px 以上影像啟用)")]
		public bool EnablePreResize { get; set; } = false;

		/// <summary>預縮放比例 (0.25-1.0)</summary>
		[Category("效能優化")]
		[DisplayName("預縮放比例")] [Description("縮放比例，例如 0.5 表示縮小為原本的一半 (建議 0.25-0.5)")]
		public double PreResizeScale { get; set; } = 0.5;

		// ===== 偵測參數 =====

		/// <summary>累加器解析度與影像比例 (1 為相同)</summary>
		[DisplayName("dp")] [Description("累加器解析度與影像解析度的反比 (1 = 相同解析度，2 = 一半解析度加速運算)")]
		public double Dp { get; set; } = 1.5;

		/// <summary>偵測到圓心間的最小距離</summary>
		[DisplayName("最小圓心距離 (px)")] [Description("偵測到的圓心之間的最小距離 (越大越快)")]
		public double MinDist { get; set; } = 60;

		/// <summary>內部 Canny 高閾值</summary>
		[DisplayName("Canny 高閾值")] [Description("內部 Canny 邊緣偵測的高閾值")]
		public double Param1 { get; set; } = 100;

		/// <summary>累加器閾值，越小越容易偵測到圓形</summary>
		[DisplayName("累加器閾值")] [Description("圓心累加器閾值，越大越快但可能漏檢 (建議 40-80)")]
		public double Param2 { get; set; } = 50;

		/// <summary>最小偵測半徑 (像素)</summary>
		[DisplayName("最小半徑 (px)")] [Description("限制搜尋的半徑下界 (設定越精確越快)")]
		public int MinRadius { get; set; } = 10;

		/// <summary>最大偵測半徑 (像素)</summary>
		[DisplayName("最大半徑 (px)")] [Description("限制搜尋的半徑上界 (設定越精確越快)")]
		public int MaxRadius { get; set; } = 100;

		/// <summary>輸出數量限制，設為 0 表示不限制</summary>
		[DisplayName("最大圓形數量")] [Description("限制輸出的圓形個數，提早結束可加速。設為 0 表示不限制。")]
		public int MaxCircles { get; set; } = 10;

		/// <summary>輸出結果排序依據</summary>
		[DisplayName("排序依據")] [Description("選擇輸出的優先順序")]
		public SortType SortBy { get; set; } = SortType.Confidence;

		// ===== 結果過濾 =====

		/// <summary>後半徑過濾下界</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最小半徑 (px)")] [Description("結果過濾：只輸出半徑 >= 此值的圓形。設為 0 表示不限制。")]
		public int FilterMinRadius { get; set; } = 0;

		/// <summary>後半徑過濾上界</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最大半徑 (px)")] [Description("結果過濾：只輸出半徑 <= 此值的圓形。設為 0 表示不限制。")]
		public int FilterMaxRadius { get; set; } = 0;

		/// <summary>後面積過濾下界 (面積 = π × r²)</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最小面積 (px²)")] [Description("結果過濾：只輸出面積 >= 此值的圓形。設為 0 表示不限制。(面積 = π × 半徑²)")]
		public int FilterMinArea { get; set; } = 0;

		/// <summary>後面積過濾上界 (面積 = π × r²)</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最大面積 (px²)")] [Description("結果過濾：只輸出面積 <= 此值的圓形。設為 0 表示不限制。(面積 = π × 半徑²)")]
		public int FilterMaxArea { get; set; } = 0;
	}
}
