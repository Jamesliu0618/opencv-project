using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>對比亮度調整參數 (對應 AxImageGainOffset)</summary>
	public class GainOffsetParameters
	{
		/// <summary>對比度 (Gain/Alpha)，1.0 為原始</summary>
		[DisplayName("對比度 (Gain)")] [Description("對比度調整，1.0 為原始，大於 1 增加對比")]
		public double Gain { get; set; } = 1.0;

		/// <summary>亮度偏移 (Offset/Beta)</summary>
		[DisplayName("亮度 (Offset)")] [Description("亮度偏移，0 為原始，正值調亮，負值調暗")]
		public double Offset { get; set; } = 0;
	}
}
