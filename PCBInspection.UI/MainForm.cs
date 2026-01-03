using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PCBInspection.Core;
using PCBInspection.Core.Detectors;
using PCBInspection.Core.Models;
using PCBInspection.Core.Services;

namespace PCBInspection.UI
{
    public partial class MainForm : Form
    {
        private string _currentImagePath;
        private InspectionResult _lastResult;
        private Bitmap _originalImage;
        private Bitmap _currentDisplayImage;

        // 滑鼠圈選放大相關變數
        private bool _isSelecting;
        private System.Drawing.Point _selectionStart;
        private Rectangle _selectionRect;
        private double _currentZoom = 1.0;

        // 檢測項目
        private class InspectionItem
        {
            public string Name { get; set; }
            public object Parameters { get; set; }
            // 修改 Action 簽名以接受 Parameters
            public Func<Mat, List<Component>, object, (bool IsOk, List<Defect> Defects)> Action { get; set; }

            public override string ToString() => Name;
        }

        private readonly List<InspectionItem> _availableItems = new List<InspectionItem>();

        public MainForm()
        {
            InitializeComponent();
            InitializeInspectionItems();
            WireEvents();
            SetupInitialState();
        }

        private void InitializeInspectionItems()
        {
            // 定義可用檢測項目與預設參數
            _availableItems.Add(new InspectionItem
            {
                Name = "零件定位與測量",
                Parameters = new LocalizationOptions(), // 使用 Core 中的 Options
                Action = (image, comps, param) =>
                {
                    var opts = (LocalizationOptions)param;
                    var components = Localization.DetectComponents(image, opts);
                    if (components.Count == 0) return (false, new List<Defect>());
                    
                    comps.Clear();
                    comps.AddRange(components);
                    foreach (var c in comps)
                    {
                        var (w, h) = Measurement.ComponentSizeMm(c);
                        c.WidthMm = w;
                        c.HeightMm = h;
                    }
                    return (true, new List<Defect>());
                }
            });

            _availableItems.Add(new InspectionItem
            {
                Name = "焊點瑕疵 (冷焊/短路)",
                Parameters = new SolderDefectParameters(),
                Action = (image, comps, param) =>
                {
                    var p = (SolderDefectParameters)param;
                    // 使用參數重建 Detector
                    var detector = new SolderDefectDetector(p.MinArea, p.MaxArea, p.MorphKernel);
                    return RunDetector(detector, image, comps);
                }
            });

            _availableItems.Add(new InspectionItem
            {
                Name = "表面瑕疵 (刮痕/污染)",
                Parameters = new SurfaceDefectParameters(),
                Action = (image, comps, param) =>
                {
                    var p = (SurfaceDefectParameters)param;
                    var detector = new SurfaceDefectDetector(p.BlurSize, p.Threshold, p.MinDefectArea);
                    return RunDetector(detector, image, comps);
                }
            });

            _availableItems.Add(new InspectionItem
            {
                Name = "電路異常 (斷路/橋接)",
                Parameters = new CircuitDefectParameters(),
                Action = (image, comps, param) =>
                {
                    var p = (CircuitDefectParameters)param;
                    var detector = new CircuitDefectDetector(p.MinTraceWidth, p.MaxGapWidth);
                    return RunDetector(detector, image, comps);
                }
            });
        }

        private (bool, List<Defect>) RunDetector(IDefectDetector detector, Mat image, List<Component> comps)
        {
            var defects = detector.Detect(image, comps);
            // 嚴重度 >= 4 視為 Fail
            var isOk = !defects.Any(d => d.Severity >= 4);
            return (isOk, defects);
        }

        private void WireEvents()
        {
            btnLoadImage.Click += BtnLoadImage_Click;
            btnSingleShot.Click += BtnSingleShot_Click;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnCalibration.Click += BtnCalibration_Click;
            btnZoomReset.Click += BtnZoomReset_Click;
            lstDefects.SelectedIndexChanged += LstDefects_SelectedIndexChanged;

            // 列表操作事件
            btnAdd.Click += (s, e) => MoveItem(lstAvailable, lstSequence);
            btnRemove.Click += (s, e) => MoveItem(lstSequence, lstAvailable);
            btnMoveUp.Click += (s, e) => MoveItemUp(lstSequence);
            btnMoveDown.Click += (s, e) => MoveItemDown(lstSequence);

            // 清單選擇變更事件 - 更新 PropertyGrid
            lstSequence.SelectedIndexChanged += LstInspection_SelectedIndexChanged;
            lstAvailable.SelectedIndexChanged += LstInspection_SelectedIndexChanged;

            // 滑鼠圈選放大事件
            picPreview.MouseDown += PicPreview_MouseDown;
            picPreview.MouseMove += PicPreview_MouseMove;
            picPreview.MouseUp += PicPreview_MouseUp;
            picPreview.Paint += PicPreview_Paint;
        }

        private void SetupInitialState()
        {
            Calibration.SetManualScale(10.0);
            
            // 初始化清單
            foreach (var item in _availableItems)
            {
                lstAvailable.Items.Add(item);
            }
            // 預設全部加入順序
            foreach (var item in _availableItems)
            {
                lstSequence.Items.Add(item);
            }
            lstAvailable.Items.Clear();

            UpdateStatus("就緒 - 請配置檢測順序與參數");
            UpdateZoomDisplay();
        }

        #region 清單操作應用

        private void MoveItem(ListBox source, ListBox dest)
        {
            if (source.SelectedItem is InspectionItem item)
            {
                dest.Items.Add(item);
                source.Items.Remove(item);
                // 更新選擇以觸發 PropertyGrid 更新
                dest.SelectedItem = item;
            }
        }

        private void MoveItemUp(ListBox list)
        {
            int index = list.SelectedIndex;
            if (index > 0)
            {
                var item = list.SelectedItem;
                list.Items.RemoveAt(index);
                list.Items.Insert(index - 1, item);
                list.SelectedIndex = index - 1;
            }
        }

        private void MoveItemDown(ListBox list)
        {
            int index = list.SelectedIndex;
            if (index >= 0 && index < list.Items.Count - 1)
            {
                var item = list.SelectedItem;
                list.Items.RemoveAt(index);
                list.Items.Insert(index + 1, item);
                list.SelectedIndex = index + 1;
            }
        }

        private void LstInspection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ListBox list && list.SelectedItem is InspectionItem item)
            {
                propertyGrid.SelectedObject = item.Parameters;
            }
            else
            {
                propertyGrid.SelectedObject = null;
            }
        }

        #endregion

        #region 滑鼠圈選放大功能

        private void PicPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _currentDisplayImage != null)
            {
                _isSelecting = true;
                _selectionStart = e.Location;
                _selectionRect = new Rectangle(e.X, e.Y, 0, 0);
            }
            else if (e.Button == MouseButtons.Right)
            {
                ResetZoom();
            }
        }

        private void PicPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                int x = Math.Min(_selectionStart.X, e.X);
                int y = Math.Min(_selectionStart.Y, e.Y);
                int w = Math.Abs(e.X - _selectionStart.X);
                int h = Math.Abs(e.Y - _selectionStart.Y);
                _selectionRect = new Rectangle(x, y, w, h);
                picPreview.Invalidate();
            }
        }

        private void PicPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting && e.Button == MouseButtons.Left)
            {
                _isSelecting = false;

                if (_selectionRect.Width > 10 && _selectionRect.Height > 10)
                {
                    ZoomToRegion(_selectionRect);
                }
                _selectionRect = Rectangle.Empty;
                picPreview.Invalidate();
            }
        }

        private void PicPreview_Paint(object sender, PaintEventArgs e)
        {
            if (_isSelecting && _selectionRect.Width > 0 && _selectionRect.Height > 0)
            {
                using (var pen = new Pen(Color.Lime, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, _selectionRect);
                }
                using (var brush = new SolidBrush(Color.FromArgb(40, Color.Lime)))
                {
                    e.Graphics.FillRectangle(brush, _selectionRect);
                }
            }
        }

        private void ZoomToRegion(Rectangle selectionRect)
        {
            if (_originalImage == null) return;
            var picRect = GetImageDisplayRectangle();
            if (picRect.Width <= 0 || picRect.Height <= 0) return;

            double scaleX = (double)_currentDisplayImage.Width / picRect.Width;
            double scaleY = (double)_currentDisplayImage.Height / picRect.Height;

            int imgX = (int)((selectionRect.X - picRect.X) * scaleX);
            int imgY = (int)((selectionRect.Y - picRect.Y) * scaleY);
            int imgW = (int)(selectionRect.Width * scaleX);
            int imgH = (int)(selectionRect.Height * scaleY);

            imgX = Math.Max(0, Math.Min(imgX, _currentDisplayImage.Width - 1));
            imgY = Math.Max(0, Math.Min(imgY, _currentDisplayImage.Height - 1));
            imgW = Math.Min(imgW, _currentDisplayImage.Width - imgX);
            imgH = Math.Min(imgH, _currentDisplayImage.Height - imgY);

            if (imgW < 10 || imgH < 10) return;

            var cropRect = new Rectangle(imgX, imgY, imgW, imgH);
            var croppedImage = _currentDisplayImage.Clone(cropRect, _currentDisplayImage.PixelFormat);

            picPreview.Image?.Dispose();
            picPreview.Image = croppedImage;
            _currentDisplayImage = croppedImage;

            _currentZoom *= (double)_originalImage.Width / imgW;
            UpdateZoomDisplay();
            UpdateStatus($"已放大至 {_currentZoom:F1}x | 右鍵回復原圖");
        }

        private Rectangle GetImageDisplayRectangle()
        {
            if (picPreview.Image == null) return Rectangle.Empty;
            var imgSize = picPreview.Image.Size;
            var boxSize = picPreview.ClientSize;

            double imgRatio = (double)imgSize.Width / imgSize.Height;
            double boxRatio = (double)boxSize.Width / boxSize.Height;

            int displayW, displayH, displayX, displayY;

            if (imgRatio > boxRatio)
            {
                displayW = boxSize.Width;
                displayH = (int)(boxSize.Width / imgRatio);
                displayX = 0;
                displayY = (boxSize.Height - displayH) / 2;
            }
            else
            {
                displayH = boxSize.Height;
                displayW = (int)(boxSize.Height * imgRatio);
                displayX = (boxSize.Width - displayW) / 2;
                displayY = 0;
            }
            return new Rectangle(displayX, displayY, displayW, displayH);
        }

        private void ResetZoom()
        {
            if (_originalImage == null) return;
            picPreview.Image?.Dispose();
            _currentDisplayImage = (Bitmap)_originalImage.Clone();
            picPreview.Image = _currentDisplayImage;
            _currentZoom = 1.0;
            UpdateZoomDisplay();
            UpdateStatus("已回復原始大小");
        }

        private void BtnZoomReset_Click(object sender, EventArgs e) => ResetZoom();

        private void UpdateZoomDisplay()
        {
            lblZoomLevel.Text = $"縮放: {_currentZoom * 100:F0}%";
            toolStripZoomLabel.Text = $"| 縮放: {_currentZoom * 100:F0}%";
        }

        #endregion

        #region 影像載入與顯示

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "影像檔案|*.png;*.jpg;*.bmp|所有檔案|*.*";
                dlg.Title = "選擇 PCB 影像";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _currentImagePath = dlg.FileName;
                    txtImagePath.Text = _currentImagePath;
                    LoadAndDisplayImage(_currentImagePath);
                }
            }
        }

        private void LoadAndDisplayImage(string path)
        {
            try
            {
                using (var mat = Cv2.ImRead(path))
                {
                    if (mat.Empty())
                    {
                        MessageBox.Show("無法讀取影像檔案", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _originalImage?.Dispose();
                    _currentDisplayImage?.Dispose();
                    picPreview.Image?.Dispose();
                    _originalImage = BitmapConverter.ToBitmap(mat);
                    _currentDisplayImage = (Bitmap)_originalImage.Clone();
                    picPreview.Image = _currentDisplayImage;
                    _currentZoom = 1.0;
                    UpdateZoomDisplay();
                    UpdateStatus($"已載入影像: {Path.GetFileName(path)}");
                    SetResultDisplay("待機中", Color.Gray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入影像失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 檢測功能 (Fail-Fast with Parameters)

        private void BtnSingleShot_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentImagePath) || !File.Exists(_currentImagePath))
            {
                MessageBox.Show("請先載入影像檔案", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RunInspection(_currentImagePath);
        }

        private void RunInspection(string imagePath)
        {
            try
            {
                UpdateStatus("檢測中...");
                btnSingleShot.Enabled = false;
                Application.DoEvents();

                using (var image = Cv2.ImRead(imagePath))
                {
                    if (image.Empty())
                    {
                        MessageBox.Show("無法讀取影像", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var components = new List<Component>();
                    var allDefects = new List<Defect>();

                    bool globalFail = false;
                    string failReason = "";
                    InspectionDecision finalDecision = InspectionDecision.OK;

                    // 依序執行列表中的項目
                    foreach (InspectionItem item in lstSequence.Items)
                    {
                        UpdateStatus($"正在執行: {item.Name}...");
                        Application.DoEvents();

                        try
                        {
                            // 傳遞參數給 Action
                            var (isOk, defects) = item.Action(image, components, item.Parameters);
                            allDefects.AddRange(defects);

                            if (!isOk)
                            {
                                globalFail = true;
                                failReason = $"{item.Name} failed";
                                finalDecision = InspectionDecision.NG;
                                UpdateStatus($"檢測中斷: {item.Name} 未通過");
                                break; // Fail-Fast
                            }
                        }
                        catch (Exception ex)
                        {
                            globalFail = true;
                            failReason = $"Exception in {item.Name}: {ex.Message}";
                            finalDecision = InspectionDecision.NG;
                            break;
                        }
                    }

                    // 標註與顯示
                    using (var annotated = AnnotationService.Annotate(image, components, allDefects, finalDecision))
                    {
                        sw.Stop();
                        _originalImage?.Dispose();
                        _currentDisplayImage?.Dispose();
                        picPreview.Image?.Dispose();
                        _originalImage = BitmapConverter.ToBitmap(annotated);
                        _currentDisplayImage = (Bitmap)_originalImage.Clone();
                        picPreview.Image = _currentDisplayImage;
                        _currentZoom = 1.0;
                        UpdateZoomDisplay();

                        _lastResult = new InspectionResult
                        {
                            PcbId = Path.GetFileNameWithoutExtension(imagePath),
                            Ok = !globalFail,
                            ProcessingTimeMs = (int)sw.ElapsedMilliseconds
                        };
                        _lastResult.Components.AddRange(components);
                        _lastResult.Defects.AddRange(allDefects);

                        UpdateInspectionInfo(_lastResult, finalDecision, failReason);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"檢測失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetResultDisplay("錯誤", Color.Orange);
            }
            finally
            {
                btnSingleShot.Enabled = true;
            }
        }

        private void UpdateInspectionInfo(InspectionResult result, InspectionDecision decision, string failReason = "")
        {
            lblComponentCount.Text = $"零件數量: {result.Components.Count}";
            lblDefectCount.Text = $"瑕疵數量: {result.Defects.Count}";
            lblProcessTime.Text = $"處理時間: {result.ProcessingTimeMs} ms";

            lstDefects.Items.Clear();
            if (!string.IsNullOrEmpty(failReason))
            {
                lstDefects.Items.Add($"❌ {failReason}");
            }
            foreach (var defect in result.Defects)
            {
                lstDefects.Items.Add($"[S{defect.Severity}] {defect.Type} - Conf: {defect.Confidence:P0}");
            }

            switch (decision)
            {
                case InspectionDecision.OK:
                    SetResultDisplay("OK", Color.FromArgb(0, 180, 0));
                    break;
                case InspectionDecision.NG:
                    SetResultDisplay("NG", Color.FromArgb(220, 0, 0));
                    break;
                case InspectionDecision.REVIEW:
                    SetResultDisplay("REVIEW", Color.FromArgb(200, 180, 0));
                    break;
            }

            if (string.IsNullOrEmpty(failReason))
                UpdateStatus($"檢測完成 - {decision}");
            else
                UpdateStatus($"檢測失敗 - {failReason}");
        }

        private void SetResultDisplay(string text, Color bgColor)
        {
            lblResult.Text = text;
            lblResult.BackColor = bgColor;
        }

        // 預留事件處理
        private void BtnStart_Click(object sender, EventArgs e) { }
        private void BtnStop_Click(object sender, EventArgs e) { }
        private void BtnCalibration_Click(object sender, EventArgs e) { using (var w = new CalibrationWizardForm()) w.ShowDialog(this); }
        private void LstDefects_SelectedIndexChanged(object sender, EventArgs e) { }
        private void UpdateStatus(string message) => toolStripStatusLabel.Text = message;

        #endregion
    }
}
