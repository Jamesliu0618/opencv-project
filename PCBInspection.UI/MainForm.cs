using OpenCvSharp;
using OpenCvSharp.Extensions;
using PCBInspection.Core.Models;
using PCBInspection.Core.ROI;
using PCBInspection.Core.Services;
using PCBInspection.UI.Controls;
using PCBInspection.UI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
	/// <summary>
	/// 主表單類別
	/// 負責管理整個 PCB 檢測流程、影像顯示與使用者互動
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>操作歷史管理器，用於復原/重做 ROI 編輯與影像狀態</summary>
		private readonly HistoryManager       _history  = new HistoryManager();
		
		/// <summary>檢測流程的步驟清單</summary>
		private readonly List<InspectionItem> _sequence = new List<InspectionItem>();
		
		/// <summary>目前載入的影像檔案路徑</summary>
		private          string               _currImagePath;

		/// <summary>最後一次偵測到的物件清單（如圓形、缺陷、邊緣等）</summary>
		private List<DetectedObject> _lastDetectedObjects = new List<DetectedObject>();
		
		/// <summary>載入影像時保存的原始影像副本，確保檢測流程從乾淨的影像開始</summary>
		private Bitmap               _originalImage;

		/// <summary>步驟清單右鍵選單</summary>
		private ContextMenuStrip _sequenceContextMenu;

		// 循環執行相關
		/// <summary>循環執行專用計時器</summary>
		private System.Windows.Forms.Timer _loopTimer;

		/// <summary>Singleton instance for accessing from property editors</summary>
		public static MainForm Instance { get; private set; }

		/// <summary>取得當前檢測到的物件列表</summary>
		public List<DetectedObject> GetDetectedObjects()
		{
			return _lastDetectedObjects ?? new List<DetectedObject>();
		}
		
		/// <summary>目前正在循環執行的步驟索引</summary>
		private int                        _loopStepIndex = -1;
		
		/// <summary>是否處於循環執行模式</summary>
		private bool                       _isLooping;

		/// <summary>
		/// 初始化主表單
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
			
			// 設定 UI 主題與外觀
			SetupTheme();
			
			// 從視覺工廠載入所有可用工具到左側工具箱
			InitializeToolbox();
			
			// 載入工具列圖示
			LoadToolbarIcons();

			// 載入工具箱圖示
			LoadToolboxIcons();
			
			// 綁定所有控制項事件
			WireEvents();
			
			// 初始化手動標註控制項並連結影像檢視器
			if(defectLabelingControl1 != null)
			{
				defectLabelingControl1.Setup(imageViewer);
			}
		}

		/// <summary>
		/// 輸出 UI 佈局偵錯資訊，用於排查 SplitContainer 與面板大小問題
		/// </summary>
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

		/// <summary>
		/// 設定 UI 主題配色與預設高度
		/// </summary>
		private void SetupTheme()
		{
			BackColor                      = SystemColors.Control;
			toolStripMain.ImageScalingSize = new Size(32, 32);
			toolStripMain.Height           = 50;
		}

		/// <summary>
		/// 從 Resources 目錄載入工具列所需的所有 PNG 圖示
		/// </summary>
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
				btnTsSplit.Image     = LoadIcon(iconDir, "Split");
				btnTsPointer.Image   = LoadIcon(iconDir, "Pointer");
				btnTsRoiRect.Image   = LoadIcon(iconDir, "RoiRect");
				btnTsRoiCircle.Image = LoadIcon(iconDir, "RoiCircle");
				btnTsRoiPoly.Image   = LoadIcon(iconDir, "RoiPoly");
				btnTsUndo.Image      = LoadIcon(iconDir, "Undo");
				btnTsRedo.Image      = LoadIcon(iconDir, "Redo");
				btnTsBatch.Image     = LoadIcon(iconDir, "Batch");
				btnTsRecipe.Image    = LoadIcon(iconDir, "Recipe");

				// Set styles
				foreach(ToolStripItem item in toolStripMain.Items)
				{
					if(item is ToolStripButton btn)
					{
						btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
						btn.Padding      = new Padding(5, 0, 5, 0);
					}
				}
			}
			catch(Exception ex)
			{
				Log($"載入圖標失敗: {ex.Message}", TraceLevel.Warning);
			}
		}

		/// <summary>
		/// 輔助方法：從指定路徑載入單張圖示影像
		/// </summary>
		private Image LoadIcon(string dir, string name)
		{
			string path = Path.Combine(dir, $"{name}.png");

			if(File.Exists(path))
			{
				return Image.FromFile(path);
			}
			return null;
		}

		/// <summary>
		/// 載入工具箱 TreeView 所需的穩定工業風圖示
		/// </summary>
		private void LoadToolboxIcons()
		{
			try
			{
				string iconDir = @"d:\Repo\opencv\Resources\Icons";
				imgListToolbox.ImageSize = new Size(20, 20);
				imgListToolbox.ColorDepth = ColorDepth.Depth32Bit;

				imgListToolbox.Images.Add("default", LoadIcon(iconDir, "tool_default"));      // 0
				imgListToolbox.Images.Add("preprocess", LoadIcon(iconDir, "cat_preprocess")); // 1
				imgListToolbox.Images.Add("color", LoadIcon(iconDir, "cat_color"));           // 2
				imgListToolbox.Images.Add("feature", LoadIcon(iconDir, "cat_feature"));       // 3
				imgListToolbox.Images.Add("measurement", LoadIcon(iconDir, "cat_measurement")); // 4
				imgListToolbox.Images.Add("analysis", LoadIcon(iconDir, "cat_analysis"));     // 5
				imgListToolbox.Images.Add("calibration", LoadIcon(iconDir, "cat_calibration")); // 6
				imgListToolbox.Images.Add("defect", LoadIcon(iconDir, "cat_defect"));         // 7
			}
			catch (Exception ex)
			{
				Log($"載入工具箱圖標失敗: {ex.Message}", TraceLevel.Warning);
			}
		}

		/// <summary>
		/// 根據類別名稱取得對應的圖示索引
		/// </summary>
		private int GetIconIndexForCategory(string category)
		{
			if (string.IsNullOrEmpty(category)) return 0;
			if (category.Contains("預處理") || category.Contains("處理")) return 1;
			if (category.Contains("色彩")) return 2;
			if (category.Contains("特徵提取")) return 3;
			if (category.Contains("幾何") || category.Contains("測量") || category.Contains("量測")) return 4;
			if (category.Contains("分析") || category.Contains("影像處理")) return 5;
			if (category.Contains("校正")) return 6;
			if (category.Contains("背景") || category.Contains("檢測")) return 7;
			return 0;
		}

		/// <summary>
		/// 初始化左側工具箱，將 VisionToolFactory 中定義的所有工具按類別分群顯示在 TreeView
		/// </summary>
		private void InitializeToolbox()
		{
			tvTools.Nodes.Clear();
			var allTools = VisionToolFactory.GetAllTools();
			var grouped  = allTools.GroupBy(t => t.Category);

			foreach(var group in grouped)
			{
				int iconIdx = GetIconIndexForCategory(group.Key);
				var catNode = new TreeNode(group.Key)
				{
					ImageIndex = iconIdx,
					SelectedImageIndex = iconIdx
				};

				foreach(var tool in group)
				{
					var toolNode = new TreeNode(tool.Name)
					{
						Tag = tool,
						ImageIndex = 0, // 子工具使用通用方塊圖標
						SelectedImageIndex = 0
					};
					catNode.Nodes.Add(toolNode);
				}
				tvTools.Nodes.Add(catNode);
			}
			tvTools.ExpandAll();
		}

		/// <summary>
		/// 集中處理所有 UI 事件的綁定（Click, Selected, ListChanged 等）
		/// </summary>
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
			if(btnTsOpen != null)
			{
				btnTsOpen.Click -= BtnTsOpen_Click; // Clear existing
				btnTsOpen.Click += BtnTsOpen_Click;
			}
			else
			{
				MessageBox.Show("[DEBUG] btnTsOpen is NULL in WireEvents!");
			}
			
			btnTsRunOnce.Click += (s, e) => RunSequence();
			btnTsRunLoop.Click += (s, e) => StartLoopExecution(_sequence.Count - 1);
			btnTsStop.Click    += (s, e) => StopLoopExecution();

			// Toolbar - Zoom
			btnTsZoomIn.Click  += (s, e) => imageViewer.ZoomIn();
			btnTsZoomOut.Click += (s, e) => imageViewer.ZoomOut();
			btnTsFit.Click     += (s, e) => imageViewer.FitToWindow();

			// Toolbar - Split View
			btnTsSplit.Click += (s, e) => ToggleSplitView();

			SetupViewerSync();

			// Toolbar - ROI Modes
			btnTsPointer.Click   += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.EditROI;
			btnTsRoiRect.Click   += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawRect;
			btnTsRoiCircle.Click += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawCircle;
			btnTsRoiPoly.Click   += (s, e) => imageViewer.Mode = InteractiveImageViewer.ViewerMode.DrawPoly;

			// Toolbar - Undo/Redo
			btnTsUndo.Click += (s, e) => PerformUndo();
			btnTsRedo.Click += (s, e) => PerformRedo();

			// Toolbar - Batch & Recipe
			btnTsBatch.Click  += (s, e) => OpenBatchProcessing();
			btnTsRecipe.Click += (s, e) => OpenRecipeManager();

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
			dgvSequence.CellClick += DgvSequence_CellClick;

			// Log rendering
			lstLog.DrawItem += LstLog_DrawItem;

			// Viewer Events
			imageViewer.MousePixelChanged += (s, p) => lblStatusMain.Text = $"X: {p.X}, Y: {p.Y}";

			imageViewer.RoiListChanged += (s, args) =>
			{
				_history.PushState(imageViewer.Image, imageViewer.Rois, "編輯 ROI");
				// UpdateUndoRedoButtons(); // Handled by StateChanged

				// 同步 ROI 清單到 Core 層的 RoiManager (支援編號選取)
				RoiManager.SetRois(imageViewer.Rois);

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

			// 清除檢測結果事件
			infoPanel.ClearResultsRequested += (s, e) =>
			{
				_lastDetectedObjects.Clear();
				infoPanel.BindDetectionResults(_lastDetectedObjects);
				// 恢復原始影像（若有）
				if (_originalImage != null)
				{
					imageViewer.Image = (Bitmap)_originalImage.Clone();
				}
				Log("已清除檢測結果", TraceLevel.Info);
			};

			// ThumbnailBar 事件
			thumbnailBar.ThumbnailClicked += (s, tag) =>
			{
				if(tag is string path && File.Exists(path))
				{
					LoadForInspection(path);
					RunSequence();
				}
			};


			// History Events
			_history.StateChanged += (s, args) =>
			{
				if(InvokeRequired)
				{
					Invoke(new Action(() => {
                        UpdateUndoRedoButtons();
                        UpdateHistoryList();
                    }));
				}
				else
				{
					UpdateUndoRedoButtons();
					UpdateHistoryList();
				}
			};
		}

		/// <summary>
		/// 更新左側歷史紀錄清單顯示
		/// </summary>
		private void UpdateHistoryList()
		{
			lstHistory.Items.Clear();
			var items = _history.GetHistoryItems();
			foreach(var item in items)
			{
				lstHistory.Items.Add(item);
			}
		}

		/// <summary>
		/// 切換比對視窗模式（顯示/隱藏左側的參考影像）
		/// </summary>
		private void ToggleSplitView()
		{
			bool isCollapsed = splitContainerImages.Panel1Collapsed;
			splitContainerImages.Panel1Collapsed = !isCollapsed;

			if(isCollapsed) // 原本是摺疊狀態，現在開啟
			{
				if(imageViewerRef.Image == null && imageViewer.Image != null)
				{
					// 為了 UX 體驗，若參考影像為空則複製當前檢測影像
					imageViewerRef.Image = (Bitmap)imageViewer.Image.Clone();
				}
				// 同步兩個視窗的縮放與位置
				imageViewerRef.SetView(imageViewer.ScaleFactor, imageViewer.OffsetX, imageViewer.OffsetY);
			}
		}

		/// <summary>
		/// 設定主視窗與參考視窗的雙向同步（縮放、位移）
		/// </summary>
		private void SetupViewerSync()
		{
			// 主視窗連動參考視窗
			imageViewer.ViewChanged += (s, e) =>
			{
				if(!splitContainerImages.Panel1Collapsed)
				{
					imageViewerRef.SetView(imageViewer.ScaleFactor, imageViewer.OffsetX, imageViewer.OffsetY);
				}
			};

			// 參考視窗連動主視窗
			imageViewerRef.ViewChanged += (s, e) =>
			{
				if(!splitContainerImages.Panel1Collapsed)
				{
					imageViewer.SetView(imageViewerRef.ScaleFactor, imageViewerRef.OffsetX, imageViewerRef.OffsetY);
				}
			};
		}

		/// <summary>
		/// 根據歷史狀態更新工具列復原/重做按鈕的啟用狀態
		/// </summary>
		private void UpdateUndoRedoButtons()
		{
			btnTsUndo.Enabled = _history.CanUndo;
			btnTsRedo.Enabled = _history.CanRedo;
		}

		/// <summary>
		/// 執行復原操作（Undo）
		/// </summary>
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

		/// <summary>
		/// 執行重做操作（Redo）
		/// </summary>
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

		/// <summary>
		/// 將選定的工具加入檢測流程步驟清單
		/// </summary>
		private void AddToolToSequence(TreeNode node)
		{
			if(node.Tag is VisionToolFactory.ToolDefinition toolDef)
			{
				InspectionItem item = new InspectionItem
				{
					Name       = toolDef.Name,
					Action     = toolDef.Action,
					// 實例化該工具的預設參數物件
					Parameters = Activator.CreateInstance(toolDef.DefaultParameters.GetType()),
				};
				CopyProperties(toolDef.DefaultParameters, item.Parameters);
				
				_sequence.Add(item);
				// 在 DataGridView 中新增一行
				int idx = dgvSequence.Rows.Add(item.Name, "", "Wait");
				dgvSequence.Rows[idx].Tag = item;
				dgvSequence.ClearSelection();
				dgvSequence.Rows[idx].Selected = true;
				
				Log($"已加入步驟: {item.Name}", TraceLevel.Info);
			}
		}

		/// <summary>
		/// 輔助方法：使用反射複製物件的所有可讀寫屬性
		/// </summary>
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

		/// <summary>
		/// 移動步驟順序（上移或下移）
		/// </summary>
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
				// 交換 _sequence 內容
				InspectionItem item = _sequence[idx];
				_sequence.RemoveAt(idx);
				_sequence.Insert(newIdx, item);
				
				// 交換 DataGridViewRow
				DataGridViewRow row = dgvSequence.Rows[idx];
				dgvSequence.Rows.RemoveAt(idx);
				dgvSequence.Rows.Insert(newIdx, row);
				row.Selected = true;
			}
		}

		/// <summary>
		/// 移除當前選取的檢測步驟
		/// </summary>
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

		/// <summary>
		/// 當步驟清單點選改變時，將該步驟的參數載入到屬性視窗 (PropertyGrid)，並顯示該步驟的最後結果圖
		/// </summary>
		private void DgvSequence_SelectionChanged(object sender, EventArgs e)
		{
			if(dgvSequence.SelectedRows.Count > 0)
			{
				DataGridViewRow row = dgvSequence.SelectedRows[0];

				if(row.Tag is InspectionItem item)
				{
					// 指向該步驟的參數物件，允許使用者在下方調整
					propertyGrid.SelectedObject = item.Parameters;

					// 若該步驟已有執行過且有結果圖，則顯示之
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
		private void DgvSequence_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if(e.RowIndex < 0) return;

			int colRunIndex = dgvSequence.Columns["colRun"].Index;
			int colLoopIndex = dgvSequence.Columns["colLoop"].Index;
				
			// 檢查是否點擊「▶」按鈕欄位
			if(e.ColumnIndex == colRunIndex)
			{
				StopLoopExecution(); // 停止循環
				RunSingleStep(e.RowIndex);
			}
			// 檢查是否點擊「↻」循環按鈕
			else if(e.ColumnIndex == colLoopIndex)
			{
				ToggleLoopExecution(e.RowIndex);
			}
		}

		/// <summary>
		/// 開啟檔案按鈕事件
		/// </summary>
		private void BtnTsOpen_Click(object sender, EventArgs e)
		{
            LoadImage();
		}

		/// <summary>
		/// 顯示開啟檔案對話框，支援多選
		/// 會讀取檔案並產生縮圖顯示在下方的縮圖列中
		/// </summary>
		private void LoadImage()
		{
			try
			{
				using(OpenFileDialog dlg = new OpenFileDialog())
				{
					dlg.Multiselect = true;
					dlg.Filter = "影像檔案|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|所有檔案|*.*";

					if(dlg.ShowDialog(this) == DialogResult.OK)
					{
						thumbnailBar.Clear();
						string[] files = dlg.FileNames;

						if(files.Length > 0)
						{
							foreach(string file in files)
							{
								try
								{
									// 僅讀取縮圖用於顯示
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
									/* 忽略無效影像 */
								}
							}

							// 預設載入清單中的第一張影像進行檢測
							LoadForInspection(files[0]);

							// 自動選取縮圖列中的第一個項目
							thumbnailBar.SelectItem(files[0]);
						}
					}
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show($"開啟檔案發生錯誤:\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Log($"開啟檔案失敗: {ex.Message}", TraceLevel.Error);
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
				_history.PushState(imageViewer.Image, imageViewer.Rois, "載入影像");
				// UpdateUndoRedoButtons(); // Handled by StateChanged
				// UpdateUndoRedoButtons(); // Handled by StateChanged
				Log($"載入影像: {Path.GetFileName(path)}", TraceLevel.Info);

				// [新增] 載入標註資料
				if(defectLabelingControl1 != null)
				{
					defectLabelingControl1.LoadImageLabels(path);
				}
			}
			catch(Exception ex)
			{
				Log($"載入失敗: {ex.Message} \nStack: {ex.StackTrace}", TraceLevel.Error);
				MessageBox.Show($"載入影像失敗:\n{path}\n{ex.Message}", "載入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void RunSequence()
		{
			if(_originalImage == null)
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
			
		// 使用原始影像的副本開始
			// [Fix] Bitmap.ToMat() 可能產生 RGB 格式，強制轉為 BGR 確保色彩正確
			Mat currentMat = _originalImage.ToMat();
			if(currentMat.Channels() >= 3) Cv2.CvtColor(currentMat, currentMat, ColorConversionCodes.RGB2BGR);
			Mat roiMask    = null;
			List<Defect> accumulatedDefects = new List<Defect>(); // [Fix] 累積缺陷 context

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
					Application.DoEvents(); // 讓 UI 更新
					
					// [Fix] 注入 Context
					InjectContext(item.Parameters, accumulatedDefects);
					
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
						
						// 執行工具
						(bool IsOk, Mat ResultImage, List<Defect> Defects) result = item.Action(inputToTool, item.Parameters);
						swStep.Stop();
						row.Cells[1].Value = $"{swStep.ElapsedMilliseconds}ms";
						
						// [Fix] 收集缺陷
						if (result.Defects != null)
						{
							accumulatedDefects.AddRange(result.Defects);
						}

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

								// [Fix] 若原圖是灰階 (1ch) 但結果是彩色 (3ch/4ch)，必須將基底圖轉為彩色才能顯示顏色
								if(dstCh == 1 && srcCh >= 3)
								{
									finalResult = new Mat();
									Cv2.CvtColor(currentMat, finalResult, ColorConversionCodes.GRAY2BGR);
									
									if(srcCh == 4)
									{
										Mat bgrResult = new Mat();
										Cv2.CvtColor(resultToMerge, bgrResult, ColorConversionCodes.BGRA2BGR);
										if(resultToMerge != result.ResultImage) resultToMerge.Dispose();
										resultToMerge = bgrResult;
									}
								}
								else
								{
									finalResult = currentMat.Clone(); // Default 

									if(dstCh != srcCh)
									{
										resultToMerge = new Mat();
										if(dstCh == 4 && srcCh == 1) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGRA);
										else if(dstCh == 4 && srcCh == 3) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGR2BGRA);
										else if(dstCh == 3 && srcCh == 1) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGR);
										else if(dstCh == 3 && srcCh == 4) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGRA2BGR);
									}
								}

								// 將結果的 ROI 區域複製到原圖上
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
							imageViewer.SetImagePreserveView((Bitmap)item.LastResultImage.Clone());
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
						// 更新 Viewer 顯示最終結果 (保留縮放比例)
						imageViewer.SetImagePreserveView((Bitmap)lastValidItem.LastResultImage.Clone());
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

			// 從原始結果影像重新繪製高亮 (避免高亮疊加，保留縮放比例)
			var lastItem = _sequence.LastOrDefault(s => s.LastResultImage != null);
			if(lastItem?.LastResultImage != null)
			{
				imageViewer.SetImagePreserveView((Bitmap)lastItem.LastResultImage.Clone());
			}

			// 在影像上繪製高亮框
			using(Graphics g = Graphics.FromImage(imageViewer.Image))
			{
				// 使用明亮的黃色粗線框住物件
				using(Pen pen = new Pen(Color.Yellow, 4))
				{
					pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

					if(obj.Type == "圓形")
					{
						// 圓形：繪製圓形高亮
						float x = (float)(obj.CenterX - obj.Radius);
						float y = (float)(obj.CenterY - obj.Radius);
						float d = (float)(obj.Radius * 2);
						g.DrawEllipse(pen, x, y, d, d);

						// 加繪十字準心
						using(Pen crossPen = new Pen(Color.Magenta, 2))
						{
							int cx = (int)obj.CenterX;
							int cy = (int)obj.CenterY;
							int r  = (int)obj.Radius + 15;
							g.DrawLine(crossPen, cx - r, cy, cx + r, cy);
							g.DrawLine(crossPen, cx, cy - r, cx, cy + r);
						}
					}
					else if(obj.Type == "直線")
					{
						// 直線：使用 BoundingBox 範圍繪製高亮矩形
						var bb = obj.BoundingBox;
						int padding = 10;
						g.DrawRectangle(pen, bb.X - padding, bb.Y - padding, bb.Width + padding * 2, bb.Height + padding * 2);

						// 高亮中心點
						using(Brush brush = new SolidBrush(Color.Magenta))
						{
							g.FillEllipse(brush, (float)(obj.CenterX - 6), (float)(obj.CenterY - 6), 12, 12);
						}
					}
					else
					{
						// 其他物件：使用 BoundingBox 繪製矩形高亮
						var bb = obj.BoundingBox;
						if(bb.Width > 0 && bb.Height > 0)
						{
							g.DrawRectangle(pen, bb.X, bb.Y, bb.Width, bb.Height);
						}
						else
						{
							// 如果沒有有效的 BoundingBox，使用 CenterX/CenterY 繪製十字標記
							int cx = (int)obj.CenterX;
							int cy = (int)obj.CenterY;
							g.DrawLine(pen, cx - 20, cy, cx + 20, cy);
							g.DrawLine(pen, cx, cy - 20, cx, cy + 20);
						}
					}
				}

				// 繪製編號標籤
				using(Font font = new Font("Arial", 14, FontStyle.Bold))
				using(Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.Yellow)))
				using(Brush txtBrush = new SolidBrush(Color.Black))
				{
					string label = $"#{obj.Id}";
					var size = g.MeasureString(label, font);
					float lx = (float)(obj.CenterX - size.Width / 2);
					float ly = (float)(obj.CenterY - obj.Radius - size.Height - 10);
					if(ly < 5) ly = (float)(obj.CenterY + obj.Radius + 5);
					g.FillRectangle(bgBrush, lx - 2, ly - 2, size.Width + 4, size.Height + 4);
					g.DrawString(label, font, txtBrush, lx, ly);
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
												Confidence = d.Confidence, // [Fix] 確保分數能被正確顯示
	                                    Area    = d.Type == "圓形" ? Math.PI * d.Confidence * d.Confidence : d.BoundingBox != null && d.BoundingBox.Length >= 4 ? d.BoundingBox[2] * d.BoundingBox[3] : 0,
	                                    Status  = "OK",
	                                    Angle   = d.Angle,
	                                    Circularity = d.Circularity,
	                                    Rectangularity = d.Rectangularity,
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
			if(inputMat.Channels() >= 3) Cv2.CvtColor(inputMat, inputMat, ColorConversionCodes.RGB2BGR);
			Mat roiMask  = null;

			try
			{
				// [新增] 收集先前步驟的缺陷資料，用於 Context 注入 (例如測量工具參考物件 ID)
				var accumulatedDefects = new List<Defect>();
				for(int i = 0; i < stepIndex; i++)
				{
					if(_sequence[i].LastDefects != null)
					{
						accumulatedDefects.AddRange(_sequence[i].LastDefects);
					}
				}
				// [新增] 將收集到的缺陷注入到當前工具參數中
				InjectContext(item.Parameters, accumulatedDefects);

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

								// [Fix] 若原圖是灰階 (1ch) 但結果是彩色 (3ch/4ch)，必須將基底圖轉為彩色才能顯示顏色
								if(dstCh == 1 && srcCh >= 3)
								{
									finalResult = new Mat();
									Cv2.CvtColor(inputMat, finalResult, ColorConversionCodes.GRAY2BGR);
									
									// 如果結果是 4ch，轉為 3ch 以便合併
									if(srcCh == 4)
									{
										Mat bgrResult = new Mat();
										Cv2.CvtColor(resultToMerge, bgrResult, ColorConversionCodes.BGRA2BGR);
										if(resultToMerge != result.ResultImage) resultToMerge.Dispose();
										resultToMerge = bgrResult;
									}
								}
								else
								{
									finalResult = inputMat.Clone();

									if(dstCh != srcCh)
									{
										resultToMerge = new Mat();
										if(dstCh == 4)
										{
											if(srcCh == 1) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGRA);
											else if(srcCh == 3) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGR2BGRA);
										}
										else if(dstCh == 3)
										{
											if(srcCh == 1) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGR);
											else if(srcCh == 4) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGRA2BGR);
										}
									}
								}

								// 將結果的 ROI 區域複製到原圖上
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
					imageViewer.SetImagePreserveView((Bitmap)item.LastResultImage.Clone());
					RefreshInfoPanel(swStep.ElapsedMilliseconds);

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
			if(currentMat.Channels() >= 3) Cv2.CvtColor(currentMat, currentMat, ColorConversionCodes.RGB2BGR);
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

			// 收集先前步驟的缺陷上下文
			var accumulatedDefects = new List<Defect>();
			for(int i = 0; i < startIndex; i++)
			{
				if(_sequence[i].LastDefects != null)
				{
					accumulatedDefects.AddRange(_sequence[i].LastDefects);
				}
			}

			try
			{
				for(int i = startIndex; i < _sequence.Count; i++)
				{
					InspectionItem  item = _sequence[i];
					DataGridViewRow row  = dgvSequence.Rows[i];
					row.Cells[2].Value = "Running...";
					Application.DoEvents();
					Stopwatch swStep = Stopwatch.StartNew();
					
					// 注入
					InjectContext(item.Parameters, accumulatedDefects);

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

							accumulatedDefects.AddRange(item.LastDefects);
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
							imageViewer.SetImagePreserveView((Bitmap)item.LastResultImage.Clone());
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

		/// <summary>開啟批次處理對話框</summary>
		private void OpenBatchProcessing()
		{
			using (var form = new BatchProcessingForm())
			{
				form.ShowDialog(this);
			}
		}

		/// <summary>開啟配方管理器對話框</summary>
		private void OpenRecipeManager()
		{
			using (var form = new RecipeManagerForm())
			{
				if (form.ShowDialog(this) == DialogResult.OK && form.SelectedRecipe != null)
				{
					// 載入選取的配方到工具序列
					LoadRecipeToSequence(form.SelectedRecipe);
				}
			}
		}

		/// <summary>載入配方到工具序列</summary>
		private void LoadRecipeToSequence(Core.Models.Recipe recipe)
		{
			if (recipe == null || recipe.Steps == null || recipe.Steps.Count == 0)
			{
				Log("配方不包含任何步驟", TraceLevel.Warning);
				return;
			}

			// 清空現有序列
			_sequence.Clear();
			dgvSequence.Rows.Clear();

			var allTools = VisionToolFactory.GetAllTools();

			foreach (var step in recipe.Steps)
			{
				if (!step.Enabled) continue;

				var toolDef = allTools.FirstOrDefault(t => t.Name == step.ToolName);
				if (toolDef == null)
				{
					Log($"找不到工具: {step.ToolName}", TraceLevel.Warning);
					continue;
				}

				var item = new InspectionItem
				{
					Name       = toolDef.Name,
					Action     = toolDef.Action,
					Parameters = Activator.CreateInstance(toolDef.DefaultParameters.GetType()),
				};
				CopyProperties(toolDef.DefaultParameters, item.Parameters);
				_sequence.Add(item);
				int idx = dgvSequence.Rows.Add(item.Name, "", "Wait");
				dgvSequence.Rows[idx].Tag = item;
			}

			Log($"已載入配方: {recipe.Name} ({recipe.Steps.Count} 步驟)", TraceLevel.Info);
		}

		/// <summary>切換循環執行狀態</summary>
		private void ToggleLoopExecution(int stepIndex)
		{
			if (_isLooping && _loopStepIndex == stepIndex)
			{
				// 正在循環同一個步驟，停止它
				StopLoopExecution();
			}
			else
			{
				// 開始新的循環
				StopLoopExecution(); // 先停止之前的
				StartLoopExecution(stepIndex);
			}
		}

		/// <summary>開始循環執行指定步驟</summary>
		private void StartLoopExecution(int stepIndex)
		{
			if (imageViewer.Image == null)
			{
				Log("請先載入影像", TraceLevel.Error);
				return;
			}

			if (stepIndex < 0 || stepIndex >= _sequence.Count) return;

			_loopStepIndex = stepIndex;
			_isLooping     = true;

			// 更新 UI 顯示循環中
			var row = dgvSequence.Rows[stepIndex];
			row.Cells["colLoop"].Value = "■";
			row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;

			// 初始化計時器 (已改用 async/await 迴圈，保留此處為了相容性，實際不啟動)
			if (_loopTimer == null)
			{
				_loopTimer = new System.Windows.Forms.Timer { Interval = 200 };
			}
			
			// 啟動非同步循環
			RunLoopAsync(stepIndex);

			Log($"開始循環執行步驟 [{_sequence[stepIndex].Name}]", TraceLevel.Info);
		}

		/// <summary>非同步循環執行核心</summary>
		private async void RunLoopAsync(int stepIndex)
		{
			try
			{
				while (_isLooping && _loopStepIndex == stepIndex)
				{
					// 執行到指定步驟
					RunToStep(stepIndex);

					// 等待一段時間再執行下一次，保持 UI 響應
					await Task.Delay(200);

					// 檢查是否已被停止
					if (!_isLooping || _loopStepIndex != stepIndex) break;
				}
			}
			catch (Exception ex)
			{
				Log($"循環執行發生異常: {ex.Message}", TraceLevel.Error);
				StopLoopExecution();
			}
		}

		/// <summary>停止循環執行</summary>
		private void StopLoopExecution()
		{
			if (!_isLooping) return;

			_isLooping = false;
			_loopTimer?.Stop(); // 確保 Timer 停止 (如果還有遺留)

			// 還原 UI
			if (_loopStepIndex >= 0 && _loopStepIndex < dgvSequence.Rows.Count)
			{
				var row = dgvSequence.Rows[_loopStepIndex];
				row.Cells["colLoop"].Value = "↻";
				row.DefaultCellStyle.BackColor = System.Drawing.Color.White;
			}

			Log($"停止循環執行", TraceLevel.Info);
			_loopStepIndex = -1;
		}

		/// <summary>循環計時器事件 (已棄用，保留避免編譯錯誤)</summary>
		private void LoopTimer_Tick(object sender, EventArgs e)
		{
			// Legacy support or fallback
		}

		private void InjectContext(object parameters, List<Defect> defects)
		{
			if(parameters == null || defects == null) return;
			try
			{
				var prop = parameters.GetType().GetProperty("ContextDefects");
				if(prop != null)
				{
					prop.SetValue(parameters, new List<Defect>(defects)); // Clone/Copy reference
				}
			}
			catch { }
		}

		/// <summary>執行到指定步驟 (從原始影像開始)</summary>
		private void RunToStep(int targetStep)
		{
			if (_originalImage == null || targetStep < 0 || targetStep >= _sequence.Count) return;

			// 停止計時器避免重入
			_loopTimer?.Stop();

			try
			{
				Mat currentMat = _originalImage.ToMat();
				if(currentMat.Channels() >= 3) Cv2.CvtColor(currentMat, currentMat, ColorConversionCodes.RGB2BGR);
				Mat roiMask    = null;
				
				// [Fix] 建立 ROI 遮罩
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

				var sw         = System.Diagnostics.Stopwatch.StartNew();
				var accumulatedDefects = new List<Defect>(); // 累積所有步驟產生的缺陷上下文

				for (int i = 0; i <= targetStep; i++)
				{
					var item = _sequence[i];
					var row  = dgvSequence.Rows[i];
					
					Application.DoEvents(); // [Fix] 保持 UI 回應

					try
					{
						// 注入上下文缺陷
						InjectContext(item.Parameters, accumulatedDefects);

						Mat inputToTool = currentMat;
						Mat maskedInput = null;

						if(roiMask != null)
						{
							maskedInput = new Mat();
							currentMat.CopyTo(maskedInput, roiMask);
							inputToTool = maskedInput;
						}

						var result = item.Action(inputToTool, item.Parameters);
						sw.Stop();
						row.Cells["colTime"].Value = $"{sw.ElapsedMilliseconds}ms";

						if (result.IsOk)
						{
							row.Cells["colStatus"].Value           = "OK";
							row.Cells["colStatus"].Style.ForeColor = System.Drawing.Color.Green;

							// 如果有 ROI，將結果合併回原始影像
							Mat finalResult = result.ResultImage;

							if(roiMask != null)
							{
								// 處理通道合併 (參考 RunSequence)
								Mat resultToMerge = result.ResultImage;
								int dstCh = currentMat.Channels();
								int srcCh = result.ResultImage.Channels();

								// [Fix] 若原圖是灰階 (1ch) 但結果是彩色 (3ch/4ch)，必須將基底圖轉為彩色才能顯示顏色
								if(dstCh == 1 && srcCh >= 3)
								{
									finalResult = new Mat();
									Cv2.CvtColor(currentMat, finalResult, ColorConversionCodes.GRAY2BGR);
									
									if(srcCh == 4)
									{
										Mat bgrResult = new Mat();
										Cv2.CvtColor(resultToMerge, bgrResult, ColorConversionCodes.BGRA2BGR);
										if(resultToMerge != result.ResultImage) resultToMerge.Dispose();
										resultToMerge = bgrResult;
									}
								}
								else
								{
									finalResult = currentMat.Clone(); // Default 

									if(dstCh != srcCh)
									{
										resultToMerge = new Mat();
										if(dstCh == 4 && srcCh == 1) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGRA);
										else if(dstCh == 4 && srcCh == 3) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGR2BGRA);
										else if(dstCh == 3 && srcCh == 1) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.GRAY2BGR);
										else if(dstCh == 3 && srcCh == 4) Cv2.CvtColor(result.ResultImage, resultToMerge, ColorConversionCodes.BGRA2BGR);
									}
								}

								resultToMerge.CopyTo(finalResult, roiMask);

								if(resultToMerge != result.ResultImage) resultToMerge.Dispose();
								result.ResultImage.Dispose();
							}

							item.LastResultImage?.Dispose();
							item.LastResultImage = finalResult.ToBitmap();
							item.LastDefects     = result.Defects ?? new List<Defect>();
							
							// 累積缺陷
							accumulatedDefects.AddRange(item.LastDefects);

							var oldMat = currentMat;
							currentMat = finalResult;
							if (oldMat != finalResult) oldMat.Dispose();
						}
						else
						{
							row.Cells["colStatus"].Value           = "NG";
							row.Cells["colStatus"].Style.ForeColor = System.Drawing.Color.Red;
							result.ResultImage?.Dispose();
						}
						
						maskedInput?.Dispose();
						sw.Restart();
					}
					catch (Exception ex)
					{
						row.Cells["colStatus"].Value = "ERR";
						Log($"步驟 {item.Name} 執行錯誤: {ex.Message}", TraceLevel.Error);
						break;
					}
				}

				// 顯示結果
				var lastItem = _sequence[targetStep];
				if (lastItem.LastResultImage != null)
				{
					imageViewer.SetImagePreserveView((Bitmap)lastItem.LastResultImage.Clone());
				}
				
				RefreshInfoPanel(sw.ElapsedMilliseconds); // 更新右側面板

				currentMat?.Dispose();
				roiMask?.Dispose();
			}
			catch (Exception ex)
			{
				Log($"執行錯誤: {ex.Message}", TraceLevel.Error);
			}
			finally
			{
				// 重新啟動計時器
				if (_isLooping)
				{
					_loopTimer?.Start();
				}
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