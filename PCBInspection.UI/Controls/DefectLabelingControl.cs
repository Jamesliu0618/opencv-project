using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PCBInspection.Core.ROI;
using PCBInspection.Core.Services;

namespace PCBInspection.UI.Controls
{
	/// <summary>
	/// 缺陷標註控制項
	/// 負責管理手動標註功能，與 InteractiveImageViewer 互動
	/// </summary>
	public partial class DefectLabelingControl : UserControl
	{
		private InteractiveImageViewer _viewer;
		private string _currentImagePath;
		private LabelingService _service;
		private List<LabelItem> _labels;

		public DefectLabelingControl()
		{
			InitializeComponent();
			_service = new LabelingService();
			_labels = new List<LabelItem>();

			// 綁定 UI 事件
			btnAdd.Click += BtnAdd_Click;
			btnDelete.Click += BtnDelete_Click;
			btnSave.Click += BtnSave_Click;
			dgvLabels.SelectionChanged += DgvLabels_SelectionChanged;

			// 預設缺陷類型
			cbType.Items.AddRange(new object[] { "刮傷", "汙點", "缺件", "短路", "斷路", "其他" });
			if(cbType.Items.Count > 0) cbType.SelectedIndex = 0;
		}

		/// <summary>
		/// 初始化設定
		/// </summary>
		/// <param name="viewer">影像檢視器實例</param>
		public void Setup(InteractiveImageViewer viewer)
		{
			_viewer = viewer;
			if (_viewer != null)
			{
				// 訂閱 ROI 建立事件
				_viewer.RoiCreated += OnRoiCreated;
			}
		}

		/// <summary>
		/// 載入影像的標註資料
		/// </summary>
		/// <param name="imagePath">影像檔案路徑</param>
		public void LoadImageLabels(string imagePath)
		{
			_currentImagePath = imagePath;
			dgvLabels.Rows.Clear();
			_labels.Clear();

			if (string.IsNullOrEmpty(imagePath)) return;

			// 透過服務載入標註
			_labels = _service.LoadLabels(imagePath);

			// 更新列表與檢視器
			foreach (var label in _labels)
			{
				AddLabelToGrid(label);
				// TODO: 在 Viewer 上繪製這些已存在的標註 ROI (若需要)
				// 目前 Viewer 的 Rois 清單可能主要用於當前繪製與測量
				// 若要顯示標註，可能需要再 Viewer 增加專門的顯示層或直接加入 Rois 並設為鎖定
				AddRoiToViewer(label);
			}
		}

		/// <summary>
		/// 將標註物件轉換為 ROI 並加入檢視器
		/// </summary>
		private void AddRoiToViewer(LabelItem label)
		{
			if (_viewer == null || label.BoundingBox == null || label.BoundingBox.Length < 4) return;

			// 建立對應的 ROI
			var rect = new RectangleF(label.BoundingBox[0], label.BoundingBox[1], label.BoundingBox[2], label.BoundingBox[3]);
			var roi = new RectangleRoi(rect.X, rect.Y, rect.Width, rect.Height);
			// 可以設定 Tag 或其他屬性來連結 ID
			
			_viewer.Rois.Add(roi);
			_viewer.Invalidate();
		}

		/// <summary>
		/// 處理 Viewer 的 ROI 建立事件
		/// </summary>
		private void OnRoiCreated(RoiBase roi)
		{
			// 僅在標註模式下且有選取類型時處理
			if (_viewer.Mode != InteractiveImageViewer.ViewerMode.Labeling) return;

			// 取得目前設定的類型與嚴重度
			string type = cbType.Text;
			string severity = cbSeverity.Text;

			// 建立新標註資料
			var label = new LabelItem
			{
				Type = type,
				Severity = severity,
			};

			if (roi is RectangleRoi r)
			{
				label.BoundingBox = new int[] { (int)r.Rect.X, (int)r.Rect.Y, (int)r.Rect.Width, (int)r.Rect.Height };
			}
			else if (roi is CircleRoi c)
			{
				// 圓形轉為外接矩形
				label.BoundingBox = new int[] { (int)(c.Center.X - c.Radius), (int)(c.Center.Y - c.Radius), (int)(c.Radius * 2), (int)(c.Radius * 2) };
			}
			// 多邊形暫略，先支援矩形

			// 加入資料列表
			_labels.Add(label);
			AddLabelToGrid(label);

			// 將暫時的 ROI 正式加入 Viewer 顯示
			_viewer.Rois.Add(roi);
			_viewer.Invalidate();
		}

		/// <summary>
		/// 將標註加入 Grid 顯示
		/// </summary>
		private void AddLabelToGrid(LabelItem label)
		{
			int idx = dgvLabels.Rows.Add(label.Id, label.Type, label.Severity);
			dgvLabels.Rows[idx].Tag = label;
		}

		/// <summary>
		/// 儲存按鈕點擊事件
		/// </summary>
		private void BtnSave_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(_currentImagePath) || _viewer.Image == null) return;

			try
			{
				_service.SaveLabels(_currentImagePath, _labels, _viewer.Image.Size);
				MessageBox.Show("標註儲存成功！", "資訊", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"除存失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// 刪除按鈕點擊事件
		/// </summary>
		private void BtnDelete_Click(object sender, EventArgs e)
		{
			if (dgvLabels.SelectedRows.Count == 0) return;

			foreach (DataGridViewRow row in dgvLabels.SelectedRows)
			{
				if (row.Tag is LabelItem label)
				{
					_labels.Remove(label);
					dgvLabels.Rows.Remove(row);
					
					// TODO: 同步移除 Viewer 上的 ROI
					// 這需要一些關聯機制，目前暫時重繪
				}
			}
			_viewer.Invalidate();
		}

		private void BtnAdd_Click(object sender, EventArgs e)
		{
			// 手動切換 Viewer 到標註模式，提示使用者畫框
			if (_viewer != null)
			{
				_viewer.Mode = InteractiveImageViewer.ViewerMode.Labeling;
				MessageBox.Show("請在影像上拖曳滑鼠進行標註。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void DgvLabels_SelectionChanged(object sender, EventArgs e)
		{
			// 當列表選取改變時，可以高亮 Viewer 上的對應 ROI
		}
	}
}
