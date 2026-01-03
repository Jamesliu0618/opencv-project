using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>背景相減參數</summary>
	public class BackgroundSubtractionParameters
	{
		/// <summary>背景建模演算法類型</summary>
		public enum SubtractionMethod
		{
			/// <summary>高斯混合模型 MOG2</summary>
			MOG2,
			/// <summary>K-近鄰演算法 KNN</summary>
			KNN,
		}

		/// <summary>選擇背景分割演算法</summary>
		[DisplayName("方法")] [Description("背景分割演算法:\\n- MOG2: 高斯混合模型\\n- KNN: K近鄰演算法")]
		public SubtractionMethod Method { get; set; } = SubtractionMethod.MOG2;

		/// <summary>背景模型學習率 (0~1)，-1 為自動</summary>
		[DisplayName("學習率")] [Description("背景模型的更新速度 (0-1)，設為 -1 表示自動")]
		public double LearningRate { get; set; } = 0.01;

		/// <summary>用於建立背景模型的歷史影像幀數</summary>
		[DisplayName("歷史幀數")] [Description("用於建立背景模型的歷史幀數")]
		public int History { get; set; } = 500;

		/// <summary>像素方差閾值 (用於 MOG2)</summary>
		[DisplayName("方差閾值")] [Description("MOG2 的像素方差閾值")]
		public double VarThreshold { get; set; } = 16;

		/// <summary>是否偵測並標註影像中的陰影區域</summary>
		[DisplayName("偵測陰影")] [Description("是否偵測並標記陰影區域")]
		public bool DetectShadows { get; set; } = false;

		/// <summary>是否對前景遮罩進行形態學去噪後處理</summary>
		[DisplayName("形態學後處理")] [Description("對前景遮罩進行形態學開運算去除雜訊")]
		public bool MorphologicalPostProcess { get; set; } = true;

		/// <summary>形態學後處理的核心大小 (像素)</summary>
		[DisplayName("核心大小 (px)")] [Description("形態學後處理的核心大小")]
		public int MorphKernelSize { get; set; } = 3;
	}
}
