using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCBInspection.Core.Services
{
	public static class VisionToolFactory
	{
		// 定義工具執行的委派簽名
		public delegate (bool IsOk, Mat ResultImage, List<Defect> Defects) VisionAction(Mat input, object param);

		// Logging event: Message, IsError
		public static event Action<string, bool> OnLog;

		public static List<ToolDefinition> GetAllTools()
		{
			var tools = new List<ToolDefinition>();

			// =========================================================
			// 01. 預處理 (Preprocessing)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "灰階化 (Grayscale)",
				Category          = "01. 預處理",
				DefaultParameters = new GrayscaleParameters(),
				Action = (img, p) =>
				{
					var result = new Mat();

					if(img.Channels() == 3)
					{
						Cv2.CvtColor(img, result, ColorConversionCodes.BGR2GRAY);
					}
					else if(img.Channels() == 4)
					{
						Cv2.CvtColor(img, result, ColorConversionCodes.BGRA2GRAY);
					}
					else
					{
						img.CopyTo(result);
					}
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "濾波模糊 (Blur)",
				Category          = "01. 預處理",
				DefaultParameters = new BlurParameters(),
				Action = (img, p) =>
				{
					var pp     = (BlurParameters)p;
					var result = new Mat();
					var k      = pp.KernelSize % 2 == 1 ? pp.KernelSize : pp.KernelSize + 1;

					switch(pp.Type)
					{
						case BlurParameters.BlurType.Gaussian:
							// SigmaX default 0 implies calculated from kernel size
							Cv2.GaussianBlur(img, result, new Size(k, k), pp.Sigma);
							break;
						case BlurParameters.BlurType.Median:
							Cv2.MedianBlur(img, result, k);
							break;
						case BlurParameters.BlurType.Box:
							Cv2.Blur(img, result, new Size(k, k));
							break;
						case BlurParameters.BlurType.Bilateral:
							// d=9 is common, sigmaColor/sigmaSpace set to pp.Sigma or default
							double sig = pp.Sigma > 0 ? pp.Sigma : 75;

							// Bilateral supports 1 or 3 channels only
							if(img.Channels() == 4)
							{
								using(var bgr = new Mat())
								{
									Cv2.CvtColor(img, bgr, ColorConversionCodes.BGRA2BGR);
									Cv2.BilateralFilter(bgr, result, 9, sig, sig);
								}
							}
							else
							{
								Cv2.BilateralFilter(img, result, 9, sig, sig);
							}
							break;
					}
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "二值化 (Threshold)",
				Category          = "01. 預處理",
				DefaultParameters = new ThresholdParameters(),
				Action = (img, p) =>
				{
					var pp     = (ThresholdParameters)p;
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

					if(pp.Method == ThresholdParameters.ThreshMethod.Adaptive)
					{
						int blockSize = pp.AdaptiveBlockSize % 2 == 1 ? pp.AdaptiveBlockSize : pp.AdaptiveBlockSize + 1;
						Cv2.AdaptiveThreshold(gray, result, pp.MaxVal, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, blockSize, pp.AdaptiveC);
					}
					else
					{
						ThresholdTypes type = ThresholdTypes.Binary;

						if(pp.Method == ThresholdParameters.ThreshMethod.BinaryInv)
						{
							type = ThresholdTypes.BinaryInv;
						}
						else if(pp.Method == ThresholdParameters.ThreshMethod.Otsu)
						{
							type = ThresholdTypes.Otsu | ThresholdTypes.Binary;
						}
						else if(pp.Method == ThresholdParameters.ThreshMethod.ToZero)
						{
							type = ThresholdTypes.Tozero;
						}
						Cv2.Threshold(gray, result, pp.Threshold, pp.MaxVal, type);
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "形態學 (Morphology)",
				Category          = "01. 預處理",
				DefaultParameters = new MorphologyParameters(),
				Action = (img, p) =>
				{
					var pp     = (MorphologyParameters)p;
					var result = new Mat();

					using(var kernel = Cv2.GetStructuringElement(pp.Shape, new Size(pp.KernelSize, pp.KernelSize)))
					{
						MorphTypes op = MorphTypes.Open;

						switch(pp.Operation)
						{
							case MorphologyParameters.MorphOp.Erode:
								op = MorphTypes.Erode;
								break;
							case MorphologyParameters.MorphOp.Dilate:
								op = MorphTypes.Dilate;
								break;
							case MorphologyParameters.MorphOp.Close:
								op = MorphTypes.Close;
								break;
							case MorphologyParameters.MorphOp.Gradient:
								op = MorphTypes.Gradient;
								break;
							case MorphologyParameters.MorphOp.TopHat:
								op = MorphTypes.TopHat;
								break;
							case MorphologyParameters.MorphOp.BlackHat:
								op = MorphTypes.BlackHat;
								break;
						}
						Cv2.MorphologyEx(img, result, op, kernel, iterations: pp.Iterations);
					}
					return (true, result, new List<Defect>());
				},
			});

			// =========================================================
			// 02. 色彩處理 (Color Processing)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "色彩轉換 (Color Convert)",
				Category          = "02. 色彩處理",
				DefaultParameters = new ColorConvertParameters(),
				Action = (img, p) =>
				{
					var                  pp     = (ColorConvertParameters)p;
					var                  result = new Mat();
					ColorConversionCodes code   = ColorConversionCodes.BGR2GRAY;

					switch(pp.Type)
					{
						case ColorConvertParameters.ConversionType.BGR2HSV:
							code = ColorConversionCodes.BGR2HSV;
							break;
						case ColorConvertParameters.ConversionType.BGR2Lab:
							code = ColorConversionCodes.BGR2Lab;
							break;
						case ColorConvertParameters.ConversionType.HSV2BGR:
							code = ColorConversionCodes.HSV2BGR;
							break;
						case ColorConvertParameters.ConversionType.Gray2BGR:
							code = ColorConversionCodes.GRAY2BGR;
							break;
					}

					// Basic check to prevent crash if channels mismatch (e.g. converting Gray to Gray)
					try
					{
						Cv2.CvtColor(img, result, code);
					}
					catch
					{
						img.CopyTo(result); // Fallback
					}
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "直方圖均衡 (Equalize)",
				Category          = "02. 色彩處理",
				DefaultParameters = new HistogramEqualizeParameters(),
				Action = (img, p) =>
				{
					var pp     = (HistogramEqualizeParameters)p;
					var result = new Mat();
					var gray   = new Mat();

					if(img.Channels() == 3)
					{
						Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					}
					else
					{
						img.CopyTo(gray);
					}

					if(pp.UseCLAHE)
					{
						var clahe = Cv2.CreateCLAHE(pp.ClipLimit, new Size(pp.TileGridSize, pp.TileGridSize));
						clahe.Apply(gray, result);
					}
					else
					{
						Cv2.EqualizeHist(gray, result);
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "色彩過濾 (In Range)",
				Category          = "02. 色彩處理",
				DefaultParameters = new InRangeParameters(),
				Action = (img, p) =>
				{
					var pp     = (InRangeParameters)p;
					var result = new Mat();
					var lower  = new Scalar(pp.LowerH, pp.LowerS, pp.LowerV);
					var upper  = new Scalar(pp.UpperH, pp.UpperS, pp.UpperV);

					// Assume user knows what current color space is. 
					// Usually applied on HSV or BGR
					Cv2.InRange(img, lower, upper, result);
					return (true, result, new List<Defect>());
				},
			});

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

					// Usually needs edge detection first. User responsibility? 
					// Let's apply Canny internally if not binary?
					// To keep it "Toolbox" style, user should add Canny before. 
					// But HoughLines needs binary edge map.

					if(pp.UseProbabilistic)
					{
						var lines = Cv2.HoughLinesP(gray, pp.Rho, pp.ThetaDeg * Math.PI / 180, pp.Threshold, pp.MinLineLength, pp.MaxLineGap);

						foreach(var line in lines)
						{
							Cv2.Line(result, line.P1, line.P2, Scalar.Red, 2);
						}
					}
					else
					{
						var lines = Cv2.HoughLines(gray, pp.Rho, pp.ThetaDeg * Math.PI / 180, pp.Threshold);

						// Standard Hough returns (rho, theta), harder to draw infinite lines easily
						// Visualization omitted for standard Hough for brevity, falling back to P
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
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

			// =========================================================
			// 04. 繪圖與標註 (Drawing)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "繪製文字 (Draw Text)",
				Category          = "04. 繪圖與標註",
				DefaultParameters = new DrawTextParameters(),
				Action = (img, p) =>
				{
					var pp     = (DrawTextParameters)p;
					var result = img.Clone();

					// Ensure BGR
					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					var color = ParseColor(pp.ColorBGR);
					Cv2.PutText(result, pp.Text, new Point(pp.X, pp.Y), HersheyFonts.HersheySimplex, pp.FontScale, color, pp.Thickness);
					return (true, result, new List<Defect>());
				},
			});

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

			// =========================================================
			// 06. 降噪 (Denoise)
			// =========================================================
			tools.Add(new ToolDefinition
			{
				Name              = "非局部均值降噪 (NlMeans)",
				Category          = "06. 降噪",
				DefaultParameters = new DenoiseParameters(),
				Action = (img, p) =>
				{
					var pp     = (DenoiseParameters)p;
					var result = new Mat();
					var temp   = new Mat();

					// NlMeans only supports 8-bit?
					// Supports 8-bit 1-channel, 2-channel, 3-channel
					img.CopyTo(temp);

					if(img.Channels() == 3)
					{
						Cv2.FastNlMeansDenoisingColored(temp, result, pp.FilterStrength, pp.FilterStrength, pp.TemplateWindowSize, pp.SearchWindowSize);
					}
					else
					{
						Cv2.FastNlMeansDenoising(temp, result, pp.FilterStrength, pp.TemplateWindowSize, pp.SearchWindowSize);
					}
					temp.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			// =========================================================
			// 07. 特徵點 (Keypoints)
			// =========================================================
			tools.Add(new ToolDefinition
			{
				Name              = "特徵點偵測 (Keypoints)",
				Category          = "07. 特徵點",
				DefaultParameters = new FeatureDetectParameters(),
				Action = (img, p) =>
				{
					var pp     = (FeatureDetectParameters)p;
					var result = img.Clone();
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
					KeyPoint[] keypoints = null;

					if(pp.Detector == FeatureDetectParameters.DetectorType.ORB)
					{
						using(var orb = ORB.Create(pp.MaxFeatures))
						{
							keypoints = orb.Detect(gray);
						}
					}
					else if(pp.Detector == FeatureDetectParameters.DetectorType.FAST)
					{
						using(var fast = FastFeatureDetector.Create())
						{
							keypoints = fast.Detect(gray);
						}
					}
					else if(pp.Detector == FeatureDetectParameters.DetectorType.BRISK)
					{
						using(var brisk = BRISK.Create())
						{
							keypoints = brisk.Detect(gray);
						}
					}
					else if(pp.Detector == FeatureDetectParameters.DetectorType.AKAZE)
					{
						using(var akaze = AKAZE.Create())
						{
							keypoints = akaze.Detect(gray);
						}
					}

					// SIFT might not be available in standard build or needs xfeatures2d
					// Omitting SIFT for compatibility 

					// 依大小過濾
					if(keypoints != null)
					{
						if(pp.MinSize > 0)
						{
							keypoints = keypoints.Where(k => k.Size >= pp.MinSize).ToArray();
						}

						if(pp.MaxSize > 0)
						{
							keypoints = keypoints.Where(k => k.Size <= pp.MaxSize).ToArray();
						}
					}

					if(pp.DrawKeypoints && keypoints != null)
					{
						Cv2.DrawKeypoints(gray, keypoints, result, Scalar.RandomColor(), DrawMatchesFlags.DrawRichKeypoints);
					}
					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});

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

		public class ToolDefinition
		{
			public string       Name              { get; set; }
			public string       Category          { get; set; }
			public object       DefaultParameters { get; set; }
			public VisionAction Action            { get; set; }
		}
	}
}