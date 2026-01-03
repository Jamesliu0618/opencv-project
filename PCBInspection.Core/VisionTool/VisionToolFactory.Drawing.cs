using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
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