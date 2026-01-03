using System;

namespace PCBInspection.Core
{
	/// <summary>
	///     空間量測 (Measurement) 靜態類別。
	///     提供組件尺寸、間距及中心距離等物理量測功能。
	/// </summary>
	public static class Measurement
	{
		/// <summary>
		///     計算組件的物理長寬 (mm)。
		/// </summary>
		public static (double wMm, double hMm) ComponentSizeMm(Component c)
		{
			// 1. 取得中心點物理座標 (測試用途)
			(double cx, double cy) = Calibration.PixelToMm(c.CenterX_Px, c.CenterY_Px);

			// 2. 將像素尺寸除以目前的 PixelsPerMm 比例
			double wMm = c.SizeW_Px / GetPixelsPerMm();
			double hMm = c.SizeH_Px / GetPixelsPerMm();
			return (wMm, hMm);
		}

		/// <summary>
		///     計算兩個組件中心點之間的物理距離 (mm)。
		/// </summary>
		public static double DistanceBetweenCentersMm(Component a, Component b)
		{
			(double ax, double ay) = Calibration.PixelToMm(a.CenterX_Px, a.CenterY_Px);
			(double bx, double by) = Calibration.PixelToMm(b.CenterX_Px, b.CenterY_Px);
			double dx = ax           - bx;
			double dy = ay           - by;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>
		///     計算兩個組件在 X 軸方向上的物理邊緣間距 (Gap, mm)。
		/// </summary>
		public static double GapBetweenComponentsAlongXmm(Component left, Component right)
		{
			// 計算左側組件的右邊緣像素位置 與 右側組件的左邊緣像素位置
			double leftRightEdgePx = left.CenterX_Px  + left.SizeW_Px  / 2.0;
			double rightLeftEdgePx = right.CenterX_Px - right.SizeW_Px / 2.0;
			double gapPx           = rightLeftEdgePx  - leftRightEdgePx;

			if(gapPx < 0)
			{
				return 0.0; // 表示兩組件發生重疊 (Overlapping)
			}
			return gapPx / GetPixelsPerMm();
		}

		/// <summary>
		///     輔助方法：從校正模組擷取目前的 PixelsPerMm 比例。
		///     透過模擬 1 像素的轉換來反向求得比例，確保與校正矩陣同步。
		/// </summary>
		private static double GetPixelsPerMm()
		{
			(double x1, _) = Calibration.PixelToMm(1, 0);
			(double x0, _) = Calibration.PixelToMm(0, 0);
			double mmPerPixel = x1 - x0; 

			if(mmPerPixel <= 0)
			{
				throw new InvalidOperationException("校正比例無效，請確保已完成相機校正。");
			}
			return 1.0 / mmPerPixel;
		}
	}
}