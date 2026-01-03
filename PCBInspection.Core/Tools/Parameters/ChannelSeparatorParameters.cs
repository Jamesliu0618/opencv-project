using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>色版分離參數 (對應 AxImageRgbSeparator 等)</summary>
	public class ChannelSeparatorParameters
	{
		/// <summary>色彩空間類型</summary>
		public enum ColorSpaceType
		{
			/// <summary>RGB</summary>
			RGB,
			/// <summary>HSV</summary>
			HSV,
			/// <summary>HSI</summary>
			HSI,
			/// <summary>Lab</summary>
			Lab,
			/// <summary>Luv</summary>
			Luv,
			/// <summary>XYZ</summary>
			XYZ,
			/// <summary>YCrCb</summary>
			YCrCb,
		}

		/// <summary>選擇色彩空間</summary>
		[DisplayName("色彩空間")]
		public ColorSpaceType ColorSpace { get; set; } = ColorSpaceType.RGB;

		/// <summary>選擇輸出通道 (0/1/2)</summary>
		[DisplayName("輸出通道")] [Description("選擇輸出的通道索引 (0=第一通道)")]
		public int ChannelIndex { get; set; } = 0;
	}
}
