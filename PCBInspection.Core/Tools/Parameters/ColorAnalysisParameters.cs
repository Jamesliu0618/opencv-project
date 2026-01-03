using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>色彩分析參數</summary>
	public class ColorAnalysisParameters
	{
		/// <summary>是否使用 K-Means 演算法計算主要顏色成分</summary>
		[DisplayName("計算主要顏色")] [Description("使用 K-Means 聚類提取主要顏色")]
		public bool ComputeDominantColors { get; set; } = true;

		/// <summary>提取主要顏色的數量 (K 值)</summary>
		[DisplayName("顏色數量")] [Description("K-Means 聚類的顏色數量 (K 值)")]
		public int ColorCount { get; set; } = 5;

		/// <summary>是否顯示顏色分布色塊圖</summary>
		[DisplayName("顯示顏色分布圖")] [Description("以色塊方式顯示提取的主要顏色")]
		public bool ShowColorDistribution { get; set; } = true;

		/// <summary>是否計算與標準色間的 ΔE 色差</summary>
		[DisplayName("計算色差")] [Description("與標準色計算 ΔE 色差值")]
		public bool ComputeColorDifference { get; set; } = false;

		/// <summary>標準參考色的 Lab 數值 (格式: L,a,b)</summary>
		[DisplayName("標準色 Lab 值")] [Description("參考標準色的 Lab 值 (格式: L,a,b)")]
		public string StandardLabColor { get; set; } = "50,0,0";

		/// <summary>判斷色差合格的 ΔE 閾值</summary>
		[DisplayName("色差容許範圍 ΔE")] [Description("允許的最大色差值")]
		public double DeltaEThreshold { get; set; } = 5.0;
	}
}
