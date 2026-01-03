using OpenCvSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace PCBInspection.Core.ROI
{
	/// <summary>
	/// 多邊形 ROI
	/// </summary>
	public class PolygonRoi : RoiBase
	{
		public PolygonRoi()
		{
			Name = "Poly";
		}

		/// <summary>
		/// 多邊形頂點清單
		/// </summary>
		public List<PointF> Points { get; set; } = new List<PointF>();

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			if(Points.Count < 2) return;

			var screenPoints = new List<PointF>();
			foreach(var p in Points)
			{
				screenPoints.Add(ImageToScreen(p, scale, offsetX, offsetY));
			}

			using(var pen = new Pen(IsSelected ? Color.Yellow : Color, 2))
			{
				pen.DashStyle = IsSelected ? DashStyle.Dash : DashStyle.Solid;
				// 頂點數大於 2 則繪製封閉多邊形，否則僅繪製線條
				if(Points.Count > 2) g.DrawPolygon(pen, screenPoints.ToArray());
				else g.DrawLines(pen, screenPoints.ToArray());
			}

			if(IsSelected)
			{
				foreach(var sp in screenPoints)
				{
					DrawHandle(g, new Point((int)sp.X, (int)sp.Y));
				}
			}
		}

		public override bool HitTest(Point mousePt, float scale, float offsetX, float offsetY)
		{
			var imgPt = ScreenToImage(mousePt, scale, offsetX, offsetY);
			
			// 使用射線演算法 (Ray Casting Algorithm) 判定點是否在多邊形內
			bool inside = false;
			for(int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
			{
				if(((Points[i].Y > imgPt.Y) != (Points[j].Y > imgPt.Y)) &&
				   (imgPt.X < (Points[j].X - Points[i].X) * (imgPt.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X))
				{
					inside = !inside;
				}
			}
			return inside;
		}

		public override void Move(int dx, int dy)
		{
			for(int i = 0; i < Points.Count; i++)
			{
				Points[i] = new PointF(Points[i].X + dx, Points[i].Y + dy);
			}
		}

		public override Mat GetMask(Size imageSize)
		{
			var mask = new Mat(imageSize, MatType.CV_8UC1, Scalar.All(0));

			if(Points.Count >= 3)
			{
				var cvPoints = new List<OpenCvSharp.Point>();

				foreach(var p in Points)
				{
					cvPoints.Add(new OpenCvSharp.Point((int)p.X, (int)p.Y));
				}
				Cv2.FillPoly(mask, new[] { cvPoints }, Scalar.All(255));
			}
			return mask;
		}
	}
}
