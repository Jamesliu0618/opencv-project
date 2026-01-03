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

			// =========================================================
			// 08. 相機校正 (Camera Calibration)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "相機校正 (Camera Calibration)",
				Category          = "08. 校正",
				DefaultParameters = new CameraCalibrationParameters(),
				Action = (img, p) =>
				{
					var pp = (CameraCalibrationParameters)p;

					if(string.IsNullOrEmpty(pp.CalibrationImagesFolder) || !Directory.Exists(pp.CalibrationImagesFolder))
					{
						throw new InvalidOperationException("校正影像資料夾不存在或未指定。");
					}

					var imageFiles = Directory.GetFiles(pp.CalibrationImagesFolder, "*.jpg")
						.Concat(Directory.GetFiles(pp.CalibrationImagesFolder, "*.png"))
						.Concat(Directory.GetFiles(pp.CalibrationImagesFolder, "*.bmp"))
						.ToArray();

					if(imageFiles.Length < 3)
					{
						throw new InvalidOperationException("至少需要 3 張校正影像。");
					}

					var patternSize = new Size(pp.PatternWidth, pp.PatternHeight);
					var objPoints   = new List<List<Point3f>>();
					var imgPoints   = new List<List<Point2f>>();
					Size imageSize  = new Size();

					// 建立 3D 物件點
					var objp = new List<Point3f>();
					for(int y = 0; y < pp.PatternHeight; y++)
						for(int x = 0; x < pp.PatternWidth; x++)
							objp.Add(new Point3f(x * pp.SquareSize, y * pp.SquareSize, 0));

					int validCount = 0;
					foreach(var file in imageFiles)
					{
						using(var calibImg = Cv2.ImRead(file, ImreadModes.Grayscale))
						{
							if(calibImg.Empty()) continue;
							imageSize = calibImg.Size();

							if(Cv2.FindChessboardCorners(calibImg, patternSize, out Point2f[] corners))
							{
								Cv2.CornerSubPix(calibImg, corners, new Size(11, 11), new Size(-1, -1),
									new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 0.001));
								objPoints.Add(objp);
								imgPoints.Add(corners.ToList());
								validCount++;
							}
						}
					}

					if(validCount < 3)
					{
						throw new InvalidOperationException($"僅找到 {validCount} 張有效的棋盤格影像，至少需要 3 張。");
					}

					// 校正 - 將點座標轉換為 Mat 陣列
					var cameraMatrix = new Mat();
					var distCoeffs   = new Mat();

					// 轉換為 IEnumerable<Mat> 格式
					var objPointsMats = new List<Mat>();
					var imgPointsMats = new List<Mat>();
					foreach(var pts in objPoints)
					{
						var mat = new Mat(pts.Count, 1, MatType.CV_32FC3);
						for(int i = 0; i < pts.Count; i++)
							mat.Set(i, 0, new Vec3f(pts[i].X, pts[i].Y, pts[i].Z));
						objPointsMats.Add(mat);
					}
					foreach(var pts in imgPoints)
					{
						var mat = new Mat(pts.Count, 1, MatType.CV_32FC2);
						for(int i = 0; i < pts.Count; i++)
							mat.Set(i, 0, new Vec2f(pts[i].X, pts[i].Y));
						imgPointsMats.Add(mat);
					}

					double rms = Cv2.CalibrateCamera(objPointsMats, imgPointsMats,
						imageSize, cameraMatrix, distCoeffs, out Mat[] rvecsOut, out Mat[] tvecsOut);

					// 釋放暫時 Mat
					foreach(var m in objPointsMats) m.Dispose();
					foreach(var m in imgPointsMats) m.Dispose();

					// 儲存結果
					using(var fs = new FileStorage(pp.OutputCameraMatrixPath, FileStorage.Modes.Write))
					{
						fs.Write("camera_matrix", cameraMatrix);
					}
					using(var fs = new FileStorage(pp.OutputDistCoeffsPath, FileStorage.Modes.Write))
					{
						fs.Write("dist_coeffs", distCoeffs);
					}

					if(pp.ShowReprojectionError)
					{
						OnLog?.Invoke($"[校正] 重投影誤差 RMS: {rms:F4} ({validCount} 張影像)", false);
					}

					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					Cv2.PutText(result, $"Calibration RMS: {rms:F4}", new Point(10, 30), HersheyFonts.HersheySimplex, 0.8, Scalar.Green, 2);
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "畸變矯正 (Undistort)",
				Category          = "08. 校正",
				DefaultParameters = new UndistortParameters(),
				Action = (img, p) =>
				{
					var pp = (UndistortParameters)p;

					if(!File.Exists(pp.CameraMatrixPath))
						throw new InvalidOperationException($"相機矩陣檔案不存在: {pp.CameraMatrixPath}");
					if(!File.Exists(pp.DistCoeffsPath))
						throw new InvalidOperationException($"畸變係數檔案不存在: {pp.DistCoeffsPath}");

					Mat cameraMatrix, distCoeffs;
					using(var fs = new FileStorage(pp.CameraMatrixPath, FileStorage.Modes.Read))
					{
						cameraMatrix = fs["camera_matrix"].ReadMat();
					}
					using(var fs = new FileStorage(pp.DistCoeffsPath, FileStorage.Modes.Read))
					{
						distCoeffs = fs["dist_coeffs"].ReadMat();
					}

					var result = new Mat();
					if(pp.AutoCropBlackBorder)
					{
						var newCameraMatrix = Cv2.GetOptimalNewCameraMatrix(cameraMatrix, distCoeffs, img.Size(), 0, img.Size(), out Rect roi);
						Cv2.Undistort(img, result, cameraMatrix, distCoeffs, newCameraMatrix);
						if(roi.Width > 0 && roi.Height > 0)
						{
							result = new Mat(result, roi).Clone();
						}
					}
					else
					{
						Cv2.Undistort(img, result, cameraMatrix, distCoeffs);
					}
					cameraMatrix.Dispose();
					distCoeffs.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "透視變換 (Perspective Transform)",
				Category          = "08. 校正",
				DefaultParameters = new PerspectiveTransformParameters(),
				Action = (img, p) =>
				{
					var pp = (PerspectiveTransformParameters)p;

					var srcPoints = new Point2f[]
					{
						new Point2f(pp.SrcTopLeftX, pp.SrcTopLeftY),
						new Point2f(pp.SrcTopRightX, pp.SrcTopRightY),
						new Point2f(pp.SrcBottomRightX, pp.SrcBottomRightY),
						new Point2f(pp.SrcBottomLeftX, pp.SrcBottomLeftY),
					};

					var dstPoints = new Point2f[]
					{
						new Point2f(0, 0),
						new Point2f(pp.OutputWidth - 1, 0),
						new Point2f(pp.OutputWidth - 1, pp.OutputHeight - 1),
						new Point2f(0, pp.OutputHeight - 1),
					};

					var M = Cv2.GetPerspectiveTransform(srcPoints, dstPoints);
					var result = new Mat();

					InterpolationFlags interp = InterpolationFlags.Linear;
					switch(pp.Interpolation)
					{
						case PerspectiveTransformParameters.InterpolationType.Nearest: interp = InterpolationFlags.Nearest; break;
						case PerspectiveTransformParameters.InterpolationType.Cubic: interp = InterpolationFlags.Cubic; break;
						case PerspectiveTransformParameters.InterpolationType.Lanczos4: interp = InterpolationFlags.Lanczos4; break;
					}

					Cv2.WarpPerspective(img, result, M, new Size(pp.OutputWidth, pp.OutputHeight), interp);
					M.Dispose();
					return (true, result, new List<Defect>());
				},
			});

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
					var pp = (MeasurementToolParameters)p;
					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					double dx = pp.EndX - pp.StartX;
					double dy = pp.EndY - pp.StartY;
					double distPx = Math.Sqrt(dx * dx + dy * dy);
					double distReal = distPx * pp.PixelScale;

					string unit = pp.DisplayUnit;
					if(unit == "μm") distReal *= 1000;
					else if(unit == "cm") distReal /= 10;

					string format = $"F{pp.DecimalPlaces}";
					string text = $"{distReal.ToString(format)} {unit}";

					if(pp.DrawOnImage)
					{
						Cv2.Line(result, new Point(pp.StartX, pp.StartY), new Point(pp.EndX, pp.EndY), Scalar.Cyan, 2);
						Cv2.Circle(result, new Point(pp.StartX, pp.StartY), 4, Scalar.Green, -1);
						Cv2.Circle(result, new Point(pp.EndX, pp.EndY), 4, Scalar.Red, -1);
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
					var pp = (ObjectAnalysisParameters)p;
					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					var gray = new Mat();
					if(img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else img.CopyTo(gray);

					Cv2.FindContours(gray, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
					var defects = new List<Defect>();
					int idx = 0;

					foreach(var contour in contours)
					{
						double area = Cv2.ContourArea(contour);
						if(area < pp.FilterMinArea) continue;
						if(pp.FilterMaxArea > 0 && area > pp.FilterMaxArea) continue;

						double perimeter = Cv2.ArcLength(contour, true);
						double circularity = 4 * Math.PI * area / (perimeter * perimeter);
						var rect = Cv2.BoundingRect(contour);
						var minRect = Cv2.MinAreaRect(contour);
						double aspectRatio = (double)rect.Width / Math.Max(rect.Height, 1);
						double rectangularity = area / (rect.Width * rect.Height + 0.001);

						if(pp.ComputeBoundingRect)
							Cv2.Rectangle(result, rect, Scalar.Green, 1);

						if(pp.ComputeMinAreaRect)
						{
							var pts = Cv2.BoxPoints(minRect).Select(pt => new Point((int)pt.X, (int)pt.Y)).ToArray();
							Cv2.Polylines(result, new[] { pts }, true, Scalar.Cyan, 1);
						}

						if(pp.ComputeMinEnclosingCircle)
						{
							Cv2.MinEnclosingCircle(contour, out Point2f center, out float radius);
							Cv2.Circle(result, (int)center.X, (int)center.Y, (int)radius, Scalar.Magenta, 1);
						}

						if(pp.ComputeConvexHull)
						{
							var hull = Cv2.ConvexHull(contour);
							Cv2.Polylines(result, new[] { hull }, true, Scalar.Yellow, 1);
						}

						if(pp.LabelObjectIndex)
						{
							Cv2.PutText(result, $"#{idx + 1}", new Point(rect.X, rect.Y - 5), HersheyFonts.HersheySimplex, 0.4, Scalar.White, 1);
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
					var pp = (HistogramAnalysisParameters)p;
					var result = img.Clone();

					var gray = new Mat();
					if(img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else img.CopyTo(gray);

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
					using(var hist = new Mat())
					{
						int[] histSize = { 256 };
						Rangef[] ranges = { new Rangef(0, 256) };
						Cv2.CalcHist(new[] { gray }, new[] { 0 }, null, hist, 1, histSize, ranges);
						Cv2.Normalize(hist, hist, 0, 100, NormTypes.MinMax);

						if(pp.DrawOnImage)
						{
							int histW = 256, histH = 100;
							int offsetX = result.Width - histW - 10;
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
					var pp = (ProfileLineParameters)p;
					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					var gray = new Mat();
					if(img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else img.CopyTo(gray);

					// 取得剖面線上的點
					int dx = pp.EndX - pp.StartX;
					int dy = pp.EndY - pp.StartY;
					int length = (int)Math.Max(Math.Sqrt(dx * dx + dy * dy), 1);
					var profileData = new List<byte>();

					for(int i = 0; i <= length; i++)
					{
						int x = pp.StartX + (int)(dx * i / (double)length);
						int y = pp.StartY + (int)(dy * i / (double)length);
						x = Math.Max(0, Math.Min(x, gray.Width - 1));
						y = Math.Max(0, Math.Min(y, gray.Height - 1));
						profileData.Add(gray.At<byte>(y, x));
					}

					// 繪製剖面線標示
					Cv2.Line(result, new Point(pp.StartX, pp.StartY), new Point(pp.EndX, pp.EndY), Scalar.Cyan, 2);
					Cv2.Circle(result, new Point(pp.StartX, pp.StartY), 4, Scalar.Green, -1);
					Cv2.Circle(result, new Point(pp.EndX, pp.EndY), 4, Scalar.Red, -1);

					// 繪製小型剖面圖在影像右下角
					int graphW = Math.Min(256, length);
					int graphH = 60;
					int offsetX = result.Width - graphW - 10;
					int offsetY = result.Height - graphH - 10;

					Cv2.Rectangle(result, new Rect(offsetX - 2, offsetY - 2, graphW + 4, graphH + 4), Scalar.Black, -1);
					for(int i = 1; i < graphW && i < profileData.Count; i++)
					{
						int idx1 = (i - 1) * profileData.Count / graphW;
						int idx2 = i * profileData.Count / graphW;
						int y1 = offsetY + graphH - profileData[idx1] * graphH / 255;
						int y2 = offsetY + graphH - profileData[idx2] * graphH / 255;
						Cv2.Line(result, new Point(offsetX + i - 1, y1), new Point(offsetX + i, y2), Scalar.Green);
					}

					if(pp.OutputToCsv && !string.IsNullOrEmpty(pp.CsvOutputPath))
					{
						var lines = profileData.Select((v, i) => $"{i},{v}");
						File.WriteAllLines(pp.CsvOutputPath, new[] { "Index,GrayValue" }.Concat(lines));
						OnLog?.Invoke($"[剖面線] 已輸出 {profileData.Count} 筆數據到 {pp.CsvOutputPath}", false);
					}

					gray.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			// =========================================================
			// 10. 影像品質評估 (Quality Assessment)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "品質評估 (Quality Assessment)",
				Category          = "10. 品質評估",
				DefaultParameters = new QualityAssessmentParameters(),
				Action = (img, p) =>
				{
					var pp = (QualityAssessmentParameters)p;

					if(string.IsNullOrEmpty(pp.ReferenceImagePath) || !File.Exists(pp.ReferenceImagePath))
					{
						throw new InvalidOperationException($"參考影像不存在: {pp.ReferenceImagePath}");
					}

					using(var refImg = Cv2.ImRead(pp.ReferenceImagePath))
					{
						if(refImg.Empty())
							throw new InvalidOperationException("無法載入參考影像。");

						// 確保尺寸相同
						Mat imgResized = img;
						bool needDispose = false;
						if(img.Width != refImg.Width || img.Height != refImg.Height)
						{
							imgResized = new Mat();
							Cv2.Resize(img, imgResized, refImg.Size());
							needDispose = true;
						}

						double score = 0;
						string metricName = "";

						var result = imgResized.Clone();
						if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

						switch(pp.Metric)
						{
							case QualityAssessmentParameters.QualityMetric.PSNR:
								score = Cv2.PSNR(imgResized, refImg);
								metricName = "PSNR";
								break;

							case QualityAssessmentParameters.QualityMetric.MSE:
								using(var diff = new Mat())
								{
									Cv2.Absdiff(imgResized, refImg, diff);
									diff.ConvertTo(diff, MatType.CV_32F);
									Cv2.Multiply(diff, diff, diff);
									score = Cv2.Mean(diff).Val0;
								}
								metricName = "MSE";
								break;

							case QualityAssessmentParameters.QualityMetric.SSIM:
								// 簡化版 SSIM 計算
								using(var gray1 = new Mat())
								using(var gray2 = new Mat())
								{
									if(imgResized.Channels() >= 3) Cv2.CvtColor(imgResized, gray1, ColorConversionCodes.BGR2GRAY);
									else imgResized.CopyTo(gray1);
									if(refImg.Channels() >= 3) Cv2.CvtColor(refImg, gray2, ColorConversionCodes.BGR2GRAY);
									else refImg.CopyTo(gray2);

									score = Cv2.PSNR(gray1, gray2) / 50.0; // 近似 SSIM (簡化)
									if(score > 1) score = 1;
								}
								metricName = "SSIM (approx)";
								break;
						}

						if(pp.ShowScore)
						{
							Cv2.PutText(result, $"{metricName}: {score:F4}", new Point(10, 30), HersheyFonts.HersheySimplex, 0.8, Scalar.Green, 2);
						}

						if(pp.OutputReport)
						{
							OnLog?.Invoke($"[品質評估] {metricName}: {score:F4}", false);
						}

						if(needDispose) imgResized.Dispose();
						return (true, result, new List<Defect>());
					}
				},
			});

			// =========================================================
			// 11. 顏色分析 (Color Analysis)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "顏色分析 (Color Analysis)",
				Category          = "02. 色彩處理",
				DefaultParameters = new ColorAnalysisParameters(),
				Action = (img, p) =>
				{
					var pp = (ColorAnalysisParameters)p;
					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					if(pp.ComputeDominantColors && img.Channels() >= 3)
					{
						// 將影像轉為 float 並 reshape 為 Nx3
						using(var samples = new Mat())
						{
							img.ConvertTo(samples, MatType.CV_32FC3);
							samples.Reshape(1, img.Rows * img.Cols);
							var data = new Mat(img.Rows * img.Cols, 3, MatType.CV_32F);

							for(int y = 0; y < img.Rows; y++)
								for(int x = 0; x < img.Cols; x++)
								{
									var pixel = img.At<Vec3b>(y, x);
									int idx = y * img.Cols + x;
									data.Set(idx, 0, (float)pixel.Item0);
									data.Set(idx, 1, (float)pixel.Item1);
									data.Set(idx, 2, (float)pixel.Item2);
								}

							var labels = new Mat();
							var centers = new Mat();
							Cv2.Kmeans(data, pp.ColorCount, labels,
								new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 10, 1.0),
								3, KMeansFlags.PpCenters, centers);

							if(pp.ShowColorDistribution)
							{
								int swatchH = 30;
								int swatchW = result.Width / pp.ColorCount;
								int offsetY = result.Height - swatchH - 5;

								for(int i = 0; i < centers.Rows; i++)
								{
									var b = (int)centers.At<float>(i, 0);
									var g = (int)centers.At<float>(i, 1);
									var r = (int)centers.At<float>(i, 2);
									Cv2.Rectangle(result, new Rect(i * swatchW, offsetY, swatchW, swatchH), new Scalar(b, g, r), -1);
								}
							}

							data.Dispose();
							labels.Dispose();
							centers.Dispose();
						}
					}

					if(pp.ComputeColorDifference)
					{
						// 計算平均 Lab 色差
						try
						{
							var parts = pp.StandardLabColor.Split(',');
							if(parts.Length == 3)
							{
								double stdL = double.Parse(parts[0]);
								double stdA = double.Parse(parts[1]);
								double stdB = double.Parse(parts[2]);

								using(var lab = new Mat())
								{
									if(img.Channels() >= 3) Cv2.CvtColor(img, lab, ColorConversionCodes.BGR2Lab);
									else throw new InvalidOperationException("色差計算需要彩色影像。");

									var mean = Cv2.Mean(lab);
									double dL = mean.Val0 - stdL;
									double dA = mean.Val1 - 128 - stdA;
									double dB = mean.Val2 - 128 - stdB;
									double deltaE = Math.Sqrt(dL * dL + dA * dA + dB * dB);

									bool pass = deltaE <= pp.DeltaEThreshold;
									Cv2.PutText(result, $"ΔE: {deltaE:F2} ({(pass ? "PASS" : "FAIL")})", new Point(10, 30),
										HersheyFonts.HersheySimplex, 0.7, pass ? Scalar.Green : Scalar.Red, 2);
									OnLog?.Invoke($"[顏色分析] ΔE: {deltaE:F2} (閾值: {pp.DeltaEThreshold})", false);
								}
							}
						}
						catch { }
					}

					return (true, result, new List<Defect>());
				},
			});

			// =========================================================
			// 12. 背景處理 (Background Processing)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "背景分割 (Background Subtraction)",
				Category          = "12. 背景處理",
				DefaultParameters = new BackgroundSubtractionParameters(),
				Action = (img, p) =>
				{
					var pp = (BackgroundSubtractionParameters)p;
					var fgMask = new Mat();

					// 建立背景分割器 (由於是單張影像，這裡示範基本用法)
					if(pp.Method == BackgroundSubtractionParameters.SubtractionMethod.MOG2)
					{
						using(var bgSub = BackgroundSubtractorMOG2.Create(pp.History, pp.VarThreshold, pp.DetectShadows))
						{
							bgSub.Apply(img, fgMask, pp.LearningRate);
						}
					}
					else
					{
						using(var bgSub = BackgroundSubtractorKNN.Create(pp.History, pp.VarThreshold, pp.DetectShadows))
						{
							bgSub.Apply(img, fgMask, pp.LearningRate);
						}
					}

					if(pp.MorphologicalPostProcess)
					{
						using(var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(pp.MorphKernelSize, pp.MorphKernelSize)))
						{
							Cv2.MorphologyEx(fgMask, fgMask, MorphTypes.Open, kernel);
							Cv2.MorphologyEx(fgMask, fgMask, MorphTypes.Close, kernel);
						}
					}

					var result = new Mat();
					Cv2.CvtColor(fgMask, result, ColorConversionCodes.GRAY2BGR);
					return (true, result, new List<Defect>());
				},
			});

			// =========================================================
			// 13. 缺陷檢測 (Defect Detection)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "缺陷偵測 (Defect Detection)",
				Category          = "13. 缺陷檢測",
				DefaultParameters = new DefectDetectionParameters(),
				Action = (img, p) =>
				{
					var pp = (DefectDetectionParameters)p;
					var result = img.Clone();
					if(result.Channels() == 1) Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					var defects = new List<Defect>();
					Mat diffMask = new Mat();

					if(pp.Mode == DefectDetectionParameters.DetectionMode.TemplateDiff)
					{
						if(string.IsNullOrEmpty(pp.ReferenceSamplePath) || !File.Exists(pp.ReferenceSamplePath))
						{
							throw new InvalidOperationException($"參考樣本影像不存在: {pp.ReferenceSamplePath}");
						}

						using(var refImg = Cv2.ImRead(pp.ReferenceSamplePath))
						{
							if(refImg.Empty())
								throw new InvalidOperationException("無法載入參考樣本影像。");

							Mat imgResized = img;
							bool needDispose = false;
							if(img.Width != refImg.Width || img.Height != refImg.Height)
							{
								imgResized = new Mat();
								Cv2.Resize(img, imgResized, refImg.Size());
								needDispose = true;
								Cv2.Resize(result, result, refImg.Size());
							}

							using(var gray1 = new Mat())
							using(var gray2 = new Mat())
							using(var diff = new Mat())
							{
								if(imgResized.Channels() >= 3) Cv2.CvtColor(imgResized, gray1, ColorConversionCodes.BGR2GRAY);
								else imgResized.CopyTo(gray1);
								if(refImg.Channels() >= 3) Cv2.CvtColor(refImg, gray2, ColorConversionCodes.BGR2GRAY);
								else refImg.CopyTo(gray2);

								Cv2.Absdiff(gray1, gray2, diff);
								Cv2.Threshold(diff, diffMask, pp.DifferenceThreshold, 255, ThresholdTypes.Binary);
							}

							if(needDispose) imgResized.Dispose();
						}
					}
					else if(pp.Mode == DefectDetectionParameters.DetectionMode.EdgeBased)
					{
						var gray = new Mat();
						if(img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
						else img.CopyTo(gray);

						Cv2.Canny(gray, diffMask, 50, 150);
						using(var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
						{
							Cv2.Dilate(diffMask, diffMask, kernel);
						}
						gray.Dispose();
					}
					else // ColorBased
					{
						using(var hsv = new Mat())
						{
							if(img.Channels() >= 3) Cv2.CvtColor(img, hsv, ColorConversionCodes.BGR2HSV);
							else throw new InvalidOperationException("色彩檢測需要彩色影像。");

							// 簡單的飽和度閾值
							Cv2.ExtractChannel(hsv, diffMask, 1);
							Cv2.Threshold(diffMask, diffMask, pp.DifferenceThreshold, 255, ThresholdTypes.Binary);
						}
					}

					// 找輪廓作為缺陷
					Cv2.FindContours(diffMask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
					int idx = 0;
					var csvLines = new List<string> { "Index,X,Y,Width,Height,Area,Type" };

					foreach(var contour in contours)
					{
						double area = Cv2.ContourArea(contour);
						if(area < pp.MinDefectArea) continue;
						if(pp.MaxDefectArea > 0 && area > pp.MaxDefectArea) continue;

						var rect = Cv2.BoundingRect(contour);

						if(pp.HighlightDefects)
						{
							Cv2.Rectangle(result, rect, Scalar.Red, 2);
							Cv2.PutText(result, $"D{idx + 1}", new Point(rect.X, rect.Y - 5), HersheyFonts.HersheySimplex, 0.4, Scalar.Red, 1);
						}

						defects.Add(new Defect
						{
							Id          = $"D{idx + 1}",
							Type        = pp.TypeFilter.ToString(),
							Confidence  = area,
							BoundingBox = new[] { rect.X, rect.Y, rect.Width, rect.Height },
						});

						csvLines.Add($"{idx + 1},{rect.X},{rect.Y},{rect.Width},{rect.Height},{area:F0},{pp.TypeFilter}");
						idx++;
					}

					if(pp.OutputDefectReport)
					{
						OnLog?.Invoke($"[缺陷檢測] 共發現 {idx} 個缺陷區域", false);
					}

					if(pp.OutputToCsv && !string.IsNullOrEmpty(pp.CsvOutputPath))
					{
						File.WriteAllLines(pp.CsvOutputPath, csvLines);
						OnLog?.Invoke($"[缺陷檢測] 已輸出報告到 {pp.CsvOutputPath}", false);
					}

					diffMask.Dispose();
					return (true, result, defects);
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