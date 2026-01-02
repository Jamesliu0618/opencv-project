using System;
using OpenCvSharp;

namespace PCBInspection.Core
{
    public static class Calibration
    {
        // Simple calibration storage: pixel-to-mm scale and optional rotation/translation
        private static double _pixelsPerMm = 1.0; // default 1px == 1mm (override with ComputeCalibration)
        private static double _rotationDeg = 0.0;

        public static void ComputeCalibration(string[] imagePaths, int patternCols, int patternRows, double squareSizeMm)
        {
            // Attempt to find chessboard corners in first usable image and compute pixels-per-mm
            foreach (var img in imagePaths)
            {
                try
                {
                    using (var m = Cv2.ImRead(img, ImreadModes.Grayscale))
                    {
                        if (m == null || m.Empty()) continue;
                        var patternSize = new Size(patternCols, patternRows);
                        var found = Cv2.FindChessboardCorners(m, patternSize, out var corners, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);
                        if (found && corners != null && corners.Length >= 2)
                        {
                            // compute average distance in pixels between adjacent corners in X and Y
                            double total = 0; int count = 0;
                            for (int y = 0; y < patternRows; y++)
                            {
                                for (int x = 0; x < patternCols - 1; x++)
                                {
                                    var idx1 = y * patternCols + x;
                                    var idx2 = idx1 + 1;
                                    var dx = corners[idx1].X - corners[idx2].X;
                                    var dy = corners[idx1].Y - corners[idx2].Y;
                                    var d = Math.Sqrt(dx * dx + dy * dy);
                                    total += d; count++;
                                }
                            }
                            if (count > 0)
                            {
                                var avgPixelsPerSquare = total / count; // pixels per square
                                _pixelsPerMm = avgPixelsPerSquare / squareSizeMm;
                                return;
                            }
                        }
                    }
                }
                catch { continue; }
            }
            throw new InvalidOperationException("Calibration failed: no chessboard corners found in provided images.");
        }

        public static double[,] GetPixelToMmMatrix()
        {
            // Simple uniform scale matrix with optional rotation
            var rad = _rotationDeg * Math.PI / 180.0;
            var s = 1.0 / _pixelsPerMm; // mm per pixel
            var cos = Math.Cos(rad); var sin = Math.Sin(rad);
            return new double[3, 3]
            {
                { s * cos, -s * sin, 0 },
                { s * sin, s * cos, 0 },
                { 0, 0, 1 }
            };
        }

        // Backwards compatible alias for older tests / callers
        public static double[,] GetPixelToMm() => GetPixelToMmMatrix();

        public static (double xMm, double yMm) PixelToMm(double xPx, double yPx)
        {
            var M = GetPixelToMmMatrix();
            var x = M[0,0] * xPx + M[0,1] * yPx + M[0,2];
            var y = M[1,0] * xPx + M[1,1] * yPx + M[1,2];
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