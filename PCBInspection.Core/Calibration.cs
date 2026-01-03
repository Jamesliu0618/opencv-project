using OpenCvSharp;
using System;

namespace PCBInspection.Core
{
	/// <summary>
	///     相機校正 (Calibration) 靜態類別。
	///     負責計算像素與實體毫米 (mm) 之間的轉換關係，包含棋盤格標定演算法。
	/// </summary>
	public static class Calibration
	{
		// 儲存校正參數：每毫米像素數 (Pixels Per Mm) 與 旋轉角度
		private static          double _pixelsPerMm = 1.0; // default 1px == 1mm (override with ComputeCalibration)
		private static readonly double _rotationDeg = 0.0;

		/// <summary>
		///     使用棋盤格影像計算校正參數。
		///     演算法：影像讀取 -> FindChessboardCorners -> 計算相鄰角點平均像素距離 -> 求出 Pixels/mm。
		/// </summary>
		/// <param name="imagePaths">校正用影像路徑列表</param>
		/// <param name="patternCols">棋盤格內角點欄數</param>
		/// <param name="patternRows">棋盤格內角點列數</param>
		/// <param name="squareSizeMm">每個棋盤格的實際毫米大小</param>
		public static void ComputeCalibration(string[] imagePaths, int patternCols, int patternRows, double squareSizeMm)
		{
			// Attempt to find chessboard corners in first usable image and compute pixels-per-mm
			foreach(string img in imagePaths)
			{
				try
				{
					using(Mat m = Cv2.ImRead(img, ImreadModes.Grayscale))
					{
						if(m == null || m.Empty())
						{
							continue;
						}
						Size patternSize = new Size(patternCols, patternRows);
						// 1. 偵測棋盤格角點
						bool found       = Cv2.FindChessboardCorners(m, patternSize, out Point2f[] corners);

						if(found && corners != null && corners.Length >= 2)
						{
							// 2. 計算相鄰角點間的平均像素距離 (X/Y 方向)
							double total = 0;
							int    count = 0;

							for(int y = 0; y < patternRows; y++)
							{
								for(int x = 0; x < patternCols - 1; x++)
								{
									int    idx1 = y * patternCols + x;
									int    idx2 = idx1            + 1;
									float  dx   = corners[idx1].X - corners[idx2].X;
									float  dy   = corners[idx1].Y - corners[idx2].Y;
									double d    = Math.Sqrt(dx * dx + dy * dy);
									total += d;
									count++;
								}
							}

							if(count > 0)
							{
								// 3. 更新全域校正係數
								double avgPixelsPerSquare = total / count; // pixels per square
								_pixelsPerMm = avgPixelsPerSquare / squareSizeMm;
								return;
							}
						}
					}
				}
				catch
				{
					// 若影像讀取或偵測失敗，嘗試下一張
				}
			}
			throw new InvalidOperationException("校正失敗：在提供的影像中找不到棋盤格角點。");
		}

		/// <summary>
		///     取得「像素轉毫米」的仿射變換矩陣 (3x3)。
		/// </summary>
		public static double[,] GetPixelToMmMatrix()
		{
			// Simple uniform scale matrix with optional rotation
			double rad = _rotationDeg * Math.PI / 180.0;
			double s   = 1.0                    / _pixelsPerMm; // mm per pixel
			double cos = Math.Cos(rad);
			double sin = Math.Sin(rad);

			return new double[3, 3]
			{
				{ s * cos, -s * sin, 0 },
				{ s * sin, s  * cos, 0 },
				{ 0, 0, 1 },
			};
		}

		/// <summary>向前相容別名</summary>
		public static double[,] GetPixelToMm() => GetPixelToMmMatrix();

		/// <summary>
		///     將影像像素座標轉換為物理世界毫米座標。
		/// </summary>
		public static (double xMm, double yMm) PixelToMm(double xPx, double yPx)
		{
			double[,] M = GetPixelToMmMatrix();
			double    x = M[0, 0] * xPx + M[0, 1] * yPx + M[0, 2];
			double    y = M[1, 0] * xPx + M[1, 1] * yPx + M[1, 2];
			return (x, y);
		}

		/// <summary>手動設定校正係數 (用於已知解析度的情況)</summary>
		public static void SetManualScale(double pixelsPerMm)
		{
			_pixelsPerMm = pixelsPerMm;
		}

		// expose scale and rotation for persistence / UI
		public static double GetPixelsPerMm() => _pixelsPerMm;
		public static double GetRotationDeg() => _rotationDeg;
	}
}