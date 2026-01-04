using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace PCBInspection.Core.ROI
{
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

			// 顯示 ROI 編號 (在左上角)
			if(Index > 0)
			{
				string label = Index.ToString();
				using(var font = new Font("Arial", 10, FontStyle.Bold))
				using(var brush = new SolidBrush(IsSelected ? Color.Yellow : Color))
				{
					g.DrawString(label, font, brush, screenRect.X + 2, screenRect.Y + 2);
				}
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
}
