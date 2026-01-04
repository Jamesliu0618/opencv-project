using OpenCvSharp;
using System;
using System.Drawing;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace PCBInspection.Core.ROI
{
	/// <summary>
	/// ROI 形狀基底類別
	/// </summary>
	public abstract class RoiBase
	{
		public Guid   Id         { get; } = Guid.NewGuid();
		/// <summary>ROI 編號 (從 1 開始)</summary>
		public int    Index      { get; set; } = 0;
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
}
