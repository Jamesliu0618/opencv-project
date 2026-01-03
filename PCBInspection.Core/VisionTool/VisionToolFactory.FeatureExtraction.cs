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
		/// 註冊特徵提取類別工具。
		/// 包含：邊緣偵測 (Canny/Sobel/Laplacian)、輪廓搜尋、霍夫直線/圓形偵測、模板匹配。
		/// </summary>
		/// <param name="tools">欲加入工具定義的清單物件</param>
		private static void AddFeatureExtractionTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 03. 特徵提取 (Feature Extraction)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "邊緣偵測 (Edge)",
				Category          = "03. 特徵提取",
				DefaultParameters = new EdgeDetectionParameters(),
				Action = (img, p) =>
				{
					var pp     = (EdgeDetectionParameters)p;
					var result = new Mat();
					var gray   = new Mat();

					if(img.Channels() == 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else if(img.Channels() == 4)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGRA2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}

					if(pp.Method == EdgeDetectionParameters.EdgeMethod.Canny)
					{
						Cv2.Canny(gray, result, pp.Threshold1, pp.Threshold2);
					}
					else if(pp.Method == EdgeDetectionParameters.EdgeMethod.Sobel)
					{
						// Use default ksize 3 if not specified
						Cv2.Sobel(gray, result, MatType.CV_8U, 1, 1, ksize: 3);
					}
					else if(pp.Method == EdgeDetectionParameters.EdgeMethod.Laplacian)
					{
						Cv2.Laplacian(gray, result, MatType.CV_8U, ksize: 3);
					}
					else
					{
						Cv2.Scharr(gray, result, MatType.CV_8U, 1, 0);
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "輪廓搜尋 (Find Contours)",
				Category          = "03. 特徵提取",
				DefaultParameters = new ContourFindParameters(),
				Action = (img, p) =>
				{
					var pp     = (ContourFindParameters)p;
					var result = img.Clone(); // Draw on original

					// Need binary image for FindContours
					var gray = new Mat();

					if(img.Channels() == 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else if(img.Channels() == 4)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGRA2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}

					// Simple threshold if not binary? Or assume input is binary.
					// Let's assume input might be gray, so we threshold it locally just in case, 
					// or assume user added Threshold step before this.
					// FOR ROBUSTNESS: If average pixel > 1 and < 254 (likely gray), do Auto Threshold
					// But FindContours works on any 8-bit, treating non-zero as 1. So it's fine.
					RetrievalModes mode = RetrievalModes.External;

					if(pp.Mode == ContourFindParameters.ContourModeType.List)
					{
						mode = RetrievalModes.List;
					}
					else if(pp.Mode == ContourFindParameters.ContourModeType.CComp)
					{
						mode = RetrievalModes.CComp;
					}
					else if(pp.Mode == ContourFindParameters.ContourModeType.Tree)
					{
						mode = RetrievalModes.Tree;
					}
					ContourApproximationModes method = ContourApproximationModes.ApproxSimple;

					if(pp.ApproxMethod == ContourFindParameters.ContourApproxType.None)
					{
						method = ContourApproximationModes.ApproxNone;
					}
					Cv2.FindContours(gray, out Point[][] contours, out HierarchyIndex[] hierarchy, mode, method);

					if(pp.DrawContours)
					{
						// Filter by area
						var validContours = new List<Point[]>();

						foreach(var c in contours)
						{
							double area = Cv2.ContourArea(c);

							if(area >= pp.MinArea && (pp.MaxArea <= 0 || area <= pp.MaxArea))
							{
								validContours.Add(c);
							}
						}

						// Draw
						if(result.Channels() == 1)
						{
							Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
						}
						Cv2.DrawContours(result, validContours, -1, Scalar.Red, 2);
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "霍夫直線 (Hough Lines)",
				Category          = "03. 特徵提取",
				DefaultParameters = new HoughLinesParameters(),
				Action = (img, p) =>
				{
					var pp     = (HoughLinesParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					var gray = new Mat();

					if(img.Channels() == 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else if(img.Channels() == 4)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGRA2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}

					// 自動套用 Canny 邊緣偵測
					var edgeInput = gray;
					if(pp.AutoCanny)
					{
						edgeInput = new Mat();
						Cv2.Canny(gray, edgeInput, pp.CannyThreshold1, pp.CannyThreshold2);
					}

					// 解析線條顏色
					Scalar lineColor = new Scalar(0, 255, 0); // 預設綠色
					try
					{
						var parts = pp.LineColorBGR.Split(',');
						if(parts.Length >= 3)
						{
							lineColor = new Scalar(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
						}
					}
					catch { }

					var defects = new List<Defect>();
					int lineCount = 0;

					if(pp.UseProbabilistic)
					{
						var lines = Cv2.HoughLinesP(edgeInput, pp.Rho, pp.ThetaDeg * Math.PI / 180, pp.Threshold, pp.MinLineLength, pp.MaxLineGap);

						foreach(var line in lines)
						{
							if(pp.MaxLines > 0 && lineCount >= pp.MaxLines) break;

							Cv2.Line(result, line.P1, line.P2, lineColor, pp.LineThickness);
							lineCount++;

							// 計算線段長度與角度
							double length = Math.Sqrt(Math.Pow(line.P2.X - line.P1.X, 2) + Math.Pow(line.P2.Y - line.P1.Y, 2));
							double angle  = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180 / Math.PI;

							defects.Add(new Defect
							{
								Id          = lineCount.ToString(),
								Type        = "直線",
								Confidence  = length,
								BoundingBox = new[] { Math.Min(line.P1.X, line.P2.X), Math.Min(line.P1.Y, line.P2.Y), 
								                      Math.Abs(line.P2.X - line.P1.X), Math.Abs(line.P2.Y - line.P1.Y) },
							});
						}
					}
					else
					{
						var lines = Cv2.HoughLines(edgeInput, pp.Rho, pp.ThetaDeg * Math.PI / 180, pp.Threshold);

						// 標準霍夫返回 (rho, theta)，需要轉換為線段端點
						foreach(var line in lines)
						{
							if(pp.MaxLines > 0 && lineCount >= pp.MaxLines) break;

							double rho   = line.Rho;
							double theta = line.Theta;
							double a     = Math.Cos(theta);
							double b     = Math.Sin(theta);
							double x0    = a * rho;
							double y0    = b * rho;
							int    length = 2000; // 延伸長度

							Point pt1 = new Point((int)(x0 + length * (-b)), (int)(y0 + length * a));
							Point pt2 = new Point((int)(x0 - length * (-b)), (int)(y0 - length * a));

							Cv2.Line(result, pt1, pt2, lineColor, pp.LineThickness);
							lineCount++;

							defects.Add(new Defect
							{
								Id          = lineCount.ToString(),
								Type        = "直線",
								Confidence  = rho,
								BoundingBox = new[] { (int)x0 - 50, (int)y0 - 50, 100, 100 },
							});
						}
					}

					// 清理
					if(pp.AutoCanny && edgeInput != gray)
					{
						edgeInput.Dispose();
					}
					gray.Dispose();

					return (true, result, defects);
				},
			});


			tools.Add(new ToolDefinition
			{
				Name              = "霍夫圓 (Hough Circles)",
				Category          = "03. 特徵提取",
				DefaultParameters = new HoughCirclesParameters(),
				Action = (img, p) =>
				{
					var pp     = (HoughCirclesParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					var gray = new Mat();

					if(img.Channels() == 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else if(img.Channels() == 4)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGRA2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}

					// HoughCircles has built-in Canny
					var circles = Cv2.HoughCircles(gray, HoughModes.Gradient, pp.Dp, pp.MinDist, pp.Param1, pp.Param2, pp.MinRadius, pp.MaxRadius);

					// 依半徑範圍過濾
					IEnumerable<CircleSegment> filteredCircles = circles;

					if(pp.FilterMinRadius > 0)
					{
						filteredCircles = filteredCircles.Where(c => c.Radius >= pp.FilterMinRadius);
					}

					if(pp.FilterMaxRadius > 0)
					{
						filteredCircles = filteredCircles.Where(c => c.Radius <= pp.FilterMaxRadius);
					}

					// 依面積範圍過濾 (面積 = π × r²)
					if(pp.FilterMinArea > 0)
					{
						filteredCircles = filteredCircles.Where(c => Math.PI * c.Radius * c.Radius >= pp.FilterMinArea);
					}

					if(pp.FilterMaxArea > 0)
					{
						filteredCircles = filteredCircles.Where(c => Math.PI * c.Radius * c.Radius <= pp.FilterMaxArea);
					}

					// 排序
					IEnumerable<CircleSegment> sortedCircles = filteredCircles;

					switch(pp.SortBy)
					{
						case HoughCirclesParameters.SortType.SmallestRadius:
							sortedCircles = circles.OrderBy(c => c.Radius);
							break;
						case HoughCirclesParameters.SortType.LargestRadius:
							sortedCircles = circles.OrderByDescending(c => c.Radius);
							break;
						case HoughCirclesParameters.SortType.XPosition:
							sortedCircles = circles.OrderBy(c => c.Center.X);
							break;
						case HoughCirclesParameters.SortType.Confidence:
						default:
							// OpenCV 預設通常是依 confidence (Accumulator value) 排序，但 API 無法直接保證
							// 若要嚴格 confidence 排序需要修改 OpenCV 參數或無法取得。
							// 但通常前幾個就是分數最高的。
							break;
					}
					var finalCircles = sortedCircles.ToArray();

					// 限制圓形數量
					int count = finalCircles.Length;

					if(pp.MaxCircles > 0 && count > pp.MaxCircles)
					{
						count = pp.MaxCircles;
					}
					var defects = new List<Defect>();

					for(int i = 0; i < count; i++)
					{
						var c = finalCircles[i];
						Cv2.Circle(result, (int)c.Center.X, (int)c.Center.Y, (int)c.Radius, Scalar.Cyan, 2);
						Cv2.Circle(result, (int)c.Center.X, (int)c.Center.Y, 2,             Scalar.Red,  3); // center

						// 將圓形資訊加入 Defect 列表
						defects.Add(new Defect
						{
							Id          = (i + 1).ToString(),
							Type        = "圓形",
							Confidence  = c.Radius, // 使用 Confidence 存儲半徑
							BoundingBox = new[] { (int)(c.Center.X - c.Radius), (int)(c.Center.Y - c.Radius), (int)(c.Radius * 2), (int)(c.Radius * 2) },
						});
					}
					gray.Dispose();
					return (true, result, defects);
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "模板匹配 (Template Match)",
				Category          = "03. 特徵提取",
				DefaultParameters = new TemplateMatchParameters(),
				Action = (img, p) =>
				{
					var pp     = (TemplateMatchParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}

					if(File.Exists(pp.TemplatePath))
					{
						using(var tmpl = Cv2.ImRead(pp.TemplatePath))
						{
							if(!tmpl.Empty())
							{
								// Match
								int w = tmpl.Width;
								int h = tmpl.Height;

								// Convert both to gray? Or Color?
								// Usually gray is safe
								var imgGray = new Mat();

								if(img.Channels() == 3)
								{
									Cv2.CvtColor(img, imgGray, ColorConversionCodes.BGR2GRAY);
								}
								else if(img.Channels() == 4)
								{
									Cv2.CvtColor(img, imgGray, ColorConversionCodes.BGRA2GRAY);
								}
								else
								{
									img.CopyTo(imgGray);
								}
								var tmplGray = new Mat();

								if(tmpl.Channels() == 3)
								{
									Cv2.CvtColor(tmpl, tmplGray, ColorConversionCodes.BGR2GRAY);
								}
								else if(tmpl.Channels() == 4)
								{
									Cv2.CvtColor(tmpl, tmplGray, ColorConversionCodes.BGRA2GRAY);
								}
								else
								{
									tmpl.CopyTo(tmplGray);
								}
								var                resMap = new Mat();
								TemplateMatchModes mode   = TemplateMatchModes.CCoeffNormed; // simplify

								switch(pp.Method)
								{
									case TemplateMatchParameters.MatchMethod.SqDiff:
										mode = TemplateMatchModes.SqDiff;
										break;
									case TemplateMatchParameters.MatchMethod.SqDiffNormed:
										mode = TemplateMatchModes.SqDiffNormed;
										break;
									case TemplateMatchParameters.MatchMethod.CCorr:
										mode = TemplateMatchModes.CCorr;
										break;
									case TemplateMatchParameters.MatchMethod.CCorrNormed:
										mode = TemplateMatchModes.CCorrNormed;
										break;
									case TemplateMatchParameters.MatchMethod.CCoeff:
										mode = TemplateMatchModes.CCoeff;
										break;
									case TemplateMatchParameters.MatchMethod.CCoeffNormed:
										mode = TemplateMatchModes.CCoeffNormed;
										break;
								}
								Cv2.MatchTemplate(imgGray, tmplGray, resMap, mode);
								Cv2.MinMaxLoc(resMap, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);
								bool  isMatch  = false;
								Point matchLoc = new Point();

								if(mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed)
								{
									// For SqDiff, lower is better. Not easy to threshold generally 0.1?
									// Let's use user threshold as "Max Allowed SqDiff" or similar?
									// For simplicity, let's just draw best match if using SqDiff
									matchLoc = minLoc;
									isMatch  = true;
								}
								else
								{
									// Higher is better
									if(maxVal >= pp.MatchThreshold)
									{
										matchLoc = maxLoc;
										isMatch  = true;
									}
								}

								if(isMatch)
								{
									Cv2.Rectangle(result, new Rect(matchLoc.X, matchLoc.Y, w, h), Scalar.Magenta, 2);
								}
								imgGray.Dispose();
								tmplGray.Dispose();
								resMap.Dispose();
							}
						}
					}
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}