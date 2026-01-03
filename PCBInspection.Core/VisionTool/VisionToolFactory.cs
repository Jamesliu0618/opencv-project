using OpenCvSharp;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		// 定義工具執行的委派簽名
		public delegate (bool IsOk, Mat ResultImage, List<Defect> Defects) VisionAction(Mat input, object param);

		// Logging event: Message, IsError
		public static event Action<string, bool> OnLog;

		public static List<ToolDefinition> GetAllTools()
		{
			var tools = new List<ToolDefinition>();

			// Call partial methods to populate tools
			AddPreprocessingTools(tools);
			AddColorProcessingTools(tools);
			AddFeatureExtractionTools(tools);
			AddDrawingTools(tools);
			AddTransformTools(tools);
			AddDenoiseTools(tools);
			AddKeypointsTools(tools);
			AddCalibrationTools(tools);
			AddAnalysisTools(tools);
			AddBackgroundDefectTools(tools);

			// Wrap calls with logging
			var wrappedTools = new List<ToolDefinition>();

			foreach(var tool in tools)
			{
				var originalAction = tool.Action;

				tool.Action = (img, p) =>
				{
					try
					{
						var res = originalAction(img, p);
						OnLog?.Invoke($"[Vision] {tool.Name} OK.", false);
						return res;
					}
					catch(Exception ex)
					{
						OnLog?.Invoke($"[Vision] {tool.Name} ERROR: {ex.Message}", true);
						throw;
					}
				};
				wrappedTools.Add(tool);
			}
			return wrappedTools;
		}

		private static Scalar ParseColor(string colorStr)
		{
			try
			{
				var parts = colorStr.Split(',');

				if(parts.Length == 3)
				{
					return new Scalar(double.Parse(parts[0]), double.Parse(parts[1]), double.Parse(parts[2]));
				}
			}
			catch
			{
			}
			return Scalar.Green;
		}

		public class ToolDefinition
		{
			public string       Name              { get; set; }
			public string       Category          { get; set; }
			public object       DefaultParameters { get; set; }
			public VisionAction Action            { get; set; }
		}
	}
}