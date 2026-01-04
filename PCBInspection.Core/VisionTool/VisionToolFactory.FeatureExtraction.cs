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
		/// 包含：邊緣偵測 (Canny/Sobel/Laplacian)、輪廓搜尋、霍夫直線/圓形偵測、模板匹配、幾何匹配。
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

					// 5. 進行區塊分析 (Blob Analysis)
					var defects = new List<Defect>();
					int blobCount = 0;

					foreach(var c in contours)
					{
						// 計算面積
						double area = Cv2.ContourArea(c);
						
						// 面積過濾
						if(area < pp.MinArea) continue;
						if(pp.MaxArea > 0 && area > pp.MaxArea) continue;

						blobCount++;
						
						// 計算幾何特徵
						var rect = Cv2.BoundingRect(c);
						double perimeter = Cv2.ArcLength(c, true);
						
						// 計算形心 (Centroid) / 重心
						var moments = Cv2.Moments(c);
						double cx = 0, cy = 0;
						if(moments.M00 != 0)
						{
							cx = moments.M10 / moments.M00;
							cy = moments.M01 / moments.M00;
						}
						else
						{
							// 若面積為 0 (極端情況)，使用 BoundingBox 中心
							cx = rect.X + rect.Width / 2.0;
							cy = rect.Y + rect.Height / 2.0;
						}

						// 計算形狀因子
						double circularity = (perimeter > 0) ? (4 * Math.PI * area) / (perimeter * perimeter) : 0;
						double rectangularity = (rect.Width * rect.Height > 0) ? area / (rect.Width * rect.Height) : 0;

						// 加入 Defect 列表
						var defect = new Defect
						{
							Id          = blobCount.ToString(),
							Type        = "Blob",
							Confidence  = area, // 將面積做為主要的特徵值
							Area        = area,
							CenterX     = cx,
							CenterY     = cy,
							BoundingBox = new[] { rect.X, rect.Y, rect.Width, rect.Height },
							Circularity = circularity,
							Rectangularity = rectangularity,
							Angle       = 0
						};
						
						// 擬合旋轉矩形以取得角度 (若點數夠多)
						if(c.Length >= 5)
						{
							var rotRect = Cv2.MinAreaRect(c);
							defect.Angle = rotRect.Angle;
						}
						
						defects.Add(defect);

						// 繪製輪廓
						if(pp.DrawContours)
						{
							// 繪製輪廓線 (紅色)
							if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
							Cv2.DrawContours(result, new[] { c }, -1, Scalar.Red, 2);
							
							// 繪製重心 (綠色十字)
							Cv2.DrawMarker(result, new Point((int)cx, (int)cy), Scalar.Lime, MarkerTypes.Cross, 10, 1);
							
							// 繪製外接矩形 (黃色)
							Cv2.Rectangle(result, rect, Scalar.Yellow, 1);
						}
					}
					
					gray.Dispose();
					return (true, result, defects);
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
					else if(result.Channels() == 4)
					{
						var tmp = new Mat();
						Cv2.CvtColor(result, tmp, ColorConversionCodes.BGRA2BGR);
						result.Dispose();
						result = tmp;
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

					// ===== 效能優化：影像預縮放 =====
					CircleSegment[] circles;

					if(pp.EnablePreResize && pp.PreResizeScale > 0 && pp.PreResizeScale < 1.0)
					{
						double scale = pp.PreResizeScale;
						// 使用 using 確保資源釋放
						using(var workingGray = new Mat())
						{
							Cv2.Resize(gray, workingGray, new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Linear);

							// 調整半徑範圍與距離參數
							int scaledMinRadius = Math.Max(1, (int)(pp.MinRadius * scale));
							int scaledMaxRadius = Math.Max(1, (int)(pp.MaxRadius * scale));
							double scaledMinDist = Math.Max(1, pp.MinDist * scale);

							// HoughCircles 在縮放後的影像上執行
							var rawCircles = Cv2.HoughCircles(workingGray, HoughModes.Gradient, pp.Dp, scaledMinDist, pp.Param1, pp.Param2, scaledMinRadius, scaledMaxRadius);

							// 將結果座標轉換回原始尺度
							circles = new CircleSegment[rawCircles.Length];
							for(int i = 0; i < rawCircles.Length; i++)
							{
								circles[i] = new CircleSegment(
									new Point2f(rawCircles[i].Center.X / (float)scale, rawCircles[i].Center.Y / (float)scale),
									rawCircles[i].Radius / (float)scale
								);
							}
						}
					}
					else
					{
						// 標準模式：直接在原圖執行
						circles = Cv2.HoughCircles(gray, HoughModes.Gradient, pp.Dp, pp.MinDist, pp.Param1, pp.Param2, pp.MinRadius, pp.MaxRadius);
					}

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
					var pp     = (TemplateMatchParameters)p;
					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					if(!File.Exists(pp.TemplatePath))
					{
						OnLog?.Invoke("[TemplateMatch] 模板檔案不存在。", true);
						return (false, result, new List<Defect>());
					}

					using(var tmplFull = Cv2.ImRead(pp.TemplatePath))
					{
						if(tmplFull.Empty()) return (false, result, new List<Defect>());

						// 1. 處理模板 ROI (優先使用 TemplateRoiIndex)
						Mat tmpl;
						Rect? tmplRect = null;
						if(pp.TemplateRoiIndex > 0)
						{
							tmplRect = RoiManager.GetRectByIndex(pp.TemplateRoiIndex);
						}
						else if(pp.EnableTemplateRoi && pp.TemplateRoiWidth > 0 && pp.TemplateRoiHeight > 0)
						{
							tmplRect = new Rect(pp.TemplateRoiX, pp.TemplateRoiY, pp.TemplateRoiWidth, pp.TemplateRoiHeight);
						}

						if(tmplRect.HasValue)
						{
							var tRect = tmplRect.Value.Intersect(new Rect(0, 0, tmplFull.Width, tmplFull.Height));
							tmpl = new Mat(tmplFull, tRect);
						}
						else { tmpl = tmplFull; }

						int w = tmpl.Width;
						int h = tmpl.Height;

						// 2. 處理來源 ROI (優先使用 SourceRoiIndex)
						Mat srcRegion;
						int offsetX = 0, offsetY = 0;
						Rect? srcRect = null;
						if(pp.SourceRoiIndex > 0)
						{
							srcRect = RoiManager.GetRectByIndex(pp.SourceRoiIndex);
						}
						else if(pp.EnableSourceRoi && pp.SourceRoiWidth > 0 && pp.SourceRoiHeight > 0)
						{
							srcRect = new Rect(pp.SourceRoiX, pp.SourceRoiY, pp.SourceRoiWidth, pp.SourceRoiHeight);
						}

						if(srcRect.HasValue)
						{
							var sRect = srcRect.Value.Intersect(new Rect(0, 0, img.Width, img.Height));
							srcRegion = new Mat(img, sRect);
							offsetX = sRect.X;
							offsetY = sRect.Y;
						}
						else { srcRegion = img; }

						// 3. 灰階轉換
						using(var imgGray = new Mat())
						using(var tmplGray = new Mat())
						{
							if(srcRegion.Channels() >= 3) Cv2.CvtColor(srcRegion, imgGray, ColorConversionCodes.BGR2GRAY);
							else srcRegion.CopyTo(imgGray);

							if(tmpl.Channels() >= 3) Cv2.CvtColor(tmpl, tmplGray, ColorConversionCodes.BGR2GRAY);
							else tmpl.CopyTo(tmplGray);

							// 4. 執行模板匹配
							TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed;
							switch(pp.Method)
							{
								case TemplateMatchParameters.MatchMethod.SqDiff: mode = TemplateMatchModes.SqDiff; break;
								case TemplateMatchParameters.MatchMethod.SqDiffNormed: mode = TemplateMatchModes.SqDiffNormed; break;
								case TemplateMatchParameters.MatchMethod.CCorr: mode = TemplateMatchModes.CCorr; break;
								case TemplateMatchParameters.MatchMethod.CCorrNormed: mode = TemplateMatchModes.CCorrNormed; break;
								case TemplateMatchParameters.MatchMethod.CCoeff: mode = TemplateMatchModes.CCoeff; break;
								case TemplateMatchParameters.MatchMethod.CCoeffNormed: mode = TemplateMatchModes.CCoeffNormed; break;
							}

							using(var resMap = new Mat())
							{
								Cv2.MatchTemplate(imgGray, tmplGray, resMap, mode);

								var defects = new List<Defect>();
								int maxMatches = pp.MaxMatches <= 0 ? 1 : pp.MaxMatches;
								bool useSqDiff = (mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed);

								for(int i = 0; i < maxMatches; i++)
								{
									Cv2.MinMaxLoc(resMap, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

									double score = useSqDiff ? (1 - minVal) : maxVal;
									Point matchLoc = useSqDiff ? minLoc : maxLoc;

									if(!useSqDiff && maxVal < pp.MatchThreshold) break;
									if(useSqDiff && minVal > (1 - pp.MatchThreshold)) break;

									// 將座標轉換回原圖
									int realX = matchLoc.X + offsetX;
									int realY = matchLoc.Y + offsetY;

									Cv2.Rectangle(result, new Rect(realX, realY, w, h), Scalar.Magenta, 2);

									defects.Add(new Defect
									{
										Id          = (i + 1).ToString(),
										Type        = "模板匹配",
										Confidence  = score,
										CenterX     = realX + w / 2.0,
										CenterY     = realY + h / 2.0,
										BoundingBox = new[] { realX, realY, w, h }
									});

									// 遮蔽已找到的區域，避免重複偵測 (NMS 風格)
									Cv2.Rectangle(resMap, new Rect(matchLoc.X - w / 2, matchLoc.Y - h / 2, w, h),
										useSqDiff ? new Scalar(1) : new Scalar(0), -1);
								}

								if(pp.EnableTemplateRoi && tmpl != tmplFull) tmpl.Dispose();
								if(pp.EnableSourceRoi && srcRegion != img) srcRegion.Dispose();

								return (true, result, defects);
							}
						}
					}
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "幾何匹配 (Geometric Match)",
				Category          = "03. 特徵提取",
				DefaultParameters = new GeometricMatchParameters(),
				Action = (img, p) =>
				{
					var pp     = (GeometricMatchParameters)p;
					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					else if(result.Channels() == 4) { var tmp = new Mat(); Cv2.CvtColor(result, tmp, ColorConversionCodes.BGRA2BGR); result.Dispose(); result = tmp; }

					if(!File.Exists(pp.TemplatePath))
					{
						OnLog?.Invoke("[GeometricMatch] 模板檔案不存在。", true);
						return (false, result, new List<Defect>());
					}

					using(var tmpl = Cv2.ImRead(pp.TemplatePath))
					{
						if(tmpl.Empty()) return (false, result, new List<Defect>());

						// 1. 影像預處理：縮放與灰階
						double scale = pp.SearchScale;
						using(var srcGray  = new Mat())
						using(var tmplGray = new Mat())
						{
							if(img.Channels() >= 3) Cv2.CvtColor(img, srcGray, ColorConversionCodes.BGR2GRAY); else img.CopyTo(srcGray);
							if(tmpl.Channels() >= 3) Cv2.CvtColor(tmpl, tmplGray, ColorConversionCodes.BGR2GRAY); else tmpl.CopyTo(tmplGray);

							using(var srcWork  = new Mat())
							using(var tmplWork = new Mat())
							{
								if(scale < 1.0)
								{
									Cv2.Resize(srcGray, srcWork, new Size(), scale, scale);
									Cv2.Resize(tmplGray, tmplWork, new Size(), scale, scale);
								}
								else { srcGray.CopyTo(srcWork); tmplGray.CopyTo(tmplWork); }

								// 2. 邊緣提取 (取得幾何特徵)
								using(var srcEdges  = new Mat())
								using(var tmplEdges = new Mat())
								{
									Cv2.Canny(srcWork, srcEdges, pp.CannyThreshold1, pp.CannyThreshold2);
									Cv2.Canny(tmplWork, tmplEdges, pp.CannyThreshold1, pp.CannyThreshold2);

									double maxScore = -1;
									Point  bestLoc  = new Point();
									double bestAngle = 0;

									// 3. 旋轉搜尋 (若啟用)
									if(pp.EnableRotation)
									{
										var angles = new List<double>();
										for(double angle = pp.MinAngle; angle <= pp.MaxAngle; angle += pp.AngleStep)
											angles.Add(angle);

										object lockObj = new object();

										System.Threading.Tasks.Parallel.ForEach(angles, (angle) =>
										{
											using(var rotTmpl = new Mat())
											{
												var center = new Point2f(tmplEdges.Cols / 2f, tmplEdges.Rows / 2f);
												using(var matrix = Cv2.GetRotationMatrix2D(center, angle, 1.0))
												{
													Cv2.WarpAffine(tmplEdges, rotTmpl, matrix, tmplEdges.Size());
												}

												using(var resMap = new Mat())
												{
													Cv2.MatchTemplate(srcEdges, rotTmpl, resMap, TemplateMatchModes.CCoeffNormed);
													Cv2.MinMaxLoc(resMap, out _, out double curMax, out _, out Point curLoc);

													lock(lockObj)
													{
														if(curMax > maxScore)
														{
															maxScore = curMax;
															bestLoc  = curLoc;
															bestAngle = angle;
														}
													}
												}
											}
										});
									}
									else
									{
										using(var resMap = new Mat())
										{
											Cv2.MatchTemplate(srcEdges, tmplEdges, resMap, TemplateMatchModes.CCoeffNormed);
											Cv2.MinMaxLoc(resMap, out _, out maxScore, out _, out bestLoc);
										}
									}

									// 4. 結果輸出
									var defects = new List<Defect>();
									if(maxScore >= pp.MatchThreshold)
									{
										int realX = (int)(bestLoc.X / scale);
										int realY = (int)(bestLoc.Y / scale);
										int realW = (int)(tmpl.Width);
										int realH = (int)(tmpl.Height);

										if(pp.EnableRotation)
										{
											var center = new Point2f(realX + realW / 2f, realY + realH / 2f);
											var rect   = new RotatedRect(center, new Size2f(realW, realH), (float)bestAngle);
											var pts    = rect.Points();
											for(int j = 0; j < 4; j++) Cv2.Line(result, (Point)pts[j], (Point)pts[(j + 1) % 4], Scalar.Yellow, 2);
										}
										else
										{
											Cv2.Rectangle(result, new Rect(realX, realY, realW, realH), Scalar.Yellow, 2);
										}

										defects.Add(new Defect
										{
											Id          = "1",
											Type        = "幾何匹配",
											Confidence  = maxScore,
											CenterX     = realX + realW / 2.0,
											CenterY     = realY + realH / 2.0,
											Angle       = bestAngle,
											BoundingBox = new[] { realX, realY, realW, realH }
										});
									}
									return (true, result, defects);
								}
							}
						}
					}
				},
			});
		}
	}
}