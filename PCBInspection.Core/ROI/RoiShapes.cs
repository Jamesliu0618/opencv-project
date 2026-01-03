using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace PCBInspection.Core.ROI
{
	/// <summary>
	/// ROI 控制點類型定義
	/// </summary>
	public enum RoiHandle
	{
		None,       // 無
		Body,       // 本體（用於平移）
		TopLeft,    // 左上角
		Top,        // 頂邊
		TopRight,   // 右上角
		Right,      // 右邊
		BottomRight, // 右下角
		Bottom,     // 底邊
		BottomLeft, // 左下角
		Left,       // 左邊
		Edge        // 圓周邊緣 (用於調整半徑)
	}

	/// <summary>
	/// ROI 形狀基底類別
	/// </summary>
	public abstract class RoiBase
	{
		public Guid   Id         { get; } = Guid.NewGuid();
		public string Name       { get; set; }
		public Color  Color      { get; set; } = Color.Lime; // 預設亮綠色
		public bool   IsSelected { get; set; }

		// 抽象方法：繪製、點擊測試、移動以及生成二元遮罩
		public abstract void Draw(Graphics g,       float scale, float offsetX, float offsetY);
		public abstract bool HitTest(Point mousePt, float scale, float offsetX, float offsetY);
		public abstract void Move(int      dx,      int   dy);
		
		/// <summary>
		/// 根據當前 ROI 形狀生成 OpenCV 遮罩影像 (黑色背景，白色 ROI)
		/// </summary>
		public abstract Mat  GetMask(Size  imageSize);

		/// <summary>
		/// 取得滑鼠位置對應的控制點類型
		/// </summary>
		public virtual RoiHandle GetHandle(Point mousePt, float scale, float offsetX, float offsetY)
		{
			return HitTest(mousePt, scale, offsetX, offsetY) ? RoiHandle.Body : RoiHandle.None;
		}

		/// <summary>
		/// 調整 ROI 大小
		/// </summary>
		public virtual void Resize(RoiHandle handle, PointF newImgPt) { }

		// 座標轉換輔助方法 (螢幕視窗座標 <-> 原始影像座標)
		protected PointF ScreenToImage(Point screenPt, float scale, float offsetX, float offsetY)
		{
			return new PointF((screenPt.X - offsetX) / scale, (screenPt.Y - offsetY) / scale);
		}

		protected Point ImageToScreen(PointF imagePt, float scale, float offsetX, float offsetY)
		{
			return new Point((int)(imagePt.X * scale + offsetX), (int)(imagePt.Y * scale + offsetY));
		}

		/// <summary>
		/// 在螢幕上繪製控制點方框
		/// </summary>
		protected void DrawHandle(Graphics g, Point screenPt)
		{
			int s = 6;
			g.FillRectangle(Brushes.White, screenPt.X - s / 2, screenPt.Y - s / 2, s, s);
			g.DrawRectangle(Pens.Black, screenPt.X - s / 2, screenPt.Y - s / 2, s, s);
		}

		/// <summary>
		/// 判定兩個螢幕點是否足夠接近 (用於控制點偵測)
		/// </summary>
		protected bool IsPointNear(Point pt1, Point pt2, int dist = 8)
		{
			return Math.Abs(pt1.X - pt2.X) <= dist && Math.Abs(pt1.Y - pt2.Y) <= dist;
		}
	}

	/// <summary>
	/// 矩形 ROI
	/// </summary>
	public class RectangleRoi : RoiBase
	{
		public RectangleRoi(float x, float y, float w, float h)
		{
			Rect = new RectangleF(x, y, w, h);
			Name = "Rect";
		}

		public RectangleF Rect { get; set; }

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			// 計算在螢幕上的位置與大小
			var screenRect = new RectangleF(Rect.X * scale + offsetX, Rect.Y * scale + offsetY, Rect.Width * scale, Rect.Height * scale);

			using(var pen = new Pen(IsSelected ? Color.Yellow : Color, 2))
			{
				// 選取時顯示虛線與黃色，否則為實線與預設色
				pen.DashStyle = IsSelected ? DashStyle.Dash : DashStyle.Solid;
				g.DrawRectangle(pen, screenRect.X, screenRect.Y, screenRect.Width, screenRect.Height);
			}

			// 顯示控制點以便調整大小
			if(IsSelected)
			{
				DrawHandle(g, ImageToScreen(new PointF(Rect.Left, Rect.Top), scale, offsetX, offsetY));     // TL
				DrawHandle(g, ImageToScreen(new PointF(Rect.Right, Rect.Top), scale, offsetX, offsetY));    // TR
				DrawHandle(g, ImageToScreen(new PointF(Rect.Right, Rect.Bottom), scale, offsetX, offsetY)); // BR
				DrawHandle(g, ImageToScreen(new PointF(Rect.Left, Rect.Bottom), scale, offsetX, offsetY));  // BL
			}
		}

		public override bool HitTest(Point mousePt, float scale, float offsetX, float offsetY)
		{
			var imgPt = ScreenToImage(mousePt, scale, offsetX, offsetY);
			return Rect.Contains(imgPt);
		}

		public override RoiHandle GetHandle(Point mousePt, float scale, float offsetX, float offsetY)
		{
			if(!IsSelected) return base.GetHandle(mousePt, scale, offsetX, offsetY);

			// 檢查是否點中四個角
			var tl = ImageToScreen(new PointF(Rect.Left, Rect.Top), scale, offsetX, offsetY);
			var tr = ImageToScreen(new PointF(Rect.Right, Rect.Top), scale, offsetX, offsetY);
			var br = ImageToScreen(new PointF(Rect.Right, Rect.Bottom), scale, offsetX, offsetY);
			var bl = ImageToScreen(new PointF(Rect.Left, Rect.Bottom), scale, offsetX, offsetY);

			if(IsPointNear(mousePt, tl)) return RoiHandle.TopLeft;
			if(IsPointNear(mousePt, tr)) return RoiHandle.TopRight;
			if(IsPointNear(mousePt, br)) return RoiHandle.BottomRight;
			if(IsPointNear(mousePt, bl)) return RoiHandle.BottomLeft;

			return HitTest(mousePt, scale, offsetX, offsetY) ? RoiHandle.Body : RoiHandle.None;
		}

		public override void Resize(RoiHandle handle, PointF newImgPt)
		{
			float l = Rect.Left, t = Rect.Top, r = Rect.Right, b = Rect.Bottom;

			switch(handle)
			{
				case RoiHandle.TopLeft: l = newImgPt.X; t = newImgPt.Y; break;
				case RoiHandle.TopRight: r = newImgPt.X; t = newImgPt.Y; break;
				case RoiHandle.BottomRight: r = newImgPt.X; b = newImgPt.Y; break;
				case RoiHandle.BottomLeft: l = newImgPt.X; b = newImgPt.Y; break;
			}

			// 自動正交化避免負值寬高
			Rect = RectangleF.FromLTRB(Math.Min(l, r), Math.Min(t, b), Math.Max(l, r), Math.Max(t, b));
		}

		public override void Move(int dx, int dy)
		{
			Rect = new RectangleF(Rect.X + dx, Rect.Y + dy, Rect.Width, Rect.Height);
		}

		public override Mat GetMask(Size imageSize)
		{
			var mask = new Mat(imageSize, MatType.CV_8UC1, Scalar.All(0));
			var rect = new Rect((int)Rect.X, (int)Rect.Y, (int)Rect.Width, (int)Rect.Height);

			// 裁切使其不超出影像邊界
			rect = rect.Intersect(new Rect(0, 0, imageSize.Width, imageSize.Height));

			if(rect.Width > 0 && rect.Height > 0)
			{
				Cv2.Rectangle(mask, rect, Scalar.All(255), -1);
			}
			return mask;
		}
	}

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
