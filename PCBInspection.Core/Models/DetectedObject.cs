using System.ComponentModel;

namespace PCBInspection.Core.Models
{
	/// <summary>檢測物件資料類別</summary>
	public class DetectedObject
	{
		[DisplayName("編號")]
		public int Id { get; set; }

		[DisplayName("類型")]
		public string Type { get; set; } = "未知";

		[DisplayName("中心X")]
		public double CenterX { get; set; }

		[DisplayName("中心Y")]
		public double CenterY { get; set; }

		[DisplayName("寬度")]
		public double Width { get; set; }

		[DisplayName("高度")]
		public double Height { get; set; }

		[DisplayName("面積")]
		public double Area { get; set; }

		[DisplayName("周長")]
		public double Perimeter { get; set; }

		[DisplayName("圓度")]
		public double Circularity { get; set; }

		[DisplayName("角度")]
		public double Angle { get; set; }

		[DisplayName("半徑")]
		public double Radius { get; set; }

		[DisplayName("狀態")]
		public string Status { get; set; } = "OK";

		[Browsable(false)]
		public bool IsPass { get => Status == "OK"; }

		[Browsable(false)]
		public bool IsSelected { get; set; }
	}
}