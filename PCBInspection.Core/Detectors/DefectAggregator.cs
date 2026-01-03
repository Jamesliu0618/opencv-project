using OpenCvSharp;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PCBInspection.Core.Detectors
{
    /// <summary>
    ///     瑕疵聚合器 - 整合多個偵測器結果並計算嚴重度分級
    /// </summary>
    public class DefectAggregator
	{
		private readonly List<IDefectDetector> _detectors;

		public DefectAggregator()
		{
			_detectors = new List<IDefectDetector>
			{
				new SolderDefectDetector(),
				new SurfaceDefectDetector(),
				new CircuitDefectDetector(),
			};
		}

		public DefectAggregator(IEnumerable<IDefectDetector> detectors)
		{
			_detectors = detectors?.ToList() ?? new List<IDefectDetector>();
		}

        /// <summary>
        ///     執行所有偵測器並聚合結果
        /// </summary>
        public AggregatedResult Detect(Mat image, List<Component> components = null)
		{
			var allDefects = new List<Defect>();

			foreach(var detector in _detectors)
			{
				try
				{
					var detected = detector.Detect(image, components);
					allDefects.AddRange(detected);
				}
				catch(Exception ex)
				{
					// 記錄錯誤但繼續執行其他偵測器
					Debug.WriteLine($"Detector {detector.Name} failed: {ex.Message}");
				}
			}

			// 過濾重疊瑕疵（NMS）
			var filtered = NonMaximumSuppression(allDefects, 0.5);

			// 計算整體判定
			var maxSeverity = filtered.Count > 0 ? filtered.Max(d => d.Severity) : 0;
			var decision    = DetermineDecision(maxSeverity);

			return new AggregatedResult
			{
				Defects     = filtered,
				MaxSeverity = maxSeverity,
				Decision    = decision,
				IsOk        = decision == InspectionDecision.OK,
			};
		}

		private List<Defect> NonMaximumSuppression(List<Defect> defects, double iouThreshold)
		{
			if(defects.Count <= 1)
			{
				return defects;
			}
			var sorted     = defects.OrderByDescending(d => d.Severity).ThenByDescending(d => d.Confidence).ToList();
			var result     = new List<Defect>();
			var suppressed = new bool[sorted.Count];

			for(int i = 0; i < sorted.Count; i++)
			{
				if(suppressed[i])
				{
					continue;
				}
				result.Add(sorted[i]);

				for(int j = i + 1; j < sorted.Count; j++)
				{
					if(suppressed[j])
					{
						continue;
					}

					if(CalculateIoU(sorted[i].BoundingBox, sorted[j].BoundingBox) > iouThreshold)
					{
						suppressed[j] = true;
					}
				}
			}
			return result;
		}

		private double CalculateIoU(int[] box1, int[] box2)
		{
			if(box1 == null || box2 == null || box1.Length < 4 || box2.Length < 4)
			{
				return 0;
			}
			var x1           = Math.Max(box1[0], box2[0]);
			var y1           = Math.Max(box1[1], box2[1]);
			var x2           = Math.Min(box1[0] + box1[2], box2[0] + box2[2]);
			var y2           = Math.Min(box1[1] + box1[3], box2[1] + box2[3]);
			var intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
			var area1        = box1[2]              * box1[3];
			var area2        = box2[2]              * box2[3];
			var union        = area1 + area2 - intersection;
			return union > 0 ? (double)intersection / union : 0;
		}

		private InspectionDecision DetermineDecision(int maxSeverity)
		{
			// 根據 spec 定義的嚴重度策略
			// 1-2: 記錄並繼續生產
			// 3: 人工覆核
			// 4-5: 自動 NG
			if(maxSeverity >= 4)
			{
				return InspectionDecision.NG;
			}

			if(maxSeverity == 3)
			{
				return InspectionDecision.REVIEW;
			}
			return InspectionDecision.OK;
		}
	}

    /// <summary>
    ///     聚合結果
    /// </summary>
    public class AggregatedResult
	{
		public List<Defect>       Defects     { get; set; } = new List<Defect>();
		public int                MaxSeverity { get; set; }
		public InspectionDecision Decision    { get; set; }
		public bool               IsOk        { get; set; }
	}

    /// <summary>
    ///     檢測判定結果
    /// </summary>
    public enum InspectionDecision
	{
		OK,
		REVIEW,
		NG,
	}
}