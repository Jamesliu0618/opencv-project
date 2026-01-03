using System.ComponentModel;

namespace PCBInspection.Core.Models
{
	/// <summary>錫膏缺陷檢測參數</summary>
	public class SolderDefectParameters
	{
		/// <summary>最小判定面積 (像素)</summary>
		[DisplayName("最小焊點面積 (px)")] [Description("過濾小於此面積的區域。\n單位: 像素 (Pixel)")]
		public double MinArea { get; set; } = 50;

		/// <summary>最大判定面積 (像素)</summary>
		[DisplayName("最大焊點面積 (px)")] [Description("過濾大於此面積的區域。\n單位: 像素 (Pixel)")]
		public double MaxArea { get; set; } = 5000;

		/// <summary>形態學結構元素大小 (像素)</summary>
		[DisplayName("形態學核大小 (px)")] [Description("用於去除雜訊的結構元素大小，必須為奇數 (e.g. 3, 5, 7)。\n單位: 像素 (Pixel)")]
		public int MorphKernel { get; set; } = 3;
	}

	/// <summary>表面缺陷檢測參數 (如刮傷、汙漬)</summary>
	public class SurfaceDefectParameters
	{
		/// <summary>模糊處理核心大小 (像素)</summary>
		[DisplayName("模糊半徑 (px)")] [Description("中值濾波的核大小，用於降低雜訊干擾，必須為奇數。\n單位: 像素 (Pixel)")]
		public int BlurSize { get; set; } = 5;

		/// <summary>二值化分割閾值 (0-255)</summary>
		[DisplayName("偵測閾值 (0-255)")] [Description("差異影像的二值化閾值，值越小越敏感。\n範圍: 0-255")]
		public double Threshold { get; set; } = 30;

		/// <summary>最小缺陷區域面積 (像素)</summary>
		[DisplayName("最小瑕疵面積 (px)")] [Description("過濾小於此面積的瑕疵區域。\n單位: 像素 (Pixel)")]
		public int MinDefectArea { get; set; } = 100;
	}

	/// <summary>線路缺陷檢測參數 (如斷路、短路)</summary>
	public class CircuitDefectParameters
	{
		/// <summary>判定斷路的最小線寬閾值 (像素)</summary>
		[DisplayName("最小線寬 (px)")] [Description("判定斷路的線寬閾值。若線寬小於此值則視為斷路。\n單位: 像素 (Pixel)")]
		public int MinTraceWidth { get; set; } = 3;

		/// <summary>判定短路的最大間隙閾值 (像素)</summary>
		[DisplayName("最大間隙 (px)")] [Description("判定橋接的間隙閾值。若間隙小於此值則視為短路/橋接。\n單位: 像素 (Pixel)")]
		public int MaxGapWidth { get; set; } = 5;
	}
}