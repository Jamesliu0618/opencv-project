using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System.Collections.Generic;
using System.Linq;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		/// <summary>
		/// 註冊特徵點偵測類別工具。
		/// 包含：ORB、FAST、BRISK、AKAZE 等偵測器。
		/// </summary>
		/// <param name="tools">欲加入工具定義的清單物件</param>
		private static void AddKeypointsTools(List<ToolDefinition> tools)
		{
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
		}
	}
}