using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>點到線距離計算參數 (對應 AxPointLineDistanceMsr)</summary>
	public class PointLineDistanceParameters
	{
		/// <summary>點座標</summary>
		[DisplayName("目標點")]
		public Models.GeoPoint TargetPoint { get; set; } = new Models.GeoPoint(50, 50);

		/// <summary>線段起點</summary>
		[DisplayName("線起點")]
		public Models.GeoPoint LineStart { get; set; } = new Models.GeoPoint(0, 0);

		/// <summary>線段終點</summary>
		[DisplayName("線終點")]
		public Models.GeoPoint LineEnd { get; set; } = new Models.GeoPoint(100, 0);

		/// <summary>每像素對應的實際長度 (毫米/像素)</summary>
		[DisplayName("像素比例 (mm/px)")]
		public double PixelScale { get; set; } = 0.1;

		/// <summary>結果顯示單位</summary>
		[DisplayName("顯示單位")]
		public string DisplayUnit { get; set; } = "mm";

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;
	}
}
