using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>影像品質評估參數</summary>
	public class QualityAssessmentParameters
	{
		/// <summary>品質評估指標</summary>
		public enum QualityMetric
		{
			/// <summary>峰值信噪比</summary>
			PSNR,
			/// <summary>結構相似性</summary>
			SSIM,
			/// <summary>均方誤差</summary>
			MSE,
		}

		/// <summary>選擇品質計算方法</summary>
		[DisplayName("品質指標")] [Description("選擇影像品質評估的計算方式:\\n- PSNR: 峰值信噪比\\n- SSIM: 結構相似性\\n- MSE: 均方誤差")]
		public QualityMetric Metric { get; set; } = QualityMetric.PSNR;

		/// <summary>作為比對基礎的參考影像 (標準影像) 路徑</summary>
		[DisplayName("參考影像路徑")] [Description("作為品質比較基準的參考影像檔案路徑")]
		public string ReferenceImagePath { get; set; } = "";

		/// <summary>是否在輸出影像上顯示計算的分數</summary>
		[DisplayName("顯示品質分數")] [Description("在輸出影像上顯示計算的品質分數")]
		public bool ShowScore { get; set; } = true;

		/// <summary>是否將詳細品質評估結果輸出至 Log</summary>
		[DisplayName("輸出品質報告")] [Description("將品質評估結果輸出到 Log")]
		public bool OutputReport { get; set; } = true;

		/// <summary>是否視覺化 SSIM 差異熱圖</summary>
		[DisplayName("SSIM 視覺化")] [Description("顯示 SSIM 差異熱圖 (僅適用於 SSIM 指標)")]
		public bool VisualizeSSIM { get; set; } = false;
	}
}
