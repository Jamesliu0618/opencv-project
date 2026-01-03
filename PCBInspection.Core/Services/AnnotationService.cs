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
		///     執行影像標註。
		///     根據傳入的元件、缺陷與判定結果，在影像副本上繪製視覺化資訊並回傳。
		/// </summary>
		public static Mat Annotate(Mat image, List<Component> components, List<Defect> defects, InspectionDecision decision, AnnotationOptions options = null)
		{
			if(image == null || image.Empty())
			{
				return image;
			}
			options = options ?? new AnnotationOptions();
			var result = image.Clone(); // 複製影像以避免修改原始圖層

			// 1. 繪製元件邊框與尺寸 (如: 黃色矩形框)
			if(options.ShowComponents && components != null)
			{
				foreach(var comp in components)
				{
					DrawComponent(result, comp, options);
				}
			}

			// 2. 繪製瑕疵標記 (依嚴重度決定顏色與粗細)
			if(options.ShowDefects && defects != null)
			{
				foreach(var defect in defects)
				{
					DrawDefect(result, defect, options);
				}
			}

			// 3. 繪製整體判定結果 (左上角 OK/NG 看板)
			if(options.ShowDecision)
			{
				DrawDecision(result, decision, options);
			}

			// 4. 繪製時間戳 (右下角系統時間)
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
			// 取得瑕疵座標 [X, Y, W, H]
			Rect rect = new Rect(defect.BoundingBox[0], defect.BoundingBox[1], defect.BoundingBox[2], defect.BoundingBox[3]);

			// 根據嚴重度選擇對應顏色 (1-輕微綠色 -> 5-致命深紅)
			Scalar color = GetSeverityColor(defect.Severity);

			// 繪製邊框 (嚴重度越高則線條越粗，範圍 1~4 px)
			int thickness = Math.Min(defect.Severity, 4);
			Cv2.Rectangle(image, rect, color, thickness);

			// 繪製瑕疵類型與嚴重度標籤 (如: "Scratch (S3)")
			string label    = $"{defect.Type} (S{defect.Severity})";
			Point  labelPos = new Point(rect.X, rect.Y - 8);
			Cv2.PutText(image, label, labelPos, HersheyFonts.HersheySimplex, opts.FontScale * 0.8, color);

			// 若為嚴重瑕疵 (S4, S5)，額外在右側繪製驚嘆號警示
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
		///     繪製一般檢測物件的視覺標籤。
		///     包含序號、面積、圓度等統計資訊，並自動處理標籤避讓（避免超出影像頂部）。
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

				// 繪製邊界框 (矩形框)
				if (style.ShowBoundingBox)
				{
					Cv2.Rectangle(image, rect, color, style.LineThickness);
				}

				// 組合要顯示的標籤內容
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

				// 1. 計算文字所需的尺寸 (用於繪製背景底色塊)
				int baseline;
				var textSize = Cv2.GetTextSize(labelText, HersheyFonts.HersheySimplex, style.FontScale, 1, out baseline);

				// 2. 決定標籤放置位置 (預設頂端，若空間不足則移至底部)
				var labelRect = new Rect(rect.X, rect.Y - textSize.Height - 6, textSize.Width + 8, textSize.Height + 6);
				if (labelRect.Y < 0)
				{
					labelRect.Y = rect.Y + rect.Height + 2; 
				}

				// 3. 繪製半透明背景底色 (提高文字辨識度)
				using (var overlay = image.Clone())
				{
					Cv2.Rectangle(overlay, labelRect, color, -1);
					Cv2.AddWeighted(overlay, 0.6, image, 0.4, 0, image);
				}

				// 4. 繪製標籤文字與中心輔助點
				var textPos = new Point(labelRect.X + 4, labelRect.Y + labelRect.Height - 4);
				Cv2.PutText(image, labelText, textPos, HersheyFonts.HersheySimplex, style.FontScale, Scalar.White, 1);
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
