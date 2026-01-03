using OpenCvSharp;
using System;

namespace PCBInspection.Core
{
	public static class Calibration
	{
		// Simple calibration storage: pixel-to-mm scale and optional rotation/translation
		private static          double _pixelsPerMm = 1.0; // default 1px == 1mm (override with ComputeCalibration)
		private static readonly double _rotationDeg = 0.0;

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
						bool found       = Cv2.FindChessboardCorners(m, patternSize, out Point2f[] corners);

						if(found && corners != null && corners.Length >= 2)
						{
							// compute average distance in pixels between adjacent corners in X and Y
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
								double avgPixelsPerSquare = total / count; // pixels per square
								_pixelsPerMm = avgPixelsPerSquare / squareSizeMm;
								return;
							}
						}
					}
				}
				catch
				{
				}
			}
			throw new InvalidOperationException("Calibration failed: no chessboard corners found in provided images.");
		}

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

		// Backwards compatible alias for older tests / callers
		public static double[,] GetPixelToMm() => GetPixelToMmMatrix();

		public static (double xMm, double yMm) PixelToMm(double xPx, double yPx)
		{
			double[,] M = GetPixelToMmMatrix();
			double    x = M[0, 0] * xPx + M[0, 1] * yPx + M[0, 2];
			double    y = M[1, 0] * xPx + M[1, 1] * yPx + M[1, 2];
			return (x, y);
		}

		public static void SetManualScale(double pixelsPerMm)
		{
			_pixelsPerMm = pixelsPerMm;
		}

		// expose scale and rotation for persistence / UI
		public static double GetPixelsPerMm() => _pixelsPerMm;
		public static double GetRotationDeg() => _rotationDeg;
	}
}