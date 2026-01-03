using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace PCBInspection.Core.Services
{
	/// <summary>影像處理擴充工具 (對應 AISYS OVK OvkImage/OvkColor 模組)</summary>
	public static partial class VisionToolFactory
	{
		/// <summary>註冊影像處理擴充工具</summary>
		private static void AddImageProcessingTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 15. 影像處理擴充工具 (Image Processing - AISYS OVK 對應)
			// =========================================================

			// ---------------------------
			// 算術邏輯運算 (AxImageALops)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "算術邏輯運算 (Arithmetic Logic)",
				Category          = "15. 影像處理",
				DefaultParameters = new ImageArithmeticParameters(),
				Action = (img, p) =>
				{
					var pp     = (ImageArithmeticParameters)p;
					var result = new Mat();

					if (pp.UseScalar)
					{
						var scalar = new Scalar(pp.ScalarValue, pp.ScalarValue, pp.ScalarValue);
						switch (pp.Operation)
						{
							case ImageArithmeticParameters.OperationType.Add:
								Cv2.Add(img, scalar, result);
								break;
							case ImageArithmeticParameters.OperationType.Subtract:
								Cv2.Subtract(img, scalar, result);
								break;
							case ImageArithmeticParameters.OperationType.BitwiseNot:
								Cv2.BitwiseNot(img, result);
								break;
							default:
								img.CopyTo(result);
								OnLog?.Invoke($"[算術運算] 純量模式不支援 {pp.Operation}，請使用第二影像", true);
								break;
						}
					}
					else if (!string.IsNullOrEmpty(pp.SecondImagePath) && File.Exists(pp.SecondImagePath))
					{
						using (var img2 = Cv2.ImRead(pp.SecondImagePath, ImreadModes.Unchanged))
						{
							// 確保尺寸相同
							if (img.Size() != img2.Size())
							{
								Cv2.Resize(img2, img2, img.Size());
							}

							switch (pp.Operation)
							{
								case ImageArithmeticParameters.OperationType.Add:
									Cv2.Add(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.Subtract:
									Cv2.Subtract(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.Multiply:
									Cv2.Multiply(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.Divide:
									Cv2.Divide(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.BitwiseAnd:
									Cv2.BitwiseAnd(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.BitwiseOr:
									Cv2.BitwiseOr(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.BitwiseXor:
									Cv2.BitwiseXor(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.AbsDiff:
									Cv2.Absdiff(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.Max:
									Cv2.Max(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.Min:
									Cv2.Min(img, img2, result);
									break;
								case ImageArithmeticParameters.OperationType.BitwiseNot:
									Cv2.BitwiseNot(img, result);
									break;
							}
						}
					}
					else if (pp.Operation == ImageArithmeticParameters.OperationType.BitwiseNot)
					{
						Cv2.BitwiseNot(img, result);
					}
					else
					{
						img.CopyTo(result);
						OnLog?.Invoke("[算術運算] 找不到第二影像，請設定路徑或使用純量模式", true);
					}

					OnLog?.Invoke($"[算術運算] 完成 {pp.Operation} 運算", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 對比亮度調整 (AxImageGainOffset)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "對比亮度調整 (Gain/Offset)",
				Category          = "15. 影像處理",
				DefaultParameters = new GainOffsetParameters(),
				Action = (img, p) =>
				{
					var pp     = (GainOffsetParameters)p;
					var result = new Mat();

					// convertTo: dst = alpha * src + beta
					img.ConvertTo(result, -1, pp.Gain, pp.Offset);

					OnLog?.Invoke($"[對比亮度] Gain={pp.Gain:F2}, Offset={pp.Offset:F0}", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// LUT 色彩轉換 (AxImageLut)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "LUT 色彩轉換 (Look-Up Table)",
				Category          = "15. 影像處理",
				DefaultParameters = new LutParameters(),
				Action = (img, p) =>
				{
					var pp     = (LutParameters)p;
					var result = new Mat();

					// 建立 LUT 表格
					byte[] lut = new byte[256];

					switch (pp.Preset)
					{
						case LutParameters.LutPreset.Invert:
							for (int i = 0; i < 256; i++)
								lut[i] = (byte)(255 - i);
							break;

						case LutParameters.LutPreset.Gamma:
							double gamma = pp.GammaValue > 0 ? pp.GammaValue : 1.0;
							for (int i = 0; i < 256; i++)
								lut[i] = (byte)(255 * Math.Pow(i / 255.0, 1.0 / gamma));
							break;

						case LutParameters.LutPreset.Log:
							double c = 255.0 / Math.Log(256);
							for (int i = 0; i < 256; i++)
								lut[i] = (byte)(c * Math.Log(1 + i));
							break;

						case LutParameters.LutPreset.Custom:
							if (!string.IsNullOrEmpty(pp.CustomLut))
							{
								string[] vals = pp.CustomLut.Split(';');
								for (int i = 0; i < Math.Min(256, vals.Length); i++)
								{
									if (byte.TryParse(vals[i], out byte v))
										lut[i] = v;
								}
							}
							else
							{
								for (int i = 0; i < 256; i++)
									lut[i] = (byte)i;
							}
							break;
					}

					// 轉為灰階再套用 LUT (若為彩色則各通道分別套用)
					using (var lutMat = new Mat(1, 256, MatType.CV_8UC1, lut))
					{
						Cv2.LUT(img, lutMat, result);
					}

					OnLog?.Invoke($"[LUT] 套用 {pp.Preset} LUT 轉換", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 影像投影 (AxImageProjector)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "影像投影 (Image Projection)",
				Category          = "15. 影像處理",
				DefaultParameters = new ImageProjectionParameters(),
				Action = (img, p) =>
				{
					var pp     = (ImageProjectionParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					// 轉為灰階進行投影計算
					var gray = new Mat();
					if (img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else img.CopyTo(gray);

					// 設定 reduce 的維度和類型
					ReduceDimension dim = pp.Direction == ImageProjectionParameters.ProjectionDirection.Horizontal ? ReduceDimension.Column : ReduceDimension.Row;
					ReduceTypes rtype;
					switch (pp.Type)
					{
						case ImageProjectionParameters.ReduceType.Sum:     rtype = ReduceTypes.Sum;     break;
						case ImageProjectionParameters.ReduceType.Average: rtype = ReduceTypes.Avg;     break;
						case ImageProjectionParameters.ReduceType.Max:     rtype = ReduceTypes.Max;     break;
						case ImageProjectionParameters.ReduceType.Min:     rtype = ReduceTypes.Min;     break;
						default:                                           rtype = ReduceTypes.Avg;     break;
					}

					var projection = new Mat();
					Cv2.Reduce(gray, projection, dim, rtype, MatType.CV_32F);

					// 繪製投影圖
					if (pp.DrawProjection)
					{
						Scalar color = new Scalar(0, 255, 255);
						int graphSize = 60;

						if (pp.Direction == ImageProjectionParameters.ProjectionDirection.Horizontal)
						{
							// 水平投影 -> 結果為列向量 (rows x 1)
							float maxVal = 0;
							for (int i = 0; i < projection.Rows; i++)
								maxVal = Math.Max(maxVal, projection.At<float>(i, 0));

							for (int i = 0; i < projection.Rows; i++)
							{
								int barLen = (int)(graphSize * projection.At<float>(i, 0) / (maxVal + 1));
								Cv2.Line(result, new Point(result.Width - graphSize, i), new Point(result.Width - graphSize + barLen, i), color);
							}
						}
						else
						{
							// 垂直投影 -> 結果為行向量 (1 x cols)
							float maxVal = 0;
							for (int i = 0; i < projection.Cols; i++)
								maxVal = Math.Max(maxVal, projection.At<float>(0, i));

							for (int i = 0; i < projection.Cols; i++)
							{
								int barLen = (int)(graphSize * projection.At<float>(0, i) / (maxVal + 1));
								Cv2.Line(result, new Point(i, result.Height - graphSize), new Point(i, result.Height - graphSize + barLen), color);
							}
						}
					}

					gray.Dispose();
					projection.Dispose();

					OnLog?.Invoke($"[投影] {pp.Direction} 方向，{pp.Type} 累加方式", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 對焦評估 (AxImageFocusRatio)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "對焦評估 (Focus Ratio)",
				Category          = "15. 影像處理",
				DefaultParameters = new FocusRatioParameters(),
				Action = (img, p) =>
				{
					var pp     = (FocusRatioParameters)p;
					var result = img.Clone();

					if (result.Channels() == 1)
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);

					var gray = new Mat();
					if (img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else img.CopyTo(gray);

					double focusScore = 0;

					switch (pp.Method)
					{
						case FocusRatioParameters.FocusMethod.LaplacianVariance:
							using (var laplacian = new Mat())
							{
								Cv2.Laplacian(gray, laplacian, MatType.CV_64F);
								Cv2.MeanStdDev(laplacian, out _, out Scalar stddev);
								focusScore = stddev.Val0 * stddev.Val0; // 方差
							}
							break;

						case FocusRatioParameters.FocusMethod.SobelGradient:
							using (var sobelX = new Mat())
							using (var sobelY = new Mat())
							{
								Cv2.Sobel(gray, sobelX, MatType.CV_64F, 1, 0);
								Cv2.Sobel(gray, sobelY, MatType.CV_64F, 0, 1);
								focusScore = Cv2.Mean(sobelX.Abs()).Val0 + Cv2.Mean(sobelY.Abs()).Val0;
							}
							break;

						case FocusRatioParameters.FocusMethod.Tenengrad:
							using (var sobelX = new Mat())
							using (var sobelY = new Mat())
							using (var magnitude = new Mat())
							{
								Cv2.Sobel(gray, sobelX, MatType.CV_64F, 1, 0);
								Cv2.Sobel(gray, sobelY, MatType.CV_64F, 0, 1);
								Cv2.Magnitude(sobelX, sobelY, magnitude);
								focusScore = Cv2.Mean(magnitude).Val0;
							}
							break;
					}

					if (pp.ShowScore)
					{
						string text = $"Focus: {focusScore:F2}";
						Cv2.PutText(result, text, new Point(10, 30), HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 0), 2);
					}

					gray.Dispose();
					OnLog?.Invoke($"[對焦評估] {pp.Method}: {focusScore:F2}", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// 色版分離 (AxImageRgbSeparator 等)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "色版分離 (Channel Separator)",
				Category          = "15. 影像處理",
				DefaultParameters = new ChannelSeparatorParameters(),
				Action = (img, p) =>
				{
					var pp = (ChannelSeparatorParameters)p;

					if (img.Channels() < 3)
					{
						OnLog?.Invoke("[色版分離] 需要彩色影像", true);
						return (true, img.Clone(), new List<Defect>());
					}

					var converted = new Mat();

					// 轉換色彩空間
					switch (pp.ColorSpace)
					{
						case ChannelSeparatorParameters.ColorSpaceType.RGB:
							img.CopyTo(converted); // BGR 即是
							break;
						case ChannelSeparatorParameters.ColorSpaceType.HSV:
							Cv2.CvtColor(img, converted, ColorConversionCodes.BGR2HSV);
							break;
						case ChannelSeparatorParameters.ColorSpaceType.Lab:
							Cv2.CvtColor(img, converted, ColorConversionCodes.BGR2Lab);
							break;
						case ChannelSeparatorParameters.ColorSpaceType.Luv:
							Cv2.CvtColor(img, converted, ColorConversionCodes.BGR2Luv);
							break;
						case ChannelSeparatorParameters.ColorSpaceType.XYZ:
							Cv2.CvtColor(img, converted, ColorConversionCodes.BGR2XYZ);
							break;
						case ChannelSeparatorParameters.ColorSpaceType.YCrCb:
							Cv2.CvtColor(img, converted, ColorConversionCodes.BGR2YCrCb);
							break;
						default:
							img.CopyTo(converted);
							break;
					}

					// 分離通道
					Mat[] channels = Cv2.Split(converted);
					int idx = Math.Max(0, Math.Min(pp.ChannelIndex, channels.Length - 1));
					var result = channels[idx].Clone();

					foreach (var ch in channels) ch.Dispose();
					converted.Dispose();

					OnLog?.Invoke($"[色版分離] {pp.ColorSpace} 第 {idx} 通道", false);
					return (true, result, new List<Defect>());
				},
			});

			// ---------------------------
			// RGB 色版合成 (AxImageRgbComposer)
			// ---------------------------
			tools.Add(new ToolDefinition
			{
				Name              = "RGB 色版合成 (Channel Merge)",
				Category          = "15. 影像處理",
				DefaultParameters = new RgbComposerParameters(),
				Action = (img, p) =>
				{
					var pp = (RgbComposerParameters)p;

					Mat blue, green, red;

					// 載入各通道影像，若路徑為空則使用輸入影像的灰階版本
					if (!string.IsNullOrEmpty(pp.BlueChannelPath) && File.Exists(pp.BlueChannelPath))
						blue = Cv2.ImRead(pp.BlueChannelPath, ImreadModes.Grayscale);
					else
					{
						blue = new Mat();
						if (img.Channels() >= 3)
						{
							Mat[] chs = Cv2.Split(img);
							blue = chs[0].Clone();
							foreach (var ch in chs) ch.Dispose();
						}
						else
							img.CopyTo(blue);
					}

					if (!string.IsNullOrEmpty(pp.GreenChannelPath) && File.Exists(pp.GreenChannelPath))
						green = Cv2.ImRead(pp.GreenChannelPath, ImreadModes.Grayscale);
					else
					{
						green = new Mat();
						if (img.Channels() >= 3)
						{
							Mat[] chs = Cv2.Split(img);
							green = chs[1].Clone();
							foreach (var ch in chs) ch.Dispose();
						}
						else
							img.CopyTo(green);
					}

					if (!string.IsNullOrEmpty(pp.RedChannelPath) && File.Exists(pp.RedChannelPath))
						red = Cv2.ImRead(pp.RedChannelPath, ImreadModes.Grayscale);
					else
					{
						red = new Mat();
						if (img.Channels() >= 3)
						{
							Mat[] chs = Cv2.Split(img);
							red = chs[2].Clone();
							foreach (var ch in chs) ch.Dispose();
						}
						else
							img.CopyTo(red);
					}

					// 確保尺寸一致
					Size targetSize = blue.Size();
					if (green.Size() != targetSize) Cv2.Resize(green, green, targetSize);
					if (red.Size() != targetSize) Cv2.Resize(red, red, targetSize);

					var result = new Mat();
					Cv2.Merge(new[] { blue, green, red }, result);

					blue.Dispose();
					green.Dispose();
					red.Dispose();

					OnLog?.Invoke("[RGB合成] 完成三通道合成", false);
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}
