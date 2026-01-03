using System.Windows.Forms;
using System.Drawing;

namespace PCBInspection.UI.Controls
{
	partial class DefectLabelingControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.pnlTop = new System.Windows.Forms.Panel();
			this.lblType = new System.Windows.Forms.Label();
			this.cbType = new System.Windows.Forms.ComboBox();
			this.lblSeverity = new System.Windows.Forms.Label();
			this.cbSeverity = new System.Windows.Forms.ComboBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.dgvLabels = new System.Windows.Forms.DataGridView();
			this.pnlBottom = new System.Windows.Forms.Panel();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();

			((System.ComponentModel.ISupportInitialize)(this.dgvLabels)).BeginInit();
			this.pnlTop.SuspendLayout();
			this.pnlBottom.SuspendLayout();
			this.SuspendLayout();

			// pnlTop
			this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlTop.Height = 40;
			this.pnlTop.Controls.Add(this.btnAdd);
			this.pnlTop.Controls.Add(this.cbSeverity);
			this.pnlTop.Controls.Add(this.lblSeverity);
			this.pnlTop.Controls.Add(this.cbType);
			this.pnlTop.Controls.Add(this.lblType);

			// lblType
			this.lblType.AutoSize = true;
			this.lblType.Location = new System.Drawing.Point(5, 12);
			this.lblType.Text = "類型:";

			// cbType
			this.cbType.Location = new System.Drawing.Point(40, 8);
			this.cbType.Width = 100;
			this.cbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

			// lblSeverity
			this.lblSeverity.AutoSize = true;
			this.lblSeverity.Location = new System.Drawing.Point(150, 12);
			this.lblSeverity.Text = "嚴重度:";

			// cbSeverity
			this.cbSeverity.Location = new System.Drawing.Point(200, 8);
			this.cbSeverity.Width = 80;
			this.cbSeverity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbSeverity.Items.AddRange(new object[] { "1 (輕微)", "2 (一般)", "3 (嚴重)" });
			this.cbSeverity.SelectedIndex = 1;

			// btnAdd
			this.btnAdd.Location = new System.Drawing.Point(290, 7);
			this.btnAdd.Text = "手動加入";
			this.btnAdd.Size = new Size(75, 25);
			// 實際上主要透過 Viewer 畫圖加入，此按鈕可選

			// pnlBottom
			this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlBottom.Height = 40;
			this.pnlBottom.Controls.Add(this.btnDelete);
			this.pnlBottom.Controls.Add(this.btnSave);

			// btnSave
			this.btnSave.Location = new System.Drawing.Point(5, 8);
			this.btnSave.Text = "儲存標註";
			this.btnSave.Size = new Size(80, 25);
			this.btnSave.BackColor = Color.LightGreen;

			// btnDelete
			this.btnDelete.Location = new System.Drawing.Point(90, 8);
			this.btnDelete.Text = "刪除選取";
			this.btnDelete.Size = new Size(80, 25);

			// dgvLabels
			this.dgvLabels.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvLabels.AllowUserToAddRows = false;
			this.dgvLabels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			this.dgvLabels.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvLabels.BackgroundColor = Color.White;

			// Col1 ID
			var colId = new DataGridViewTextBoxColumn();
			colId.Name = "colId";
			colId.HeaderText = "ID";
			colId.ReadOnly = true;
			colId.FillWeight = 30;

			// Col2 Type
			var colType = new DataGridViewTextBoxColumn();
			colType.Name = "colType";
			colType.HeaderText = "類型";

			// Col3 Severity
			var colSev = new DataGridViewTextBoxColumn();
			colSev.Name = "colSeverity";
			colSev.HeaderText = "等級";
			colSev.FillWeight = 50;
			
			this.dgvLabels.Columns.AddRange(new DataGridViewColumn[] { colId, colType, colSev });

			// Control
			this.Controls.Add(this.dgvLabels);
			this.Controls.Add(this.pnlTop);
			this.Controls.Add(this.pnlBottom);
			this.Size = new System.Drawing.Size(400, 500);

			((System.ComponentModel.ISupportInitialize)(this.dgvLabels)).EndInit();
			this.pnlTop.ResumeLayout(false);
			this.pnlBottom.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private System.Windows.Forms.Panel pnlTop;
		private System.Windows.Forms.Label lblType;
		private System.Windows.Forms.ComboBox cbType;
		private System.Windows.Forms.Label lblSeverity;
		private System.Windows.Forms.ComboBox cbSeverity;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.DataGridView dgvLabels;
		private System.Windows.Forms.Panel pnlBottom;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnDelete;
	}
}
