using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>二值化分割參數</summary>
	public class ThresholdParameters
	{
		/// <summary>二值化處理方法</summary>
		public enum ThreshMethod
		{
			/// <summary>標準二值化</summary>
			Binary,
			/// <summary>反向二值化</summary>
			BinaryInv,
			/// <summary>Otsu 自動閾值</summary>
			Otsu,
			/// <summary>自適應閾值</summary>
			Adaptive,
			/// <summary>低於閾值歸零</summary>
			ToZero,
		}

		/// <summary>選擇二值化方案</summary>
		[DisplayName("閾值方法")] [Description("選擇二值化方法:\n- Binary: 標準二值化\n- BinaryInv: 反向二值化\n- Otsu: 自動計算最佳閾值\n- Adaptive: 自適應閾值\n- ToZero: 低於閾值設為 0")]
		public ThreshMethod Method { get; set; } = ThreshMethod.Binary;

		/// <summary>手動設定之閾值 (0-255)</summary>
		[DisplayName("閾值 (0-255)")] [Description("手動閾值。若使用 Otsu/Adaptive 則此值會被忽略。")]
		public double Threshold { get; set; } = 128;

		/// <summary>二值化後的最大亮度值 (通常為 255)</summary>
		[DisplayName("最大值 (0-255)")] [Description("二值化後的最大值 (通常為 255)")]
		public double MaxVal { get; set; } = 255;

		/// <summary>自適應區塊大小，須為奇數</summary>
		[DisplayName("自適應區塊大小")] [Description("僅用於 Adaptive 方法，必須為奇數")]
		public int AdaptiveBlockSize { get; set; } = 11;

		/// <summary>自適應常數 C，從均值中減去之數值</summary>
		[DisplayName("自適應常數 C")] [Description("僅用於 Adaptive 方法，從均值中減去的常數")]
		public double AdaptiveC { get; set; } = 2;
	}
}
