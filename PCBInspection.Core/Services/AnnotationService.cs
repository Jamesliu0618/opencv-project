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
		///     繪製檢測物件標籤（編號、面積、圓度等）
		/// </summary>
		public static void DrawObjectLabels(Mat image, List<DetectedObject> objects, ObjectLabelStyle style = null)
		{
			if (image == null || image.Empty() || objects == null) return;

			style = style ?? new ObjectLabelStyle();

			for (int i = 0; i < objects.Count; i++)
			{
				var obj   = objects[i];
				var color = obj.IsOk ? style.OkColor : style.NgColor;
				var rect  = new Rect(obj.BoundingBox.X, obj.BoundingBox.Y, obj.BoundingBox.Width, obj.BoundingBox.Height);

				// 繪製邊界框
				if (style.ShowBoundingBox)
				{
					Cv2.Rectangle(image, rect, color, style.LineThickness);
				}

				// 建立標籤文字
				var labels = new List<string>();

				if (style.ShowId)
				{
					labels.Add($"#{obj.ObjectId}");
				}
				if (style.ShowArea && obj.Area > 0)
				{
					labels.Add($"{obj.Area:F0}px²");
				}
				if (style.ShowCircularity && obj.Circularity > 0)
				{
					labels.Add($"C:{obj.Circularity:F2}");
				}

				if (labels.Count == 0) continue;

				string labelText = string.Join(" | ", labels);

				// 計算文字大小
				int baseline;
				var textSize = Cv2.GetTextSize(labelText, HersheyFonts.HersheySimplex, style.FontScale, 1, out baseline);

				// 繪製標籤背景
				var labelRect = new Rect(rect.X, rect.Y - textSize.Height - 6, textSize.Width + 8, textSize.Height + 6);
				if (labelRect.Y < 0)
				{
					labelRect.Y = rect.Y + rect.Height + 2; // 移到下方
				}

				using (var overlay = image.Clone())
				{
					Cv2.Rectangle(overlay, labelRect, color, -1);
					Cv2.AddWeighted(overlay, 0.6, image, 0.4, 0, image);
				}

				// 繪製文字
				var textPos = new Point(labelRect.X + 4, labelRect.Y + labelRect.Height - 4);
				Cv2.PutText(image, labelText, textPos, HersheyFonts.HersheySimplex, style.FontScale, Scalar.White, 1);

				// 繪製中心點
				Cv2.Circle(image, new Point(obj.CenterX, obj.CenterY), 3, color, -1);
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

		// 物件標籤選項
		/// <summary>是否顯示物件編號</summary>
		public bool ShowObjectId { get; set; } = true;

		/// <summary>是否顯示面積</summary>
		public bool ShowArea { get; set; } = false;

		/// <summary>是否顯示圓度</summary>
		public bool ShowCircularity { get; set; } = false;

		/// <summary>OK 物件顏色</summary>
		public Scalar OkColor { get; set; } = new Scalar(0, 255, 0); // 綠色

		/// <summary>NG 物件顏色</summary>
		public Scalar NgColor { get; set; } = new Scalar(0, 0, 255); // 紅色

		/// <summary>標籤背景透明度 (0-1)</summary>
		public double LabelBackgroundOpacity { get; set; } = 0.6;
	}

	/// <summary>檢測物件樣式設定</summary>
	public class ObjectLabelStyle
	{
		/// <summary>是否顯示編號</summary>
		public bool ShowId { get; set; } = true;

		/// <summary>是否顯示面積</summary>
		public bool ShowArea { get; set; } = true;

		/// <summary>是否顯示圓度</summary>
		public bool ShowCircularity { get; set; } = false;

		/// <summary>是否顯示邊界框</summary>
		public bool ShowBoundingBox { get; set; } = true;

		/// <summary>OK 物件顏色</summary>
		public Scalar OkColor { get; set; } = new Scalar(0, 255, 0);

		/// <summary>NG 物件顏色</summary>
		public Scalar NgColor { get; set; } = new Scalar(0, 0, 255);

		/// <summary>標籤字體大小</summary>
		public double FontScale { get; set; } = 0.45;

		/// <summary>線條粗細</summary>
		public int LineThickness { get; set; } = 2;
	}
}
