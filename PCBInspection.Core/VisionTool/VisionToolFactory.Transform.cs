using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		private static void AddTransformTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 05. 幾何變換 (Result Transform)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "旋轉翻轉 (Rotate/Flip)",
				Category          = "05. 幾何變換",
				DefaultParameters = new RotateFlipParameters(),
				Action = (img, p) =>
				{
					var pp     = (RotateFlipParameters)p;
					var result = img.Clone();

					if(pp.Rotation == RotateFlipParameters.RotationType.Rotate90CW)
					{
						Cv2.Rotate(result, result, RotateFlags.Rotate90Clockwise);
					}
					else if(pp.Rotation == RotateFlipParameters.RotationType.Rotate180)
					{
						Cv2.Rotate(result, result, RotateFlags.Rotate180);
					}
					else if(pp.Rotation == RotateFlipParameters.RotationType.Rotate90CCW)
					{
						Cv2.Rotate(result, result, RotateFlags.Rotate90Counterclockwise);
					}

					if(pp.Flip == RotateFlipParameters.FlipType.Horizontal)
					{
						Cv2.Flip(result, result, FlipMode.Y);
					}
					else if(pp.Flip == RotateFlipParameters.FlipType.Vertical)
					{
						Cv2.Flip(result, result, FlipMode.X);
					}
					else if(pp.Flip == RotateFlipParameters.FlipType.Both)
					{
						Cv2.Flip(result, result, FlipMode.XY);
					}
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "影像縮放 (Resize)",
				Category          = "05. 幾何變換",
				DefaultParameters = new ResizeParameters(),
				Action = (img, p) =>
				{
					var  pp     = (ResizeParameters)p;
					var  result = new Mat();
					Size sz     = new Size();

					if(pp.Mode == ResizeParameters.ResizeMode.ByScale)
					{
						sz = new Size(0, 0); // calculated by fx, fy
					}
					else
					{
						sz = new Size(pp.TargetWidth, pp.TargetHeight);
					}
					Cv2.Resize(img, result, sz, pp.Scale, pp.Scale, pp.Interpolation);
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "影像裁切 (Crop)",
				Category          = "05. 幾何變換",
				DefaultParameters = new CropParameters(),
				Action = (img, p) =>
				{
					var pp = (CropParameters)p;

					// Validate rect
					int x = Math.Max(0, pp.X);
					int y = Math.Max(0, pp.Y);
					int w = Math.Min(pp.Width,  img.Width  - x);
					int h = Math.Min(pp.Height, img.Height - y);

					if(w > 0 && h > 0)
					{
						var roi    = new Rect(x, y, w, h);
						var result = new Mat(img, roi).Clone(); // Clone is important to own data
						return (true, result, new List<Defect>());
					}
					return (true, img.Clone(), new List<Defect>()); // Fail safe
				},
			});
		}
	}
}