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
					// 1. 參數轉換與灰階預處理
					var pp     = (EdgeDetectionParameters)p;
					var result = new Mat();
					var gray   = new Mat();

					// 邊緣偵測通常在灰階影像上執行效果最好
					if(img.Channels() == 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else if(img.Channels() == 4) Cv2.CvtColor(img, gray, ColorConversionCodes.BGRA2GRAY);
					else img.CopyTo(gray);

					// 2. 根據所選演算法執行邊緣計算
					if(pp.Method == EdgeDetectionParameters.EdgeMethod.Canny)
					{
						// Canny 算子：最常用的邊緣偵測，包含雜訊抑制與雙門檻偵側
						Cv2.Canny(gray, result, pp.Threshold1, pp.Threshold2);
					}
					else if(pp.Method == EdgeDetectionParameters.EdgeMethod.Sobel)
					{
						// Sobel 算子：利用一階導數計算像素亮度差值
						Cv2.Sobel(gray, result, MatType.CV_8U, 1, 1, ksize: 3);
					}
					else if(pp.Method == EdgeDetectionParameters.EdgeMethod.Laplacian)
					{
						// Laplacian 算子：利用二階導數計算亮度突變點
						Cv2.Laplacian(gray, result, MatType.CV_8U, ksize: 3);
					}
					else
					{
						// Scharr 算子：比 Sobel 更精確的梯度計算
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
					// 1. 初始化與灰階轉換
					var pp     = (ContourFindParameters)p;
					var result = img.Clone(); // 在副本上繪製

					var gray = new Mat();
					if(img.Channels() == 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else if(img.Channels() == 4) Cv2.CvtColor(img, gray, ColorConversionCodes.BGRA2GRAY);
					else img.CopyTo(gray);

					// 2. 設定檢索模式 (如：僅外部、所有層級、二層樹狀)
					RetrievalModes mode = RetrievalModes.External;
					if(pp.Mode == ContourFindParameters.ContourModeType.List) mode = RetrievalModes.List;
					else if(pp.Mode == ContourFindParameters.ContourModeType.CComp) mode = RetrievalModes.CComp;
					else if(pp.Mode == ContourFindParameters.ContourModeType.Tree) mode = RetrievalModes.Tree;

					// 3. 設定近似方法 (如：保留所有點、僅保留轉折點)
					ContourApproximationModes method = ContourApproximationModes.ApproxSimple;
					if(pp.ApproxMethod == ContourFindParameters.ContourApproxType.None) method = ContourApproximationModes.ApproxNone;

					// 4. 執行輪廓搜尋
					Cv2.FindContours(gray, out Point[][] contours, out HierarchyIndex[] hierarchy, mode, method);

					// 5. 繪製符合條件的輪廓
					if(pp.DrawContours)
					{
						// 依據面積大小過濾
						var validContours = new List<Point[]>();
						foreach(var c in contours)
						{
							double area = Cv2.ContourArea(c);
							if(area >= pp.MinArea && (pp.MaxArea <= 0 || area <= pp.MaxArea))
							{
								validContours.Add(c);
							}
						}

						// 繪製輪廓到結果圖 (使用紅色線條)
						if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
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
					// 1. 初始化灰階圖與參數
					var pp     = (HoughLinesParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					
					var gray = new Mat();
					if(img.Channels() == 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else if(img.Channels() == 4) Cv2.CvtColor(img, gray, ColorConversionCodes.BGRA2GRAY);
					else img.CopyTo(gray);

					// 2. 邊緣偵測預處理 (霍夫變換需要邊緣影像作為輸入)
					var edgeInput = gray;
					if(pp.AutoCanny)
					{
						edgeInput = new Mat();
						Cv2.Canny(gray, edgeInput, pp.CannyThreshold1, pp.CannyThreshold2);
					}

					// 3. 解析自定義繪圖顏色
					Scalar lineColor = new Scalar(0, 255, 0); // 預設亮綠色
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

					// 4. 執行霍夫直線偵測
					if(pp.UseProbabilistic)
					{
						// 概率霍夫變換 (HoughLinesP)：直接回傳線段端點 (P1, P2)，效能較好
						var lines = Cv2.HoughLinesP(edgeInput, pp.Rho, pp.ThetaDeg * Math.PI / 180, pp.Threshold, pp.MinLineLength, pp.MaxLineGap);

						foreach(var line in lines)
						{
							if(pp.MaxLines > 0 && lineCount >= pp.MaxLines) break;

							Cv2.Line(result, line.P1, line.P2, lineColor, pp.LineThickness);
							lineCount++;

							// 計算屬性並存為 DetectedObject
							double length  = Math.Sqrt(Math.Pow(line.P2.X - line.P1.X, 2) + Math.Pow(line.P2.Y - line.P1.Y, 2));
							int    minX    = Math.Min(line.P1.X, line.P2.X);
							int    minY    = Math.Min(line.P1.Y, line.P2.Y);
							int    width   = Math.Max(Math.Abs(line.P2.X - line.P1.X), 20);
							int    height  = Math.Max(Math.Abs(line.P2.Y - line.P1.Y), 20);

							defects.Add(new Defect
							{
								Id          = lineCount.ToString(),
								Type        = "直線",
								Confidence  = length,
								BoundingBox = new[] { minX, minY, width, height },
								Angle       = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180.0 / Math.PI,
							});
						}
					}
					else
					{
						// 標準霍夫變換 (HoughLines)：返回 (rho, theta) 極座標表示法
						var lines = Cv2.HoughLines(edgeInput, pp.Rho, pp.ThetaDeg * Math.PI / 180, pp.Threshold);

						foreach(var line in lines)
						{
							if(pp.MaxLines > 0 && lineCount >= pp.MaxLines) break;

							// 極座標轉直角座標繪製
							double rho   = line.Rho;
							double theta = line.Theta;
							double a     = Math.Cos(theta);
							double b     = Math.Sin(theta);
							double x0    = a * rho;
							double y0    = b * rho;
							int    extLen = 2000; // 延伸長度以覆蓋畫布

							Point pt1 = new Point((int)(x0 + extLen * (-b)), (int)(y0 + extLen * a));
							Point pt2 = new Point((int)(x0 - extLen * (-b)), (int)(y0 - extLen * a));

							Cv2.Line(result, pt1, pt2, lineColor, pp.LineThickness);
							lineCount++;

							defects.Add(new Defect
							{
								Id          = lineCount.ToString(),
								Type        = "直線",
								Confidence  = rho,
								BoundingBox = new[] { (int)x0 - 50, (int)y0 - 50, 100, 100 },
								Angle       = theta * 180.0 / Math.PI,
							});
						}
					}

					// 5. 資源清理
					if(pp.AutoCanny && edgeInput != gray) edgeInput.Dispose();
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

					// 4. 排序偵測到的圓形 (依據使用者設定)
					IEnumerable<CircleSegment> sortedCircles = filteredCircles;

					switch(pp.SortBy)
					{
						case HoughCirclesParameters.SortType.SmallestRadius:
							// 依據半徑由小到大排序
							sortedCircles = filteredCircles.OrderBy(c => c.Radius);
							break;
						case HoughCirclesParameters.SortType.LargestRadius:
							// 依據半徑由大到小排序
							sortedCircles = filteredCircles.OrderByDescending(c => c.Radius);
							break;
						case HoughCirclesParameters.SortType.XPosition:
							// 依據 X 座標由左到右排序
							sortedCircles = filteredCircles.OrderBy(c => c.Center.X);
							break;
						case HoughCirclesParameters.SortType.Confidence:
						default:
							// OpenCV 的 HoughCircles 輸出預設即依據累加器確信度排序 (Top results first)
							break;
					}
					
					var finalCircles = sortedCircles.ToArray();

					// 5. 限制並輸出圓形結果
					int count = finalCircles.Length;
					if(pp.MaxCircles > 0 && count > pp.MaxCircles)
					{
						count = pp.MaxCircles;
					}
					
					var defects = new List<Defect>();

					for(int i = 0; i < count; i++)
					{
						var c = finalCircles[i];
						
						// 繪製圓框 (青色) 與 圓心 (紅色)
						Cv2.Circle(result, (int)c.Center.X, (int)c.Center.Y, (int)c.Radius, Scalar.Cyan, 2);
						Cv2.Circle(result, (int)c.Center.X, (int)c.Center.Y, 2,             Scalar.Red,  3);

						// 將圓形資訊包裝成 Defect 物件 (供後續距離測量工具抓取 BoundingBox 中心點)
						defects.Add(new Defect
						{
							Id          = (i + 1).ToString(),
							Type        = "圓形",
							Confidence  = c.Radius, // 暫將半徑存為信心度，供檢視參考
							BoundingBox = new[] { (int)(c.Center.X - c.Radius), (int)(c.Center.Y - c.Radius), (int)(c.Radius * 2), (int)(c.Radius * 2) },
							Circularity = 1.0,      // 由於是 HoughCircles 偵測出的，圓度設為 1.0
							Rectangularity = Math.PI / 4.0,
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
					// 1. 初始化與參數準備
					var pp     = (TemplateMatchParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					// 檢查標靶圖案路徑是否存在
					if(File.Exists(pp.TemplatePath))
					{
						using(var tmpl = Cv2.ImRead(pp.TemplatePath))
						{
							if(!tmpl.Empty())
							{
								// 2. 影像預處理：將來源與模板皆轉為灰階圖以提升比對效率
								int w = tmpl.Width;
								int h = tmpl.Height;
								var imgGray = new Mat();
								if(img.Channels() >= 3) Cv2.CvtColor(img, imgGray, ColorConversionCodes.BGR2GRAY);
								else img.CopyTo(imgGray);

								var tmplGray = new Mat();
								if(tmpl.Channels() >= 3) Cv2.CvtColor(tmpl, tmplGray, ColorConversionCodes.BGR2GRAY);
								else tmpl.CopyTo(tmplGray);

								// 3. 執行模板匹配運算
								var resMap = new Mat();
								TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed; // 預設使用正規化相關係數

								switch(pp.Method)
								{
									case TemplateMatchParameters.MatchMethod.SqDiff: mode = TemplateMatchModes.SqDiff; break;
									case TemplateMatchParameters.MatchMethod.SqDiffNormed: mode = TemplateMatchModes.SqDiffNormed; break;
									case TemplateMatchParameters.MatchMethod.CCorr: mode = TemplateMatchModes.CCorr; break;
									case TemplateMatchParameters.MatchMethod.CCorrNormed: mode = TemplateMatchModes.CCorrNormed; break;
									case TemplateMatchParameters.MatchMethod.CCoeff: mode = TemplateMatchModes.CCoeff; break;
									case TemplateMatchParameters.MatchMethod.CCoeffNormed: mode = TemplateMatchModes.CCoeffNormed; break;
								}
								
								Cv2.MatchTemplate(imgGray, tmplGray, resMap, mode);
								
								// 4. 尋找最佳匹配點 (最小值或最大值位置)
								Cv2.MinMaxLoc(resMap, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);
								
								bool  isMatch  = false;
								Point matchLoc = new Point();

								// 差異類演算法值越小越準；係數類演算法值越大越準
								if(mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed)
								{
									matchLoc = minLoc;
									isMatch  = true; // 此處暫簡化為一律匹配，實務上需定義差異上限
								}
								else
								{
									if(maxVal >= pp.MatchThreshold)
									{
										matchLoc = maxLoc;
										isMatch  = true;
									}
								}

								// 5. 繪製匹配結果 (洋紅色方塊)
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