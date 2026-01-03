using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>迴歸圓參數 (對應 AxCircleRegression)</summary>
	public class CircleRegressionParameters
	{
		/// <summary>點群座標 (格式: x1,y1;x2,y2;...)</summary>
		[DisplayName("點群座標")] [Description("格式: x1,y1;x2,y2;x3,y3;... 例如: 100,50;150,100;100,150;50,100")]
		public string PointsData { get; set; } = "100,50;150,100;100,150;50,100";

		/// <summary>是否使用偵測到的輪廓邊緣點</summary>
		[DisplayName("使用輪廓邊緣點")] [Description("自動從輸入二值影像提取邊緣點，忽略手動輸入的點群座標")]
		public bool UseContourPoints { get; set; } = false;

		/// <summary>每像素對應的實際長度 (毫米/像素)</summary>
		[DisplayName("像素比例 (mm/px)")]
		public double PixelScale { get; set; } = 0.1;

		/// <summary>結果顯示單位</summary>
		[DisplayName("顯示單位")]
		public string DisplayUnit { get; set; } = "mm";

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;

		/// <summary>是否標記原始點</summary>
		[DisplayName("標記原始點")]
		public bool DrawPoints { get; set; } = true;

		/// <summary>點標記半徑</summary>
		[DisplayName("點標記半徑 (px)")]
		public int PointRadius { get; set; } = 3;
	}
}
