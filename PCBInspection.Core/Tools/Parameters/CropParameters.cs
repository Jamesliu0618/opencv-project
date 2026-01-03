using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>影像裁切參數</summary>
	public class CropParameters
	{
		/// <summary>裁切區域左上角 X 座標</summary>
		[DisplayName("X 起點")]
		public int X { get; set; } = 0;

		/// <summary>裁切區域左上角 Y 座標</summary>
		[DisplayName("Y 起點")]
		public int Y { get; set; } = 0;

		/// <summary>裁切區域寬度</summary>
		[DisplayName("寬度")]
		public int Width { get; set; } = 100;

		/// <summary>裁切區域高度</summary>
		[DisplayName("高度")]
		public int Height { get; set; } = 100;
	}
}
