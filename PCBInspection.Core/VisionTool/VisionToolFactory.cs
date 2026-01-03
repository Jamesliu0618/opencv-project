using OpenCvSharp;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	/// <summary>視覺工具工廠類別，提供各類影像處理工具的定義與實作</summary>
	public static partial class VisionToolFactory
	{
		/// <summary>視覺工具執行的委派簽名</summary>
		public delegate (bool IsOk, Mat ResultImage, List<Defect> Defects) VisionAction(Mat input, object param);

		/// <summary>記錄日誌的事件 (訊息內容, 是否為錯誤)</summary>
		public static event Action<string, bool> OnLog;

		/// <summary>取得所有可用的視覺工具清單</summary>
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

		/// <summary>解析顏色字串 (格式: B,G,R)</summary>
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

		/// <summary>工具定義類別，封裝工具名稱、類別、參數與執行動作</summary>
		public class ToolDefinition
		{
			/// <summary>工具名稱</summary>
			public string       Name              { get; set; }
			/// <summary>所屬類別</summary>
			public string       Category          { get; set; }
			/// <summary>預設參數物件</summary>
			public object       DefaultParameters { get; set; }
			/// <summary>執行工具的動作</summary>
			public VisionAction Action            { get; set; }
		}
	}
}