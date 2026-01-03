using System.ComponentModel;

namespace PCBInspection.Core.Models
{
	public class SolderDefectParameters
	{
		[DisplayName("最小焊點面積 (px)")] [Description("過濾小於此面積的區域。\n單位: 像素 (Pixel)")]
		public double MinArea { get; set; } = 50;

		[DisplayName("最大焊點面積 (px)")] [Description("過濾大於此面積的區域。\n單位: 像素 (Pixel)")]
		public double MaxArea { get; set; } = 5000;

		[DisplayName("形態學核大小 (px)")] [Description("用於去除雜訊的結構元素大小，必須為奇數 (e.g. 3, 5, 7)。\n單位: 像素 (Pixel)")]
		public int MorphKernel { get; set; } = 3;
	}

	public class SurfaceDefectParameters
	{
		[DisplayName("模糊半徑 (px)")] [Description("中值濾波的核大小，用於降低雜訊干擾，必須為奇數。\n單位: 像素 (Pixel)")]
		public int BlurSize { get; set; } = 5;

		[DisplayName("偵測閾值 (0-255)")] [Description("差異影像的二值化閾值，值越小越敏感。\n範圍: 0-255")]
		public double Threshold { get; set; } = 30;

		[DisplayName("最小瑕疵面積 (px)")] [Description("過濾小於此面積的瑕疵區域。\n單位: 像素 (Pixel)")]
		public int MinDefectArea { get; set; } = 100;
	}

	public class CircuitDefectParameters
	{
		[DisplayName("最小線寬 (px)")] [Description("判定斷路的線寬閾值。若線寬小於此值則視為斷路。\n單位: 像素 (Pixel)")]
		public int MinTraceWidth { get; set; } = 3;

		[DisplayName("最大間隙 (px)")] [Description("判定橋接的間隙閾值。若間隙小於此值則視為短路/橋接。\n單位: 像素 (Pixel)")]
		public int MaxGapWidth { get; set; } = 5;
	}
}