using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System.Collections.Generic;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		/// <summary>
		/// 註冊預處理類別工具。
		/// 包含：灰階化、濾波模糊、二值化、形態學運算。
		/// </summary>
		/// <param name="tools">欲加入工具定義的清單物件</param>
		private static void AddPreprocessingTools(List<ToolDefinition> tools)
		{
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
		}
	}
}