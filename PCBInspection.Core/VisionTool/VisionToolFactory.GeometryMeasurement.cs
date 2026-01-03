using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCBInspection.Core.Services
{
	/// <summary>幾何量測工具 (對應 AISYS OVK OvkMsr 模組)</summary>
	public static partial class VisionToolFactory
	{
		/// <summary>註冊幾何量測類別工具</summary>
		private static void AddGeometryMeasurementTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 14. 幾何量測工具 (Geometry Measurement - AISYS OVK 對應)
			// =========================================================

			// ---------------------------
			// 角度量測 (AxAngleMsr)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "角度量測 (Angle Measurement)",
				Category          = "14. 幾何量測",
				DefaultParameters = new AngleMeasurementParameters(),
				Action = (img, p) =>
				{
					var pp     = (AngleMeasurementParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					else if (result.Channels() == 4)
						Cv2.CvtColor(result, result, ColorConversionCodes.BGRA2BGR);

					double angleDeg;
					Scalar color = new Scalar(0, 255, 0);

					if (pp.Mode == AngleMeasurementParameters.AngleMode.ThreePoints)
					{
						// 三點夾角計算：頂點 V，端點 P1 和 P2
						double v1x = pp.Point1X - pp.VertexX;
						double v1y = pp.Point1Y - pp.VertexY;
						double v2x = pp.Point2X - pp.VertexX;
						double v2y = pp.Point2Y - pp.VertexY;

						double dot      = v1x * v2x + v1y * v2y;
						double mag1     = Math.Sqrt(v1x * v1x + v1y * v1y);
						double mag2     = Math.Sqrt(v2x * v2x + v2y * v2y);
						double cosAngle = dot / (mag1 * mag2 + 1e-10);
						cosAngle        = Math.Max(-1, Math.Min(1, cosAngle));
						angleDeg        = Math.Acos(cosAngle) * 180.0 / Math.PI;

						if (pp.DrawOnImage)
						{
							Cv2.Line(result, new Point(pp.VertexX, pp.VertexY), new Point(pp.Point1X, pp.Point1Y), color, 2);
							Cv2.Line(result, new Point(pp.VertexX, pp.VertexY), new Point(pp.Point2X, pp.Point2Y), color, 2);
							Cv2.Circle(result, new Point(pp.VertexX, pp.VertexY), 5, color, -1);
							Cv2.Circle(result, new Point(pp.Point1X, pp.Point1Y), 4, color, -1);
							Cv2.Circle(result, new Point(pp.Point2X, pp.Point2Y), 4, color, -1);
						}
					}
					else
					{
						// 兩線夾角計算
						double d1x = pp.Line1EndX - pp.Line1StartX;
						double d1y = pp.Line1EndY - pp.Line1StartY;
						double d2x = pp.Line2EndX - pp.Line2StartX;
						double d2y = pp.Line2EndY - pp.Line2StartY;

						double dot      = d1x * d2x + d1y * d2y;
						double mag1     = Math.Sqrt(d1x * d1x + d1y * d1y);
						double mag2     = Math.Sqrt(d2x * d2x + d2y * d2y);
						double cosAngle = dot / (mag1 * mag2 + 1e-10);
						cosAngle        = Math.Max(-1, Math.Min(1, cosAngle));
						angleDeg        = Math.Acos(cosAngle) * 180.0 / Math.PI;

						if (pp.DrawOnImage)
						{
							Cv2.Line(result, new Point(pp.Line1StartX, pp.Line1StartY), new Point(pp.Line1EndX, pp.Line1EndY), color, 2);
							Cv2.Line(result, new Point(pp.Line2StartX, pp.Line2StartY), new Point(pp.Line2EndX, pp.Line2EndY), new Scalar(255, 255, 0), 2);
						}
					}

					string format = $"F{pp.DecimalPlaces}";
					string text   = $"{angleDeg.ToString(format)}°";

					if (pp.DrawOnImage)
					{
						int textX = pp.Mode == AngleMeasurementParameters.AngleMode.ThreePoints ? pp.VertexX + 10 : (pp.Line1StartX + pp.Line1EndX) / 2;
						int textY = pp.Mode == AngleMeasurementParameters.AngleMode.ThreePoints ? pp.VertexY - 10 : (pp.Line1StartY + pp.Line1EndY) / 2;
						Cv2.PutText(result, text, new Point(textX, textY), HersheyFonts.HersheySimplex, 0.7, color, 2);
					}

					OnLog?.Invoke($"[角度量測] 夾角: {text}", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 直線交點 (AxIntersectionMsr)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "直線交點 (Line Intersection)",
				Category          = "14. 幾何量測",
				DefaultParameters = new LineIntersectionParameters(),
				Action = (img, p) =>
				{
					var pp     = (LineIntersectionParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					else if (result.Channels() == 4)
						Cv2.CvtColor(result, result, ColorConversionCodes.BGRA2BGR);

					Scalar colorLine1  = new Scalar(0, 255, 0);
					Scalar colorLine2  = new Scalar(255, 255, 0);
					Scalar colorMarker = new Scalar(0, 0, 255);

					// 計算交點 (使用向量叉積法)
					double x1 = pp.Line1StartX, y1 = pp.Line1StartY;
					double x2 = pp.Line1EndX,   y2 = pp.Line1EndY;
					double x3 = pp.Line2StartX, y3 = pp.Line2StartY;
					double x4 = pp.Line2EndX,   y4 = pp.Line2EndY;

					double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

					if (Math.Abs(denom) < 1e-10)
					{
						OnLog?.Invoke("[直線交點] 兩線平行，無交點", true);
						if (pp.DrawOnImage)
						{
							Cv2.Line(result, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), colorLine1, 2);
							Cv2.Line(result, new Point((int)x3, (int)y3), new Point((int)x4, (int)y4), colorLine2, 2);
							Cv2.PutText(result, "Parallel", new Point(10, 30), HersheyFonts.HersheySimplex, 0.7, colorMarker, 2);
						}
						return (true, result, new List<Defect>());
					}

					double px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denom;
					double py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denom;

					if (pp.DrawOnImage)
					{
						Cv2.Line(result, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), colorLine1, 2);
						Cv2.Line(result, new Point((int)x3, (int)y3), new Point((int)x4, (int)y4), colorLine2, 2);
						Cv2.Circle(result, new Point((int)px, (int)py), pp.MarkerRadius, colorMarker, -1);
						Cv2.PutText(result, $"({px:F1}, {py:F1})", new Point((int)px + 10, (int)py - 10), HersheyFonts.HersheySimplex, 0.5, colorMarker, 1);
					}

					OnLog?.Invoke($"[直線交點] 交點座標: ({px:F2}, {py:F2})", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 點到線距離 (AxPointLineDistanceMsr)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "點線距離 (Point-Line Distance)",
				Category          = "14. 幾何量測",
				DefaultParameters = new PointLineDistanceParameters(),
				Action = (img, p) =>
				{
					var pp     = (PointLineDistanceParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					else if (result.Channels() == 4)
						Cv2.CvtColor(result, result, ColorConversionCodes.BGRA2BGR);

					Scalar colorLine  = new Scalar(0, 255, 0);
					Scalar colorPoint = new Scalar(0, 0, 255);
					Scalar colorDist  = new Scalar(255, 255, 0);

					// 點到直線距離公式: |ax + by + c| / sqrt(a² + b²)
					double x0 = pp.PointX;
					double y0 = pp.PointY;
					double x1 = pp.LineStartX;
					double y1 = pp.LineStartY;
					double x2 = pp.LineEndX;
					double y2 = pp.LineEndY;

					double dx       = x2 - x1;
					double dy       = y2 - y1;
					double lineMag  = Math.Sqrt(dx * dx + dy * dy);
					double distPx   = Math.Abs((y2 - y1) * x0 - (x2 - x1) * y0 + x2 * y1 - y2 * x1) / (lineMag + 1e-10);

					// 計算垂足點
					double t  = ((x0 - x1) * dx + (y0 - y1) * dy) / (lineMag * lineMag + 1e-10);
					double fx = x1 + t * dx;
					double fy = y1 + t * dy;

					// 單位換算
					double distReal = distPx * pp.PixelScale;
					string unit     = pp.DisplayUnit;
					if (unit == "μm") distReal *= 1000;
					else if (unit == "cm") distReal /= 10;

					string format = $"F{pp.DecimalPlaces}";
					string text   = $"{distReal.ToString(format)} {unit}";

					if (pp.DrawOnImage)
					{
						Cv2.Line(result, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), colorLine, 2);
						Cv2.Circle(result, new Point((int)x0, (int)y0), 5, colorPoint, -1);
						Cv2.Line(result, new Point((int)x0, (int)y0), new Point((int)fx, (int)fy), colorDist, 2, LineTypes.AntiAlias);
						Cv2.Circle(result, new Point((int)fx, (int)fy), 4, colorDist, -1);

						int midX = (int)((x0 + fx) / 2);
						int midY = (int)((y0 + fy) / 2);
						Cv2.PutText(result, text, new Point(midX + 5, midY - 5), HersheyFonts.HersheySimplex, 0.6, colorDist, 2);
					}

					OnLog?.Invoke($"[點線距離] 距離: {text} (像素: {distPx:F2})", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 迴歸直線 (AxLineRegression)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "迴歸直線 (Line Regression)",
				Category          = "14. 幾何量測",
				DefaultParameters = new LineRegressionParameters(),
				Action = (img, p) =>
				{
					var pp     = (LineRegressionParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					else if (result.Channels() == 4)
						Cv2.CvtColor(result, result, ColorConversionCodes.BGRA2BGR);

					Scalar colorLine  = new Scalar(0, 255, 0);
					Scalar colorPoint = new Scalar(0, 0, 255);

					// 解析點群座標
					List<Point2f> points = new List<Point2f>();
					if (pp.UseContourPoints)
					{
						// 從二值影像提取邊緣點
						var gray = new Mat();
						if (img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
						else img.CopyTo(gray);

						Cv2.FindContours(gray, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);
						foreach (var contour in contours)
							foreach (var pt in contour)
								points.Add(new Point2f(pt.X, pt.Y));
						gray.Dispose();
					}
					else
					{
						// 解析手動輸入的座標
						string[] pairs = pp.PointsData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var pair in pairs)
						{
							string[] coords = pair.Split(',');
							if (coords.Length == 2 && float.TryParse(coords[0], out float x) && float.TryParse(coords[1], out float y))
								points.Add(new Point2f(x, y));
						}
					}

					if (points.Count < 2)
					{
						OnLog?.Invoke("[迴歸直線] 點數不足，需要至少 2 個點", true);
						return (true, result, new List<Defect>());
					}

					// 使用 OpenCV fitLine
					var lineOutput = new Mat();
					Cv2.FitLine(InputArray.Create(points), lineOutput, DistanceTypes.L2, 0, 0.01, 0.01);

					// line: (vx, vy, x0, y0)
					float vx = lineOutput.At<float>(0), vy = lineOutput.At<float>(1), x0 = lineOutput.At<float>(2), y0 = lineOutput.At<float>(3);
					lineOutput.Dispose();

					// 計算延伸後的線段端點
					int ext     = pp.LineExtension;
					int startX  = (int)(x0 - ext * vx);
					int startY  = (int)(y0 - ext * vy);
					int endX    = (int)(x0 + ext * vx);
					int endY    = (int)(y0 + ext * vy);

					if (pp.DrawOnImage)
					{
						Cv2.Line(result, new Point(startX, startY), new Point(endX, endY), colorLine, 2);

						if (pp.DrawPoints)
						{
							foreach (var pt in points)
								Cv2.Circle(result, new Point((int)pt.X, (int)pt.Y), pp.PointRadius, colorPoint, -1);
						}
					}

					// 計算斜率與截距
					double slope     = vy / (vx + 1e-10);
					double intercept = y0 - slope * x0;
					OnLog?.Invoke($"[迴歸直線] 方向向量: ({vx:F4}, {vy:F4}), 過點: ({x0:F1}, {y0:F1}), 斜率: {slope:F4}", false);

					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 迴歸圓 (AxCircleRegression)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "迴歸圓 (Circle Regression)",
				Category          = "14. 幾何量測",
				DefaultParameters = new CircleRegressionParameters(),
				Action = (img, p) =>
				{
					var pp     = (CircleRegressionParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					else if (result.Channels() == 4)
						Cv2.CvtColor(result, result, ColorConversionCodes.BGRA2BGR);

					Scalar colorCircle = new Scalar(0, 255, 0);
					Scalar colorPoint  = new Scalar(0, 0, 255);
					Scalar colorCenter = new Scalar(255, 0, 255);

					// 解析點群座標
					List<Point2f> points = new List<Point2f>();
					if (pp.UseContourPoints)
					{
						var gray = new Mat();
						if (img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
						else img.CopyTo(gray);

						Cv2.FindContours(gray, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);
						foreach (var contour in contours)
							foreach (var pt in contour)
								points.Add(new Point2f(pt.X, pt.Y));
						gray.Dispose();
					}
					else
					{
						string[] pairs = pp.PointsData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var pair in pairs)
						{
							string[] coords = pair.Split(',');
							if (coords.Length == 2 && float.TryParse(coords[0], out float x) && float.TryParse(coords[1], out float y))
								points.Add(new Point2f(x, y));
						}
					}

					if (points.Count < 5)
					{
						OnLog?.Invoke("[迴歸圓] 點數不足，需要至少 5 個點才能擬合橢圓", true);
						return (true, result, new List<Defect>());
					}

					// 使用 OpenCV fitEllipse (也可近似為圓)
					var ellipse  = Cv2.FitEllipse(points);
					float cx     = ellipse.Center.X;
					float cy     = ellipse.Center.Y;
					float radius = (ellipse.Size.Width + ellipse.Size.Height) / 4; // 平均半徑

					// 單位換算
					double radiusReal = radius * pp.PixelScale;
					string unit       = pp.DisplayUnit;
					if (unit == "μm") radiusReal *= 1000;
					else if (unit == "cm") radiusReal /= 10;

					string format = $"F{pp.DecimalPlaces}";

					if (pp.DrawOnImage)
					{
						Cv2.Circle(result, new Point((int)cx, (int)cy), (int)radius, colorCircle, 2);
						Cv2.Circle(result, new Point((int)cx, (int)cy), 4, colorCenter, -1);
						Cv2.PutText(result, $"R={radiusReal.ToString(format)} {unit}", new Point((int)cx + 10, (int)cy - 10), HersheyFonts.HersheySimplex, 0.5, colorCircle, 1);

						if (pp.DrawPoints)
						{
							foreach (var pt in points)
								Cv2.Circle(result, new Point((int)pt.X, (int)pt.Y), pp.PointRadius, colorPoint, -1);
						}
					}

					OnLog?.Invoke($"[迴歸圓] 圓心: ({cx:F1}, {cy:F1}), 半徑: {radiusReal.ToString(format)} {unit} ({radius:F1} px)", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 兩線間距 (AxLineLineGapMsr)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "兩線間距 (Line-Line Gap)",
				Category          = "14. 幾何量測",
				DefaultParameters = new LineLineGapParameters(),
				Action = (img, p) =>
				{
					var pp     = (LineLineGapParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					else if (result.Channels() == 4)
						Cv2.CvtColor(result, result, ColorConversionCodes.BGRA2BGR);

					Scalar colorLine1 = new Scalar(0, 255, 0);
					Scalar colorLine2 = new Scalar(255, 255, 0);
					Scalar colorGap   = new Scalar(0, 0, 255);

					// 計算線1中點到線2的距離 (假定兩線近乎平行)
					double midX = (pp.Line1StartX + pp.Line1EndX) / 2.0;
					double midY = (pp.Line1StartY + pp.Line1EndY) / 2.0;

					double x1 = pp.Line2StartX, y1 = pp.Line2StartY;
					double x2 = pp.Line2EndX,   y2 = pp.Line2EndY;
					double dx = x2 - x1, dy = y2 - y1;
					double lineMag = Math.Sqrt(dx * dx + dy * dy);
					double distPx  = Math.Abs((y2 - y1) * midX - (x2 - x1) * midY + x2 * y1 - y2 * x1) / (lineMag + 1e-10);

					// 計算垂足點
					double t  = ((midX - x1) * dx + (midY - y1) * dy) / (lineMag * lineMag + 1e-10);
					double fx = x1 + t * dx;
					double fy = y1 + t * dy;

					// 單位換算
					double distReal = distPx * pp.PixelScale;
					string unit     = pp.DisplayUnit;
					if (unit == "μm") distReal *= 1000;
					else if (unit == "cm") distReal /= 10;

					string format = $"F{pp.DecimalPlaces}";
					string text   = $"{distReal.ToString(format)} {unit}";

					if (pp.DrawOnImage)
					{
						Cv2.Line(result, new Point(pp.Line1StartX, pp.Line1StartY), new Point(pp.Line1EndX, pp.Line1EndY), colorLine1, 2);
						Cv2.Line(result, new Point(pp.Line2StartX, pp.Line2StartY), new Point(pp.Line2EndX, pp.Line2EndY), colorLine2, 2);
						Cv2.Line(result, new Point((int)midX, (int)midY), new Point((int)fx, (int)fy), colorGap, 2, LineTypes.AntiAlias);
						Cv2.Circle(result, new Point((int)midX, (int)midY), 4, colorGap, -1);
						Cv2.Circle(result, new Point((int)fx, (int)fy), 4, colorGap, -1);

						int textX = (int)((midX + fx) / 2) + 5;
						int textY = (int)((midY + fy) / 2) - 5;
						Cv2.PutText(result, text, new Point(textX, textY), HersheyFonts.HersheySimplex, 0.6, colorGap, 2);
					}

					OnLog?.Invoke($"[兩線間距] 間距: {text} (像素: {distPx:F2})", false);
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}
