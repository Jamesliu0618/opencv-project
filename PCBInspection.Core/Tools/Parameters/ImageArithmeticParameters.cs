using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>算術邏輯運算參數 (對應 AxImageALops)</summary>
	public class ImageArithmeticParameters
	{
		/// <summary>運算類型</summary>
		public enum OperationType
		{
			/// <summary>加法</summary>
			Add,
			/// <summary>減法</summary>
			Subtract,
			/// <summary>乘法</summary>
			Multiply,
			/// <summary>除法</summary>
			Divide,
			/// <summary>位元 AND</summary>
			BitwiseAnd,
			/// <summary>位元 OR</summary>
			BitwiseOr,
			/// <summary>位元 XOR</summary>
			BitwiseXor,
			/// <summary>位元 NOT</summary>
			BitwiseNot,
			/// <summary>絕對差值</summary>
			AbsDiff,
			/// <summary>最大值</summary>
			Max,
			/// <summary>最小值</summary>
			Min,
		}

		/// <summary>選擇運算類型</summary>
		[DisplayName("運算類型")] [Description("影像算術邏輯運算類型")]
		public OperationType Operation { get; set; } = OperationType.Add;

		/// <summary>第二張影像路徑 (用於雙運算元運算)</summary>
		[DisplayName("第二影像路徑")] [Description("用於雙運算元運算的第二張影像 (BitwiseNot 除外)")]
		public string SecondImagePath { get; set; } = "";

		/// <summary>純量值 (用於影像+純量運算)</summary>
		[DisplayName("純量值")] [Description("用於 Add/Subtract 時的純量值 (0~255)")]
		public int ScalarValue { get; set; } = 50;

		/// <summary>是否使用純量而非第二影像</summary>
		[DisplayName("使用純量")] [Description("使用純量進行運算，而非第二張影像")]
		public bool UseScalar { get; set; } = true;
	}
}
