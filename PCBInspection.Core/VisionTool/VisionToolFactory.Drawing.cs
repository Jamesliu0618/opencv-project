using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	/// <summary>
	/// 註冊繪圖與標註類別工具。
	/// 包含：繪製文字與圖形標註功能。
	/// </summary>
	public static partial class VisionToolFactory
	{
		/// <summary>
		/// 註冊幾何變換類別工具。
		/// 包含：旋轉翻轉、縮放與裁切 (ROI) 運算。
		/// </summary>
		/// <param name="tools">欲加入工具定義的清單物件</param>
		private static void AddDrawingTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 04. 繪圖與標註 (Drawing)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "繪製文字 (Draw Text)",
				Category          = "04. 繪圖與標註",
				DefaultParameters = new DrawTextParameters(),
				Action = (img, p) =>
				{
					var pp     = (DrawTextParameters)p;
					var result = img.Clone();

					// Ensure BGR
					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					var color = ParseColor(pp.ColorBGR);
					Cv2.PutText(result, pp.Text, new Point(pp.X, pp.Y), HersheyFonts.HersheySimplex, pp.FontScale, color, pp.Thickness);
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}