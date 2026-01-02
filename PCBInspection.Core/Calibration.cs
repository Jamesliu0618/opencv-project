using System;

namespace PCBInspection.Core
{
    public static class Calibration
    {
        // Placeholder: compute pixel->mm transform
        public static void ComputeCalibration(string[] imagePaths, int patternCols, int patternRows, double squareSizeMm)
        {
            // TODO: implement calibration
            throw new NotImplementedException();
        }

        public static double[,] GetPixelToMm()
        {
            // TODO: return calibration matrix
            return new double[3,3];
        }
    }
}