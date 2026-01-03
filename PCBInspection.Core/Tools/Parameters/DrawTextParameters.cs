using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>繪製文字參數</summary>
	public class DrawTextParameters
	{
		/// <summary>欲繪製之字串</summary>
		[DisplayName("文字內容")]
		public string Text { get; set; } = "Sample";

		/// <summary>繪製起點 X 座標</summary>
		[DisplayName("X 座標")]
		public int X { get; set; } = 10;

		/// <summary>繪製起點 Y 座標</summary>
		[DisplayName("Y 座標")]
		public int Y { get; set; } = 30;

		/// <summary>字體比例因子</summary>
		[DisplayName("字體大小")]
		public double FontScale { get; set; } = 1.0;

		/// <summary>文字顏色 (格式: B,G,R)</summary>
		[DisplayName("顏色 (BGR)")] [Description("格式: B,G,R (例如 255,0,0 為藍色)")]
		public string ColorBGR { get; set; } = "0,255,0";

		/// <summary>筆觸粗細 (像素)</summary>
		[DisplayName("線條粗細")]
		public int Thickness { get; set; } = 2;
	}
}
