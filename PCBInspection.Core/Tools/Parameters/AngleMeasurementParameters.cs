using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>角度量測參數 (對應 AxAngleMsr)</summary>
	public class AngleMeasurementParameters
	{
		/// <summary>量測模式</summary>
		public enum AngleMode
		{
			/// <summary>三點夾角 (頂點 + 兩端點)</summary>
			ThreePoints,
			/// <summary>兩線夾角</summary>
			TwoLines,
		}

		/// <summary>選擇角度量測模式</summary>
		[DisplayName("量測模式")] [Description("三點模式使用頂點+兩端點計算夾角；兩線模式使用直線方程式計算")]
		public AngleMode Mode { get; set; } = AngleMode.ThreePoints;

		// 三點模式參數
		/// <summary>頂點座標</summary>
		[Category("三點模式")] [DisplayName("頂點")]
		public Models.GeoPoint Vertex { get; set; } = new Models.GeoPoint(100, 100);
		/// <summary>第一端點座標</summary>
		[Category("三點模式")] [DisplayName("端點1")]
		public Models.GeoPoint Point1 { get; set; } = new Models.GeoPoint(50, 50);
		/// <summary>第二端點座標</summary>
		[Category("三點模式")] [DisplayName("端點2")]
		public Models.GeoPoint Point2 { get; set; } = new Models.GeoPoint(150, 50);

		// 兩線模式參數 (線1: 點A到點B, 線2: 點C到點D)
		/// <summary>線1 起點</summary>
		[Category("兩線模式")] [DisplayName("線1 起點")]
		public Models.GeoPoint Line1Start { get; set; } = new Models.GeoPoint(0, 0);
		/// <summary>線1 終點</summary>
		[Category("兩線模式")] [DisplayName("線1 終點")]
		public Models.GeoPoint Line1End { get; set; } = new Models.GeoPoint(100, 0);
		/// <summary>線2 起點</summary>
		[Category("兩線模式")] [DisplayName("線2 起點")]
		public Models.GeoPoint Line2Start { get; set; } = new Models.GeoPoint(0, 0);
		/// <summary>線2 終點</summary>
		[Category("兩線模式")] [DisplayName("線2 終點")]
		public Models.GeoPoint Line2End { get; set; } = new Models.GeoPoint(0, 100);

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;
	}
}
