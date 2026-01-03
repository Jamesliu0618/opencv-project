using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>影像旋轉與翻轉參數</summary>
	public class RotateFlipParameters
	{
		/// <summary>影像翻轉類型</summary>
		public enum FlipType
		{
			/// <summary>不翻轉</summary>
			None,
			/// <summary>水平翻轉</summary>
			Horizontal,
			/// <summary>垂直翻轉</summary>
			Vertical,
			/// <summary>兩者皆翻轉</summary>
			Both,
		}

		/// <summary>影像旋轉類型</summary>
		public enum RotationType
		{
			/// <summary>不旋轉</summary>
			None,
			/// <summary>順時針 90 度</summary>
			Rotate90CW,
			/// <summary>180 度</summary>
			Rotate180,
			/// <summary>逆時針 90 度</summary>
			Rotate90CCW,
		}

		/// <summary>設定旋轉角度</summary>
		[DisplayName("旋轉")] [Description("順時針旋轉角度")]
		public RotationType Rotation { get; set; } = RotationType.None;

		/// <summary>設定翻轉方向</summary>
		[DisplayName("翻轉")] [Description("影像翻轉方向")]
		public FlipType Flip { get; set; } = FlipType.None;
	}
}
