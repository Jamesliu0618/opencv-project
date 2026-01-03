using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PCBInspection.UI.Controls
{
	/// <summary>
	/// 幾何座標選擇器控件
	/// 提供下拉選單選擇檢測物件中心座標，或手動輸入座標值
	/// </summary>
	public class GeometryPointSelector : UserControl
	{
		private TextBox txtX;
		private TextBox txtY;
		private ComboBox cboObjects;
		private Label lblX, lblY;

		/// <summary>物件列表資料來源</summary>
		private List<DetectedObject> _objects = new List<DetectedObject>();

		/// <summary>座標變更事件</summary>
		public event EventHandler<(double X, double Y)> CoordinateChanged;

		/// <summary>取得或設定 X 座標</summary>
		public double X
		{
			get => double.TryParse(txtX.Text, out double v) ? v : 0;
			set => txtX.Text = value.ToString("F1");
		}

		/// <summary>取得或設定 Y 座標</summary>
		public double Y
		{
			get => double.TryParse(txtY.Text, out double v) ? v : 0;
			set => txtY.Text = value.ToString("F1");
		}

		public GeometryPointSelector()
		{
			InitializeComponents();
		}

		private void InitializeComponents()
		{
			SuspendLayout();
			Height = 56;
			Font   = new Font("Microsoft JhengHei", 9f);

			// 下拉選單列
			var panelTop = new Panel { Dock = DockStyle.Top, Height = 26 };
			cboObjects = new ComboBox
			{
				Dock          = DockStyle.Fill,
				DropDownStyle = ComboBoxStyle.DropDownList,
			};
			cboObjects.Items.Add("(手動輸入)");
			cboObjects.SelectedIndex         =  0;
			cboObjects.SelectedIndexChanged += CboObjects_SelectedIndexChanged;
			panelTop.Controls.Add(cboObjects);
			Controls.Add(panelTop);

			// 座標輸入列
			var panelBottom = new TableLayoutPanel
			{
				Dock        = DockStyle.Top,
				Height      = 26,
				ColumnCount = 4,
			};
			panelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25)); // X:
			panelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			panelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25)); // Y:
			panelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

			lblX = new Label { Text = "X:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
			txtX = new TextBox { Dock = DockStyle.Fill, Text = "0" };
			txtX.TextChanged += (s, e) => OnCoordinateChanged();

			lblY = new Label { Text = "Y:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
			txtY = new TextBox { Dock = DockStyle.Fill, Text = "0" };
			txtY.TextChanged += (s, e) => OnCoordinateChanged();

			panelBottom.Controls.Add(lblX, 0, 0);
			panelBottom.Controls.Add(txtX, 1, 0);
			panelBottom.Controls.Add(lblY, 2, 0);
			panelBottom.Controls.Add(txtY, 3, 0);
			Controls.Add(panelBottom);

			ResumeLayout(false);
		}

		/// <summary>更新可選擇的檢測物件列表</summary>
		public void UpdateObjectList(List<DetectedObject> objects)
		{
			_objects = objects ?? new List<DetectedObject>();
			cboObjects.Items.Clear();
			cboObjects.Items.Add("(手動輸入)");

			foreach (var obj in _objects)
			{
				string label = $"#{obj.Id} {obj.Type} ({obj.CenterX:F0}, {obj.CenterY:F0})";
				cboObjects.Items.Add(label);
			}
			cboObjects.SelectedIndex = 0;
		}

		private void CboObjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			int idx = cboObjects.SelectedIndex;
			if (idx <= 0 || idx - 1 >= _objects.Count) return;

			var obj = _objects[idx - 1];
			txtX.Text = obj.CenterX.ToString("F1");
			txtY.Text = obj.CenterY.ToString("F1");
		}

		private void OnCoordinateChanged()
		{
			CoordinateChanged?.Invoke(this, (X, Y));
		}

		/// <summary>設定座標值</summary>
		public void SetCoordinate(double x, double y)
		{
			txtX.Text = x.ToString("F1");
			txtY.Text = y.ToString("F1");
		}
	}
}
