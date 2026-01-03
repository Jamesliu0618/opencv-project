using OpenCvSharp;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Detectors
{
	/// <summary>
	///     表面瑕疵偵測器 - 使用形態學操作偵測刮痕、污染、氧化
	/// </summary>
	public class SurfaceDefectDetector : IDefectDetector
	{
		private readonly int    _blurSize;
		private readonly int    _minDefectArea;
		private readonly double _thresholdValue;

		public SurfaceDefectDetector(int blurSize = 5, double threshold = 30, int minArea = 100)
		{
			_blurSize       = blurSize;
			_thresholdValue = threshold;
			_minDefectArea  = minArea;
		}

		public string Name { get => "SurfaceDefectDetector"; }

		/// <summary>
		///     執行表面瑕疵偵測演算法。
		///     邏輯：影像相減法 (背景相減近似值) -> 閾值分割 -> 輪廓分類。
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
				using(var blurred = new Mat())
				{
					using(var diff = new Mat())
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

							// 2. 中值濾波：生成「平滑背景」影像，消除細小雜訊
							Cv2.MedianBlur(gray, blurred, _blurSize);

							// 3. 背景相減：原始圖減去平滑圖，剩餘部分即為突發性的表面異常 (如刮痕、污染)
							Cv2.Subtract(gray, blurred, diff);
							Cv2.ConvertScaleAbs(diff, diff);

							// 4. 二值化分割與形態學閉運算 (Close)：連接斷裂的瑕疵塊
							Cv2.Threshold(diff, thresh, _thresholdValue, 255, ThresholdTypes.Binary);

							// 形態學閉運算連接相鄰瑕疵
							using(var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
							{
								Cv2.MorphologyEx(thresh, thresh, MorphTypes.Close, kernel);
							}

							// 5. 提取輪廓並分類
							Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
							int idx = 0;

							foreach(var contour in contours)
							{
								var area = Cv2.ContourArea(contour);

								if(area < _minDefectArea)
								{
									continue;
								}
								var rect       = Cv2.BoundingRect(contour);
								var defectInfo = ClassifySurfaceDefect(contour, area, rect);

								defects.Add(new Defect
								{
									Id          = $"surface_{idx++}",
									Type        = defectInfo.Item1,
									Severity    = defectInfo.Item2,
									Confidence  = 0.8,
									BoundingBox = new[] { rect.X, rect.Y, rect.Width, rect.Height },
								});
							}
						}
					}
				}
			}
			return defects;
		}

		private (string, int) ClassifySurfaceDefect(Point[] contour, double area, Rect rect)
		{
			double aspectRatio = (double)rect.Width / Math.Max(rect.Height, 1);

			// 長條形 (高 aspect ratio) => 刮痕
			if(aspectRatio > 5 || aspectRatio < 0.2)
			{
				return ("SCRATCH", 3);
			}

			// 大面積塊狀 => 污染
			if(area > 500)
			{
				return ("CONTAMINATION", 3);
			}

			// 小面積斑點 => 氧化/腐蝕
			return ("OXIDATION", 2);
		}
	}
}