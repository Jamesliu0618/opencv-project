using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>RGB 色版合成參數 (對應 AxImageRgbComposer)</summary>
	public class RgbComposerParameters
	{
		/// <summary>紅色通道影像路徑</summary>
		[DisplayName("R 通道影像")]
		public string RedChannelPath { get; set; } = "";

		/// <summary>綠色通道影像路徑</summary>
		[DisplayName("G 通道影像")]
		public string GreenChannelPath { get; set; } = "";

		/// <summary>藍色通道影像路徑</summary>
		[DisplayName("B 通道影像")]
		public string BlueChannelPath { get; set; } = "";
	}
}
