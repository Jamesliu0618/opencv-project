using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>直方圖均衡化參數</summary>
	public class HistogramEqualizeParameters
	{
		/// <summary>是否使用 CLAHE (對比度受限自適應直方圖均衡化)</summary>
		[DisplayName("使用 CLAHE")] [Description("使用對比度受限自適應直方圖均衡化 (CLAHE)，適用於局部對比度增強")]
		public bool UseCLAHE { get; set; } = false;

		/// <summary>CLAHE 對比度限制閾值</summary>
		[DisplayName("CLAHE Clip Limit")] [Description("對比度限制閾值 (僅用於 CLAHE)")]
		public double ClipLimit { get; set; } = 2.0;

		/// <summary>CLAHE 網格區塊大小</summary>
		[DisplayName("CLAHE 區塊大小")] [Description("區塊大小 (僅用於 CLAHE)")]
		public int TileGridSize { get; set; } = 8;
	}
}
