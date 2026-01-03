using PCBInspection.Core.ROI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PCBInspection.UI.Controls
{
	public class InteractiveImageViewer : UserControl
	{
		/// <summary>
		/// 影像檢視器的互動模式
		/// </summary>
		public enum ViewerMode
		{
			None,
			Pan,       // 平移模式 (滑鼠拖動)
			DrawRect,  // 繪製矩形 ROI
			DrawCircle, // 繪製圓形 ROI
			DrawPoly,   // 繪製多邊形 ROI
			EditROI,    // 編輯/移動現有 ROI
			Labeling,   // 手動標註缺陷模式
		}

		// Image
		private Bitmap     _image;
		private Point      _lastMousePos;
		private ViewerMode _mode = ViewerMode.Pan;
		private float      _offsetX;
		private float      _offsetY;

		// Transformation
		private float   _scale = 1.0f;
		private RoiBase _tempRoi; // For drawing

		public InteractiveImageViewer()
		{
			DoubleBuffered = true;
			BackColor      = Color.Silver;
			Cursor         = Cursors.Hand;
		}

		/// <summary>
		/// 當前顯示的影像
		/// </summary>
		public Bitmap Image
		{
			get => _image;
			set
			{
				_image = value;
				FitToWindow(); // 自動縮放至視窗大小
				Invalidate();
			}
		}

		/// <summary>
		/// 當前互動模式（平移、繪圖、編輯、標註）
		/// </summary>
		public ViewerMode Mode
		{
			get => _mode;
			set
			{
				_mode  = value;
				// 根據模式切換鼠標圖示
				Cursor = _mode == ViewerMode.Pan ? Cursors.Hand : Cursors.Cross;

				if(_mode == ViewerMode.EditROI)
				{
					Cursor = Cursors.Default;
				}
			}
		}

		// ROI
		public List<RoiBase>      Rois        { get; } = new List<RoiBase>();
		public RoiBase            SelectedRoi { get; private set; }
		public event EventHandler RoiListChanged;

		// Events
		// Events
		public event EventHandler<Point> MousePixelChanged;
		public event EventHandler        ViewChanged;
		public event Action<RoiBase>     RoiCreated; // 標註建立事件
		
		public float ScaleFactor => _scale;
		public float OffsetX => _offsetX;
		public float OffsetY => _offsetY;

		public void SetView(float scale, float ox, float oy)
		{
			if(Math.Abs(_scale - scale) < 0.0001f && Math.Abs(_offsetX - ox) < 0.1f && Math.Abs(_offsetY - oy) < 0.1f) return;
			
			_scale = scale;
			_offsetX = ox;
			_offsetY = oy;
			Invalidate();
			ViewChanged?.Invoke(this, EventArgs.Empty);
		}

		public string GetDebugInfo()
		{
			return $"Scale={_scale:F3}, Offset={_offsetX:F1},{_offsetY:F1}, " + $"CtlSize={Width}x{Height}, Dock={Dock}, Parent={Parent?.Name} ({Parent?.Width}x{Parent?.Height}), " + $"ImgSize={(_image == null ? "null" : $"{_image.Width}x{_image.Height}")}, " + $"Visible={Visible}, Mode={Mode}";
		}

		// --- Transformation Helpers ---

		/// <summary>
		/// 自動調整影像縮放比例與位置，使其完整顯示於視窗內
		/// </summary>
		public void FitToWindow()
		{
			if(_image == null)
			{
				return;
			}
			float scaleW = (float)Width  / _image.Width;
			float scaleH = (float)Height / _image.Height;
			
			// 縮放比取寬高較小者，並留 10% 邊距
			_scale = Math.Min(scaleW, scaleH) * 0.9f;

			// 將影像置中
			_offsetX = (Width  - _image.Width  * _scale) / 2;
			_offsetY = (Height - _image.Height * _scale) / 2;
			Invalidate();
			ViewChanged?.Invoke(this, EventArgs.Empty);
		}

		public void ZoomIn()  => ApplyZoom(1.2f, new Point(Width / 2, Height / 2));
		public void ZoomOut() => ApplyZoom(0.8f, new Point(Width / 2, Height / 2));

		public void SetZoom100()
		{
			_scale = 1.0f;
			CenterImage();
			Invalidate();
			ViewChanged?.Invoke(this, EventArgs.Empty);
		}

		public void CenterAt(PointF p)
		{
			_offsetX = Width  / 2.0f - p.X * _scale;
			_offsetY = Height / 2.0f - p.Y * _scale;
			Invalidate();
			ViewChanged?.Invoke(this, EventArgs.Empty);
		}

		private void CenterImage()
		{
			if(_image == null)
			{
				return;
			}
			_offsetX = (Width  - _image.Width  * _scale) / 2;
			_offsetY = (Height - _image.Height * _scale) / 2;
		}

		/// <summary>
		/// 執行縮放操作
		/// </summary>
		/// <param name="factor">縮放因子</param>
		/// <param name="center">視窗上的縮放中心點（通常為滑鼠位置）</param>
		private void ApplyZoom(float factor, Point center)
		{
			float oldScale = _scale;
			_scale *= factor;

			// 限制縮放範圍 (5% ~ 2000%)
			if(_scale < 0.05f) _scale = 0.05f;
			if(_scale > 20.0f) _scale = 20.0f;

			// 調整偏移量以確保縮放中心點保持相對靜止 (Zoom around mouse)
			_offsetX = center.X - (center.X - _offsetX) * (_scale / oldScale);
			_offsetY = center.Y - (center.Y - _offsetY) * (_scale / oldScale);
			
			Invalidate();
			ViewChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 將視窗座標 (Screen/Control) 轉換為原始影像座標 (Pixel)
		/// </summary>
		private PointF ScreenToImage(Point p)
		{
			return new PointF((p.X - _offsetX) / _scale, (p.Y - _offsetY) / _scale);
		}

		public RectangleF GetViewport()
		{
			var p1 = ScreenToImage(new Point(0,     0));
			var p2 = ScreenToImage(new Point(Width, Height));
			return new RectangleF(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
		}

		// --- Drawing ---

		/// <summary>
		/// 影像繪製邏輯
		/// </summary>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			var g = e.Graphics;
			// 使用 NearestNeighbor 放大時不模糊，保持像素邊界
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode   = PixelOffsetMode.Half;

			// 1. 繪製除錯狀態資訊 (縮放比、偏移、影像尺寸)
			try
			{
				string debugText = $"[INFO] Scale: {_scale:F2}, Offset: {_offsetX:F0},{_offsetY:F0}";
				if(_image != null) debugText += $" | Img: {_image.Width}x{_image.Height}";
				g.DrawString(debugText, SystemFonts.DefaultFont, Brushes.LimeGreen, 10, 10);
			}
			catch { }

			// 2. 繪製主影像 (套用目前的縮放與平移)
			if(_image != null)
			{
				try
				{
					g.DrawImage(_image, _offsetX, _offsetY, _image.Width * _scale, _image.Height * _scale);
				}
				catch(Exception ex)
				{
					g.DrawString($"影像顯示錯誤: {ex.Message}", SystemFonts.DefaultFont, Brushes.Red, 10, 30);
				}
			}

			// 3. 繪製所有 ROI 物件
			foreach(var roi in Rois)
			{
				roi.Draw(g, _scale, _offsetX, _offsetY);
			}

			// 4. 繪製目前正在拉取的臨時 ROI
			_tempRoi?.Draw(g, _scale, _offsetX, _offsetY);
		}

		// --- Interaction ---

		private RoiHandle _activeHandle = RoiHandle.None;

		/// <summary>
		/// 滑鼠按下：開始平移、開始繪製或選取 ROI
		/// </summary>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			_lastMousePos = e.Location;
			Focus();

			// 處理平移 (中鍵或平移模式下的左鍵)
			if(e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && Mode == ViewerMode.Pan))
			{
				Cursor = Cursors.NoMove2D;
				return;
			}
			var imgPt = ScreenToImage(e.Location);

			if(e.Button == MouseButtons.Left)
			{
				if(Mode == ViewerMode.DrawRect || Mode == ViewerMode.Labeling) // Labeling 也使用矩形
				{
					_tempRoi = new RectangleRoi(imgPt.X, imgPt.Y, 0, 0);
				}
				else if(Mode == ViewerMode.DrawCircle)
				{
					var circ = new CircleRoi(imgPt, 0);
					circ.StartCorner = imgPt;
					_tempRoi         = circ;
				}
				else if(Mode == ViewerMode.DrawPoly)
				{
					if(_tempRoi == null)
					{
						_tempRoi = new PolygonRoi();
					}
					((PolygonRoi)_tempRoi).Points.Add(imgPt);
					Invalidate();
				}
				else if(Mode == ViewerMode.EditROI)
				{
					_activeHandle = RoiHandle.None;

					// 先檢查是否點擊了目前選取 ROI 的控制點
					if(SelectedRoi != null)
					{
						_activeHandle = SelectedRoi.GetHandle(e.Location, _scale, _offsetX, _offsetY);
					}

					if(_activeHandle == RoiHandle.None)
					{
						// Hit Test 新選取
						SelectedRoi = null;

						foreach(var roi in Rois)
						{
							roi.IsSelected = false;

							if(roi.HitTest(e.Location, _scale, _offsetX, _offsetY))
							{
								SelectedRoi = roi;
							}
						}

						if(SelectedRoi != null)
						{
							SelectedRoi.IsSelected = true;
							_activeHandle = SelectedRoi.GetHandle(e.Location, _scale, _offsetX, _offsetY);
						}
						// 請求重新繪製以更新選取狀態
						Invalidate();
					}
				}
			}
		}

		/// <summary>
		/// 滑鼠移動：處理平移、繪製預覽或調整 ROI 大小
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			var imgPt = ScreenToImage(e.Location);

			// 1. 回報當前滑鼠指向的像素座標 (觸發 MousePixelChanged 事件)
			if(_image != null && imgPt.X >= 0 && imgPt.X < _image.Width && imgPt.Y >= 0 && imgPt.Y < _image.Height)
			{
				MousePixelChanged?.Invoke(this, new Point((int)imgPt.X, (int)imgPt.Y));
			}

			// 2. 編輯模式下：根據滑鼠位置更新游標形狀 (例如呈現調整大小的箭頭)
			if(Mode == ViewerMode.EditROI && SelectedRoi != null && e.Button == MouseButtons.None)
			{
				var handle = SelectedRoi.GetHandle(e.Location, _scale, _offsetX, _offsetY);
				switch(handle)
				{
					case RoiHandle.TopLeft:
					case RoiHandle.BottomRight:
						Cursor = Cursors.SizeNWSE;
						break;
					case RoiHandle.TopRight:
					case RoiHandle.BottomLeft:
						Cursor = Cursors.SizeNESW;
						break;
					case RoiHandle.Right:
					case RoiHandle.Left:
						Cursor = Cursors.SizeWE;
						break;
					case RoiHandle.Top:
					case RoiHandle.Bottom:
						Cursor = Cursors.SizeNS;
						break;
					case RoiHandle.Body:
						Cursor = Cursors.SizeAll;
						break;
					default:
						Cursor = Cursors.Default;
						break;
				}
			}

			// 3. 處理平移 (Pan)
			if(e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && Cursor == Cursors.NoMove2D))
			{
				_offsetX      += e.X - _lastMousePos.X;
				_offsetY      += e.Y - _lastMousePos.Y;
				_lastMousePos =  e.Location;
				Invalidate();
				ViewChanged?.Invoke(this, EventArgs.Empty);
				return;
			}

			// 4. 處理 ROI 繪製預覽 (Dragging shape creation)
			if(e.Button == MouseButtons.Left && _tempRoi != null)
			{
				if(_tempRoi is RectangleRoi rect)
				{
					float w = imgPt.X - rect.Rect.X;
					float h = imgPt.Y - rect.Rect.Y;
					rect.Rect = new RectangleF(rect.Rect.X, rect.Rect.Y, w, h);
				}
				else if(_tempRoi is CircleRoi circ)
				{
					// 外接矩形繪法：計算起始點與當前點形成的內切圓
					float x1     = circ.StartCorner.X;
					float y1     = circ.StartCorner.Y;
					float x2     = imgPt.X;
					float y2     = imgPt.Y;
					float minX   = Math.Min(x1, x2);
					float minY   = Math.Min(y1, y2);
					float maxX   = Math.Max(x1, x2);
					float maxY   = Math.Max(y1, y2);
					float width  = maxX - minX;
					float height = maxY - minY;

					circ.Center = new PointF(minX + width / 2, minY + height / 2);
					circ.Radius = Math.Min(width, height) / 2;
				}
				Invalidate();
			}
			// 5. 處理現有 ROI 的移動或大小調整
			else if(e.Button == MouseButtons.Left && SelectedRoi != null && Mode == ViewerMode.EditROI)
			{
				if(_activeHandle == RoiHandle.Body)
				{
					// 移動整體位置
					int dx = (int)(imgPt.X - ScreenToImage(_lastMousePos).X);
					int dy = (int)(imgPt.Y - ScreenToImage(_lastMousePos).Y);

					if(dx != 0 || dy != 0)
					{
						SelectedRoi.Move(dx, dy);
						_lastMousePos = e.Location;
						Invalidate();
					}
				}
				else if(_activeHandle != RoiHandle.None)
				{
					// 調整特定控制點 (邊界縮放)
					SelectedRoi.Resize(_activeHandle, imgPt);
					Invalidate();
				}
			}
		}

		/// <summary>
		/// 滑鼠放開：完成繪製、平移或編輯
		/// </summary>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_activeHandle = RoiHandle.None;

			// 重置鼠標
			if(Cursor == Cursors.NoMove2D)
			{
				Cursor = Mode == ViewerMode.Pan ? Cursors.Hand : Cursors.Cross;
				if(Mode == ViewerMode.EditROI) Cursor = Cursors.Default;
			}

			// 完成繪圖
			if(_tempRoi != null && Mode != ViewerMode.DrawPoly)
			{
				// 確保矩形正交化 (處理反向拖取導致的寬高為負)
				if(_tempRoi is RectangleRoi r)
				{
					float x = r.Rect.X, y = r.Rect.Y, w = r.Rect.Width, h = r.Rect.Height;
					if(w < 0) { x += w; w = Math.Abs(w); }
					if(h < 0) { y += h; h = Math.Abs(h); }
					r.Rect = new RectangleF(x, y, w, h);
				}

				if(Mode == ViewerMode.Labeling)
				{
					// 標註模式：觸發建立事件，不直接加入 Rois 清單
					RoiCreated?.Invoke(_tempRoi);
					_tempRoi = null;
					Invalidate();
				}
				else
				{
					// 一般模式：加入清單並轉為編輯模式
					Rois.Add(_tempRoi);
					_tempRoi = null;
					Mode     = ViewerMode.EditROI; 
					RoiListChanged?.Invoke(this, EventArgs.Empty);
					Invalidate();
				}
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			// Zoom to mouse ptr
			float factor = e.Delta > 0 ? 1.1f : 0.9f;
			ApplyZoom(factor, e.Location);
		}

		/// <summary>
		/// 滑鼠雙擊：用於結束多邊形繪製
		/// </summary>
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if(Mode == ViewerMode.DrawPoly && _tempRoi != null)
			{
				Rois.Add(_tempRoi);
				_tempRoi = null;
				Mode     = ViewerMode.EditROI;
				RoiListChanged?.Invoke(this, EventArgs.Empty);
				Invalidate();
			}
		}

		/// <summary>
		///     Required method for Designer support - do not modify
		///     the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			SuspendLayout();

			// 
			// InteractiveImageViewer
			// 
			Name = "InteractiveImageViewer";
			Size = new Size(233, 228);
			ResumeLayout(false);
		}
	}
}