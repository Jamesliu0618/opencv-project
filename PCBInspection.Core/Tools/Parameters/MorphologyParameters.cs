using OpenCvSharp;
using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>形態學運算參數</summary>
	public class MorphologyParameters
	{
		/// <summary>形態學運算子類型</summary>
		public enum MorphOp
		{
			/// <summary>腐蝕</summary>
			Erode,
			/// <summary>膨脹</summary>
			Dilate,
			/// <summary>開運算</summary>
			Open,
			/// <summary>閉運算</summary>
			Close,
			/// <summary>形態學梯度</summary>
			Gradient,
			/// <summary>頂帽</summary>
			TopHat,
			/// <summary>黑帽</summary>
			BlackHat,
		}

		/// <summary>選擇形態學運算子</summary>
		[DisplayName("運算類型")] [Description("形態學運算:\n- Erode: 腐蝕\n- Dilate: 膨脹\n- Open: 開運算 (先腐後膨)\n- Close: 閉運算 (先膨後腐)\n- Gradient: 形態學梯度\n- TopHat: 頂帽\n- BlackHat: 黑帽")]
		public MorphOp Operation { get; set; } = MorphOp.Open;

		/// <summary>結構元素 (核心) 之形狀</summary>
		[DisplayName("核心形狀")] [Description("結構元素形狀: Rect (矩形), Cross (十字), Ellipse (橢圓)")]
		public MorphShapes Shape { get; set; } = MorphShapes.Rect;

		/// <summary>結構元素之大小 (像素)</summary>
		[DisplayName("核心大小 (px)")] [Description("結構元素大小")]
		public int KernelSize { get; set; } = 3;

		/// <summary>重疊執行運算的次數</summary>
		[DisplayName("迭代次數")] [Description("運算重複執行的次數")]
		public int Iterations { get; set; } = 1;
	}
}
