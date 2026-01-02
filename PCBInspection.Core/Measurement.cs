using System;
using System.Collections.Generic;
using System.Linq;

namespace PCBInspection.Core
{
    public static class Measurement
    {
        public static (double wMm, double hMm) ComponentSizeMm(Component c)
        {
            var (cx, cy) = Calibration.PixelToMm(c.CenterX_Px, c.CenterY_Px);
            // Use sizes in pixels and convert widths/heights to mm
            var wMm = c.SizeW_Px / GetPixelsPerMm();
            var hMm = c.SizeH_Px / GetPixelsPerMm();
            return (wMm, hMm);
        }

        public static double DistanceBetweenCentersMm(Component a, Component b)
        {
            var (ax, ay) = Calibration.PixelToMm(a.CenterX_Px, a.CenterY_Px);
            var (bx, by) = Calibration.PixelToMm(b.CenterX_Px, b.CenterY_Px);
            var dx = ax - bx; var dy = ay - by;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double GapBetweenComponentsAlongXmm(Component left, Component right)
        {
            // compute right edge of left and left edge of right in pixels
            var leftRightEdgePx = left.CenterX_Px + left.SizeW_Px / 2.0;
            var rightLeftEdgePx = right.CenterX_Px - right.SizeW_Px / 2.0;
            var gapPx = rightLeftEdgePx - leftRightEdgePx;
            if (gapPx < 0) return 0.0; // overlapping
            return gapPx / GetPixelsPerMm();
        }

        private static double GetPixelsPerMm()
        {
            // Access underlying value via small helper by measuring conversion of 1 pixel
            // PixelToMm(1,0).xMm = 1 / pixelsPerMm (approx)
            var (x1, _) = Calibration.PixelToMm(1, 0);
            var (x0, _) = Calibration.PixelToMm(0, 0);
            var mmPerPixel = x1 - x0; // mm per pixel
            if (mmPerPixel <= 0) throw new InvalidOperationException("Invalid calibration scale");
            return 1.0 / mmPerPixel;
        }
    }
}