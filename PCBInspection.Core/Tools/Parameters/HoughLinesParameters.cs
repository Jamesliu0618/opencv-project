using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>霍夫直線偵測參數</summary>
	public class HoughLinesParameters
	{
		/// <summary>是否使用概率霍夫變換 (HoughLinesP)</summary>
		[DisplayName("使用機率霍夫")] [Description("使用 HoughLinesP (機率霍夫) 而非標準霍夫")]
		public bool UseProbabilistic { get; set; } = true;

		/// <summary>是否自動套用 Canny 邊緣偵測</summary>
		[DisplayName("自動邊緣偵測")] [Description("自動套用 Canny 邊緣偵測 (建議啟用，除非輸入已為邊緣圖)")]
		public bool AutoCanny { get; set; } = true;

		/// <summary>Canny 低閾值</summary>
		[DisplayName("Canny 低閾值")] [Description("Canny 邊緣偵測的低閾值 (僅當自動邊緣偵測啟用時)")]
		public double CannyThreshold1 { get; set; } = 50;

		/// <summary>Canny 高閾值</summary>
		[DisplayName("Canny 高閾值")] [Description("Canny 邊緣偵測的高閾值 (建議為低閾值的 2-3 倍)")]
		public double CannyThreshold2 { get; set; } = 150;

		/// <summary>距離解析度 (像素)</summary>
		[DisplayName("Rho (px)")] [Description("累加器的距離解析度 (像素)")]
		public double Rho { get; set; } = 1;

		/// <summary>角度解析度 (度)</summary>
		[DisplayName("Theta (度)")] [Description("累加器的角度解析度 (度)")]
		public double ThetaDeg { get; set; } = 1;

		/// <summary>累加器閾值，大於此值的候選線段才會被保留</summary>
		[DisplayName("閾值")] [Description("累加器閾值，越高則線條越確定")]
		public int Threshold { get; set; } = 50;

		/// <summary>最短線段長度 (像素，僅適用機率霍夫)</summary>
		[DisplayName("最小線長 (px)")] [Description("機率霍夫的最小線段長度")]
		public double MinLineLength { get; set; } = 50;

		/// <summary>最大線段間隙 (像素，僅適用機率霍夫)</summary>
		[DisplayName("最大線間距 (px)")] [Description("機率霍夫的最大線段間距")]
		public double MaxLineGap { get; set; } = 10;

		/// <summary>繪製線條顏色</summary>
		[DisplayName("線條顏色 (BGR)")] [Description("繪製直線的顏色 (格式: B,G,R)")]
		public string LineColorBGR { get; set; } = "255,255,0";

		/// <summary>線條粗細</summary>
		[DisplayName("線條粗細 (px)")] [Description("繪製直線的粗細")]
		public int LineThickness { get; set; } = 2;

		/// <summary>最大輸出直線數量</summary>
		[DisplayName("最大直線數量")] [Description("限制輸出的直線數量。設為 0 表示不限制。")]
		public int MaxLines { get; set; } = 50;
	}
}
