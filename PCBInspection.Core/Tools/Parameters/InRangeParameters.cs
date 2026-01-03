using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>色彩範圍過濾參數 (In-Range 閾值)</summary>
	public class InRangeParameters
	{
		/// <summary>色彩範圍下界 - 第 1 通道 (H/B)</summary>
		[DisplayName("下界 H/B")]
		public int LowerH { get; set; } = 0;
		/// <summary>色彩範圍下界 - 第 2 通道 (S/G)</summary>
		[DisplayName("下界 S/G")]
		public int LowerS { get; set; } = 0;
		/// <summary>色彩範圍下界 - 第 3 通道 (V/R)</summary>
		[DisplayName("下界 V/R")]
		public int LowerV { get; set; } = 0;

		/// <summary>色彩範圍上界 - 第 1 通道 (H/B)</summary>
		[DisplayName("上界 H/B")]
		public int UpperH { get; set; } = 180;
		/// <summary>色彩範圍上界 - 第 2 通道 (S/G)</summary>
		[DisplayName("上界 S/G")]
		public int UpperS { get; set; } = 255;
		/// <summary>色彩範圍上界 - 第 3 通道 (V/R)</summary>
		[DisplayName("上界 V/R")]
		public int UpperV { get; set; } = 255;
	}
}
