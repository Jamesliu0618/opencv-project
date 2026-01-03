using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		private static void AddDenoiseTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 06. 降噪 (Denoise)
			// =========================================================
			tools.Add(new ToolDefinition
			{
				Name              = "非局部均值降噪 (NlMeans)",
				Category          = "06. 降噪",
				DefaultParameters = new DenoiseParameters(),
				Action = (img, p) =>
				{
					var pp     = (DenoiseParameters)p;
					var result = new Mat();
					var temp   = new Mat();

					// NlMeans only supports 8-bit?
					// Supports 8-bit 1-channel, 2-channel, 3-channel
					img.CopyTo(temp);

					if(img.Channels() == 3)
					{
						Cv2.FastNlMeansDenoisingColored(temp, result, pp.FilterStrength, pp.FilterStrength, pp.TemplateWindowSize, pp.SearchWindowSize);
					}
					else
					{
						Cv2.FastNlMeansDenoising(temp, result, pp.FilterStrength, pp.TemplateWindowSize, pp.SearchWindowSize);
					}
					temp.Dispose();
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}