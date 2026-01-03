using OpenCvSharp;
using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>影像縮放參數</summary>
	public class ResizeParameters
	{
		/// <summary>縮放模式</summary>
		public enum ResizeMode
		{
			/// <summary>依比例縮放</summary>
			ByScale,
			/// <summary>指定目標尺寸</summary>
			BySize,
		}

		/// <summary>選擇縮放模式</summary>
		[DisplayName("縮放模式")]
		public ResizeMode Mode { get; set; } = ResizeMode.ByScale;

		/// <summary>縮放比例 (當模式為 ByScale 時使用)</summary>
		[DisplayName("縮放比例")] [Description("當模式為 ByScale 時使用 (例如 0.5 = 縮小一半)")]
		public double Scale { get; set; } = 1.0;

		/// <summary>目標寬度 (當模式為 BySize 時使用)</summary>
		[DisplayName("目標寬度 (px)")] [Description("當模式為 BySize 時使用")]
		public int TargetWidth { get; set; } = 640;

		/// <summary>目標高度 (當模式為 BySize 時使用)</summary>
		[DisplayName("目標高度 (px)")] [Description("當模式為 BySize 時使用")]
		public int TargetHeight { get; set; } = 480;

		/// <summary>影像插值方法</summary>
		[DisplayName("插值方法")] [Description("影像插值方法")]
		public InterpolationFlags Interpolation { get; set; } = InterpolationFlags.Linear;
	}
}
