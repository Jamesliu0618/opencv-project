using OpenCvSharp;
using PCBInspection.Core.Detectors;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PCBInspection.Core.Services
{
	/// <summary>
	///     標註影像產生服務 - 在影像上繪製瑕疵框、元件標記和量測資訊
	/// </summary>
	public static class AnnotationService
	{
		/// <summary>
		///     繪製完整標註影像
		/// </summary>
		public static Mat Annotate(Mat image, List<Component> components, List<Defect> defects, InspectionDecision decision, AnnotationOptions options = null)
		{
			if(image == null || image.Empty())
			{
				return image;
			}
			options = options ?? new AnnotationOptions();
			var result = image.Clone();

			// 繪製元件邊框與尺寸
			if(options.ShowComponents && components != null)
			{
				foreach(var comp in components)
				{
					DrawComponent(result, comp, options);
				}
			}

			// 繪製瑕疵標記
			if(options.ShowDefects && defects != null)
			{
				foreach(var defect in defects)
				{
					DrawDefect(result, defect, options);
				}
			}

			// 繪製整體判定結果
			if(options.ShowDecision)
			{
				DrawDecision(result, decision, options);
			}

			// 繪製時間戳
			if(options.ShowTimestamp)
			{
				DrawTimestamp(result, options);
			}
			return result;
		}

		private static void DrawComponent(Mat image, Component comp, AnnotationOptions opts)
		{
			var center = new Point((int)comp.CenterX_Px, (int)comp.CenterY_Px);
			var halfW  = (int)(comp.SizeW_Px / 2);
			var halfH  = (int)(comp.SizeH_Px / 2);
			var rect   = new Rect(center.X - halfW, center.Y - halfH, halfW * 2, halfH * 2);

			// 繪製邊框
			Cv2.Rectangle(image, rect, opts.ComponentColor, opts.LineThickness);

			// 繪製尺寸標籤
			if(opts.ShowMeasurements && comp.WidthMm > 0)
			{
				var label    = $"{comp.WidthMm:F2}x{comp.HeightMm:F2}mm";
				var labelPos = new Point(rect.X, rect.Y - 5);
				Cv2.PutText(image, label, labelPos, HersheyFonts.HersheySimplex, opts.FontScale, opts.MeasurementColor);
			}

			// 繪製中心點
			if(opts.ShowCenterPoint)
			{
				Cv2.Circle(image, center, 3, opts.ComponentColor, -1);
			}
		}

		private static void DrawDefect(Mat image, Defect defect, AnnotationOptions opts)
		{
			if(defect.BoundingBox == null || defect.BoundingBox.Length < 4)
			{
				return;
			}
			Rect rect = new Rect(defect.BoundingBox[0], defect.BoundingBox[1], defect.BoundingBox[2], defect.BoundingBox[3]);

			// 根據嚴重度選擇顏色
			Scalar color = GetSeverityColor(defect.Severity);

			// 繪製邊框（嚴重度越高線條越粗）
			int thickness = Math.Min(defect.Severity, 4);
			Cv2.Rectangle(image, rect, color, thickness);

			// 繪製瑕疵類型標籤
			string label    = $"{defect.Type} (S{defect.Severity})";
			Point  labelPos = new Point(rect.X, rect.Y - 8);
			Cv2.PutText(image, label, labelPos, HersheyFonts.HersheySimplex, opts.FontScale * 0.8, color);

			// 嚴重瑕疵加上警告圖示
			if(defect.Severity >= 4)
			{
				Point iconPos = new Point(rect.X + rect.Width + 5, rect.Y + rect.Height / 2);
				Cv2.PutText(image, "!", iconPos, HersheyFonts.HersheySimplex, 1.0, color, 2);
			}
		}

		private static void DrawDecision(Mat image, InspectionDecision decision, AnnotationOptions opts)
		{
			string text  = decision.ToString();
			Scalar color = decision == InspectionDecision.OK ? new Scalar(0, 200, 0) : decision == InspectionDecision.REVIEW ? new Scalar(0, 200, 200) : new Scalar(0, 0, 255);

			// 繪製半透明背景
			Rect bgRect = new Rect(10, 10, 150, 50);

			using(Mat overlay = image.Clone())
			{
				Cv2.Rectangle(overlay, bgRect, color, -1);
				Cv2.AddWeighted(overlay, 0.3, image, 0.7, 0, image);
			}

			// 繪製判定文字
			Cv2.PutText(image, text, new Point(25, 45), HersheyFonts.HersheySimplex, 1.2, Scalar.White, 2);
		}

		private static void DrawTimestamp(Mat image, AnnotationOptions opts)
		{
			string ts  = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			Point  pos = new Point(image.Width - 200, image.Height - 15);
			Cv2.PutText(image, ts, pos, HersheyFonts.HersheySimplex, 0.5, new Scalar(200, 200, 200));
		}

		private static Scalar GetSeverityColor(int severity)
		{
			switch(severity)
			{
				case 1:
					return new Scalar(200, 200, 0); // 藍綠色 - 輕微
				case 2:
					return new Scalar(0, 200, 200); // 黃色 - 輕度
				case 3:
					return new Scalar(0, 165, 255); // 橙色 - 中度
				case 4:
					return new Scalar(0, 0, 255); // 紅色 - 嚴重
				case 5:
					return new Scalar(0, 0, 180); // 深紅色 - 致命
				default:
					return new Scalar(128, 128, 128);
			}
		}

		/// <summary>
		///     儲存標註影像
		/// </summary>
		public static string SaveAnnotatedImage(Mat annotatedImage, string outputDir, string pcbId)
		{
			Directory.CreateDirectory(outputDir);
			string filename = $"{pcbId}_annotated_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
			string path     = Path.Combine(outputDir, filename);
			Cv2.ImWrite(path, annotatedImage);
			return path;
		}
	}

	/// <summary>
	///     標註選項
	/// </summary>
	public class AnnotationOptions
	{
		public bool   ShowComponents   { get; set; } = true;
		public bool   ShowDefects      { get; set; } = true;
		public bool   ShowDecision     { get; set; } = true;
		public bool   ShowTimestamp    { get; set; } = true;
		public bool   ShowMeasurements { get; set; } = true;
		public bool   ShowCenterPoint  { get; set; } = false;
		public double FontScale        { get; set; } = 0.5;
		public int    LineThickness    { get; set; } = 2;
		public Scalar ComponentColor   { get; set; } = new Scalar(0,   255, 0); // 綠色
		public Scalar MeasurementColor { get; set; } = new Scalar(255, 200, 0); // 藍色
	}
}