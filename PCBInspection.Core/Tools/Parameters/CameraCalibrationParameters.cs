using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>相機校正參數</summary>
	public class CameraCalibrationParameters
	{
		/// <summary>棋盤格內角點的水平數量</summary>
		[DisplayName("棋盤格寬度 (格數)")] [Description("棋盤格內角點的水平數量 (通常為 9)")]
		public int PatternWidth { get; set; } = 9;

		/// <summary>棋盤格內角點的垂直數量</summary>
		[DisplayName("棋盤格高度 (格數)")] [Description("棋盤格內角點的垂直數量 (通常為 6)")]
		public int PatternHeight { get; set; } = 6;

		/// <summary>棋盤格每個方格的實際物理邊長 (毫米)</summary>
		[DisplayName("方格大小 (mm)")] [Description("每個方格的實際邊長 (毫米)")]
		public float SquareSize { get; set; } = 25.0f;

		/// <summary>存放校正用標定影像的資料夾路徑</summary>
		[DisplayName("校正影像資料夾")] [Description("包含多張棋盤格影像的資料夾路徑")]
		public string CalibrationImagesFolder { get; set; } = "";

		/// <summary>相機內參矩陣儲存路徑 (.xml)</summary>
		[DisplayName("輸出相機矩陣路徑")] [Description("儲存相機內參矩陣的檔案路徑 (.xml)")]
		public string OutputCameraMatrixPath { get; set; } = "camera_matrix.xml";

		/// <summary>相機畸變係數儲存路徑 (.xml)</summary>
		[DisplayName("輸出畸變係數路徑")] [Description("儲存畸變係數的檔案路徑 (.xml)")]
		public string OutputDistCoeffsPath { get; set; } = "dist_coeffs.xml";

		/// <summary>是否在 Log 中輸出重投影誤差</summary>
		[DisplayName("顯示重投影誤差")] [Description("是否在 Log 中顯示校正的重投影誤差值")]
		public bool ShowReprojectionError { get; set; } = true;
	}
}
