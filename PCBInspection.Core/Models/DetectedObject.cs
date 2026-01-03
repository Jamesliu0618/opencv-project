using System.ComponentModel;

namespace PCBInspection.Core.Models
{
	/// <summary>檢測物件資料類別，用於存儲輪廓分析或特徵點偵測的結果</summary>
	public class DetectedObject
	{
		/// <summary>物件識別編號</summary>
		[DisplayName("編號")]
		public int Id { get; set; }

		/// <summary>物件類型名稱</summary>
		[DisplayName("類型")]
		public string Type { get; set; } = "未知";

		/// <summary>物件中心 X 座標 (像素)</summary>
		[DisplayName("中心X")]
		public double CenterX { get; set; }

		/// <summary>物件中心 Y 座標 (像素)</summary>
		[DisplayName("中心Y")]
		public double CenterY { get; set; }

		/// <summary>邊界框寬度 (像素)</summary>
		[DisplayName("寬度")]
		public double Width { get; set; }

		/// <summary>邊界框高度 (像素)</summary>
		[DisplayName("高度")]
		public double Height { get; set; }

		/// <summary>物件所佔面積 (像素平方)</summary>
		[DisplayName("面積")]
		public double Area { get; set; }

		/// <summary>輪廓周長 (像素)</summary>
		[DisplayName("周長")]
		public double Perimeter { get; set; }

		/// <summary>物件圓度 (0-1，1 為正圓)</summary>
		[DisplayName("圓度")]
		public double Circularity { get; set; }

		/// <summary>物件旋轉角度 (度)</summary>
		[DisplayName("角度")]
		public double Angle { get; set; }

		/// <summary>若是圓形，則為其半徑</summary>
		[DisplayName("半徑")]
		public double Radius { get; set; }

		/// <summary>檢測狀態 (如: OK, NG)</summary>
		[DisplayName("狀態")]
		public string Status { get; set; } = "OK";

		/// <summary>判定是否通過檢測</summary>
		[Browsable(false)]
		public bool IsPass { get => Status == "OK"; }

		/// <summary>此物件是否在 UI 中被選中</summary>
		[Browsable(false)]
		public bool IsSelected { get; set; }
	}
}