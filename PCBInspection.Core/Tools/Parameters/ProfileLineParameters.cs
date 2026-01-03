using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>灰階剖面線分析參數</summary>
	public class ProfileLineParameters
	{
		/// <summary>剖面線起點 X 座標</summary>
		[DisplayName("起點 X")] [Description("剖面線起點的 X 座標")]
		public int StartX { get; set; } = 0;

		/// <summary>剖面線起點 Y 座標</summary>
		[DisplayName("起點 Y")] [Description("剖面線起點的 Y 座標")]
		public int StartY { get; set; } = 0;

		/// <summary>剖面線終點 X 座標</summary>
		[DisplayName("終點 X")] [Description("剖面線終點的 X 座標")]
		public int EndX { get; set; } = 100;

		/// <summary>剖面線終點 Y 座標</summary>
		[DisplayName("終點 Y")] [Description("剖面線終點的 Y 座標")]
		public int EndY { get; set; } = 0;

		/// <summary>是否開啟獨立視窗顯示剖面曲線圖 (Intensity Profile)</summary>
		[DisplayName("顯示灰階剖面圖")] [Description("以獨立視窗顯示灰階剖面曲線圖")]
		public bool ShowProfileWindow { get; set; } = true;

		/// <summary>剖面取樣的線條寬度 (像素平均)</summary>
		[DisplayName("剖面線寬度 (px)")] [Description("取樣時平均的線條寬度")]
		public int LineWidth { get; set; } = 1;

		/// <summary>是否將剖面數據匯出為 CSV 檔案</summary>
		[DisplayName("輸出數據到 CSV")] [Description("將剖面數據輸出為 CSV 檔案")]
		public bool OutputToCsv { get; set; } = false;

		/// <summary>CSV 檔案儲存路徑</summary>
		[DisplayName("CSV 輸出路徑")] [Description("CSV 檔案的儲存路徑")]
		public string CsvOutputPath { get; set; } = "profile_data.csv";
	}
}
