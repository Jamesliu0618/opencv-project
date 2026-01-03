using OpenCvSharp;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Detectors
{
	/// <summary>
	///     焊點瑕疵偵測器 - 使用形態學與輪廓分析偵測冷焊、虛焊、漏焊、短路
	/// </summary>
	public class SolderDefectDetector : IDefectDetector
	{
		private readonly double _maxSolderArea;

		private readonly double _minSolderArea;
		private readonly int    _morphKernel;

		public SolderDefectDetector(double minArea = 50, double maxArea = 5000, int morphKernel = 3)
		{
			_minSolderArea = minArea;
			_maxSolderArea = maxArea;
			_morphKernel   = morphKernel;
		}

		public string Name { get => "SolderDefectDetector"; }

		/// <summary>
		///     執行焊點瑕疵偵測演算法。
		///     邏輯：自適應二值化 -> 形態學開運算 -> 輪廓分析 (面積、圓度、長寬比)。
		/// </summary>
		public List<Defect> Detect(Mat image, List<Component> components = null)
		{
			var defects = new List<Defect>();

			if(image == null || image.Empty())
			{
				return defects;
			}

			using(var gray = new Mat())
			{
				using(var thresh = new Mat())
				{
					// 1. 預處理：轉灰階
					if(image.Channels() == 3)
					{
						Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
					}
					else
					{
						image.CopyTo(gray);
					}

					// 2. 自適應二值化：偵測焊點區域，BinaryInv 反轉使前景為白色
					Cv2.AdaptiveThreshold(gray, thresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 5);

					// 3. 形態學開運算 (Opening)：先腐蝕後膨脹，去除細小噪點並平滑輪廓
					using(var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(_morphKernel, _morphKernel)))
					{
						Cv2.MorphologyEx(thresh, thresh, MorphTypes.Open, kernel);
					}

					// 4. 提取外部輪廓
					Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
					int idx = 0;

					foreach(var contour in contours)
					{
						var area = Cv2.ContourArea(contour);

						// 過濾過小或過大的非焊點區域
						if(area < _minSolderArea || area > _maxSolderArea)
						{
							continue;
						}
						var rect        = Cv2.BoundingRect(contour);
						var circularity = CalculateCircularity(contour, area); // 計算圓度 (判定焊點飽滿度)

						// 5. 根據幾何特徵 (面積、圓度、長寬比) 進行瑕疵分類
						var defectType = AnalyzeSolderDefect(area, circularity, rect);

						if(defectType != null)
						{
							defects.Add(new Defect
							{
								Id          = $"solder_{idx++}",
								Type        = defectType.Value.Item1,
								Severity    = defectType.Value.Item2,
								Confidence  = circularity,
								BoundingBox = new[] { rect.X, rect.Y, rect.Width, rect.Height },
							});
						}
					}
				}
			}
			return defects;
		}

		private double CalculateCircularity(Point[] contour, double area)
		{
			double perimeter = Cv2.ArcLength(contour, true);

			if(perimeter == 0)
			{
				return 0;
			}
			return 4 * Math.PI * area / (perimeter * perimeter);
		}

		private (string, int)? AnalyzeSolderDefect(double area, double circularity, Rect rect)
		{
			// 高圓形度 (>0.7) 可能是正常焊點，跳過
			if(circularity > 0.7)
			{
				return null;
			}

			// 低圓形度 + 大面積 => 可能短路 (severity 4-5)
			if(circularity < 0.3 && area > 1000)
			{
				return ("SHORT_CIRCUIT", 5);
			}

			// 長條形 (aspect ratio > 3) => 可能橋接
			double aspectRatio = (double)rect.Width / Math.Max(rect.Height, 1);

			if(aspectRatio > 3 || aspectRatio < 0.33)
			{
				return ("BRIDGE", 4);
			}

			// 小面積 + 低圓形度 => 冷焊/虛焊
			if(area < 200 && circularity < 0.5)
			{
				return ("COLD_SOLDER", 3);
			}

			// 中等異常
			if(circularity < 0.5)
			{
				return ("SOLDER_ANOMALY", 2);
			}
			return null;
		}
	}
}