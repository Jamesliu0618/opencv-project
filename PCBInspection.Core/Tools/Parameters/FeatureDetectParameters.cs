using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>特徵點偵測參數</summary>
	public class FeatureDetectParameters
	{
		/// <summary>特徵點偵測器類型</summary>
		public enum DetectorType
		{
			/// <summary>ORB (快速且免費)</summary>
			ORB,
			/// <summary>SIFT (高度精確)</summary>
			SIFT,
			/// <summary>FAST (極速偵測)</summary>
			FAST,
			/// <summary>BRISK (二進制描述子)</summary>
			BRISK,
			/// <summary>AKAZE (非線性尺度空間)</summary>
			AKAZE,
		}

		/// <summary>選擇偵測器算法</summary>
		[DisplayName("偵測器類型")] [Description("- ORB: 快速、免費\n- SIFT: 精確、需 contrib\n- FAST: 極快速\n- BRISK: 二進制描述子\n- AKAZE: 非線性尺度空間")]
		public DetectorType Detector { get; set; } = DetectorType.ORB;

		/// <summary>保留的最大特徵點數量</summary>
		[DisplayName("最大特徵點數")] [Description("保留的最大特徵點數量")]
		public int MaxFeatures { get; set; } = 500;

		/// <summary>過濾小於此大小的特徵點</summary>
		[DisplayName("最小特徵大小")] [Description("過濾小於此大小的特徵點。設為 0 表示不限制。")]
		public float MinSize { get; set; } = 0;

		/// <summary>過濾大於此大小的特徵點</summary>
		[DisplayName("最大特徵大小")] [Description("過濾大於此大小的特徵點。設為 0 表示不限制。")]
		public float MaxSize { get; set; } = 0;

		/// <summary>是否在輸出影像上標註特徵點</summary>
		[DisplayName("繪製特徵點")] [Description("是否在輸出影像上繪製特徵點")]
		public bool DrawKeypoints { get; set; } = true;
	}
}
