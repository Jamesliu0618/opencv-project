using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>LUT 色彩轉換參數 (對應 AxImageLut)</summary>
	public class LutParameters
	{
		/// <summary>預設 LUT 類型</summary>
		public enum LutPreset
		{
			/// <summary>反相</summary>
			Invert,
			/// <summary>Gamma 校正</summary>
			Gamma,
			/// <summary>對數</summary>
			Log,
			/// <summary>自訂</summary>
			Custom,
		}

		/// <summary>選擇 LUT 預設類型</summary>
		[DisplayName("LUT 類型")]
		public LutPreset Preset { get; set; } = LutPreset.Invert;

		/// <summary>Gamma 值 (用於 Gamma 校正)</summary>
		[DisplayName("Gamma 值")] [Description("Gamma 校正值，小於 1 調亮，大於 1 調暗")]
		public double GammaValue { get; set; } = 1.0;

		/// <summary>自訂 LUT 表格 (256 個值，以分號分隔)</summary>
		[DisplayName("自訂 LUT")] [Description("256 個值 (0~255)，以分號分隔")]
		public string CustomLut { get; set; } = "";
	}
}
