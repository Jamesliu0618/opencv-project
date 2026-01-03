using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>透視變換參數 (四點矯正)</summary>
	public class PerspectiveTransformParameters
	{
		/// <summary>插值方法類型</summary>
		public enum InterpolationType
		{
			/// <summary>最近鄰插值</summary>
			Nearest,
			/// <summary>線性插值</summary>
			Linear,
			/// <summary>三次插值</summary>
			Cubic,
			/// <summary>Lanczos 插值</summary>
			Lanczos4,
		}

		/// <summary>源影像左上角 X 座標</summary>
		[DisplayName("左上角 X")] [Description("源影像的左上角點 X 座標")]
		public float SrcTopLeftX { get; set; } = 0;

		/// <summary>源影像左上角 Y 座標</summary>
		[DisplayName("左上角 Y")] [Description("源影像的左上角點 Y 座標")]
		public float SrcTopLeftY { get; set; } = 0;

		/// <summary>源影像右上角 X 座標</summary>
		[DisplayName("右上角 X")] [Description("源影像的右上角點 X 座標")]
		public float SrcTopRightX { get; set; } = 100;

		/// <summary>源影像右上角 Y 座標</summary>
		[DisplayName("右上角 Y")] [Description("源影像的右上角點 Y 座標")]
		public float SrcTopRightY { get; set; } = 0;

		/// <summary>源影像右下角 X 座標</summary>
		[DisplayName("右下角 X")] [Description("源影像的右下角點 X 座標")]
		public float SrcBottomRightX { get; set; } = 100;

		/// <summary>源影像右下角 Y 座標</summary>
		[DisplayName("右下角 Y")] [Description("源影像的右下角點 Y 座標")]
		public float SrcBottomRightY { get; set; } = 100;

		/// <summary>源影像左下角 X 座標</summary>
		[DisplayName("左下角 X")] [Description("源影像的左下角點 X 座標")]
		public float SrcBottomLeftX { get; set; } = 0;

		/// <summary>源影像左下角 Y 座標</summary>
		[DisplayName("左下角 Y")] [Description("源影像的左下角點 Y 座標")]
		public float SrcBottomLeftY { get; set; } = 100;

		/// <summary>輸出影像寬度 (px)</summary>
		[DisplayName("輸出寬度 (px)")] [Description("變換後輸出影像的寬度")]
		public int OutputWidth { get; set; } = 640;

		/// <summary>輸出影像高度 (px)</summary>
		[DisplayName("輸出高度 (px)")] [Description("變換後輸出影像的高度")]
		public int OutputHeight { get; set; } = 480;

		/// <summary>影像變換的插值方法</summary>
		[DisplayName("插值方法")] [Description("影像變換的插值演算法")]
		public InterpolationType Interpolation { get; set; } = InterpolationType.Linear;
	}
}
