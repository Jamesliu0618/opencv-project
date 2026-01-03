using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>色彩空間轉換參數</summary>
	public class ColorConvertParameters
	{
		/// <summary>轉換目標類型</summary>
		public enum ConversionType
		{
			/// <summary>BGR 轉灰階</summary>
			BGR2Gray,
			/// <summary>BGR 轉 HSV</summary>
			BGR2HSV,
			/// <summary>BGR 轉 Lab</summary>
			BGR2Lab,
			/// <summary>HSV 轉 BGR</summary>
			HSV2BGR,
			/// <summary>灰階轉 BGR (3通道)</summary>
			Gray2BGR,
		}

		/// <summary>選擇色彩轉換類型</summary>
		[DisplayName("轉換類型")] [Description("色彩空間轉換:\n- BGR2Gray: 彩色轉灰階\n- BGR2HSV: 轉 HSV (色相/飽和度/明度)\n- BGR2Lab: 轉 Lab 色彩空間\n- HSV2BGR: HSV 轉回 BGR\n- Gray2BGR: 灰階轉 BGR (3通道)")]
		public ConversionType Type { get; set; } = ConversionType.BGR2Gray;
	}
}
