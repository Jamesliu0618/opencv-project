using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>濾波模糊參數</summary>
	public class BlurParameters
	{
		/// <summary>模糊演算法類型</summary>
		public enum BlurType
		{
			/// <summary>高斯模糊</summary>
			Gaussian,
			/// <summary>中值濾波</summary>
			Median,
			/// <summary>均值模糊</summary>
			Box,
			/// <summary>雙邊濾波</summary>
			Bilateral,
		}

		/// <summary>選擇模糊方案</summary>
		[DisplayName("模糊類型")] [Description("選擇模糊演算法:\n- Gaussian: 高斯模糊 (常用)\n- Median: 中值濾波 (去椒鹽雜訊)\n- Box: 均值模糊\n- Bilateral: 雙邊濾波 (保留邊緣)")]
		public BlurType Type { get; set; } = BlurType.Gaussian;

		/// <summary>核心大小 (像素)，必須為奇數</summary>
		[DisplayName("核心大小 (px)")] [Description("濾波核大小，必須為奇數 (e.g. 3, 5, 7, 9)")]
		public int KernelSize { get; set; } = 5;

		/// <summary>高斯/雙邊濾波的標準差</summary>
		[DisplayName("Sigma")] [Description("高斯/雙邊濾波的標準差。設為 0 表示自動計算。")]
		public double Sigma { get; set; } = 0;
	}
}
