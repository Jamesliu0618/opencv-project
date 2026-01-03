using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace PCBInspection.Core.Models
{
	/// <summary>
	/// 幾何座標點，支援透過 UI 編輯器選擇檢測物件
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Editor("PCBInspection.UI.Controls.GeometryPointEditor, PCBInspection.UI", "System.Drawing.Design.UITypeEditor, System.Drawing")]
	public class GeoPoint
	{
		public GeoPoint() { }
		public GeoPoint(int x, int y) { X = x; Y = y; }

		[Description("X 座標")]
		public int X { get; set; } = 0;

		[Description("Y 座標")]
		public int Y { get; set; } = 0;

		public override string ToString()
		{
			return $"{X}, {Y}";
		}
	}
}
