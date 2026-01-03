using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>去雜訊參數 (Non-local Means Denoising)</summary>
	public class DenoiseParameters
	{
		/// <summary>去雜訊強度 (h 值)，越大效果越強但細節損失越多</summary>
		[DisplayName("濾波強度")] [Description("去雜訊強度 (h 值)，越大去雜訊效果越強 but 細節損失越多")]
		public float FilterStrength { get; set; } = 10;

		/// <summary>模板區塊大小，必須為奇數</summary>
		[DisplayName("模板視窗大小")] [Description("模板區塊大小，必須為奇數")]
		public int TemplateWindowSize { get; set; } = 7;

		/// <summary>搜尋區域大小，必須為奇數</summary>
		[DisplayName("搜尋視窗大小")] [Description("搜尋區域大小，必須為奇數")]
		public int SearchWindowSize { get; set; } = 21;
	}
}
