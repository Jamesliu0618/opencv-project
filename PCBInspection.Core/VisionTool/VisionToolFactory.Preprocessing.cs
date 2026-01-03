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
					// 1. 初始化結果矩陣
					var result = new Mat();

					// 2. 依據輸入通道數執行轉換 (BGR/BGRA -> GRAY)
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
						// 若已是單通道則直接複製
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
					// 1. 初始化與參數驗證
					var pp     = (BlurParameters)p;
					var result = new Mat();
					// 核心大小(Kernel Size) 必須為奇數
					var k      = pp.KernelSize % 2 == 1 ? pp.KernelSize : pp.KernelSize + 1;

					// 2. 執行不同類型的模糊算法
					switch(pp.Type)
					{
						case BlurParameters.BlurType.Gaussian:
							// 高斯模糊：用於抑制高頻雜訊，SigmaX 為 0 代表自動依 KernelSize 計算
							Cv2.GaussianBlur(img, result, new Size(k, k), pp.Sigma);
							break;
						case BlurParameters.BlurType.Median:
							// 中值模糊：對於椒鹽雜訊 (Salt-and-pepper noise) 有極佳處理效果
							Cv2.MedianBlur(img, result, k);
							break;
						case BlurParameters.BlurType.Box:
							// 平均模糊：最基礎的均值濾波
							Cv2.Blur(img, result, new Size(k, k));
							break;
						case BlurParameters.BlurType.Bilateral:
							// 雙邊濾波：能保邊 (Preserve Edge) 並去噪，處理速度較慢
							double sig = pp.Sigma > 0 ? pp.Sigma : 75;

							// 雙邊濾波僅支援 1 或 3 通道
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
				Category          = "01. 處理",
				DefaultParameters = new ThresholdParameters(),
				Action = (img, p) =>
				{
					// 1. 初始化灰階圖 (二值化必須在單通道影像上執行)
					var pp     = (ThresholdParameters)p;
					var result = new Mat();
					var gray   = new Mat();

					if(img.Channels() >= 3) Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
					else img.CopyTo(gray);

					// 2. 執行二值化運算
					if(pp.Method == ThresholdParameters.ThreshMethod.Adaptive)
					{
						// 自適應二值化：根據局部區域亮度決定閾值，適合光照不均影像
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
							// 大津法 (Otsu)：自動尋找最佳全局閾值，適合背景與目標對比明顯的影像
							type = ThresholdTypes.Otsu | ThresholdTypes.Binary;
						}
						else if(pp.Method == ThresholdParameters.ThreshMethod.ToZero)
						{
							type = ThresholdTypes.Tozero;
						}
						
						// 執行固定閾值的二值化
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
					// 1. 取得結構元素 (Structuring Element)
					var pp     = (MorphologyParameters)p;
					var result = new Mat();

					using(var kernel = Cv2.GetStructuringElement(pp.Shape, new Size(pp.KernelSize, pp.KernelSize)))
					{
						MorphTypes op = MorphTypes.Open;

						// 2. 選擇形態學運算類型
						switch(pp.Operation)
						{
							case MorphologyParameters.MorphOp.Erode:
								// 侵蝕：收縮物件範圍，消除細小孤立點
								op = MorphTypes.Erode;
								break;
							case MorphologyParameters.MorphOp.Dilate:
								// 膨脹：擴張物件範圍，修補微小斷裂
								op = MorphTypes.Dilate;
								break;
							case MorphologyParameters.MorphOp.Open:
								// 開運算：先侵蝕後膨脹，用於平滑外輪廓並斷開窄橋
								op = MorphTypes.Open;
								break;
							case MorphologyParameters.MorphOp.Close:
								// 閉運算：先膨脹後侵蝕，用於填充小孔洞或橋接細斷裂
								op = MorphTypes.Close;
								break;
							case MorphologyParameters.MorphOp.Gradient:
								// 梯度：膨脹圖減侵蝕圖，提取邊界
								op = MorphTypes.Gradient;
								break;
							case MorphologyParameters.MorphOp.TopHat:
								// 頂帽：原圖減開運算結果，提取亮部特徵
								op = MorphTypes.TopHat;
								break;
							case MorphologyParameters.MorphOp.BlackHat:
								// 黑帽：閉運算結果減原圖，提取暗部特徵
								op = MorphTypes.BlackHat;
								break;
						}
						
						// 3. 執行形態學計算
						Cv2.MorphologyEx(img, result, op, kernel, iterations: pp.Iterations);
					}
					return (true, result, new List<Defect>());
				},
			});
		}
	}
}