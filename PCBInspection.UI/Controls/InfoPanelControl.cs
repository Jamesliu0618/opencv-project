using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions; // For BitmapConverter
using PCBInspection.Core.Models;
using PCBInspection.Core.ROI;

namespace PCBInspection.UI.Controls
{
    /// <summary>右側資訊面板控件</summary>
    public class InfoPanelControl : UserControl
    {
        // 統計資訊區
        private GroupBox grpStatistics;
        private Label lblImageSize, lblImageSizeValue;
        private Label lblColorMode, lblColorModeValue;
        private Label lblProcessTime, lblProcessTimeValue;
        private Label lblObjectCount, lblObjectCountValue;
        private Label lblPassCount, lblPassCountValue;
        private Label lblFailCount, lblFailCountValue;
        private Label lblPassRate, lblPassRateValue;
        private Label lblMeanGray, lblMeanGrayValue;
        private Label lblStdDev, lblStdDevValue;
        private Label lblMinMax, lblMinMaxValue;

        // 檢測結果區
        private GroupBox grpResults;
        private DataGridView dgvResults;
        private Label lblResultStatus;

        // 直方圖區
        private GroupBox grpHistogram;
        private PictureBox pbHistogram;

        // 擴充資訊區 (TabControl)
        private TabControl tcExtras;
        private TabPage tpRoi, tpMeasure, tpNav, tpAttr;

        // ROI Tab Controls
        private Label lblRoiPosValue, lblRoiSizeValue, lblRoiAreaValue, lblRoiMeanValue;
        private Button btnClearRoi;

        // Measure Tab Controls
        private TextBox txtPixelRatio;
        private Label lblDistPx, lblDistMm, lblAngle;
        
        // Navigation Tab Controls
        private PictureBox pbNav;
        private float _navScale;
        private int _navOffsetX, _navOffsetY;

        // Attribute Tab Controls
        private PropertyGrid pgObjectDetail;

        // 事件
        public event EventHandler<int> ObjectHighlightRequested;
        public event EventHandler<(double X, double Y)> ZoomToPointRequested;
        public event EventHandler ClearRoiRequested;
        public event EventHandler<(double X, double Y)> NavigateToRequested;

        private List<DetectedObject> _currentObjects = new List<DetectedObject>();
        private Bitmap _histogramBitmap;
        private Bitmap _navBitmap;

        public InfoPanelControl()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.SuspendLayout();
            this.BackColor = SystemColors.Control;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Microsoft JhengHei", 9f);

            // 1. 統計資訊區 (Top)
            CreateStatisticsGroup();

            // 2. 直方圖區 (Bottom 1)
            CreateHistogramGroup();

            // 3. 擴充資訊區 (Bottom 2, below Histogram? Or TabControl replaces Histogram area? 
            // Design says: Bottom SplitContainer -> Panel1: Histogram, Panel2: TabControl)
            // But here we are in a vertical layout. Let's put TabControl at the very bottom, taking remaining space?
            // Wait, the design said: 
            // Panel2 (Right Panel) -> SplitContainer (Vertical 3 splits)
            // For simplicity in UserControl, let's just dock them Top/Top/Top/Fill or use a fixed height for bottom components.
            // Let's Dock Histogram at Bottom, then TabControl at Bottom, then Results Fill.
            
            CreateExtrasTabControl(); // Dock Bottom

            // 4. 檢測結果區 (Fill remaining)
            CreateResultsGroup();

            // Layout Order (Dock property impacts order):
            // We want: 
            // Top: Stats
            // Fill: Results
            // Bottom: Histogram
            // Bottom: TabControl (Extras)
            
            // So add order:
            // 1. TabControl (Dock Bottom)
            // 2. Histogram (Dock Bottom)
            // 3. Stats (Dock Top)
            // 4. Results (Dock Fill)
            
            this.Controls.Add(grpResults);
            this.Controls.Add(grpStatistics);
            this.Controls.Add(grpHistogram);
            this.Controls.Add(tcExtras); 

            this.ResumeLayout(false);
        }

        private void CreateStatisticsGroup()
        {
            grpStatistics = new GroupBox
            {
                Text = "統計資訊",
                Dock = DockStyle.Top,
                Height = 180, // Reduced slightly needed?
                Font = new Font("Microsoft JhengHei", 10f, FontStyle.Bold),
                Padding = new Padding(5)
            };

            var statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Font = new Font("Microsoft JhengHei", 9f)
            };
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            int row = 0;
            AddStatRow(statsPanel, "影像尺寸:", out lblImageSize, out lblImageSizeValue, ref row);
            AddStatRow(statsPanel, "色彩模式:", out lblColorMode, out lblColorModeValue, ref row);
            AddStatRow(statsPanel, "處理時間:", out lblProcessTime, out lblProcessTimeValue, ref row);
            AddStatRow(statsPanel, "檢測物件:", out lblObjectCount, out lblObjectCountValue, ref row);
            AddStatRow(statsPanel, "合格數量:", out lblPassCount, out lblPassCountValue, ref row);
            AddStatRow(statsPanel, "不良數量:", out lblFailCount, out lblFailCountValue, ref row);
            AddStatRow(statsPanel, "合格率:", out lblPassRate, out lblPassRateValue, ref row);
            AddStatRow(statsPanel, "平均灰階:", out lblMeanGray, out lblMeanGrayValue, ref row);
            AddStatRow(statsPanel, "標準差:", out lblStdDev, out lblStdDevValue, ref row);
            AddStatRow(statsPanel, "最小/最大:", out lblMinMax, out lblMinMaxValue, ref row);

            grpStatistics.Controls.Add(statsPanel);
        }

        private void CreateResultsGroup()
        {
            grpResults = new GroupBox
            {
                Text = "檢測結果",
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft JhengHei", 10f, FontStyle.Bold),
                Padding = new Padding(5)
            };

            dgvResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Font = new Font("Microsoft JhengHei", 9f),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 245, 245) }
            };

            dgvResults.Columns.Add("colId", "編號");
            dgvResults.Columns.Add("colType", "類型");
            dgvResults.Columns.Add("colCenterX", "中心X");
            dgvResults.Columns.Add("colCenterY", "中心Y");
            dgvResults.Columns.Add("colRadius", "半徑");
            dgvResults.Columns.Add("colArea", "面積");
            dgvResults.Columns.Add("colStatus", "狀態");

            dgvResults.Columns["colId"].Width = 40;
            dgvResults.Columns["colType"].Width = 50;
            dgvResults.Columns["colStatus"].Width = 40;

            dgvResults.CellClick += DgvResults_CellClick;
            dgvResults.CellDoubleClick += DgvResults_CellDoubleClick;
            dgvResults.SelectionChanged += DgvResults_SelectionChanged;

            lblResultStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "共 0 項",
                BackColor = Color.WhiteSmoke
            };

            var resultsPanel = new Panel { Dock = DockStyle.Fill };
            resultsPanel.Controls.Add(dgvResults);
            resultsPanel.Controls.Add(lblResultStatus);

            grpResults.Controls.Add(resultsPanel);
        }

        private void CreateHistogramGroup()
        {
            grpHistogram = new GroupBox
            {
                Text = "直方圖",
                Dock = DockStyle.Bottom,
                Height = 120,
                Font = new Font("Microsoft JhengHei", 10f, FontStyle.Bold),
                Padding = new Padding(5)
            };

            pbHistogram = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            grpHistogram.Controls.Add(pbHistogram);
        }

        private void CreateExtrasTabControl()
        {
            tcExtras = new TabControl
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                Font = new Font("Microsoft JhengHei", 9f)
            };

            // 1. ROI Tab
            tpRoi = new TabPage("ROI");
            BuildRoiTab();
            tcExtras.TabPages.Add(tpRoi);

            // 2. Measure Tab
            tpMeasure = new TabPage("測量");
            BuildMeasureTab();
            tcExtras.TabPages.Add(tpMeasure);

            // 3. Navigation Tab
            tpNav = new TabPage("導航");
            BuildNavTab();
            tcExtras.TabPages.Add(tpNav);

            // 4. Attribute Tab
            tpAttr = new TabPage("屬性");
            BuildAttrTab();
            tcExtras.TabPages.Add(tpAttr);
        }

        private void BuildRoiTab()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            int r = 0;
            AddLabelPair(panel, "位置 (X,Y):", out lblRoiPosValue, ref r);
            AddLabelPair(panel, "尺寸 (W,H):", out lblRoiSizeValue, ref r);
            AddLabelPair(panel, "面積:", out lblRoiAreaValue, ref r);
            AddLabelPair(panel, "平均灰階:", out lblRoiMeanValue, ref r);

            btnClearRoi = new Button { Text = "清除 ROI", AutoSize = true };
            btnClearRoi.Click += (s, e) => ClearRoiRequested?.Invoke(this, EventArgs.Empty);
            panel.Controls.Add(btnClearRoi, 1, r++);

            tpRoi.Controls.Add(panel);
        }

        private void BuildMeasureTab()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            int r = 0;
            panel.Controls.Add(new Label { Text = "像素比例 (mm/px):", AutoSize = true }, 0, r);
            txtPixelRatio = new TextBox { Text = "0.010" };
            panel.Controls.Add(txtPixelRatio, 1, r++);

            AddLabelPair(panel, "距離 (px):", out lblDistPx, ref r);
            AddLabelPair(panel, "距離 (mm):", out lblDistMm, ref r);
            AddLabelPair(panel, "角度 (deg):", out lblAngle, ref r);

            tpMeasure.Controls.Add(panel);
        }

        private void BuildNavTab()
        {
            pbNav = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                BackColor = Color.Black
            };
            pbNav.MouseClick +=PbNav_MouseClick;
            tpNav.Controls.Add(pbNav);
        }

        private void BuildAttrTab()
        {
            pgObjectDetail = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                ToolbarVisible = false,
                HelpVisible = false
            };
            tpAttr.Controls.Add(pgObjectDetail);
        }

        private void AddLabelPair(TableLayoutPanel panel, string title, out Label valueLabel, ref int row)
        {
            panel.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            valueLabel = new Label { Text = "-", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, AutoSize = true };
            panel.Controls.Add(valueLabel, 1, row);
            row++;
        }

        private void AddStatRow(TableLayoutPanel panel, string title, out Label lblTitle, out Label lblValue, ref int row)
        {
            lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft JhengHei", 9f)
            };

            lblValue = new Label
            {
                Text = "-",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = Color.Black
            };

            panel.Controls.Add(lblTitle, 0, row);
            panel.Controls.Add(lblValue, 1, row);
            row++;
        }

        /// <summary>更新統計資訊</summary>
        public void UpdateStatistics(Mat image, List<DetectedObject> objects, double processTimeMs)
        {
            if (image == null || image.Empty())
            {
                ClearStatistics();
                return;
            }

            // 影像資訊
            lblImageSizeValue.Text = $"{image.Width} x {image.Height}";
            lblColorModeValue.Text = image.Channels() == 1 ? "Gray" : image.Channels() == 3 ? "BGR" : $"Ch{image.Channels()}";
            lblProcessTimeValue.Text = $"{processTimeMs:F1} ms";

            // 檢測統計
            int total = objects?.Count ?? 0;
            int pass = objects?.Count(o => o.IsPass) ?? 0;
            int fail = total - pass;
            double rate = total > 0 ? (double)pass / total * 100 : 0;

            lblObjectCountValue.Text = total.ToString();
            lblPassCountValue.Text = pass.ToString();
            lblPassCountValue.ForeColor = Color.Green;
            lblFailCountValue.Text = fail.ToString();
            lblFailCountValue.ForeColor = fail > 0 ? Color.Red : Color.Black;
            lblPassRateValue.Text = $"{rate:F1}%";
            lblPassRateValue.ForeColor = rate >= 95 ? Color.Green : rate >= 80 ? Color.Orange : Color.Red;

            // 品質指標
            using (var gray = image.Channels() == 1 ? image.Clone() : new Mat())
            {
                if (image.Channels() > 1)
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                else
                    image.CopyTo(gray);

                Cv2.MeanStdDev(gray, out Scalar mean, out Scalar stddev);
                Cv2.MinMaxLoc(gray, out double minVal, out double maxVal);

                lblMeanGrayValue.Text = $"{mean.Val0:F1}";
                lblStdDevValue.Text = $"{stddev.Val0:F1}";
                lblMinMaxValue.Text = $"{minVal:F0} / {maxVal:F0}";
            }
        }

        private void ClearStatistics()
        {
            lblImageSizeValue.Text = "-";
            lblColorModeValue.Text = "-";
            lblProcessTimeValue.Text = "-";
            lblObjectCountValue.Text = "0";
            lblPassCountValue.Text = "0";
            lblFailCountValue.Text = "0";
            lblPassRateValue.Text = "-";
            lblMeanGrayValue.Text = "-";
            lblStdDevValue.Text = "-";
            lblMinMaxValue.Text = "-";
        }

        /// <summary>綁定檢測結果</summary>
        public void BindDetectionResults(List<DetectedObject> objects)
        {
            _currentObjects = objects ?? new List<DetectedObject>();
            dgvResults.Rows.Clear();

            foreach (var obj in _currentObjects)
            {
                int rowIndex = dgvResults.Rows.Add(
                    obj.Id,
                    obj.Type,
                    obj.CenterX.ToString("F1"),
                    obj.CenterY.ToString("F1"),
                    obj.Radius.ToString("F1"),
                    obj.Area.ToString("F0"),
                    obj.Status
                );

                var row = dgvResults.Rows[rowIndex];
                row.Tag = obj;

                // 根據狀態著色
                if (obj.Status == "OK")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220);
                else if (obj.Status == "NG")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
            }

            lblResultStatus.Text = $"共 {_currentObjects.Count} 項";
        }

        /// <summary>更新直方圖</summary>
        public void UpdateHistogram(Mat image)
        {
            if (image == null || image.Empty() || pbHistogram.Width <= 0 || pbHistogram.Height <= 0)
                return;

            int width = pbHistogram.Width;
            int height = pbHistogram.Height;

            var oldBitmap = _histogramBitmap;
            _histogramBitmap = new Bitmap(width, height);

            using (var g = Graphics.FromImage(_histogramBitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(45, 45, 48));

                // 繪製網格線
                using (var gridPen = new Pen(Color.FromArgb(70, 70, 70), 1) { DashStyle = DashStyle.Dot })
                {
                    for (int i = 50; i < 256; i += 50)
                    {
                        float x = (float)i / 255 * width;
                        g.DrawLine(gridPen, x, 0, x, height);
                    }
                }

                // 計算並繪製直方圖
                if (image.Channels() == 1)
                {
                    DrawHistogramChannel(g, image, 0, Color.White, width, height);
                }
                else
                {
                    // BGR 三通道
                    Cv2.Split(image, out Mat[] ch);
                    DrawHistogramChannel(g, ch[0], 0, Color.FromArgb(180, Color.Blue), width, height);
                    DrawHistogramChannel(g, ch[1], 0, Color.FromArgb(180, Color.Lime), width, height);
                    DrawHistogramChannel(g, ch[2], 0, Color.FromArgb(180, Color.Red), width, height);
                    foreach (var c in ch) c.Dispose();
                }

                // X軸標籤
                using (var font = new Font("Consolas", 7f))
                using (var brush = new SolidBrush(Color.Gray))
                {
                    g.DrawString("0", font, brush, 2, height - 12);
                    g.DrawString("255", font, brush, width - 22, height - 12);
                }
            }

            pbHistogram.Image = _histogramBitmap;
            oldBitmap?.Dispose();
        }

        private void DrawHistogramChannel(Graphics g, Mat channel, int channelIndex, Color color, int width, int height)
        {
            int[] histSize = { 256 };
            Rangef[] ranges = { new Rangef(0, 256) };

            using (var hist = new Mat())
            {
                Cv2.CalcHist(new[] { channel }, new[] { 0 }, null, hist, 1, histSize, ranges);
                Cv2.Normalize(hist, hist, 0, height * 0.85, NormTypes.MinMax);

                var points = new List<PointF>();
                for (int i = 0; i < 256; i++)
                {
                    float x = (float)i / 255 * width;
                    float y = height - hist.At<float>(i);
                    points.Add(new PointF(x, y));
                }

                if (points.Count > 1)
                {
                    using (var pen = new Pen(color, 2))
                    {
                        g.DrawLines(pen, points.ToArray());
                    }
                }
            }
        }

        private void DgvResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _currentObjects.Count)
            {
                ObjectHighlightRequested?.Invoke(this, _currentObjects[e.RowIndex].Id);
            }
        }

        private void DgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _currentObjects.Count)
            {
                var obj = _currentObjects[e.RowIndex];
                ZoomToPointRequested?.Invoke(this, (obj.CenterX, obj.CenterY));
            }
        }

        private void DgvResults_SelectionChanged(object sender, EventArgs e)
        {
            int selected = dgvResults.SelectedRows.Count;
            lblResultStatus.Text = selected > 0
                ? $"已選取 {selected}/{_currentObjects.Count} 項"
                : $"共 {_currentObjects.Count} 項";
            
            if (selected > 0)
            {
                var obj = dgvResults.SelectedRows[0].Tag as DetectedObject;
                pgObjectDetail.SelectedObject = obj;
            }
            else
            {
                pgObjectDetail.SelectedObject = null;
            }
        }

        /// <summary>更新 ROI 資訊</summary>
        /// <summary>更新 ROI 資訊</summary>
        public void UpdateRoiInfo(List<RoiBase> rois, Mat image)
        {
            if (rois == null || rois.Count == 0 || image == null)
            {
                lblRoiPosValue.Text = "-";
                lblRoiSizeValue.Text = "-";
                lblRoiAreaValue.Text = "-";
                lblRoiMeanValue.Text = "-";
                return;
            }

            var roi = rois[0]; // 目前只顯示第一個

            if (roi is RectangleRoi rr)
            {
                lblRoiPosValue.Text = $"({rr.Rect.X:F0}, {rr.Rect.Y:F0})";
                lblRoiSizeValue.Text = $"{rr.Rect.Width:F0} x {rr.Rect.Height:F0}";
                lblRoiAreaValue.Text = $"{rr.Rect.Width * rr.Rect.Height:F0} px²";
            }
            else if (roi is CircleRoi cr)
            {
                lblRoiPosValue.Text = $"({cr.Center.X:F0}, {cr.Center.Y:F0})";
                lblRoiSizeValue.Text = $"R: {cr.Radius:F0}";
                lblRoiAreaValue.Text = $"{Math.PI * cr.Radius * cr.Radius:F0} px²";
            }
            else
            {
                lblRoiPosValue.Text = "Poly/Other";
                lblRoiSizeValue.Text = "-";
                lblRoiAreaValue.Text = "-";
            }

            try
            {
                // 使用 Mask 計算 ROI 內的平均灰階
                using (var mask = roi.GetMask(new OpenCvSharp.Size(image.Width, image.Height)))
                {
                    Scalar mean = Cv2.Mean(image, mask);
                    lblRoiMeanValue.Text = $"{mean.Val0:F1}";
                }
            }
            catch
            {
                lblRoiMeanValue.Text = "Error";
            }
        }

        /// <summary>更新導航縮圖與紅框</summary>
        public void UpdateNavigation(Bitmap fullImage, Rectangle viewport, System.Drawing.Size originalSize)
        {
            if (fullImage == null) return;
            
            // 只有當縮圖不存在或圖片改變時才重新建立
            if (_navBitmap == null || _navBitmap.Size != pbNav.Size) // Resize handled clumsily, ideally check image instance
            {
                _navBitmap?.Dispose();
                _navBitmap = new Bitmap(pbNav.Width, pbNav.Height);
            }

            // 這邊其實不用每次重畫原圖，只要重畫紅框。
            // 為了效能，可以存一個背景縮圖。
            // 簡化實作：每次重畫
            using (var g = Graphics.FromImage(_navBitmap))
            {
                g.Clear(Color.Black);
                // 畫縮圖 - 保持比例
                float scaleX = (float)pbNav.Width / originalSize.Width;
                float scaleY = (float)pbNav.Height / originalSize.Height;
                _navScale = Math.Min(scaleX, scaleY);
                
                int w = (int)(originalSize.Width * _navScale);
                int h = (int)(originalSize.Height * _navScale);
                _navOffsetX = (pbNav.Width - w) / 2;
                _navOffsetY = (pbNav.Height - h) / 2;

                g.DrawImage(fullImage, _navOffsetX, _navOffsetY, w, h);

                // 畫紅框
                if (viewport.Width > 0 && viewport.Height > 0)
                {
                    // Viewport is in original image coordinates
                    float vx = _navOffsetX + viewport.X * _navScale;
                    float vy = _navOffsetY + viewport.Y * _navScale;
                    float vw = viewport.Width * _navScale;
                    float vh = viewport.Height * _navScale;

                    using (var pen = new Pen(Color.Red, 2))
                    {
                        g.DrawRectangle(pen, vx, vy, vw, vh);
                    }
                }
            }
            pbNav.Image = _navBitmap;
            pbNav.Tag = fullImage; // simplified tracking
            pbNav.Invalidate();
        }

        private void PbNav_MouseClick(object sender, MouseEventArgs e)
        {
            if (_navScale <= 0) return;

            // 反算圖片座標
            float imgX = (e.X - _navOffsetX) / _navScale;
            float imgY = (e.Y - _navOffsetY) / _navScale;

            NavigateToRequested?.Invoke(this, (imgX, imgY));
        }

        /// <summary>清除所有資料</summary>
        public void Clear()
        {
            ClearStatistics();
            dgvResults.Rows.Clear();
            _currentObjects.Clear();
            lblResultStatus.Text = "共 0 項";
            
            var oldBitmap = _histogramBitmap;
            _histogramBitmap = null;
            pbHistogram.Image = null;
            oldBitmap?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _histogramBitmap?.Dispose();
                _navBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
