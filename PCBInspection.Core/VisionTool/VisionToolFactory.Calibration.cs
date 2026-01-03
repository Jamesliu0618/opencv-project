using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCBInspection.Core.Services
{
	public static partial class VisionToolFactory
	{
		/// <summary>
		/// 註冊校正與品質評估類別工具。
		/// 包含：相機標定、影像畸變矯正、四點透視變換與影像品質評估。
		/// </summary>
		/// <param name="tools">欲加入工具定義的清單物件</param>
		private static void AddCalibrationTools(List<ToolDefinition> tools)
		{
			// =========================================================
			// 08. 相機校正 (Camera Calibration)
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "相機校正 (Camera Calibration)",
				Category          = "08. 校正",
				DefaultParameters = new CameraCalibrationParameters(),
				Action = (img, p) =>
				{
					var pp     = (CameraCalibrationParameters)p;
					var result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}

					if(string.IsNullOrEmpty(pp.CalibrationImagesFolder) || !Directory.Exists(pp.CalibrationImagesFolder))
					{
						Cv2.PutText(result, "Please set CalibrationImagesFolder", new Point(10, 30), HersheyFonts.HersheySimplex, 0.6, Scalar.Yellow, 2);
						Cv2.PutText(result, "with chessboard images folder path", new Point(10, 55), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow);
						return (true, result, new List<Defect>());
					}
					string[] imageFiles = Directory.GetFiles(pp.CalibrationImagesFolder, "*.jpg").Concat(Directory.GetFiles(pp.CalibrationImagesFolder, "*.png")).Concat(Directory.GetFiles(pp.CalibrationImagesFolder, "*.bmp")).ToArray();

					if(imageFiles.Length < 3)
					{
						throw new InvalidOperationException("至少需要 3 張校正影像。");
					}
					Size                patternSize = new Size(pp.PatternWidth, pp.PatternHeight);
					List<List<Point3f>> objPoints   = new List<List<Point3f>>();
					List<List<Point2f>> imgPoints   = new List<List<Point2f>>();
					Size                imageSize   = new Size();

					// 建立 3D 物件點
					List<Point3f> objp = new List<Point3f>();

					for(int y = 0; y < pp.PatternHeight; y++)
					{
						for(int x = 0; x < pp.PatternWidth; x++)
						{
							objp.Add(new Point3f(x * pp.SquareSize, y * pp.SquareSize, 0));
						}
					}
					int validCount = 0;

					foreach(string file in imageFiles)
					{
						using(Mat calibImg = Cv2.ImRead(file, ImreadModes.Grayscale))
						{
							if(calibImg.Empty())
							{
								continue;
							}
							imageSize = calibImg.Size();

							if(Cv2.FindChessboardCorners(calibImg, patternSize, out Point2f[] corners))
							{
								Cv2.CornerSubPix(calibImg, corners, new Size(11, 11), new Size(-1, -1), new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 0.001));
								objPoints.Add(objp);
								imgPoints.Add(corners.ToList());
								validCount++;
							}
						}
					}

					if(validCount < 3)
					{
						throw new InvalidOperationException($"僅找到 {validCount} 張有效的棋盤格影像，至少需要 3 張。");
					}

					// 校正 - 將點座標轉換為 Mat 陣列
					Mat cameraMatrix = new Mat();
					Mat distCoeffs   = new Mat();

					// 轉換為 IEnumerable<Mat> 格式
					List<Mat> objPointsMats = new List<Mat>();
					List<Mat> imgPointsMats = new List<Mat>();

					foreach(List<Point3f> pts in objPoints)
					{
						Mat mat = new Mat(pts.Count, 1, MatType.CV_32FC3);

						for(int i = 0; i < pts.Count; i++)
						{
							mat.Set(i, 0, new Vec3f(pts[i].X, pts[i].Y, pts[i].Z));
						}
						objPointsMats.Add(mat);
					}

					foreach(List<Point2f> pts in imgPoints)
					{
						Mat mat = new Mat(pts.Count, 1, MatType.CV_32FC2);

						for(int i = 0; i < pts.Count; i++)
						{
							mat.Set(i, 0, new Vec2f(pts[i].X, pts[i].Y));
						}
						imgPointsMats.Add(mat);
					}
					double rms = Cv2.CalibrateCamera(objPointsMats, imgPointsMats, imageSize, cameraMatrix, distCoeffs, out Mat[] rvecsOut, out Mat[] tvecsOut);

					// 釋放暫時 Mat
					foreach(Mat m in objPointsMats)
					{
						m.Dispose();
					}

					foreach(Mat m in imgPointsMats)
					{
						m.Dispose();
					}

					// 儲存結果
					using(FileStorage fs = new FileStorage(pp.OutputCameraMatrixPath, FileStorage.Modes.Write))
					{
						fs.Write("camera_matrix", cameraMatrix);
					}

					using(FileStorage fs = new FileStorage(pp.OutputDistCoeffsPath, FileStorage.Modes.Write))
					{
						fs.Write("dist_coeffs", distCoeffs);
					}

					if(pp.ShowReprojectionError)
					{
						OnLog?.Invoke($"[校正] 重投影誤差 RMS: {rms:F4} ({validCount} 張影像)", false);
					}
					Cv2.PutText(result, $"Calibration RMS: {rms:F4}", new Point(10, 80), HersheyFonts.HersheySimplex, 0.8, Scalar.Green, 2);
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "畸變矯正 (Undistort)",
				Category          = "08. 校正",
				DefaultParameters = new UndistortParameters(),
				Action = (img, p) =>
				{
					UndistortParameters pp     = (UndistortParameters)p;
					Mat                 result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}

					if(!File.Exists(pp.CameraMatrixPath) || !File.Exists(pp.DistCoeffsPath))
					{
						Cv2.PutText(result, "Camera matrix or dist coeffs file not found", new Point(10, 30), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow);
						Cv2.PutText(result, "Please run Camera Calibration first",         new Point(10, 55), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow);
						return (true, result, new List<Defect>());
					}
					Mat cameraMatrix, distCoeffs;

					using(FileStorage fs = new FileStorage(pp.CameraMatrixPath, FileStorage.Modes.Read))
					{
						cameraMatrix = fs["camera_matrix"].ReadMat();
					}

					using(FileStorage fs = new FileStorage(pp.DistCoeffsPath, FileStorage.Modes.Read))
					{
						distCoeffs = fs["dist_coeffs"].ReadMat();
					}
					result.Dispose();
					result = new Mat();

					if(pp.AutoCropBlackBorder)
					{
						Mat newCameraMatrix = Cv2.GetOptimalNewCameraMatrix(cameraMatrix, distCoeffs, img.Size(), 0, img.Size(), out Rect roi);
						Cv2.Undistort(img, result, cameraMatrix, distCoeffs, newCameraMatrix);

						if(roi.Width > 0 && roi.Height > 0)
						{
							result = new Mat(result, roi).Clone();
						}
					}
					else
					{
						Cv2.Undistort(img, result, cameraMatrix, distCoeffs);
					}
					cameraMatrix.Dispose();
					distCoeffs.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			tools.Add(new ToolDefinition
			{
				Name              = "透視變換 (Perspective Transform)",
				Category          = "08. 校正",
				DefaultParameters = new PerspectiveTransformParameters(),
				Action = (img, p) =>
				{
					PerspectiveTransformParameters pp = (PerspectiveTransformParameters)p;

					Point2f[] srcPoints =
					{
						new Point2f(pp.SrcTopLeftX,     pp.SrcTopLeftY),
						new Point2f(pp.SrcTopRightX,    pp.SrcTopRightY),
						new Point2f(pp.SrcBottomRightX, pp.SrcBottomRightY),
						new Point2f(pp.SrcBottomLeftX,  pp.SrcBottomLeftY),
					};

					Point2f[] dstPoints =
					{
						new Point2f(0,                  0),
						new Point2f(pp.OutputWidth - 1, 0),
						new Point2f(pp.OutputWidth - 1, pp.OutputHeight - 1),
						new Point2f(0,                  pp.OutputHeight - 1),
					};
					Mat                M      = Cv2.GetPerspectiveTransform(srcPoints, dstPoints);
					Mat                result = new Mat();
					InterpolationFlags interp = InterpolationFlags.Linear;

					switch(pp.Interpolation)
					{
						case PerspectiveTransformParameters.InterpolationType.Nearest:
							interp = InterpolationFlags.Nearest;
							break;
						case PerspectiveTransformParameters.InterpolationType.Cubic:
							interp = InterpolationFlags.Cubic;
							break;
						case PerspectiveTransformParameters.InterpolationType.Lanczos4:
							interp = InterpolationFlags.Lanczos4;
							break;
					}
					Cv2.WarpPerspective(img, result, M, new Size(pp.OutputWidth, pp.OutputHeight), interp);
					M.Dispose();
					return (true, result, new List<Defect>());
				},
			});

			// =========================================================
			// 10. 影像品質評估 (Quality Assessment) - Move here as per plan
			// =========================================================

			tools.Add(new ToolDefinition
			{
				Name              = "品質評估 (Quality Assessment)",
				Category          = "10. 品質評估",
				DefaultParameters = new QualityAssessmentParameters(),
				Action = (img, p) =>
				{
					QualityAssessmentParameters pp     = (QualityAssessmentParameters)p;
					Mat                         result = img.Clone();

					if(result.Channels() == 1)
					{
						Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
					}

					if(string.IsNullOrEmpty(pp.ReferenceImagePath) || !File.Exists(pp.ReferenceImagePath))
					{
						Cv2.PutText(result, "Please set ReferenceImagePath", new Point(10, 30), HersheyFonts.HersheySimplex, 0.6, Scalar.Yellow, 2);
						Cv2.PutText(result, "to compare image quality",      new Point(10, 55), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow);
						return (true, result, new List<Defect>());
					}

					using(Mat refImg = Cv2.ImRead(pp.ReferenceImagePath))
					{
						if(refImg.Empty())
						{
							throw new InvalidOperationException("無法載入參考影像。");
						}

						// 確保尺寸相同
						Mat  imgResized  = img;
						bool needDispose = false;

						if(img.Width != refImg.Width || img.Height != refImg.Height)
						{
							imgResized = new Mat();
							Cv2.Resize(img, imgResized, refImg.Size());
							needDispose = true;
						}
						double score      = 0;
						string metricName = "";
						result.Dispose();
						result = imgResized.Clone();

						if(result.Channels() == 1)
						{
							Cv2.CvtColor(result, result, ColorConversionCodes.GRAY2BGR);
						}

						switch(pp.Metric)
						{
							case QualityAssessmentParameters.QualityMetric.PSNR:
								score      = Cv2.PSNR(imgResized, refImg);
								metricName = "PSNR";
								break;
							case QualityAssessmentParameters.QualityMetric.MSE:
								using(Mat diff = new Mat())
								{
									Cv2.Absdiff(imgResized, refImg, diff);
									diff.ConvertTo(diff, MatType.CV_32F);
									Cv2.Multiply(diff, diff, diff);
									score = Cv2.Mean(diff).Val0;
								}
								metricName = "MSE";
								break;
							case QualityAssessmentParameters.QualityMetric.SSIM:
								// 簡化版 SSIM 計算
								using(Mat gray1 = new Mat())
								{
									using(Mat gray2 = new Mat())
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
										score = Cv2.PSNR(gray1, gray2) / 50.0; // 近似 SSIM (簡化)

										if(score > 1)
										{
											score = 1;
										}
									}
								}
								metricName = "SSIM (approx)";
								break;
						}

						if(pp.ShowScore)
						{
							Cv2.PutText(result, $"{metricName}: {score:F4}", new Point(10, 30), HersheyFonts.HersheySimplex, 0.8, Scalar.Green, 2);
						}

						if(pp.OutputReport)
						{
							OnLog?.Invoke($"[品質評估] {metricName}: {score:F4}", false);
						}

						if(needDispose)
						{
							imgResized.Dispose();
						}
						return (true, result, new List<Defect>());
					}
				},
			});
		}
	}
}