using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		private static void AddColorProcessingTools(List<ToolDefinition> tools)
		{
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

			tools.Add(new ToolDefinition
			{
				Name              = "顏色分析 (Color Analysis)",
				Category          = "02. 色彩處理",
				DefaultParameters = new ColorAnalysisParameters(),
				Action = (img, p) =>
				{
					var pp     = (ColorAnalysisParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}

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
									int idx   = y * img.Cols + x;
									data.Set(idx, 0, (float)pixel.Item0);
									data.Set(idx, 1, (float)pixel.Item1);
									data.Set(idx, 2, (float)pixel.Item2);
								}
							var labels  = new Mat();
							var centers = new Mat();
							Cv2.Kmeans(data, pp.ColorCount, labels, new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 10, 1.0), 3, KMeansFlags.PpCenters, centers);

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
									if(img.Channels() >= 3)
									{
										Cv2.CvtColor(img, lab, ColorConversionCodes.BGR2Lab);
									}
									else
									{
										throw new InvalidOperationException("色差計算需要彩色影像。");
									}
									var    mean   = Cv2.Mean(lab);
									double dL     = mean.Val0 - stdL;
									double dA     = mean.Val1 - 128 - stdA;
									double dB     = mean.Val2 - 128 - stdB;
									double deltaE = Math.Sqrt(dL * dL + dA * dA + dB * dB);
									bool   pass   = deltaE <= pp.DeltaEThreshold;
									Cv2.PutText(result, $"ΔE: {deltaE:F2} ({(pass ? "PASS" : "FAIL")})", new Point(10, 30), HersheyFonts.HersheySimplex, 0.7, pass ? Scalar.Green : Scalar.Red, 2);
									OnLog?.Invoke($"[顏色分析] ΔE: {deltaE:F2} (閾值: {pp.DeltaEThreshold})", false);
								}
							}
						}
						catch
						{
						}
					}
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}