using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>邊緣偵測參數</summary>
	public class EdgeDetectionParameters
	{
		/// <summary>邊緣偵測演算法類型</summary>
		public enum EdgeMethod
		{
			/// <summary>Canny 邊緣偵測 (雙閾值)</summary>
			Canny,
			/// <summary>Sobel 算子 (一階微分)</summary>
			Sobel,
			/// <summary>Laplacian 算子 (二階微分)</summary>
			Laplacian,
			/// <summary>Scharr 算子 (改進型 Sobel)</summary>
			Scharr,
		}

		/// <summary>選擇邊緣偵測方法</summary>
		[DisplayName("邊緣偵測方法")] [Description("- Canny: 最常用，雙閾值\n- Sobel: 一階微分\n- Laplacian: 二階微分\n- Scharr: 改進版 Sobel")]
		public EdgeMethod Method { get; set; } = EdgeMethod.Canny;

		/// <summary>Canny 低閾值 或 Sobel/Laplacian 核心大小</summary>
		[DisplayName("閾值 1 / Ksize")] [Description("Canny: 低閾值 / Sobel/Laplacian: 核心大小")]
		public double Threshold1 { get; set; } = 50;

		/// <summary>Canny 高閾值 (通常建議為低閾值的 2~3 倍)</summary>
		[DisplayName("閾值 2")] [Description("Canny: 高閾值 (建議為閾值1的2-3倍)")]
		public double Threshold2 { get; set; } = 150;
	}
}
