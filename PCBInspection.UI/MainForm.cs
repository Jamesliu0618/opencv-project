using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PCBInspection.Core;
using PCBInspection.Core.Tools;
using PCBInspection.Core.Models;
using PCBInspection.Core.Services;
using PCBInspection.Core.ROI;
using PCBInspection.UI.Controls;
using PCBInspection.UI.Services;

namespace PCBInspection.UI
{
    public partial class MainForm : Form
    {
        private string _currImagePath;
        private List<InspectionItem> _sequence = new List<InspectionItem>();
        private HistoryManager _history = new HistoryManager();
        private Bitmap _originalImage;

        private ContextMenuStrip _sequenceContextMenu;

        private class InspectionItem
        {
            public string Name { get; set; }
            public object Parameters { get; set; }
            public VisionToolFactory.VisionAction Action { get; set; }
            public Bitmap LastResultImage { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            SetupTheme();
            InitializeToolbox();
            LoadToolbarIcons();
            WireEvents();
        }

        private void SetupTheme()
        {
            this.BackColor = SystemColors.Control;
            toolStripMain.ImageScalingSize = new System.Drawing.Size(32, 32);
            toolStripMain.Height = 50; 
        }

        private void LoadToolbarIcons()
        {
            try
            {
                string iconDir = @"d:\Repo\opencv\Resources\Icons";
                if (!Directory.Exists(iconDir)) return;

                btnTsNew.Image = LoadIcon(iconDir, "New");
                btnTsOpen.Image = LoadIcon(iconDir, "Open");
                btnTsSave.Image = LoadIcon(iconDir, "Save");
                
                btnTsRunOnce.Image = LoadIcon(iconDir, "RunOnce");
                btnTsRunLoop.Image = LoadIcon(iconDir, "RunLoop");
                btnTsStop.Image = LoadIcon(iconDir, "Stop");
                btnTsSettings.Image = LoadIcon(iconDir, "Settings");
                
                btnTsZoomIn.Image = LoadIcon(iconDir, "ZoomIn");
                btnTsZoomOut.Image = LoadIcon(iconDir, "ZoomOut");
                btnTsFit.Image = LoadIcon(iconDir, "Fit");
                
                btnTsPointer.Image = LoadIcon(iconDir, "Pointer");
                btnTsRoiRect.Image = LoadIcon(iconDir, "RoiRect");
                btnTsRoiCircle.Image = LoadIcon(iconDir, "RoiCircle");
                btnTsRoiPoly.Image = LoadIcon(iconDir, "RoiPoly");
                
                btnTsUndo.Image = LoadIcon(iconDir, "Undo");
                btnTsRedo.Image = LoadIcon(iconDir, "Redo");

                // Set styles
                foreach (ToolStripItem item in toolStripMain.Items)
                {
                    if (item is ToolStripButton btn)
                    {
                        btn.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                        btn.TextImageRelation = TextImageRelation.ImageBeforeText;
                        btn.Padding = new Padding(5, 0, 5, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"載入圖標失敗: {ex.Message}", TraceLevel.Warning);
            }
        }

        private Image LoadIcon(string dir, string name)
        {
            string path = Path.Combine(dir, $"{name}.png");
            if (File.Exists(path))
            {
                return Image.FromFile(path);
            }
            return null;
        }

        private void InitializeToolbox()
        {
            tvTools.Nodes.Clear();
            var allTools = VisionToolFactory.GetAllTools();

            var grouped = allTools.GroupBy(t => t.Category);

            foreach (var group in grouped)
            {
                var catNode = new TreeNode(group.Key);
                foreach (var tool in group)
                {
                    var toolNode = new TreeNode(tool.Name);
                    toolNode.Tag = tool; 
                    catNode.Nodes.Add(toolNode);
                }
                tvTools.Nodes.Add(catNode);
            }
            tvTools.ExpandAll();
        }

        private void WireEvents()
        {
            // Toolbar - File & Run
            btnTsOpen.Click += (s, e) => LoadImage();
            btnTsRunOnce.Click += (s, e) => RunSequence();
            
            // Toolbar - Zoom
            btnTsZoomIn.Click += (s, e) => imageViewer.ZoomIn();
            btnTsZoomOut.Click += (s, e) => imageViewer.ZoomOut();
            btnTsFit.Click += (s, e) => imageViewer.FitToWindow();
            
            // Toolbar - ROI Modes
            btnTsPointer.Click += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.EditROI;
            btnTsRoiRect.Click += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawRect;
            btnTsRoiCircle.Click += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawCircle;
            btnTsRoiPoly.Click += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawPoly;

            // Toolbar - Undo/Redo
            btnTsUndo.Click += (s, e) => PerformUndo();
            btnTsRedo.Click += (s, e) => PerformRedo();

            // Toolbox
            tvTools.NodeMouseDoubleClick += (s, e) => AddToolToSequence(e.Node);

            // Setup context menu for sequence grid
            SetupSequenceContextMenu();

            // Flow Control
            btnMoveUp.Click += (s, e) => MoveStep(-1);
            btnMoveDown.Click += (s, e) => MoveStep(1);
            btnRemoveTool.Click += (s, e) => RemoveStep();

            // Grid
            dgvSequence.SelectionChanged += DgvSequence_SelectionChanged;

            // Log rendering
            lstLog.DrawItem += LstLog_DrawItem;

            // Viewer Events
            imageViewer.MousePixelChanged += (s, p) => lblStatusMain.Text = $"X: {p.X}, Y: {p.Y}";
            imageViewer.RoiListChanged += (s, args) => 
            {
                _history.PushState(imageViewer.Image, imageViewer.Rois);
                UpdateUndoRedoButtons();
            };
        }

        private void UpdateUndoRedoButtons()
        {
            btnTsUndo.Enabled = _history.CanUndo;
            btnTsRedo.Enabled = _history.CanRedo;
        }

        private void PerformUndo()
        {
            var (img, rois) = _history.Undo(imageViewer.Image, imageViewer.Rois);
            if (img != null)
            {
                imageViewer.Image = img; 
                imageViewer.Rois.Clear();
                imageViewer.Rois.AddRange(rois);
                imageViewer.Invalidate();
            }
            UpdateUndoRedoButtons();
        }

        private void PerformRedo()
        {
             var (img, rois) = _history.Redo(imageViewer.Image, imageViewer.Rois);
            if (img != null)
            {
                imageViewer.Image = img; 
                imageViewer.Rois.Clear();
                imageViewer.Rois.AddRange(rois);
                imageViewer.Invalidate();
            }
            UpdateUndoRedoButtons();
        }

        private void AddToolToSequence(TreeNode node)
        {
            if (node.Tag is VisionToolFactory.ToolDefinition toolDef)
            {
                var item = new InspectionItem
                {
                    Name = toolDef.Name,
                    Action = toolDef.Action,
                    Parameters = Activator.CreateInstance(toolDef.DefaultParameters.GetType()) 
                };
                CopyProperties(toolDef.DefaultParameters, item.Parameters);

                _sequence.Add(item);
                
                int idx = dgvSequence.Rows.Add(item.Name, "", "Wait");
                dgvSequence.Rows[idx].Tag = item;
                dgvSequence.ClearSelection();
                dgvSequence.Rows[idx].Selected = true;
                
                Log($"已加入步驟: {item.Name}", TraceLevel.Info);
            }
        }

        private void CopyProperties(object source, object dest)
        {
            foreach (var prop in source.GetType().GetProperties())
            {
                if (prop.CanWrite)
                    prop.SetValue(dest, prop.GetValue(source));
            }
        }

        private void MoveStep(int direction)
        {
            if (dgvSequence.SelectedRows.Count == 0) return;
            int idx = dgvSequence.SelectedRows[0].Index;
            int newIdx = idx + direction;

            if (newIdx >= 0 && newIdx < dgvSequence.Rows.Count)
            {
                var item = _sequence[idx];
                _sequence.RemoveAt(idx);
                _sequence.Insert(newIdx, item);

                var row = dgvSequence.Rows[idx];
                dgvSequence.Rows.RemoveAt(idx);
                dgvSequence.Rows.Insert(newIdx, row);
                row.Selected = true;
            }
        }

        private void RemoveStep()
        {
            if (dgvSequence.SelectedRows.Count == 0) return;
            int idx = dgvSequence.SelectedRows[0].Index;
            _sequence.RemoveAt(idx);
            dgvSequence.Rows.RemoveAt(idx);
        }

        private void DgvSequence_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSequence.SelectedRows.Count > 0)
            {
                var row = dgvSequence.SelectedRows[0];
                if (row.Tag is InspectionItem item)
                {
                    propertyGrid.SelectedObject = item.Parameters;
                    if (item.LastResultImage != null)
                    {
                        imageViewer.Image = (Bitmap)item.LastResultImage.Clone();
                    }
                }
            }
            else
            {
                propertyGrid.SelectedObject = null;
            }
        }

        private void LoadImage()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _currImagePath = dlg.FileName;
                    using (var mat = Cv2.ImRead(_currImagePath))
                    {
                        var bmp = BitmapConverter.ToBitmap(mat);
                        imageViewer.Image = bmp;

                        // 保存原始圖片副本
                        _originalImage?.Dispose();
                        _originalImage = (Bitmap)bmp.Clone();
                    }
                    Log($"載入影像: {Path.GetFileName(_currImagePath)}", TraceLevel.Info);
                    
                    _history.PushState(imageViewer.Image, imageViewer.Rois);
                    UpdateUndoRedoButtons();
                }
            }
        }

        private void RunSequence()
        {
            if (imageViewer.Image == null)
            {
                Log("請先載入影像", TraceLevel.Error);
                return;
            }

            foreach (DataGridViewRow r in dgvSequence.Rows)
            {
                r.Cells[1].Value = "";
                r.Cells[2].Value = "Wait";
                r.DefaultCellStyle.BackColor = Color.White;
            }

            Mat currentMat = BitmapConverter.ToMat(imageViewer.Image);
            Mat roiMask = null;
            if (imageViewer.Rois.Count > 0)
            {
                roiMask = new Mat(currentMat.Size(), MatType.CV_8UC1, Scalar.All(0));
                foreach(var roi in imageViewer.Rois)
                {
                    using (var subMask = roi.GetMask(currentMat.Size()))
                    {
                         Cv2.BitwiseOr(roiMask, subMask, roiMask);
                    }
                }
            }

            Log("開始執行流程...", TraceLevel.Info);
            var swTotal = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                for (int i = 0; i < _sequence.Count; i++)
                {
                    var item = _sequence[i];
                    var row = dgvSequence.Rows[i];

                    row.Cells[2].Value = "Running...";
                    Application.DoEvents();

                    var swStep = System.Diagnostics.Stopwatch.StartNew();
                    
                    try
                    {
                        Mat inputToTool = currentMat;
                        Mat maskedInput = null;

                        if (roiMask != null)
                        {
                            maskedInput = new Mat();
                            currentMat.CopyTo(maskedInput, roiMask);
                            inputToTool = maskedInput;
                        }

                        var result = item.Action(inputToTool, item.Parameters);
                        
                        swStep.Stop();
                        row.Cells[1].Value = $"{swStep.ElapsedMilliseconds}ms";

                        if (result.IsOk)
                        {
                            row.Cells[2].Value = "OK";
                            row.Cells[2].Style.ForeColor = Color.Green;
                            
                            item.LastResultImage?.Dispose();
                            item.LastResultImage = BitmapConverter.ToBitmap(result.ResultImage);

                            currentMat.Dispose();
                            currentMat = result.ResultImage; 
                        }
                        else
                        {
                            row.Cells[2].Value = "NG";
                            row.Cells[2].Style.ForeColor = Color.Red;
                            result.ResultImage?.Dispose();
                            item.LastResultImage = null; 
                        }

                        if (row.Selected)
                        {
                            imageViewer.Image = (Bitmap)item.LastResultImage.Clone();
                        }
                        
                        maskedInput?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        row.Cells[2].Value = "ERR";
                        row.Cells[2].Style.ForeColor = Color.Red;
                        Log($"步驟 {item.Name} 發生錯誤: {ex.Message}", TraceLevel.Error);
                        break; 
                    }
                }
            }
            finally
            {
                swTotal.Stop();
                lblStatusTime.Text = $"總耗時: {swTotal.ElapsedMilliseconds}ms";
                
                if (_sequence.Count > 0)
                {
                    var lastItem = _sequence.Last();
                    if (lastItem.LastResultImage != null)
                    {
                        imageViewer.Image = (Bitmap)lastItem.LastResultImage.Clone();
                    }
                }
                
                currentMat?.Dispose();
                roiMask?.Dispose();
            }
        }

        private enum TraceLevel { Info, Warning, Error }

        private void Log(string msg, TraceLevel level)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string fullMsg = $"[{timestamp}] {msg}";
            lstLog.Items.Add(new LogItem { Message = fullMsg, Level = level });
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private class LogItem
        {
            public string Message { get; set; }
            public TraceLevel Level { get; set; }
        }

        private void LstLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();

            var item = (LogItem)lstLog.Items[e.Index];
            Color color = Color.Black;
            if (item.Level == TraceLevel.Error) color = Color.DarkRed;
            if (item.Level == TraceLevel.Info) color = Color.Green;

            using (var brush = new SolidBrush(color))
            {
                e.Graphics.DrawString(item.Message, e.Font, brush, e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        /// <summary>設置序列表格的右鍵選單</summary>
        private void SetupSequenceContextMenu()
        {
            _sequenceContextMenu = new ContextMenuStrip();

            var menuRunThis = new ToolStripMenuItem("執行此步驟");
            menuRunThis.Click += (s, e) =>
            {
                if (dgvSequence.SelectedRows.Count > 0)
                    RunSingleStep(dgvSequence.SelectedRows[0].Index);
            };

            var menuRunFrom = new ToolStripMenuItem("從此步驟開始執行");
            menuRunFrom.Click += (s, e) =>
            {
                if (dgvSequence.SelectedRows.Count > 0)
                    RunFromStep(dgvSequence.SelectedRows[0].Index);
            };

            _sequenceContextMenu.Items.Add(menuRunThis);
            _sequenceContextMenu.Items.Add(menuRunFrom);
            _sequenceContextMenu.Items.Add(new ToolStripSeparator());

            var menuResetImage = new ToolStripMenuItem("重設為原始影像");
            menuResetImage.Click += (s, e) => ResetToOriginalImage();
            _sequenceContextMenu.Items.Add(menuResetImage);

            dgvSequence.ContextMenuStrip = _sequenceContextMenu;
        }

        /// <summary>重設為原始影像</summary>
        private void ResetToOriginalImage()
        {
            if (_originalImage == null)
            {
                Log("沒有原始影像可以重設", TraceLevel.Warning);
                return;
            }

            imageViewer.Image = (Bitmap)_originalImage.Clone();
            Log("已重設為原始影像", TraceLevel.Info);

            // 清除所有步驟的執行結果
            foreach (DataGridViewRow r in dgvSequence.Rows)
            {
                r.Cells[1].Value = "";
                r.Cells[2].Value = "Wait";
                r.DefaultCellStyle.BackColor = Color.White;
            }
        }

        /// <summary>執行單一步驟並將結果繪製在影像上</summary>
        private void RunSingleStep(int stepIndex)
        {
            if (_originalImage == null)
            {
                Log("請先載入影像", TraceLevel.Error);
                return;
            }

            if (stepIndex < 0 || stepIndex >= _sequence.Count)
                return;

            var item = _sequence[stepIndex];
            var row = dgvSequence.Rows[stepIndex];

            // 使用原始影像來執行此步驟
            Mat inputMat = BitmapConverter.ToMat(_originalImage);
            Mat roiMask = null;

            try
            {
                if (imageViewer.Rois.Count > 0)
                {
                    roiMask = new Mat(inputMat.Size(), MatType.CV_8UC1, Scalar.All(0));
                    foreach (var roi in imageViewer.Rois)
                    {
                        using (var subMask = roi.GetMask(inputMat.Size()))
                        {
                            Cv2.BitwiseOr(roiMask, subMask, roiMask);
                        }
                    }
                }

                row.Cells[2].Value = "Running...";
                Application.DoEvents();

                var swStep = System.Diagnostics.Stopwatch.StartNew();

                Mat toolInput = inputMat;
                Mat maskedInput = null;

                if (roiMask != null)
                {
                    maskedInput = new Mat();
                    inputMat.CopyTo(maskedInput, roiMask);
                    toolInput = maskedInput;
                }

                var result = item.Action(toolInput, item.Parameters);

                swStep.Stop();
                row.Cells[1].Value = $"{swStep.ElapsedMilliseconds}ms";

                if (result.IsOk)
                {
                    row.Cells[2].Value = "OK";
                    row.Cells[2].Style.ForeColor = Color.Green;

                    item.LastResultImage?.Dispose();
                    item.LastResultImage = BitmapConverter.ToBitmap(result.ResultImage);

                    // 在原始影像上疊加結果繪製
                    imageViewer.Image = (Bitmap)item.LastResultImage.Clone();

                    result.ResultImage?.Dispose();
                }
                else
                {
                    row.Cells[2].Value = "NG";
                    row.Cells[2].Style.ForeColor = Color.Red;
                    result.ResultImage?.Dispose();
                }

                maskedInput?.Dispose();
                Log($"執行步驟: {item.Name} - {row.Cells[2].Value}", TraceLevel.Info);
            }
            catch (Exception ex)
            {
                row.Cells[2].Value = "ERR";
                row.Cells[2].Style.ForeColor = Color.Red;
                Log($"步驟 {item.Name} 發生錯誤: {ex.Message}", TraceLevel.Error);
            }
            finally
            {
                inputMat?.Dispose();
                roiMask?.Dispose();
            }
        }

        /// <summary>從指定步驟開始執行到結束</summary>
        private void RunFromStep(int startIndex)
        {
            if (_originalImage == null)
            {
                Log("請先載入影像", TraceLevel.Error);
                return;
            }

            if (startIndex < 0 || startIndex >= _sequence.Count)
                return;

            // 重設從 startIndex 開始的狀態
            for (int i = startIndex; i < dgvSequence.Rows.Count; i++)
            {
                var r = dgvSequence.Rows[i];
                r.Cells[1].Value = "";
                r.Cells[2].Value = "Wait";
                r.DefaultCellStyle.BackColor = Color.White;
            }

            Mat currentMat = BitmapConverter.ToMat(_originalImage);
            Mat roiMask = null;

            if (imageViewer.Rois.Count > 0)
            {
                roiMask = new Mat(currentMat.Size(), MatType.CV_8UC1, Scalar.All(0));
                foreach (var roi in imageViewer.Rois)
                {
                    using (var subMask = roi.GetMask(currentMat.Size()))
                    {
                        Cv2.BitwiseOr(roiMask, subMask, roiMask);
                    }
                }
            }

            Log($"從步驟 {startIndex + 1} 開始執行...", TraceLevel.Info);
            var swTotal = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                for (int i = startIndex; i < _sequence.Count; i++)
                {
                    var item = _sequence[i];
                    var row = dgvSequence.Rows[i];

                    row.Cells[2].Value = "Running...";
                    Application.DoEvents();

                    var swStep = System.Diagnostics.Stopwatch.StartNew();

                    try
                    {
                        Mat inputToTool = currentMat;
                        Mat maskedInput = null;

                        if (roiMask != null)
                        {
                            maskedInput = new Mat();
                            currentMat.CopyTo(maskedInput, roiMask);
                            inputToTool = maskedInput;
                        }

                        var result = item.Action(inputToTool, item.Parameters);

                        swStep.Stop();
                        row.Cells[1].Value = $"{swStep.ElapsedMilliseconds}ms";

                        if (result.IsOk)
                        {
                            row.Cells[2].Value = "OK";
                            row.Cells[2].Style.ForeColor = Color.Green;

                            item.LastResultImage?.Dispose();
                            item.LastResultImage = BitmapConverter.ToBitmap(result.ResultImage);

                            currentMat.Dispose();
                            currentMat = result.ResultImage;
                        }
                        else
                        {
                            row.Cells[2].Value = "NG";
                            row.Cells[2].Style.ForeColor = Color.Red;
                            result.ResultImage?.Dispose();
                            item.LastResultImage = null;
                        }

                        if (row.Selected)
                        {
                            imageViewer.Image = (Bitmap)item.LastResultImage.Clone();
                        }

                        maskedInput?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        row.Cells[2].Value = "ERR";
                        row.Cells[2].Style.ForeColor = Color.Red;
                        Log($"步驟 {item.Name} 發生錯誤: {ex.Message}", TraceLevel.Error);
                        break;
                    }
                }
            }
            finally
            {
                swTotal.Stop();
                lblStatusTime.Text = $"總耗時: {swTotal.ElapsedMilliseconds}ms";

                if (_sequence.Count > startIndex)
                {
                    var lastItem = _sequence.Last();
                    if (lastItem.LastResultImage != null)
                    {
                        imageViewer.Image = (Bitmap)lastItem.LastResultImage.Clone();
                    }
                }

                currentMat?.Dispose();
                roiMask?.Dispose();
            }
        }
    }
}
