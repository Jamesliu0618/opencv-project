using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>影像直方圖分析參數</summary>
	public class HistogramAnalysisParameters
	{
		/// <summary>直方圖取樣類型</summary>
		public enum HistogramType
		{
			/// <summary>單通道灰階</summary>
			Grayscale,
			/// <summary>三通道 RGB 彩色</summary>
			RGB,
			/// <summary>HSV 色彩空間</summary>
			HSV,
		}

		/// <summary>是否開啟獨立視窗顯示直方圖</summary>
		[DisplayName("顯示直方圖視窗")] [Description("以獨立視窗顯示直方圖")]
		public bool ShowHistogramWindow { get; set; } = true;

		/// <summary>是否計算並顯示均值、標準差等統計量</summary>
		[DisplayName("計算統計值")] [Description("計算並顯示均值、標準差、最大最小值")]
		public bool ComputeStatistics { get; set; } = true;

		/// <summary>選擇分析的直方圖色彩空間</summary>
		[DisplayName("直方圖類型")] [Description("選擇直方圖的色彩通道類型")]
		public HistogramType Type { get; set; } = HistogramType.Grayscale;

		/// <summary>是否將直方圖小圖繪製於輸出影像上</summary>
		[DisplayName("繪製於影像上")] [Description("將直方圖繪製在輸出影像的右下角")]
		public bool DrawOnImage { get; set; } = false;
	}
}
