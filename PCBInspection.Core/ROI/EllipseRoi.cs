using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace PCBInspection.Core.ROI
{
	/// <summary>橢圓形 ROI，支援中心點、長短軸半徑與旋轉角度</summary>
	public class EllipseRoi : RoiBase
	{
		public EllipseRoi(PointF center, float radiusX, float radiusY, float rotation = 0)
		{
			Center   = center;
			RadiusX  = radiusX;
			RadiusY  = radiusY;
			Rotation = rotation;
			Name     = "Ellipse";
		}

		/// <summary>橢圓中心點</summary>
		public PointF Center { get; set; }

		/// <summary>X 軸半徑</summary>
		public float RadiusX { get; set; }

		/// <summary>Y 軸半徑</summary>
		public float RadiusY { get; set; }

		/// <summary>旋轉角度 (單位: 度)</summary>
		public float Rotation { get; set; }

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			var   screenCenter  = ImageToScreen(Center, scale, offsetX, offsetY);
			float screenRadiusX = RadiusX * scale;
			float screenRadiusY = RadiusY * scale;

			using (var pen = new Pen(IsSelected ? Color.Yellow : Color, 2))
			{
				pen.DashStyle = IsSelected ? DashStyle.Dash : DashStyle.Solid;

				// 1. 保存當前畫布狀態 (用於旋轉操作)
				var state = g.Save();

				// 2. 執行坐標變換：位移至中心點並依 Rotation 旋轉
				g.TranslateTransform(screenCenter.X, screenCenter.Y);
				g.RotateTransform(Rotation);

				// 3. 繪製橢圓 (中心在 0,0，半徑向四周發散)
				g.DrawEllipse(pen, -screenRadiusX, -screenRadiusY, screenRadiusX * 2, screenRadiusY * 2);

				// 4. 恢復畫布狀態
				g.Restore(state);
			}
		}

		public override bool HitTest(Point mousePt, float scale, float offsetX, float offsetY)
		{
			var imgPt = ScreenToImage(mousePt, scale, offsetX, offsetY);

			// 1. 將點位移至以中心點為原點的座標系
			double dx = imgPt.X - Center.X;
			double dy = imgPt.Y - Center.Y;

			// 2. 將點逆向旋轉回水平橢圓的參考系
			// 旋轉公式:
			// x' = x * cos(theta) - y * sin(theta)
			// y' = x * sin(theta) + y * cos(theta)
			// 由於是逆向旋轉，所以角度取負值
			double radians = -Rotation * Math.PI / 180.0;
			double rotX    = dx * Math.Cos(radians) - dy * Math.Sin(radians);
			double rotY    = dx * Math.Sin(radians) + dy * Math.Cos(radians);

			// 3. 判定是否符合橢圓方程式: (x/a)² + (y/b)² <= 1
			// 其中 a 為 RadiusX，b 為 RadiusY
			double normalizedX = rotX / RadiusX;
			double normalizedY = rotY / RadiusY;

			return (normalizedX * normalizedX + normalizedY * normalizedY) <= 1.0;
		}

		public override void Move(int dx, int dy)
		{
			Center = new PointF(Center.X + dx, Center.Y + dy);
		}

		public override Mat GetMask(Size imageSize)
		{
			var mask = new Mat(imageSize, MatType.CV_8UC1, Scalar.All(0));
			// 使用 OpenCV 的 Ellipse 方法繪製實心遮罩 (-1 代表填滿)
			Cv2.Ellipse(
				mask,
				new OpenCvSharp.Point((int)Center.X, (int)Center.Y),
				new Size((int)RadiusX, (int)RadiusY),
				Rotation,
				0, 360,
				Scalar.All(255),
				-1);
			return mask;
		}
	}
}
