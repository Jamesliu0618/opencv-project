using OpenCvSharp;
using OpenCvSharp.Extensions;
using PCBInspection.Core.Models;
using PCBInspection.Core.ROI;
using PCBInspection.Core.Services;
using PCBInspection.UI.Controls;
using PCBInspection.UI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Size = System.Drawing.Size;

namespace PCBInspection.UI
{
	public partial class MainForm : Form
	{
		private readonly HistoryManager       _history  = new HistoryManager();
		private readonly List<InspectionItem> _sequence = new List<InspectionItem>();
		private          string               _currImagePath;

		private List<DetectedObject> _lastDetectedObjects = new List<DetectedObject>();
		private Bitmap               _originalImage;

		private ContextMenuStrip _sequenceContextMenu;

		public MainForm()
		{
			InitializeComponent();
			SetupTheme();
			InitializeToolbox();
			LoadToolbarIcons();
			WireEvents();
			Load += (s, e) => ForceLayoutFix();
		}

		private void ForceLayoutFix()
		{
			// 強制修正 Layout 問題
			if(imageViewer != null && splitContainerCenterRight != null)
			{
				// 1. 強制重設父容器 (解決被 orphaned panel 搶走的問題)
				if(imageViewer.Parent != splitContainerCenterRight.Panel1)
				{
					Log($"[LayoutFix] Reparenting ImageViewer from {imageViewer.Parent?.Name ?? "null"} to SplitContainer.Panel1", TraceLevel.Warning);
					splitContainerCenterRight.Panel1.Controls.Add(imageViewer);
				}

				// 2. 設定 Dock 與 Z-Order
				imageViewer.Dock  = DockStyle.Fill;
				thumbnailBar.Dock = DockStyle.Bottom;

				// 確保順序: ThumbnailBar (Bottom) 優先，ImageViewer (Fill) 其次
				thumbnailBar.BringToFront();
				Log("[LayoutFix] Forced Dock=Fill for ImageViewer and Dock=Bottom for ThumbnailBar", TraceLevel.Info);
			}
		}

		private void DumpLayoutInfo()
		{
			try
			{
				var sb = new StringBuilder();
				sb.AppendLine("=== Layout Debug Info ===");
				sb.AppendLine($"MainForm: {Size}, State={WindowState}");

				if(splitContainerMain != null)
				{
					sb.AppendLine($"SplitMain: {splitContainerMain.Size}, P1={splitContainerMain.Panel1.Size}, P2={splitContainerMain.Panel2.Size}, Splitter={splitContainerMain.SplitterDistance}");
				}

				if(splitContainerCenterRight != null)
				{
					var p1 = splitContainerCenterRight.Panel1;
					sb.AppendLine($"SplitRight: {splitContainerCenterRight.Size}, P1={p1.Size}, P1.Controls={p1.Controls.Count}");

					foreach(Control c in p1.Controls)
					{
						sb.AppendLine($"  - {c.Name}: Type={c.GetType().Name}, Size={c.Size}, Dock={c.Dock}, Visible={c.Visible}, Index={p1.Controls.GetChildIndex(c)}");
					}
				}
				Log(sb.ToString(), TraceLevel.Info);
			}
			catch(Exception ex)
			{
				Log($"DumpLayoutInfo Error: {ex.Message}", TraceLevel.Error);
			}
		}

		private void SetupTheme()
		{
			BackColor                      = SystemColors.Control;
			toolStripMain.ImageScalingSize = new Size(32, 32);
			toolStripMain.Height           = 50;
		}

		private void LoadToolbarIcons()
		{
			try
			{
				string iconDir = @"d:\Repo\opencv\Resources\Icons";

				if(!Directory.Exists(iconDir))
				{
					return;
				}
				btnTsNew.Image       = LoadIcon(iconDir, "New");
				btnTsOpen.Image      = LoadIcon(iconDir, "Open");
				btnTsSave.Image      = LoadIcon(iconDir, "Save");
				btnTsRunOnce.Image   = LoadIcon(iconDir, "RunOnce");
				btnTsRunLoop.Image   = LoadIcon(iconDir, "RunLoop");
				btnTsStop.Image      = LoadIcon(iconDir, "Stop");
				btnTsSettings.Image  = LoadIcon(iconDir, "Settings");
				btnTsZoomIn.Image    = LoadIcon(iconDir, "ZoomIn");
				btnTsZoomOut.Image   = LoadIcon(iconDir, "ZoomOut");
				btnTsFit.Image       = LoadIcon(iconDir, "Fit");
				btnTsPointer.Image   = LoadIcon(iconDir, "Pointer");
				btnTsRoiRect.Image   = LoadIcon(iconDir, "RoiRect");
				btnTsRoiCircle.Image = LoadIcon(iconDir, "RoiCircle");
				btnTsRoiPoly.Image   = LoadIcon(iconDir, "RoiPoly");
				btnTsUndo.Image      = LoadIcon(iconDir, "Undo");
				btnTsRedo.Image      = LoadIcon(iconDir, "Redo");

				// Set styles
				foreach(ToolStripItem item in toolStripMain.Items)
				{
					if(item is ToolStripButton btn)
					{
						btn.DisplayStyle      = ToolStripItemDisplayStyle.ImageAndText;
						btn.TextImageRelation = TextImageRelation.ImageBeforeText;
						btn.Padding           = new Padding(5, 0, 5, 0);
					}
				}
			}
			catch(Exception ex)
			{
				Log($"載入圖標失敗: {ex.Message}", TraceLevel.Warning);
			}
		}

		private Image LoadIcon(string dir, string name)
		{
			string path = Path.Combine(dir, $"{name}.png");

			if(File.Exists(path))
			{
				return Image.FromFile(path);
			}
			return null;
		}

		private void InitializeToolbox()
		{
			tvTools.Nodes.Clear();
			var allTools = VisionToolFactory.GetAllTools();
			var grouped  = allTools.GroupBy(t => t.Category);

			foreach(var group in grouped)
			{
				var catNode = new TreeNode(group.Key);

				foreach(var tool in group)
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
			// Global Vision Logging
			VisionToolFactory.OnLog += (msg, isError) =>
			{
				if(InvokeRequired)
				{
					Invoke(new Action(() => Log(msg, isError ? TraceLevel.Error : TraceLevel.Info)));
				}
				else
				{
					Log(msg, isError ? TraceLevel.Error : TraceLevel.Info);
				}
			};

			// Toolbar - File & Run
			btnTsOpen.Click    += (s, e) => LoadImage();
			btnTsRunOnce.Click += (s, e) => RunSequence();

			// Toolbar - Zoom
			btnTsZoomIn.Click  += (s, e) => imageViewer.ZoomIn();
			btnTsZoomOut.Click += (s, e) => imageViewer.ZoomOut();
			btnTsFit.Click     += (s, e) => imageViewer.FitToWindow();

			// Toolbar - ROI Modes
			btnTsPointer.Click   += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.EditROI;
			btnTsRoiRect.Click   += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawRect;
			btnTsRoiCircle.Click += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawCircle;
			btnTsRoiPoly.Click   += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawPoly;

			// Toolbar - Undo/Redo
			btnTsUndo.Click += (s, e) => PerformUndo();
			btnTsRedo.Click += (s, e) => PerformRedo();

			// Toolbox
			tvTools.NodeMouseDoubleClick += (s, e) => AddToolToSequence(e.Node);

			// Setup context menu for sequence grid
			SetupSequenceContextMenu();

			// Flow Control
			btnMoveUp.Click     += (s, e) => MoveStep(-1);
			btnMoveDown.Click   += (s, e) => MoveStep(1);
			btnRemoveTool.Click += (s, e) => RemoveStep();

			// Grid
			dgvSequence.SelectionChanged += DgvSequence_SelectionChanged;
			dgvSequence.CellContentClick += DgvSequence_CellContentClick;

			// Log rendering
			lstLog.DrawItem += LstLog_DrawItem;

			// Viewer Events
			imageViewer.MousePixelChanged += (s, p) => lblStatusMain.Text = $"X: {p.X}, Y: {p.Y}";

			imageViewer.RoiListChanged += (s, args) =>
			{
				_history.PushState(imageViewer.Image, imageViewer.Rois);
				UpdateUndoRedoButtons();

				// 更新 InfoPanel ROI 資訊
				if(imageViewer.Image != null)
				{
					using(var mat = imageViewer.Image.ToMat())
					{
						infoPanel.UpdateRoiInfo(imageViewer.Rois, mat);
					}
				}
			};

			// 視圖變更時更新導航
			imageViewer.ViewChanged += (s, args) =>
			{
				if(imageViewer.Image == null)
				{
					return;
				}
				var vp = imageViewer.GetViewport();
				var r  = new Rectangle((int)vp.X, (int)vp.Y, (int)vp.Width, (int)vp.Height);
				infoPanel.UpdateNavigation(imageViewer.Image, r, imageViewer.Image.Size);
			};

			// InfoPanel 事件
			infoPanel.ObjectHighlightRequested += InfoPanel_ObjectHighlightRequested;
			infoPanel.ZoomToPointRequested     += InfoPanel_ZoomToPointRequested;

			infoPanel.ClearRoiRequested += (s, e) =>
			{
				imageViewer.Rois.Clear();
				imageViewer.Invalidate();
				infoPanel.UpdateRoiInfo(imageViewer.Rois, null);
			};
			infoPanel.NavigateToRequested += (s, p) => { imageViewer.CenterAt(new PointF((float)p.X, (float)p.Y)); };

			// ThumbnailBar 事件
			thumbnailBar.ThumbnailClicked += (s, tag) =>
			{
				if(tag is string path && File.Exists(path))
				{
					LoadForInspection(path);
					RunSequence();
				}
			};
		}

		private void UpdateUndoRedoButtons()
		{
			btnTsUndo.Enabled = _history.CanUndo;
			btnTsRedo.Enabled = _history.CanRedo;
		}

		private void PerformUndo()
		{
			(Bitmap img, List<RoiBase> rois) = _history.Undo(imageViewer.Image, imageViewer.Rois);

			if(img != null)
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
			(Bitmap img, List<RoiBase> rois) = _history.Redo(imageViewer.Image, imageViewer.Rois);

			if(img != null)
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
			if(node.Tag is VisionToolFactory.ToolDefinition toolDef)
			{
				InspectionItem item = new InspectionItem
				{
					Name       = toolDef.Name,
					Action     = toolDef.Action,
					Parameters = Activator.CreateInstance(toolDef.DefaultParameters.GetType()),
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
			foreach(PropertyInfo prop in source.GetType().GetProperties())
			{
				if(prop.CanWrite)
				{
					prop.SetValue(dest, prop.GetValue(source));
				}
			}
		}

		private void MoveStep(int direction)
		{
			if(dgvSequence.SelectedRows.Count == 0)
			{
				return;
			}
			int idx    = dgvSequence.SelectedRows[0].Index;
			int newIdx = idx + direction;

			if(newIdx >= 0 && newIdx < dgvSequence.Rows.Count)
			{
				InspectionItem item = _sequence[idx];
				_sequence.RemoveAt(idx);
				_sequence.Insert(newIdx, item);
				DataGridViewRow row = dgvSequence.Rows[idx];
				dgvSequence.Rows.RemoveAt(idx);
				dgvSequence.Rows.Insert(newIdx, row);
				row.Selected = true;
			}
		}

		private void RemoveStep()
		{
			if(dgvSequence.SelectedRows.Count == 0)
			{
				return;
			}
			int idx = dgvSequence.SelectedRows[0].Index;
			_sequence.RemoveAt(idx);
			dgvSequence.Rows.RemoveAt(idx);
		}

		private void DgvSequence_SelectionChanged(object sender, EventArgs e)
		{
			if(dgvSequence.SelectedRows.Count > 0)
			{
				DataGridViewRow row = dgvSequence.SelectedRows[0];

				if(row.Tag is InspectionItem item)
				{
					propertyGrid.SelectedObject = item.Parameters;

					if(item.LastResultImage != null)
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

		/// <summary>處理步驟列表的按鈕點擊事件</summary>
		private void DgvSequence_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			// 檢查是否點擊「▶」按鈕欄位 (colRun 是第 4 欄，索引為 3)
			if(e.RowIndex >= 0 && e.ColumnIndex == dgvSequence.Columns["colRun"].Index)
			{
				RunSingleStep(e.RowIndex);
			}
		}

		private void LoadImage()
		{
			using(OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.Multiselect = true;

				if(dlg.ShowDialog() == DialogResult.OK)
				{
					thumbnailBar.Clear();
					string[] files = dlg.FileNames;

					if(files.Length > 0)
					{
						foreach(string file in files)
						{
							try
							{
								// 僅讀取縮圖
								using(Mat mat = Cv2.ImRead(file))
								{
									if(!mat.Empty())
									{
										using(Bitmap bmp = mat.ToBitmap())
										{
											thumbnailBar.AddThumbnail(Path.GetFileName(file), bmp, file);
										}
									}
								}
							}
							catch
							{
								/* Ignore invalid images */
							}
						}

						// 載入第一張
						LoadForInspection(files[0]);

						// 自動選取第一個縮圖
						thumbnailBar.SelectItem(files[0]);
					}
				}
			}
		}

		private void LoadForInspection(string path)
		{
			try
			{
				Log($"[DebugViewer] Attempting load: {path}", TraceLevel.Info);
				DumpLayoutInfo(); // Log layout before loading
				_currImagePath = path;

				using(Mat mat = Cv2.ImRead(path))
				{
					if(mat.Empty())
					{
						Log("[DebugViewer] Mat is Empty! Check path encoding or file integrity.", TraceLevel.Error);
						return;
					}

					// 必須深拷貝，避免 Mat 釋放後 Bitmap 資料失效
					Bitmap rawBmp = mat.ToBitmap();
					Bitmap bmp    = new Bitmap(rawBmp);
					imageViewer.Image = bmp;

					// 保存原始圖片副本
					_originalImage?.Dispose();
					_originalImage = (Bitmap)bmp.Clone();

					// 釋放暫時的 GDI 物件 (若 ToBitmap 產生了獨立物件)
					rawBmp.Dispose();

					// Debug Log
					Log($"[DebugViewer] Success. {imageViewer.GetDebugInfo()}", TraceLevel.Info);
				}

				// 清除歷史與狀態
				_history.Clear();
				_history.PushState(imageViewer.Image, imageViewer.Rois);
				UpdateUndoRedoButtons();
				Log($"載入影像: {Path.GetFileName(path)}", TraceLevel.Info);
			}
			catch(Exception ex)
			{
				Log($"載入失敗: {ex.Message} \nStack: {ex.StackTrace}", TraceLevel.Error);
			}
		}

		private void RunSequence()
		{
			if(imageViewer.Image == null)
			{
				Log("請先載入影像", TraceLevel.Error);
				return;
			}

			foreach(DataGridViewRow r in dgvSequence.Rows)
			{
				r.Cells[1].Value             = "";
				r.Cells[2].Value             = "Wait";
				r.DefaultCellStyle.BackColor = Color.White;
			}
			Mat currentMat = imageViewer.Image.ToMat();
			Mat roiMask    = null;

			if(imageViewer.Rois.Count > 0)
			{
				roiMask = new Mat(currentMat.Size(), MatType.CV_8UC1, Scalar.All(0));

				foreach(RoiBase roi in imageViewer.Rois)
				{
					using(Mat subMask = roi.GetMask(currentMat.Size()))
					{
						Cv2.BitwiseOr(roiMask, subMask, roiMask);
					}
				}
			}
			Log("開始執行流程...", TraceLevel.Info);
			Stopwatch swTotal = Stopwatch.StartNew();

			try
			{
				for(int i = 0; i < _sequence.Count; i++)
				{
					InspectionItem  item = _sequence[i];
					DataGridViewRow row  = dgvSequence.Rows[i];
					row.Cells[2].Value = "Running...";
					Application.DoEvents();
					Stopwatch swStep = Stopwatch.StartNew();

					try
					{
						Mat inputToTool = currentMat;
						Mat maskedInput = null;

						if(roiMask != null)
						{
							maskedInput = new Mat();
							currentMat.CopyTo(maskedInput, roiMask);
							inputToTool = maskedInput;
						}
						(bool IsOk, Mat ResultImage, List<Defect> Defects) result = item.Action(inputToTool, item.Parameters);
						swStep.Stop();
						row.Cells[1].Value = $"{swStep.ElapsedMilliseconds}ms";

						if(result.IsOk)
						{
							row.Cells[2].Value           = "OK";
							row.Cells[2].Style.ForeColor = Color.Green;

							// 如果有 ROI，將結果合併回原始影像 (保留 ROI 外區域)
							Mat finalResult = result.ResultImage;

							if(roiMask != null)
							{
								// 確保結果影像與原圖通道數一致 (支援 4 通道 BGRA)
								Mat resultToMerge = result.ResultImage;
								int dstCh         = currentMat.Channels();
								int srcCh         = result.ResultImage.Channels();

								if(dstCh != srcCh)
								{
									resultToMerge = new Mat();

									if(dstCh == 4)
									{
										if(srcCh == 1)
										{
											Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGRA);
										}
										else if(srcCh == 3)
										{
											Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGR2BGRA);
										}
									}
									else if(dstCh == 3)
									{
										if(srcCh == 1)
										{
											Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGR);
										}
										else if(srcCh == 4)
										{
											Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGRA2BGR);
										}
									}
								}

								// 將結果的 ROI 區域複製到原圖上
								finalResult = currentMat.Clone();
								resultToMerge.CopyTo(finalResult, roiMask);

								if(resultToMerge != result.ResultImage)
								{
									resultToMerge.Dispose();
								}
								result.ResultImage.Dispose();
							}
							item.LastResultImage?.Dispose();
							item.LastResultImage = finalResult.ToBitmap();
							item.LastDefects     = result.Defects ?? new List<Defect>();
							currentMat.Dispose();
							currentMat = finalResult;
						}
						else
						{
							row.Cells[2].Value           = "NG";
							row.Cells[2].Style.ForeColor = Color.Red;
							result.ResultImage?.Dispose();
							item.LastResultImage = null;
						}

						if(row.Selected)
						{
							imageViewer.Image = (Bitmap)item.LastResultImage.Clone();
						}
						maskedInput?.Dispose();
					}
					catch(Exception ex)
					{
						row.Cells[2].Value           = "ERR";
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

				if(_sequence.Count > 0)
				{
					// 尋找最後一個有結果的步驟 (若發生錯誤，顯示最後成功的步驟)
					InspectionItem lastValidItem = _sequence.LastOrDefault(x => x.LastResultImage != null);

					if(lastValidItem != null)
					{
						imageViewer.Image = (Bitmap)lastValidItem.LastResultImage.Clone();
						RefreshInfoPanel(swTotal.ElapsedMilliseconds);
					}
				}
				currentMat?.Dispose();
				roiMask?.Dispose();
			}
		}

		private void Log(string msg, TraceLevel level)
		{
			string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			string fullMsg   = $"[{timestamp}] {msg}";

			try
			{
				lstLog.Items.Add(new LogItem { Message = fullMsg, Level = level });
				lstLog.TopIndex = lstLog.Items.Count - 1;
			}
			catch
			{
				/* UI Error check */
			}

			try
			{
				File.AppendAllText("debug.log", $"[{timestamp}] [{level}] {msg}{Environment.NewLine}");
			}
			catch
			{
				/* File Error check */
			}
		}

		private void LstLog_DrawItem(object sender, DrawItemEventArgs e)
		{
			if(e.Index < 0)
			{
				return;
			}
			e.DrawBackground();
			LogItem item  = (LogItem)lstLog.Items[e.Index];
			Color   color = Color.Black;

			if(item.Level == TraceLevel.Error)
			{
				color = Color.DarkRed;
			}

			if(item.Level == TraceLevel.Info)
			{
				color = Color.Green;
			}

			using(SolidBrush brush = new SolidBrush(color))
			{
				e.Graphics.DrawString(item.Message, e.Font, brush, e.Bounds);
			}
			e.DrawFocusRectangle();
		}

		/// <summary>InfoPanel 物件高亮請求處理</summary>
		private void InfoPanel_ObjectHighlightRequested(object sender, int objectId)
		{
			DetectedObject obj = _lastDetectedObjects.FirstOrDefault(o => o.Id == objectId);

			if(obj == null || imageViewer.Image == null)
			{
				return;
			}

			// 複製並繪製高亮
			using(Graphics g = Graphics.FromImage(imageViewer.Image))
			{
				using(Pen pen = new Pen(Color.Yellow, 3))
				{
					if(obj.Type == "圓形")
					{
						g.DrawEllipse(pen, (float)(obj.CenterX - obj.Radius), (float)(obj.CenterY - obj.Radius), (float)(obj.Radius * 2), (float)(obj.Radius * 2));
					}
					else
					{
						g.DrawRectangle(pen, (float)(obj.CenterX - obj.Width / 2), (float)(obj.CenterY - obj.Height / 2), (float)obj.Width, (float)obj.Height);
					}
				}
			}
			imageViewer.Invalidate();
		}

		/// <summary>InfoPanel 縮放至點請求處理</summary>
		private void InfoPanel_ZoomToPointRequested(object sender, (double X, double Y) point)
		{
			// 可實作縮放並移動到指定座標
			Log($"縮放到座標: ({point.X:F1}, {point.Y:F1})", TraceLevel.Info);
		}

		private void RefreshInfoPanel(long elapsedMs)
		{
			_lastDetectedObjects = _sequence.SelectMany(s => s.LastDefects ?? new List<Defect>())
			                                .Select((d, idx) => new DetectedObject
			                                {
				                                Id      = idx + 1,
				                                Type    = d.Type ?? "未知",
				                                CenterX = d.BoundingBox != null && d.BoundingBox.Length >= 4 ? d.BoundingBox[0] + d.BoundingBox[2] / 2.0 : 0,
				                                CenterY = d.BoundingBox != null && d.BoundingBox.Length >= 4 ? d.BoundingBox[1] + d.BoundingBox[3] / 2.0 : 0,
				                                Width   = d.BoundingBox != null && d.BoundingBox.Length >= 4 ? d.BoundingBox[2] : 0,
				                                Height  = d.BoundingBox != null && d.BoundingBox.Length >= 4 ? d.BoundingBox[3] : 0,
				                                Radius  = d.Confidence,
				                                Area    = d.Type == "圓形" ? Math.PI * d.Confidence * d.Confidence : d.BoundingBox != null && d.BoundingBox.Length >= 4 ? d.BoundingBox[2] * d.BoundingBox[3] : 0,
				                                Status  = "OK",
			                                })
			                                .ToList();

			if(imageViewer.Image != null)
			{
				using(Mat resultMat = imageViewer.Image.ToMat())
				{
					UpdateInfoPanel(resultMat, elapsedMs);
				}
			}
		}

		/// <summary>更新 InfoPanel 顯示</summary>
		private void UpdateInfoPanel(Mat resultImage, double processTimeMs)
		{
			if(resultImage == null)
			{
				return;
			}
			infoPanel.UpdateStatistics(resultImage, _lastDetectedObjects, processTimeMs);
			infoPanel.BindDetectionResults(_lastDetectedObjects);
			infoPanel.UpdateHistogram(resultImage);
		}

		/// <summary>設置序列表格的右鍵選單</summary>
		private void SetupSequenceContextMenu()
		{
			_sequenceContextMenu = new ContextMenuStrip();
			ToolStripMenuItem menuRunThis = new ToolStripMenuItem("執行此步驟");

			menuRunThis.Click += (s, e) =>
			{
				if(dgvSequence.SelectedRows.Count > 0)
				{
					RunSingleStep(dgvSequence.SelectedRows[0].Index);
				}
			};
			ToolStripMenuItem menuRunFrom = new ToolStripMenuItem("從此步驟開始執行");

			menuRunFrom.Click += (s, e) =>
			{
				if(dgvSequence.SelectedRows.Count > 0)
				{
					RunFromStep(dgvSequence.SelectedRows[0].Index);
				}
			};
			_sequenceContextMenu.Items.Add(menuRunThis);
			_sequenceContextMenu.Items.Add(menuRunFrom);
			_sequenceContextMenu.Items.Add(new ToolStripSeparator());
			ToolStripMenuItem menuResetImage = new ToolStripMenuItem("重設為原始影像");
			menuResetImage.Click += (s, e) => ResetToOriginalImage();
			_sequenceContextMenu.Items.Add(menuResetImage);
			dgvSequence.ContextMenuStrip = _sequenceContextMenu;
		}

		/// <summary>重設為原始影像</summary>
		private void ResetToOriginalImage()
		{
			if(_originalImage == null)
			{
				Log("沒有原始影像可以重設", TraceLevel.Warning);
				return;
			}
			imageViewer.Image = (Bitmap)_originalImage.Clone();
			Log("已重設為原始影像", TraceLevel.Info);

			// 清除所有步驟的執行結果
			foreach(DataGridViewRow r in dgvSequence.Rows)
			{
				r.Cells[1].Value             = "";
				r.Cells[2].Value             = "Wait";
				r.DefaultCellStyle.BackColor = Color.White;
			}
		}

		/// <summary>執行單一步驟並將結果繪製在影像上</summary>
		private void RunSingleStep(int stepIndex)
		{
			if(_originalImage == null)
			{
				Log("請先載入影像", TraceLevel.Error);
				return;
			}

			if(stepIndex < 0 || stepIndex >= _sequence.Count)
			{
				return;
			}
			InspectionItem  item = _sequence[stepIndex];
			DataGridViewRow row  = dgvSequence.Rows[stepIndex];

			// 使用原始影像來執行此步驟
			Mat inputMat = _originalImage.ToMat();
			Mat roiMask  = null;

			try
			{
				if(imageViewer.Rois.Count > 0)
				{
					roiMask = new Mat(inputMat.Size(), MatType.CV_8UC1, Scalar.All(0));

					foreach(RoiBase roi in imageViewer.Rois)
					{
						using(Mat subMask = roi.GetMask(inputMat.Size()))
						{
							Cv2.BitwiseOr(roiMask, subMask, roiMask);
						}
					}
				}
				row.Cells[2].Value = "Running...";
				Application.DoEvents();
				Stopwatch swStep      = Stopwatch.StartNew();
				Mat       toolInput   = inputMat;
				Mat       maskedInput = null;

				if(roiMask != null)
				{
					maskedInput = new Mat();
					inputMat.CopyTo(maskedInput, roiMask);
					toolInput = maskedInput;
				}
				(bool IsOk, Mat ResultImage, List<Defect> Defects) result = item.Action(toolInput, item.Parameters);
				swStep.Stop();
				row.Cells[1].Value = $"{swStep.ElapsedMilliseconds}ms";

				if(result.IsOk)
				{
					row.Cells[2].Value           = "OK";
					row.Cells[2].Style.ForeColor = Color.Green;

					// 如果有 ROI，將結果合併回原始影像 (保留 ROI 外區域)
					Mat finalResult = result.ResultImage;

					if(roiMask != null)
					{
						Mat resultToMerge = result.ResultImage;
						int dstCh         = inputMat.Channels();
						int srcCh         = result.ResultImage.Channels();

						if(dstCh != srcCh)
						{
							resultToMerge = new Mat();

							if(dstCh == 4)
							{
								if(srcCh == 1)
								{
									Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGRA);
								}
								else if(srcCh == 3)
								{
									Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGR2BGRA);
								}
							}
							else if(dstCh == 3)
							{
								if(srcCh == 1)
								{
									Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGR);
								}
								else if(srcCh == 4)
								{
									Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGRA2BGR);
								}
							}
						}
						finalResult = inputMat.Clone();
						resultToMerge.CopyTo(finalResult, roiMask);

						if(resultToMerge != result.ResultImage)
						{
							resultToMerge.Dispose();
						}
						result.ResultImage.Dispose();
					}
					item.LastResultImage?.Dispose();
					item.LastResultImage = finalResult.ToBitmap();
					item.LastDefects     = result.Defects ?? new List<Defect>();
					imageViewer.Image    = (Bitmap)item.LastResultImage.Clone();
					RefreshInfoPanel(swStep.ElapsedMilliseconds);
					imageViewer.Image = (Bitmap)item.LastResultImage.Clone();

					if(finalResult != result.ResultImage)
					{
						finalResult.Dispose();
					}
				}
				else
				{
					row.Cells[2].Value           = "NG";
					row.Cells[2].Style.ForeColor = Color.Red;
					result.ResultImage?.Dispose();
				}
				maskedInput?.Dispose();
				Log($"執行步驟: {item.Name} - {row.Cells[2].Value}", TraceLevel.Info);
			}
			catch(Exception ex)
			{
				row.Cells[2].Value           = "ERR";
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
			if(_originalImage == null)
			{
				Log("請先載入影像", TraceLevel.Error);
				return;
			}

			if(startIndex < 0 || startIndex >= _sequence.Count)
			{
				return;
			}

			// 重設從 startIndex 開始的狀態
			for(int i = startIndex; i < dgvSequence.Rows.Count; i++)
			{
				DataGridViewRow r = dgvSequence.Rows[i];
				r.Cells[1].Value             = "";
				r.Cells[2].Value             = "Wait";
				r.DefaultCellStyle.BackColor = Color.White;
			}
			Mat currentMat = _originalImage.ToMat();
			Mat roiMask    = null;

			if(imageViewer.Rois.Count > 0)
			{
				roiMask = new Mat(currentMat.Size(), MatType.CV_8UC1, Scalar.All(0));

				foreach(RoiBase roi in imageViewer.Rois)
				{
					using(Mat subMask = roi.GetMask(currentMat.Size()))
					{
						Cv2.BitwiseOr(roiMask, subMask, roiMask);
					}
				}
			}
			Log($"從步驟 {startIndex + 1} 開始執行...", TraceLevel.Info);
			Stopwatch swTotal = Stopwatch.StartNew();

			try
			{
				for(int i = startIndex; i < _sequence.Count; i++)
				{
					InspectionItem  item = _sequence[i];
					DataGridViewRow row  = dgvSequence.Rows[i];
					row.Cells[2].Value = "Running...";
					Application.DoEvents();
					Stopwatch swStep = Stopwatch.StartNew();

					try
					{
						Mat inputToTool = currentMat;
						Mat maskedInput = null;

						if(roiMask != null)
						{
							maskedInput = new Mat();
							currentMat.CopyTo(maskedInput, roiMask);
							inputToTool = maskedInput;
						}
						(bool IsOk, Mat ResultImage, List<Defect> Defects) result = item.Action(inputToTool, item.Parameters);
						swStep.Stop();
						row.Cells[1].Value = $"{swStep.ElapsedMilliseconds}ms";

						if(result.IsOk)
						{
							row.Cells[2].Value           = "OK";
							row.Cells[2].Style.ForeColor = Color.Green;
							item.LastResultImage?.Dispose();
							item.LastResultImage = result.ResultImage.ToBitmap();
							item.LastDefects     = result.Defects ?? new List<Defect>();
							currentMat.Dispose();
							currentMat = result.ResultImage;
						}
						else
						{
							row.Cells[2].Value           = "NG";
							row.Cells[2].Style.ForeColor = Color.Red;
							result.ResultImage?.Dispose();
							item.LastResultImage = null;
						}

						if(row.Selected)
						{
							imageViewer.Image = (Bitmap)item.LastResultImage.Clone();
						}
						maskedInput?.Dispose();
					}
					catch(Exception ex)
					{
						row.Cells[2].Value           = "ERR";
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

				if(_sequence.Count > startIndex)
				{
					InspectionItem lastItem = _sequence.Last();

					if(lastItem.LastResultImage != null)
					{
						imageViewer.Image = (Bitmap)lastItem.LastResultImage.Clone();
						RefreshInfoPanel(swTotal.ElapsedMilliseconds);
					}
				}
				currentMat?.Dispose();
				roiMask?.Dispose();
			}
		}

		private class InspectionItem
		{
			public string                         Name            { get; set; }
			public object                         Parameters      { get; set; }
			public VisionToolFactory.VisionAction Action          { get; set; }
			public Bitmap                         LastResultImage { get; set; }
			public List<Defect>                   LastDefects     { get; set; } = new List<Defect>();
		}

		private enum TraceLevel
		{
			Info,
			Warning,
			Error,
		}

		private class LogItem
		{
			public string     Message { get; set; }
			public TraceLevel Level   { get; set; }
		}
	}
}