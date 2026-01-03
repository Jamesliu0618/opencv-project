using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PCBInspection.Core.ROI;

namespace PCBInspection.UI.Controls
{
    public class InteractiveImageViewer : UserControl
    {
        // Image
        private Bitmap _image;
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

        // Transformation
        private float _scale = 1.0f;
        private float _offsetX = 0;
        private float _offsetY = 0;
        private Point _lastMousePos;

        // Interaction Mode
        public enum ViewerMode { None, Pan, DrawRect, DrawCircle, DrawPoly, EditROI }
        private ViewerMode _mode = ViewerMode.Pan;
        public ViewerMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                Cursor = _mode == ViewerMode.Pan ? Cursors.Hand : Cursors.Cross;
                if (_mode == ViewerMode.EditROI) Cursor = Cursors.Default;
            }
        }

        // ROI
        public List<RoiBase> Rois { get; } = new List<RoiBase>();
        public RoiBase SelectedRoi { get; private set; }
        private RoiBase _tempRoi; // For drawing
        public event EventHandler RoiListChanged;

        // Events
        public event EventHandler<Point> MousePixelChanged;

        public InteractiveImageViewer()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Silver;
            this.Cursor = Cursors.Hand;
        }

        // --- Transformation Helpers ---

        public void FitToWindow()
        {
            if (_image == null) return;
            
            float scaleW = (float)Width / _image.Width;
            float scaleH = (float)Height / _image.Height;
            _scale = Math.Min(scaleW, scaleH) * 0.9f;
            
            // Center
            _offsetX = (Width - _image.Width * _scale) / 2;
            _offsetY = (Height - _image.Height * _scale) / 2;
            
            Invalidate();
        }

        public void ZoomIn() => ApplyZoom(1.2f, new Point(Width/2, Height/2));
        public void ZoomOut() => ApplyZoom(0.8f, new Point(Width/2, Height/2));
        public void SetZoom100() { _scale = 1.0f; CenterImage(); Invalidate(); }

        private void CenterImage()
        {
             if (_image == null) return;
            _offsetX = (Width - _image.Width * _scale) / 2;
            _offsetY = (Height - _image.Height * _scale) / 2;
        }

        private void ApplyZoom(float factor, Point center)
        {
            float oldScale = _scale;
            _scale *= factor;
            
            // Limit limits
            if (_scale < 0.05f) _scale = 0.05f;
            if (_scale > 20.0f) _scale = 20.0f;

            // Adjust offset to keep center point stable
            // newOffset = mouse - (mouse - oldOffset) * (newScale / oldScale)
            _offsetX = center.X - (center.X - _offsetX) * (_scale / oldScale);
            _offsetY = center.Y - (center.Y - _offsetY) * (_scale / oldScale);

            Invalidate();
        }

        private PointF ScreenToImage(Point p)
        {
            return new PointF((p.X - _offsetX) / _scale, (p.Y - _offsetY) / _scale);
        }

        // --- Drawing ---

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor; // Pixelated for precise inspection
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (_image != null)
            {
                g.DrawImage(_image, _offsetX, _offsetY, _image.Width * _scale, _image.Height * _scale);
            }

            // Draw ROIs
            foreach (var roi in Rois)
            {
                roi.Draw(g, _scale, _offsetX, _offsetY);
            }

            // Draw Temp ROI
            _tempRoi?.Draw(g, _scale, _offsetX, _offsetY);
        }

        // --- Interaction ---

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _lastMousePos = e.Location;
            this.Focus();

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && Mode == ViewerMode.Pan))
            {
                Cursor = Cursors.NoMove2D;
                return;
            }

            var imgPt = ScreenToImage(e.Location);

            if (e.Button == MouseButtons.Left)
            {
               if (Mode == ViewerMode.DrawRect)
               {
                   _tempRoi = new RectangleRoi(imgPt.X, imgPt.Y, 0, 0);
               }
               else if (Mode == ViewerMode.DrawCircle)
               {
                   _tempRoi = new CircleRoi(imgPt, 0);
               }
               else if (Mode == ViewerMode.DrawPoly)
               {
                   if (_tempRoi == null) _tempRoi = new PolygonRoi();
                   ((PolygonRoi)_tempRoi).Points.Add(imgPt);
                   Invalidate();
               }
               else if (Mode == ViewerMode.EditROI)
               {
                   // Hit Test
                   SelectedRoi = null;
                   foreach(var roi in Rois)
                   {
                       roi.IsSelected = false;
                       if (roi.HitTest(e.Location, _scale, _offsetX, _offsetY))
                       {
                           SelectedRoi = roi;
                       }
                   }
                   if (SelectedRoi != null) SelectedRoi.IsSelected = true;
                   Invalidate();
               }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            var imgPt = ScreenToImage(e.Location);
            // Report pixel pos
            if (_image != null && imgPt.X >= 0 && imgPt.X < _image.Width && imgPt.Y >= 0 && imgPt.Y < _image.Height)
            {
                MousePixelChanged?.Invoke(this, new Point((int)imgPt.X, (int)imgPt.Y));
            }

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && Cursor == Cursors.NoMove2D))
            {
                // Pan
                _offsetX += e.X - _lastMousePos.X;
                _offsetY += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                Invalidate();
                return;
            }

            if (e.Button == MouseButtons.Left && _tempRoi != null)
            {
                // Dragging shape creation
                if (_tempRoi is RectangleRoi rect)
                {
                    float w = imgPt.X - rect.Rect.X;
                    float h = imgPt.Y - rect.Rect.Y;
                    rect.Rect = new RectangleF(rect.Rect.X, rect.Rect.Y, w, h);
                }
                else if (_tempRoi is CircleRoi circ)
                {
                    // Distance
                    double radius = Math.Sqrt(Math.Pow(imgPt.X - circ.Center.X, 2) + Math.Pow(imgPt.Y - circ.Center.Y, 2));
                    circ.Radius = (float)radius;
                }
                Invalidate();
            }
            else if (e.Button == MouseButtons.Left && SelectedRoi != null && Mode == ViewerMode.EditROI)
            {
                // Move ROI
                int dx = (int)(imgPt.X - ScreenToImage(_lastMousePos).X);
                int dy = (int)(imgPt.Y - ScreenToImage(_lastMousePos).Y);
                if (dx != 0 || dy != 0)
                {
                    SelectedRoi.Move(dx, dy);
                    _lastMousePos = e.Location;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (Cursor == Cursors.NoMove2D)
            {
                Cursor = Mode == ViewerMode.Pan ? Cursors.Hand : Cursors.Cross;
                if (Mode == ViewerMode.EditROI) Cursor = Cursors.Default;
            }

            if (_tempRoi != null && Mode != ViewerMode.DrawPoly)
            {
                // Finish shape
                // Ensure Rect is normalized (width/height not negative)
                if (_tempRoi is RectangleRoi r)
                {
                    float x = r.Rect.X;
                    float y = r.Rect.Y;
                    float w = r.Rect.Width;
                    float h = r.Rect.Height;
                    if (w < 0) { x += w; w = Math.Abs(w); }
                    if (h < 0) { y += h; h = Math.Abs(h); }
                    r.Rect = new RectangleF(x, y, w, h);
                }

                Rois.Add(_tempRoi);
                _tempRoi = null;
                Mode = ViewerMode.EditROI; // Auto switch to edit after draw
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
            if (Mode == ViewerMode.DrawPoly && _tempRoi != null)
            {
                // Finish Poly
                Rois.Add(_tempRoi);
                _tempRoi = null;
                Mode = ViewerMode.EditROI;
                RoiListChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }
    }
}
