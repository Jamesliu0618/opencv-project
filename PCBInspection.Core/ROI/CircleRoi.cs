using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace PCBInspection.Core.ROI
{
	/// <summary>
	/// 圓形 ROI
	/// </summary>
	public class CircleRoi : RoiBase
	{
		public CircleRoi(PointF center, float radius)
		{
			Center = center;
			Radius = radius;
			Name   = "Circle";
		}

		public PointF Center      { get; set; }
		public float  Radius      { get; set; }
		public PointF StartCorner { get; set; } // 用於Bounding-box模式繪製

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			var   screenCenter = ImageToScreen(Center, scale, offsetX, offsetY);
			float screenRadius = Radius * scale;

			using(var pen = new Pen(IsSelected ? Color.Yellow : Color, 2))
			{
				pen.DashStyle = IsSelected ? DashStyle.Dash : DashStyle.Solid;
				g.DrawEllipse(pen, screenCenter.X - screenRadius, screenCenter.Y - screenRadius, screenRadius * 2, screenRadius * 2);
			}

			if(IsSelected)
			{
				// 在圓周右側繪製一個控制點
				DrawHandle(g, ImageToScreen(new PointF(Center.X + Radius, Center.Y), scale, offsetX, offsetY));
			}
		}

		public override bool HitTest(Point mousePt, float scale, float offsetX, float offsetY)
		{
			var imgPt = ScreenToImage(mousePt, scale, offsetX, offsetY);
			float dx = imgPt.X - Center.X;
			float dy = imgPt.Y - Center.Y;
			// 圓形碰撞檢查公式: 點到圓心距離之平方 <= 半徑之平方
			return (dx * dx + dy * dy) <= (Radius * Radius);
		}

		public override RoiHandle GetHandle(Point mousePt, float scale, float offsetX, float offsetY)
		{
			if(!IsSelected) return base.GetHandle(mousePt, scale, offsetX, offsetY);

			var rightEdge = ImageToScreen(new PointF(Center.X + Radius, Center.Y), scale, offsetX, offsetY);
			if(IsPointNear(mousePt, rightEdge)) return RoiHandle.Right;

			return HitTest(mousePt, scale, offsetX, offsetY) ? RoiHandle.Body : RoiHandle.None;
		}

		public override void Resize(RoiHandle handle, PointF newImgPt)
		{
			if(handle == RoiHandle.Right)
			{
				float dx = newImgPt.X - Center.X;
				float dy = newImgPt.Y - Center.Y;
				// 根據滑鼠位置重新計算半徑
				Radius = (float)Math.Sqrt(dx * dx + dy * dy);
			}
		}

		public override void Move(int dx, int dy)
		{
			Center = new PointF(Center.X + dx, Center.Y + dy);
		}

		public override Mat GetMask(Size imageSize)
		{
			var mask = new Mat(imageSize, MatType.CV_8UC1, Scalar.All(0));
			if(Radius > 0)
			{
				Cv2.Circle(mask, (int)Center.X, (int)Center.Y, (int)Radius, Scalar.All(255), -1);
			}
			return mask;
		}
	}
}
