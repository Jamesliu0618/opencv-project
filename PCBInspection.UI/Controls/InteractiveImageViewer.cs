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
		// Interaction Mode
		public enum ViewerMode
		{
			None,
			Pan,
			DrawRect,
			DrawCircle,
			DrawPoly,
			EditROI,
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

		public Bitmap Image
		{
			get => _image;
			set
			{
				_image = value;
				FitToWindow();
				Invalidate();
			}
		}

		public ViewerMode Mode
		{
			get => _mode;
			set
			{
				_mode  = value;
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
		public event EventHandler<Point> MousePixelChanged;
		public event EventHandler        ViewChanged;

		public string GetDebugInfo()
		{
			return $"Scale={_scale:F3}, Offset={_offsetX:F1},{_offsetY:F1}, " + $"CtlSize={Width}x{Height}, Dock={Dock}, Parent={Parent?.Name} ({Parent?.Width}x{Parent?.Height}), " + $"ImgSize={(_image == null ? "null" : $"{_image.Width}x{_image.Height}")}, " + $"Visible={Visible}, Mode={Mode}";
		}

		// --- Transformation Helpers ---

		public void FitToWindow()
		{
			if(_image == null)
			{
				return;
			}
			float scaleW = (float)Width  / _image.Width;
			float scaleH = (float)Height / _image.Height;
			_scale = Math.Min(scaleW, scaleH) * 0.9f;

			// Center
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

		private void ApplyZoom(float factor, Point center)
		{
			float oldScale = _scale;
			_scale *= factor;

			// Limit limits
			if(_scale < 0.05f)
			{
				_scale = 0.05f;
			}

			if(_scale > 20.0f)
			{
				_scale = 20.0f;
			}

			// Adjust offset to keep center point stable
			// newOffset = mouse - (mouse - oldOffset) * (newScale / oldScale)
			_offsetX = center.X - (center.X - _offsetX) * (_scale / oldScale);
			_offsetY = center.Y - (center.Y - _offsetY) * (_scale / oldScale);
			Invalidate();
			ViewChanged?.Invoke(this, EventArgs.Empty);
		}

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

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			var g = e.Graphics;
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode   = PixelOffsetMode.Half;

			// Draw Debug String FIRST to ensure visibility
			try
			{
				string debugText = $"[DEBUG] Ctl: {Width}x{Height}, Scale: {_scale:F2}, Offset: {_offsetX:F0},{_offsetY:F0}";

				if(_image != null)
				{
					debugText += $" | Img: {_image.Width}x{_image.Height} | PixelFmt: {_image.PixelFormat}";
				}
				else
				{
					debugText += " | Img: NULL";
				}
				g.DrawString(debugText, SystemFonts.DefaultFont, Brushes.Red, 10, 10);
			}
			catch
			{
				/* Ignore Font errors */
			}

			if(_image != null)
			{
				try
				{
					g.DrawImage(_image, _offsetX, _offsetY, _image.Width * _scale, _image.Height * _scale);
				}
				catch(Exception ex)
				{
					g.DrawString($"[DRAW ERROR] {ex.GetType().Name}: {ex.Message}", SystemFonts.DefaultFont, Brushes.Red, 10, 30);
				}
			}

			// Draw ROIs
			foreach(var roi in Rois)
			{
				roi.Draw(g, _scale, _offsetX, _offsetY);
			}

			// Draw Temp ROI
			_tempRoi?.Draw(g, _scale, _offsetX, _offsetY);
		}

		// --- Interaction ---

		private RoiHandle _activeHandle = RoiHandle.None;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			_lastMousePos = e.Location;
			Focus();

			if(e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && Mode == ViewerMode.Pan))
			{
				Cursor = Cursors.NoMove2D;
				return;
			}
			var imgPt = ScreenToImage(e.Location);

			if(e.Button == MouseButtons.Left)
			{
				if(Mode == ViewerMode.DrawRect)
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
						Invalidate();
					}
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			var imgPt = ScreenToImage(e.Location);

			// Report pixel pos
			if(_image != null && imgPt.X >= 0 && imgPt.X < _image.Width && imgPt.Y >= 0 && imgPt.Y < _image.Height)
			{
				MousePixelChanged?.Invoke(this, new Point((int)imgPt.X, (int)imgPt.Y));
			}

			// Update cursor based on handle
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

			if(e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && Cursor == Cursors.NoMove2D))
			{
				// Pan
				_offsetX      += e.X - _lastMousePos.X;
				_offsetY      += e.Y - _lastMousePos.Y;
				_lastMousePos =  e.Location;
				Invalidate();
				ViewChanged?.Invoke(this, EventArgs.Empty);
				return;
			}

			if(e.Button == MouseButtons.Left && _tempRoi != null)
			{
				// Dragging shape creation
				if(_tempRoi is RectangleRoi rect)
				{
					float w = imgPt.X - rect.Rect.X;
					float h = imgPt.Y - rect.Rect.Y;
					rect.Rect = new RectangleF(rect.Rect.X, rect.Rect.Y, w, h);
				}
				else if(_tempRoi is CircleRoi circ)
				{
					// Calculate bounding box from start corner to current mouse position
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

					// Inscribed circle: center of bounding box, radius = half of smaller dimension
					circ.Center = new PointF(minX + width / 2, minY + height / 2);
					circ.Radius = Math.Min(width, height) / 2;
				}
				Invalidate();
			}
			else if(e.Button == MouseButtons.Left && SelectedRoi != null && Mode == ViewerMode.EditROI)
			{
				if(_activeHandle == RoiHandle.Body)
				{
					// Move ROI
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
					// Resize ROI
					SelectedRoi.Resize(_activeHandle, imgPt);
					Invalidate();
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_activeHandle = RoiHandle.None; // Reset active handle

			if(Cursor == Cursors.NoMove2D)
			{
				Cursor = Mode == ViewerMode.Pan ? Cursors.Hand : Cursors.Cross;

				if(Mode == ViewerMode.EditROI)
				{
					Cursor = Cursors.Default;
				}
			}

			if(_tempRoi != null && Mode != ViewerMode.DrawPoly)
			{
				// Finish shape
				// Ensure Rect is normalized (width/height not negative)
				if(_tempRoi is RectangleRoi r)
				{
					float x = r.Rect.X;
					float y = r.Rect.Y;
					float w = r.Rect.Width;
					float h = r.Rect.Height;

					if(w < 0)
					{
						x += w;
						w =  Math.Abs(w);
					}

					if(h < 0)
					{
						y += h;
						h =  Math.Abs(h);
					}
					r.Rect = new RectangleF(x, y, w, h);
				}
				Rois.Add(_tempRoi);
				_tempRoi = null;
				Mode     = ViewerMode.EditROI; // Auto switch to edit after draw
				RoiListChanged?.Invoke(this, EventArgs.Empty);
				Invalidate();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			// Zoom to mouse ptr
			float factor = e.Delta > 0 ? 1.1f : 0.9f;
			ApplyZoom(factor, e.Location);
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if(Mode == ViewerMode.DrawPoly && _tempRoi != null)
			{
				// Finish Poly
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