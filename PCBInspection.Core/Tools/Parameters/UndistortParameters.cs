using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>影像畸變矯正參數</summary>
	public class UndistortParameters
	{
		/// <summary>載入相機內參矩陣的檔案路徑 (.xml)</summary>
		[DisplayName("相機矩陣檔案路徑")] [Description("載入相機內參矩陣的檔案路徑 (.xml)")]
		public string CameraMatrixPath { get; set; } = "camera_matrix.xml";

		/// <summary>載入畸變係數的檔案路徑 (.xml)</summary>
		[DisplayName("畸變係數檔案路徑")] [Description("載入畸變係數的檔案路徑 (.xml)")]
		public string DistCoeffsPath { get; set; } = "dist_coeffs.xml";

		/// <summary>是否自動裁剪掉矯正後產生的黑邊</summary>
		[DisplayName("自動裁剪黑邊")] [Description("矯正後自動裁剪邊緣的黑色區域")]
		public bool AutoCropBlackBorder { get; set; } = true;
	}
}
