using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		/// <summary>註冊背景處理與缺陷檢測類別工具</summary>
		private static void AddBackgroundDefectTools(List<ToolDefinition> tools)
		{
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
					var pp     = (BackgroundSubtractionParameters)p;
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
							Cv2.MorphologyEx(fgMask, fgMask, MorphTypes.Open,  kernel);
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
					var pp     = (DefectDetectionParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}
					var defects  = new List<Defect>();
					Mat diffMask = new Mat();

					if(pp.Mode == DefectDetectionParameters.DetectionMode.TemplateDiff)
					{
						if(string.IsNullOrEmpty(pp.ReferenceSamplePath) || !File.Exists(pp.ReferenceSamplePath))
						{
							// 若未設定參考影像，改用 EdgeBased 模式
							Cv2.PutText(result, "No reference image, using EdgeBased mode", new Point(10, 30), HersheyFonts.HersheySimplex, 0.4, Scalar.Yellow);
							Mat gray = new Mat();

							if(img.Channels() >= 3)
							{
								Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
							}
							else
							{
								img.CopyTo(gray);
							}
							Cv2.Canny(gray, diffMask, 50, 150);

							using(Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
							{
								Cv2.Dilate(diffMask, diffMask, kernel);
							}
							gray.Dispose();
						}
						else
						{
							using(Mat refImg = Cv2.ImRead(pp.ReferenceSamplePath))
							{
								if(!refImg.Empty())
								{
									Mat  imgResized  = img;
									bool needDispose = false;

									if(img.Width != refImg.Width || img.Height != refImg.Height)
									{
										imgResized = new Mat();
										Cv2.Resize(img, imgResized, refImg.Size());
										needDispose = true;
										Cv2.Resize(result, result, refImg.Size());
									}

									using(Mat gray1 = new Mat())
									{
										using(Mat gray2 = new Mat())
										{
											using(Mat diff = new Mat())
											{
												if(imgResized.Channels() >= 3)
												{
													Cv2.CvtColor(imgResized, gray1, ColorConversionCodes.BGR2GRAY);
												}
												else
												{
													imgResized.CopyTo(gray1);
												}

												if(refImg.Channels() >= 3)
												{
													Cv2.CvtColor(refImg, gray2, ColorConversionCodes.BGR2GRAY);
												}
												else
												{
													refImg.CopyTo(gray2);
												}
												Cv2.Absdiff(gray1, gray2, diff);
												Cv2.Threshold(diff, diffMask, pp.DifferenceThreshold, 255, ThresholdTypes.Binary);
											}
										}
									}

									if(needDispose)
									{
										imgResized.Dispose();
									}
								}
							}
						}
					}
					else if(pp.Mode == DefectDetectionParameters.DetectionMode.EdgeBased)
					{
						Mat gray = new Mat();

						if(img.Channels() >= 3)
						{
							Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
						}
						else
						{
							img.CopyTo(gray);
						}
						Cv2.Canny(gray, diffMask, 50, 150);

						using(Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
						{
							Cv2.Dilate(diffMask, diffMask, kernel);
						}
						gray.Dispose();
					}
					else // ColorBased
					{
						using(Mat hsv = new Mat())
						{
							if(img.Channels() >= 3)
							{
								Cv2.CvtColor(img, hsv, ColorConversionCodes.BGR2HSV);
							}
							else
							{
								throw new InvalidOperationException("色彩檢測需要彩色影像。");
							}

							// 簡單的飽和度閾值
							Cv2.ExtractChannel(hsv, diffMask, 1);
							Cv2.Threshold(diffMask, diffMask, pp.DifferenceThreshold, 255, ThresholdTypes.Binary);
						}
					}

					// 找輪廓作為缺陷
					Cv2.FindContours(diffMask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
					int          idx      = 0;
					List<string> csvLines = new List<string> { "Index,X,Y,Width,Height,Area,Type" };

					foreach(Point[] contour in contours)
					{
						double area = Cv2.ContourArea(contour);

						if(area < pp.MinDefectArea)
						{
							continue;
						}

						if(pp.MaxDefectArea > 0 && area > pp.MaxDefectArea)
						{
							continue;
						}
						Rect rect = Cv2.BoundingRect(contour);

						if(pp.HighlightDefects)
						{
							Cv2.Rectangle(result, rect, Scalar.Red, 2);
							Cv2.PutText(result, $"D{idx + 1}", new Point(rect.X, rect.Y - 5), HersheyFonts.HersheySimplex, 0.4, Scalar.Red);
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
		}
	}
}