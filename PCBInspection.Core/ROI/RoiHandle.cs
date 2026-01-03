namespace PCBInspection.Core.ROI
{
	/// <summary>
	/// ROI 控制點類型定義
	/// </summary>
	public enum RoiHandle
	{
		None,       // 無
		Body,       // 本體（用於平移）
		TopLeft,    // 左上角
		Top,        // 頂邊
		TopRight,   // 右上角
		Right,      // 右邊
		BottomRight, // 右下角
		Bottom,     // 底邊
		BottomLeft, // 左下角
		Left,       // 左邊
		Edge        // 圓周邊緣 (用於調整半徑)
	}
}
