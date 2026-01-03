using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>對焦評估參數 (對應 AxImageFocusRatio)</summary>
	public class FocusRatioParameters
	{
		/// <summary>評估方法</summary>
		public enum FocusMethod
		{
			/// <summary>Laplacian 方差</summary>
			LaplacianVariance,
			/// <summary>Sobel 梯度</summary>
			SobelGradient,
			/// <summary>Tenengrad (Sobel 平方和)</summary>
			Tenengrad,
		}

		/// <summary>選擇對焦評估方法</summary>
		[DisplayName("評估方法")]
		public FocusMethod Method { get; set; } = FocusMethod.LaplacianVariance;

		/// <summary>是否在影像上顯示對焦分數</summary>
		[DisplayName("顯示分數")]
		public bool ShowScore { get; set; } = true;
	}
}
