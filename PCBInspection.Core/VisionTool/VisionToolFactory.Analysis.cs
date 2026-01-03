using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		/// <summary>
		/// 註冊測量與分析類別工具。
		/// 包含：幾何距離測量、物件輪廓分析、影像直方圖統計與灰階剖面線分析。
		/// </summary>
		/// <param name="tools">欲加入工具定義的清單物件</param>
		private static void AddAnalysisTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 09. 測量與分析 (Measurement & Analysis)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "距離測量 (Measure Distance)",
				Category          = "09. 測量分析",
				DefaultParameters = new MeasurementToolParameters(),
				Action = (img, p) =>
				{
					var pp     = (MeasurementToolParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					double dx       = pp.EndX - pp.StartX;
					double dy       = pp.EndY - pp.StartY;
					double distPx   = Math.Sqrt(dx * dx + dy * dy);
					double distReal = distPx * pp.PixelScale;
					string unit     = pp.DisplayUnit;

					if(unit == "μm")
					{
						distReal *= 1000;
					}
					else if(unit == "cm")
					{
						distReal /= 10;
					}
					string format = $"F{pp.DecimalPlaces}";
					string text   = $"{distReal.ToString(format)} {unit}";

					if(pp.DrawOnImage)
					{
						Cv2.Line(result, new Point(pp.StartX,   pp.StartY), new Point(pp.EndX, pp.EndY), Scalar.Cyan, 2);
						Cv2.Circle(result, new Point(pp.StartX, pp.StartY), 4, Scalar.Green, -1);
						Cv2.Circle(result, new Point(pp.EndX,   pp.EndY),   4, Scalar.Red,   -1);
						int midX = (pp.StartX + pp.EndX) / 2;
						int midY = (pp.StartY + pp.EndY) / 2;
						Cv2.PutText(result, text, new Point(midX + 5, midY - 5), HersheyFonts.HersheySimplex, 0.6, Scalar.Yellow, 2);
					}
					OnLog?.Invoke($"[測量] 距離: {text} (像素: {distPx:F2})", false);
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "物件分析 (Object Analysis)",
				Category          = "09. 測量分析",
				DefaultParameters = new ObjectAnalysisParameters(),
				Action = (img, p) =>
				{
					var pp     = (ObjectAnalysisParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					var gray = new Mat();

					if(img.Channels() >= 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}
					Cv2.FindContours(gray, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
					var defects = new List<Defect>();
					int idx     = 0;

					foreach(var contour in contours)
					{
						double area = Cv2.ContourArea(contour);

						if(area < pp.FilterMinArea)
						{
							continue;
						}

						if(pp.FilterMaxArea > 0 && area > pp.FilterMaxArea)
						{
							continue;
						}
						double perimeter      = Cv2.ArcLength(contour, true);
						double circularity    = 4 * Math.PI * area / (perimeter * perimeter);
						var    rect           = Cv2.BoundingRect(contour);
						var    minRect        = Cv2.MinAreaRect(contour);
						double aspectRatio    = (double)rect.Width / Math.Max(rect.Height, 1);
						double rectangularity = area               / (rect.Width * rect.Height + 0.001);

						if(pp.ComputeBoundingRect)
						{
							Cv2.Rectangle(result, rect, Scalar.Green);
						}

						if(pp.ComputeMinAreaRect)
						{
							Point[] pts = Cv2.BoxPoints(minRect).Select(pt => new Point((int)pt.X, (int)pt.Y)).ToArray();
							Cv2.Polylines(result, new[] { pts }, true, Scalar.Cyan);
						}

						if(pp.ComputeMinEnclosingCircle)
						{
							Cv2.MinEnclosingCircle(contour, out Point2f center, out float radius);
							Cv2.Circle(result, (int)center.X, (int)center.Y, (int)radius, Scalar.Magenta);
						}

						if(pp.ComputeConvexHull)
						{
							Point[] hull = Cv2.ConvexHull(contour);
							Cv2.Polylines(result, new[] { hull }, true, Scalar.Yellow);
						}

						if(pp.LabelObjectIndex)
						{
							Cv2.PutText(result, $"#{idx + 1}", new Point(rect.X, rect.Y - 5), HersheyFonts.HersheySimplex, 0.4, Scalar.White);
						}

						defects.Add(new Defect
						{
							Id          = (idx + 1).ToString(),
							Type        = "物件",
							Confidence  = circularity,
							BoundingBox = new[] { rect.X, rect.Y, rect.Width, rect.Height },
						});

						if(pp.ComputeShapeFeatures)
						{
							OnLog?.Invoke($"[物件#{idx + 1}] 面積:{area:F0} 周長:{perimeter:F1} 圓度:{circularity:F3} 長寬比:{aspectRatio:F2} 矩形度:{rectangularity:F3}", false);
						}
						idx++;
					}
					gray.Dispose();
					OnLog?.Invoke($"[物件分析] 共找到 {idx} 個物件", false);
					return (true, result, defects);
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "直方圖分析 (Histogram Analysis)",
				Category          = "09. 測量分析",
				DefaultParameters = new HistogramAnalysisParameters(),
				Action = (img, p) =>
				{
					HistogramAnalysisParameters pp     = (HistogramAnalysisParameters)p;
					Mat                         result = img.Clone();
					Mat                         gray   = new Mat();

					if(img.Channels() >= 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}

					if(pp.ComputeStatistics)
					{
						Cv2.MeanStdDev(gray, out Scalar mean, out Scalar stddev);
						Cv2.MinMaxLoc(gray, out double minVal, out double maxVal);
						OnLog?.Invoke($"[直方圖] 均值:{mean.Val0:F2} 標準差:{stddev.Val0:F2} 最小:{minVal} 最大:{maxVal}", false);
					}

					if(pp.DrawOnImage && result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}

					// 計算並可選繪製直方圖
					using(Mat hist = new Mat())
					{
						int[]    histSize = { 256 };
						Rangef[] ranges   = { new Rangef(0, 256) };
						Cv2.CalcHist(new[] { gray }, new[] { 0 }, null, hist, 1, histSize, ranges);
						Cv2.Normalize(hist, hist, 0, 100, NormTypes.MinMax);

						if(pp.DrawOnImage)
						{
							int histW   = 256, histH = 100;
							int offsetX = result.Width  - histW - 10;
							int offsetY = result.Height - histH - 10;
							Cv2.Rectangle(result, new Rect(offsetX - 2, offsetY - 2, histW + 4, histH + 4), Scalar.Black, -1);

							for(int i = 0; i < 256; i++)
							{
								int h = (int)hist.At<float>(i);
								Cv2.Line(result, new Point(offsetX + i, offsetY + histH), new Point(offsetX + i, offsetY + histH - h), Scalar.Green);
							}
						}
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "剖面線分析 (Profile Line)",
				Category          = "09. 測量分析",
				DefaultParameters = new ProfileLineParameters(),
				Action = (img, p) =>
				{
					ProfileLineParameters pp     = (ProfileLineParameters)p;
					Mat                   result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					Mat gray = new Mat();

					if(img.Channels() >= 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}

					// 取得剖面線上的點
					int        dx          = pp.EndX - pp.StartX;
					int        dy          = pp.EndY - pp.StartY;
					int        length      = (int)Math.Max(Math.Sqrt(dx * dx + dy * dy), 1);
					List<byte> profileData = new List<byte>();

					for(int i = 0; i <= length; i++)
					{
						int x = pp.StartX + (int)(dx * i / (double)length);
						int y = pp.StartY + (int)(dy * i / (double)length);
						x = Math.Max(0, Math.Min(x, gray.Width  - 1));
						y = Math.Max(0, Math.Min(y, gray.Height - 1));
						profileData.Add(gray.At<byte>(y, x));
					}

					// 繪製剖面線標示
					Cv2.Line(result, new Point(pp.StartX,   pp.StartY), new Point(pp.EndX, pp.EndY), Scalar.Cyan, 2);
					Cv2.Circle(result, new Point(pp.StartX, pp.StartY), 4, Scalar.Green, -1);
					Cv2.Circle(result, new Point(pp.EndX,   pp.EndY),   4, Scalar.Red,   -1);

					// 繪製小型剖面圖在影像右下角
					int graphW  = Math.Min(256, length);
					int graphH  = 60;
					int offsetX = result.Width  - graphW - 10;
					int offsetY = result.Height - graphH - 10;
					Cv2.Rectangle(result, new Rect(offsetX - 2, offsetY - 2, graphW + 4, graphH + 4), Scalar.Black, -1);

					for(int i = 1; i < graphW && i < profileData.Count; i++)
					{
						int idx1 = (i - 1)                                       * profileData.Count / graphW;
						int idx2 = i                                             * profileData.Count / graphW;
						int y1   = offsetY + graphH - profileData[idx1] * graphH / 255;
						int y2   = offsetY + graphH - profileData[idx2] * graphH / 255;
						Cv2.Line(result, new Point(offsetX + i - 1, y1), new Point(offsetX + i, y2), Scalar.Green);
					}

					if(pp.OutputToCsv && !string.IsNullOrEmpty(pp.CsvOutputPath))
					{
						IEnumerable<string> lines = profileData.Select((v, i) => $"{i},{v}");
						File.WriteAllLines(pp.CsvOutputPath, new[] { "Index,GrayValue" }.Concat(lines));
						OnLog?.Invoke($"[剖面線] 已輸出 {profileData.Count} 筆數據到 {pp.CsvOutputPath}", false);
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}