using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>迴歸直線參數 (對應 AxLineRegression)</summary>
	public class LineRegressionParameters
	{
		/// <summary>點群座標 (格式: x1,y1;x2,y2;...)</summary>
		[DisplayName("點群座標")] [Description("格式: x1,y1;x2,y2;x3,y3;... 例如: 10,20;30,40;50,60;70,80;90,100")]
		public string PointsData { get; set; } = "10,20;30,40;50,60;70,80;90,100";

		/// <summary>是否使用偵測到的輪廓邊緣點</summary>
		[DisplayName("使用輪廓邊緣點")] [Description("自動從輸入二值影像提取邊緣點，忽略手動輸入的點群座標")]
		public bool UseContourPoints { get; set; } = false;

		/// <summary>擬合線延伸長度 (像素)</summary>
		[DisplayName("線延伸長度 (px)")]
		public int LineExtension { get; set; } = 500;

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
