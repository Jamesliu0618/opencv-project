using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using PCBInspection.Core.Models;

namespace PCBInspection.Core.Tools
{
	/// <summary>幾何測量參數</summary>
	public class MeasurementToolParameters
	{
		/// <summary>測量類型</summary>
		public enum MeasureType
		{
			/// <summary>點對點距離</summary>
			Distance,
			/// <summary>夾角測量</summary>
			Angle,
			/// <summary>封閉區域面積</summary>
			Area,
			/// <summary>周長</summary>
			Perimeter,
			/// <summary>圓形直徑</summary>
			Diameter,
		}

		/// <summary>每像素對應的實際長度 (毫米/像素)</summary>
		[DisplayName("像素比例 (mm/px)")] [Description("每像素對應的實際長度 (毫米/像素)")]
		public double PixelScale { get; set; } = 0.1;

		/// <summary>測量目標類型</summary>
		[DisplayName("測量類型")] [Description("選擇測量的幾何量類型")]
		public MeasureType Type { get; set; } = MeasureType.Distance;

		/// <summary>結果顯示單位 (mm, um, cm...)</summary>
		[DisplayName("顯示單位")] [Description("測量結果顯示的單位 (mm, μm, cm)")]
		public string DisplayUnit { get; set; } = "mm";

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")] [Description("測量結果顯示的小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>測量起點 X 座標</summary>
		[DisplayName("起點 X")] [Description("測量起點的 X 座標")]
		public int StartX { get; set; } = 0;

		/// <summary>測量起點 Y 座標</summary>
		[DisplayName("起點 Y")] [Description("測量起點的 Y 座標")]
		public int StartY { get; set; } = 0;

		/// <summary>測量終點 X 座標</summary>
		[DisplayName("終點 X")] [Description("測量終點的 X 座標")]
		public int EndX { get; set; } = 100;

		/// <summary>測量終點 Y 座標</summary>
		[DisplayName("終點 Y")] [Description("測量終點的 Y 座標")]
		public int EndY { get; set; } = 100;

		/// <summary>是否在輸出影像上標註測量結果</summary>
		[DisplayName("顯示結果於影像")] [Description("是否將測量結果標註在輸出影像上")]
		public bool DrawOnImage { get; set; } = true;

		/// <summary>是否使用物件編號做為測量參考點</summary>
		[Category("物件參考")]
		[DisplayName("使用物件參考")] [Description("啟用後將使用指定編號的物件中心點進行測量，而非手動座標")]
		public bool UseObjectReference { get; set; } = false;

		/// <summary>起點參考的物件編號 (例如: "1")</summary>
		[Category("物件參考")]
		[DisplayName("起點物件編號")] [Description("測量起點參考的物件編號 (需搭配前置的物件分析步驟)")]
		public string StartObjectId { get; set; } = "1";

		/// <summary>終點參考的物件編號 (例如: "2")</summary>
		[Category("物件參考")]
		[DisplayName("終點物件編號")] [Description("測量終點參考的物件編號")]
		public string EndObjectId { get; set; } = "2";

		/// <summary>上下文缺陷列表 (執行時自動注入，不顯示)</summary>
		[Browsable(false)]
		[XmlIgnore]
		public List<Defect> ContextDefects { get; set; }
	}
}
