using OpenCvSharp;
using PCBInspection.Core.Models;
using System.Collections.Generic;

namespace PCBInspection.Core.Detectors
{
	/// <summary>
	///     電路異常偵測器 - 使用連通元件分析偵測斷路、橋接
	/// </summary>
	public class CircuitDefectDetector : IDefectDetector
	{
		private readonly int _maxGapWidth;

		private readonly int _minTraceWidth;

		public CircuitDefectDetector(int minTraceWidth = 3, int maxGapWidth = 5)
		{
			_minTraceWidth = minTraceWidth;
			_maxGapWidth   = maxGapWidth;
		}

		public string Name { get => "CircuitDefectDetector"; }

		public List<Defect> Detect(Mat image, List<Component> components = null)
		{
			var defects = new List<Defect>();

			if(image == null || image.Empty())
			{
				return defects;
			}

			using(var gray = new Mat())
			{
				using(var binary = new Mat())
				{
					using(var skeleton = new Mat())
					{
						if(image.Channels() == 3)
						{
							Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
						}
						else
						{
							image.CopyTo(gray);
						}

						// 二值化提取電路線路
						Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

						// 形態學細化提取骨架
						Cv2.MorphologyEx(binary, skeleton, MorphTypes.Gradient, Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)));

						// 使用連通元件分析
						using(var labels = new Mat())
						{
							using(var stats = new Mat())
							{
								using(var centroids = new Mat())
								{
									var nLabels = Cv2.ConnectedComponentsWithStats(skeleton, labels, stats, centroids);

									// 分析每個連通區域
									for(int i = 1; i < nLabels; i++) // 跳過背景 (label 0)
									{
										var x    = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
										var y    = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
										var w    = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
										var h    = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);
										var area = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

										// 小面積且細長 => 可能是斷路
										if(area < 50 && (w < _minTraceWidth || h < _minTraceWidth))
										{
											defects.Add(new Defect
											{
												Id          = $"circuit_{defects.Count}",
												Type        = "OPEN_CIRCUIT",
												Severity    = 4,
												Confidence  = 0.7,
												BoundingBox = new[] { x, y, w, h },
											});
										}

										// 連通區域過寬 => 可能是橋接
										if(w > _maxGapWidth * 3 && h > _maxGapWidth * 3 && area > 200)
										{
											defects.Add(new Defect
											{
												Id          = $"circuit_{defects.Count}",
												Type        = "BRIDGE",
												Severity    = 4,
												Confidence  = 0.6,
												BoundingBox = new[] { x, y, w, h },
											});
										}
									}
								}
							}
						}
					}
				}
			}
			return defects;
		}
	}
}