using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>幾何匹配 (Geometric / Edge-based Matching) 參數</summary>
	public class GeometricMatchParameters
	{
		// ===== 核心參數 =====

		/// <summary>模板影像之磁碟路徑</summary>
		[Category("00. ROI 設定")]
    [DisplayName("來源 ROI 編號")]
    [Description("指定來源影像的搜尋區域 ROI 編號 (0 = 全圖)")]
    public int SourceRoiIndex { get; set; } = 0;

    [Category("00. ROI 設定")]
    [DisplayName("模板 ROI 編號")]
    [Description("指定模板影像的擷取區域 ROI 編號 (0 = 全圖)")]
    public int TemplateRoiIndex { get; set; } = 0;

    [Category("01. 模板設定")]
    [DisplayName("模板路徑")] [Description("模板影像檔案的完整路徑 (建議使用具有明顯邊緣特徵的圖案)")]
		public string TemplatePath { get; set; } = "";

		/// <summary>匹配確信度閾值 (0-1)</summary>
		[Category("02. 匹配設定")]
		[DisplayName("得分閾值")] [Description("匹配得分閾值 (0-1)，越高越嚴格")]
		public double MatchThreshold { get; set; } = 0.5;

		// ===== 邊緣特徵參數 =====

		/// <summary>Canny 邊緣偵測低閾值</summary>
		[Category("03. 邊緣提取")]
		[DisplayName("邊緣下限值")] [Description("Canny 算子的低閾值 (用於提取幾何輪廓)")]
		public double CannyThreshold1 { get; set; } = 50;

		/// <summary>Canny 邊緣偵測高閾值</summary>
		[Category("03. 邊緣提取")]
		[DisplayName("邊緣上限值")] [Description("Canny 算子的高閾值 (建議設為下限值的 2-3 倍)")]
		public double CannyThreshold2 { get; set; } = 150;

		// ===== 旋轉搜尋參數 =====

		/// <summary>是否搜尋選擇</summary>
		[Category("04. 旋轉搜尋")]
		[DisplayName("啟動旋轉搜尋")] [Description("是否搜尋不同角度的工件 (注意：角度範圍越大速度越慢)")]
		public bool EnableRotation { get; set; } = false;

		/// <summary>起始角度 (度)</summary>
		[Category("04. 旋轉搜尋")]
		[DisplayName("起始角度")] [Description("搜尋的小角度範圍開始值 (例如 -10)")]
		public double MinAngle { get; set; } = -180;

		/// <summary>結束角度 (度)</summary>
		[Category("04. 旋轉搜尋")]
		[DisplayName("結束角度")] [Description("搜尋的小角度範圍結束值 (例如 10)")]
		public double MaxAngle { get; set; } = 180;

		/// <summary>角度間距 (度)</summary>
		[Category("04. 旋轉搜尋")]
		[DisplayName("角度步進")] [Description("每次旋轉搜尋的跳動角度 (步進越小越精準，但越慢)")]
		public double AngleStep { get; set; } = 5.0;

		// ===== 效能優化 =====

		/// <summary>預縮放比例 (0.25-1.0)</summary>
		[Category("05. 效能優化")]
		[DisplayName("運算縮放比例")] [Description("降低影像解析度以提升搜尋速度 (建議 0.5)")]
		public double SearchScale { get; set; } = 1.0;
	}
}
