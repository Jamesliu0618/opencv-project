using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>直線交點計算參數 (對應 AxIntersectionMsr)</summary>
	public class LineIntersectionParameters
	{
		/// <summary>線1 起點</summary>
		[DisplayName("線1 起點")]
		public Models.GeoPoint Line1Start { get; set; } = new Models.GeoPoint(0, 0);
		/// <summary>線1 終點</summary>
		[DisplayName("線1 終點")]
		public Models.GeoPoint Line1End { get; set; } = new Models.GeoPoint(100, 100);
		/// <summary>線2 起點</summary>
		[DisplayName("線2 起點")]
		public Models.GeoPoint Line2Start { get; set; } = new Models.GeoPoint(100, 0);
		/// <summary>線2 終點</summary>
		[DisplayName("線2 終點")]
		public Models.GeoPoint Line2End { get; set; } = new Models.GeoPoint(0, 100);

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;

		/// <summary>交點標記半徑</summary>
		[DisplayName("標記半徑 (px)")]
		public int MarkerRadius { get; set; } = 5;
	}
}
