namespace PCBInspection.UI
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this.btnTsNew = new System.Windows.Forms.ToolStripButton();
            this.btnTsOpen = new System.Windows.Forms.ToolStripButton();
            this.btnTsSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsRunOnce = new System.Windows.Forms.ToolStripButton();
            this.btnTsRunLoop = new System.Windows.Forms.ToolStripButton();
            this.btnTsStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsSettings = new System.Windows.Forms.ToolStripButton();
            // New Buttons
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsZoomIn = new System.Windows.Forms.ToolStripButton();
            this.btnTsZoomOut = new System.Windows.Forms.ToolStripButton();
            this.btnTsFit = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsPointer = new System.Windows.Forms.ToolStripButton();
            this.btnTsRoiRect = new System.Windows.Forms.ToolStripButton();
            this.btnTsRoiCircle = new System.Windows.Forms.ToolStripButton();
            this.btnTsRoiPoly = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsUndo = new System.Windows.Forms.ToolStripButton();
            this.btnTsRedo = new System.Windows.Forms.ToolStripButton();

            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatusMain = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusTime = new System.Windows.Forms.ToolStripStatusLabel();
            
            // ... (Skip unchanged) ...

            // 
            // toolStripMain
            // 
            this.toolStripMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnTsNew,
            this.btnTsOpen,
            this.btnTsSave,
            this.toolStripSeparator1,
            this.btnTsRunOnce,
            this.btnTsRunLoop,
            this.btnTsStop,
            this.toolStripSeparator2,
            this.btnTsSettings,
            this.toolStripSeparator3,
            this.btnTsZoomIn,
            this.btnTsZoomOut,
            this.btnTsFit,
            this.toolStripSeparator4,
            this.btnTsPointer,
            this.btnTsRoiRect,
            this.btnTsRoiCircle,
            this.btnTsRoiPoly,
            this.toolStripSeparator5,
            this.btnTsUndo,
            this.btnTsRedo});
            this.toolStripMain.Location = new System.Drawing.Point(0, 0);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Size = new System.Drawing.Size(1264, 35);
            this.toolStripMain.TabIndex = 0;

            // Define Texts/Icons for new buttons
            this.btnTsZoomIn.Text = "+";
            this.btnTsZoomIn.ForeColor = System.Drawing.Color.White;
            this.btnTsZoomOut.Text = "-";
            this.btnTsZoomOut.ForeColor = System.Drawing.Color.White;
            this.btnTsFit.Text = "[ ]";
            this.btnTsFit.ForeColor = System.Drawing.Color.White;
            
            this.btnTsPointer.Text = "指標";
            this.btnTsPointer.ForeColor = System.Drawing.Color.White;
            this.btnTsRoiRect.Text = "□";
            this.btnTsRoiRect.ForeColor = System.Drawing.Color.White;
            this.btnTsRoiCircle.Text = "○";
            this.btnTsRoiCircle.ForeColor = System.Drawing.Color.White;
            this.btnTsRoiPoly.Text = "⬡";
            this.btnTsRoiPoly.ForeColor = System.Drawing.Color.White;

            this.btnTsUndo.Text = "↩";
            this.btnTsUndo.ForeColor = System.Drawing.Color.White;
            this.btnTsRedo.Text = "↪";
            this.btnTsRedo.ForeColor = System.Drawing.Color.White;

            // 
            // btnTsNew
            // 
            this.btnTsNew.ForeColor = System.Drawing.Color.White;
            this.btnTsNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTsNew.Name = "btnTsNew";
            this.btnTsNew.Padding = new System.Windows.Forms.Padding(5);
            this.btnTsNew.Size = new System.Drawing.Size(43, 32);
            this.btnTsNew.Text = "新建";
            // 
            // btnTsOpen
            // 
            this.btnTsOpen.ForeColor = System.Drawing.Color.White;
            this.btnTsOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTsOpen.Name = "btnTsOpen";
            this.btnTsOpen.Padding = new System.Windows.Forms.Padding(5);
            this.btnTsOpen.Size = new System.Drawing.Size(43, 32);
            this.btnTsOpen.Text = "開啟";
            // 
            // btnTsSave
            // 
            this.btnTsSave.ForeColor = System.Drawing.Color.White;
            this.btnTsSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTsSave.Name = "btnTsSave";
            this.btnTsSave.Padding = new System.Windows.Forms.Padding(5);
            this.btnTsSave.Size = new System.Drawing.Size(43, 32);
            this.btnTsSave.Text = "儲存";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 35);
            // 
            // btnTsRunOnce
            // 
            this.btnTsRunOnce.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(255)))), ((int)(((byte)(100)))));
            this.btnTsRunOnce.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTsRunOnce.Name = "btnTsRunOnce";
            this.btnTsRunOnce.Padding = new System.Windows.Forms.Padding(5);
            this.btnTsRunOnce.Size = new System.Drawing.Size(51, 32);
            this.btnTsRunOnce.Text = "► 執行";
            // 
            // btnTsRunLoop
            // 
            this.btnTsRunLoop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(255)))), ((int)(((byte)(100)))));
            this.btnTsRunLoop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTsRunLoop.Name = "btnTsRunLoop";
            this.btnTsRunLoop.Padding = new System.Windows.Forms.Padding(5);
            this.btnTsRunLoop.Size = new System.Drawing.Size(63, 32);
            this.btnTsRunLoop.Text = "↻ 循環";
            // 
            // btnTsStop
            // 
            this.btnTsStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnTsStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTsStop.Name = "btnTsStop";
            this.btnTsStop.Padding = new System.Windows.Forms.Padding(5);
            this.btnTsStop.Size = new System.Drawing.Size(51, 32);
            this.btnTsStop.Text = "■ 停止";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 35);
            // 
            // btnTsSettings
            // 
            this.btnTsSettings.ForeColor = System.Drawing.Color.White;
            this.btnTsSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTsSettings.Name = "btnTsSettings";
            this.btnTsSettings.Padding = new System.Windows.Forms.Padding(5);
            this.btnTsSettings.Size = new System.Drawing.Size(43, 32);
            this.btnTsSettings.Text = "設定";
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusMain,
            this.lblStatusTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 789);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1264, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblStatusMain
            // 
            this.lblStatusMain.ForeColor = System.Drawing.Color.White;
            this.lblStatusMain.Name = "lblStatusMain";
            this.lblStatusMain.Size = new System.Drawing.Size(32, 17);
            this.lblStatusMain.Text = "就緒";
            // 
            // lblStatusTime
            // 
            this.lblStatusTime.ForeColor = System.Drawing.Color.White;
            this.lblStatusTime.Name = "lblStatusTime";
            this.lblStatusTime.Size = new System.Drawing.Size(0, 17);
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 35);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerLeft);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.panelCenter);
            this.splitContainerMain.Size = new System.Drawing.Size(1264, 754);
            this.splitContainerMain.SplitterDistance = 350;
            this.splitContainerMain.TabIndex = 2;
            // 
            // splitContainerLeft
            // 
            this.splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLeft.Name = "splitContainerLeft";
            this.splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLeft.Panel1
            // 
            this.splitContainerLeft.Panel1.Controls.Add(this.tabToolbox);
            // 
            // splitContainerLeft.Panel2
            // 
            this.splitContainerLeft.Panel2.Controls.Add(this.grpProperties);
            this.splitContainerLeft.Panel2.Controls.Add(this.pnlFlowControl);
            this.splitContainerLeft.Panel2.Controls.Add(this.dgvSequence);
            this.splitContainerLeft.Size = new System.Drawing.Size(350, 754);
            this.splitContainerLeft.SplitterDistance = 300;
            this.splitContainerLeft.TabIndex = 0;
            // 
            // tabToolbox
            // 
            this.tabToolbox.Controls.Add(this.tabPageTools);
            this.tabToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabToolbox.Location = new System.Drawing.Point(0, 0);
            this.tabToolbox.Name = "tabToolbox";
            this.tabToolbox.SelectedIndex = 0;
            this.tabToolbox.Size = new System.Drawing.Size(350, 300);
            this.tabToolbox.TabIndex = 0;
            // 
            // tabPageTools
            // 
            this.tabPageTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tabPageTools.Controls.Add(this.tvTools);
            this.tabPageTools.Location = new System.Drawing.Point(4, 22);
            this.tabPageTools.Name = "tabPageTools";
            this.tabPageTools.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTools.Size = new System.Drawing.Size(342, 274);
            this.tabPageTools.TabIndex = 0;
            this.tabPageTools.Text = "工具箱";
            // 
            // tvTools
            // 
            this.tvTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.tvTools.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTools.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F);
            this.tvTools.ForeColor = System.Drawing.Color.White;
            this.tvTools.LineColor = System.Drawing.Color.Silver;
            this.tvTools.Location = new System.Drawing.Point(3, 3);
            this.tvTools.Name = "tvTools";
            this.tvTools.Size = new System.Drawing.Size(336, 268);
            this.tvTools.TabIndex = 0;
            // 
            // grpProperties
            // 
            this.grpProperties.Controls.Add(this.propertyGrid);
            this.grpProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpProperties.ForeColor = System.Drawing.Color.White;
            this.grpProperties.Location = new System.Drawing.Point(0, 224);
            this.grpProperties.Name = "grpProperties";
            this.grpProperties.Size = new System.Drawing.Size(350, 226);
            this.grpProperties.TabIndex = 2;
            this.grpProperties.TabStop = false;
            this.grpProperties.Text = "參數設定";
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.HelpBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(70)))));
            this.propertyGrid.HelpForeColor = System.Drawing.Color.White;
            this.propertyGrid.Location = new System.Drawing.Point(3, 19);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(344, 204);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.ViewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.propertyGrid.ViewForeColor = System.Drawing.Color.White;
            // 
            // pnlFlowControl
            // 
            this.pnlFlowControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.pnlFlowControl.Controls.Add(this.btnRemoveTool);
            this.pnlFlowControl.Controls.Add(this.btnMoveDown);
            this.pnlFlowControl.Controls.Add(this.btnMoveUp);
            this.pnlFlowControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFlowControl.Location = new System.Drawing.Point(0, 194);
            this.pnlFlowControl.Name = "pnlFlowControl";
            this.pnlFlowControl.Size = new System.Drawing.Size(350, 30);
            this.pnlFlowControl.TabIndex = 1;
            // 
            // btnRemoveTool
            // 
            this.btnRemoveTool.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveTool.ForeColor = System.Drawing.Color.White;
            this.btnRemoveTool.Location = new System.Drawing.Point(298, 3);
            this.btnRemoveTool.Name = "btnRemoveTool";
            this.btnRemoveTool.Size = new System.Drawing.Size(40, 23);
            this.btnRemoveTool.TabIndex = 2;
            this.btnRemoveTool.Text = "-";
            this.btnRemoveTool.UseVisualStyleBackColor = true;
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMoveDown.ForeColor = System.Drawing.Color.White;
            this.btnMoveDown.Location = new System.Drawing.Point(50, 3);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(40, 23);
            this.btnMoveDown.TabIndex = 1;
            this.btnMoveDown.Text = "↓";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMoveUp.ForeColor = System.Drawing.Color.White;
            this.btnMoveUp.Location = new System.Drawing.Point(4, 3);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(40, 23);
            this.btnMoveUp.TabIndex = 0;
            this.btnMoveUp.Text = "↑";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            // 
            // dgvSequence
            // 
            this.dgvSequence.AllowUserToAddRows = false;
            this.dgvSequence.AllowUserToDeleteRows = false;
            this.dgvSequence.AllowUserToResizeRows = false;
            this.dgvSequence.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.dgvSequence.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvSequence.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvSequence.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSequence.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvSequence.ColumnHeadersHeight = 30;
            this.dgvSequence.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvSequence.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStepName,
            this.colTime,
            this.colStatus});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvSequence.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvSequence.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvSequence.EnableHeadersVisualStyles = false;
            this.dgvSequence.GridColor = System.Drawing.Color.Gray;
            this.dgvSequence.Location = new System.Drawing.Point(0, 0);
            this.dgvSequence.MultiSelect = false;
            this.dgvSequence.Name = "dgvSequence";
            this.dgvSequence.ReadOnly = true;
            this.dgvSequence.RowHeadersVisible = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            this.dgvSequence.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvSequence.RowTemplate.Height = 24;
            this.dgvSequence.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSequence.Size = new System.Drawing.Size(350, 194);
            this.dgvSequence.TabIndex = 0;
            // 
            // colStepName
            // 
            this.colStepName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStepName.HeaderText = "步驟名稱";
            this.colStepName.Name = "colStepName";
            this.colStepName.ReadOnly = true;
            // 
            // colTime
            // 
            this.colTime.HeaderText = "耗時";
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            this.colTime.Width = 60;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "狀態";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 60;
            // 
            // panelCenter
            // 
            this.panelCenter.Controls.Add(this.panelImage);
            this.panelCenter.Controls.Add(this.panelLog);
            this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCenter.Location = new System.Drawing.Point(0, 0);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Size = new System.Drawing.Size(910, 754);
            this.panelCenter.TabIndex = 0;
            // 
            // panelImage
            // 
            this.panelImage.BackColor = System.Drawing.Color.Black;
            this.panelImage.Controls.Add(this.imageViewer);
            this.panelImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelImage.Location = new System.Drawing.Point(0, 0);
            this.panelImage.Name = "panelImage";
            this.panelImage.Size = new System.Drawing.Size(910, 554);
            this.panelImage.TabIndex = 0;
            // 
            // imageViewer
            // 
            this.imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewer.Location = new System.Drawing.Point(0, 0);
            this.imageViewer.Name = "imageViewer";
            this.imageViewer.Size = new System.Drawing.Size(910, 554);
            this.imageViewer.TabIndex = 0;
            this.imageViewer.BackColor = System.Drawing.Color.FromArgb(30,30,30);
            // 
            // panelLog
            // 
            this.panelLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelLog.Controls.Add(this.lstLog);
            this.panelLog.Controls.Add(this.lblLogTitle);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLog.Location = new System.Drawing.Point(0, 554);
            this.panelLog.Name = "panelLog";
            this.panelLog.Size = new System.Drawing.Size(910, 200);
            this.panelLog.TabIndex = 1;
            // 
            // lstLog
            // 
            this.lstLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstLog.ForeColor = System.Drawing.Color.LimeGreen;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 16;
            this.lstLog.Location = new System.Drawing.Point(0, 20);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(910, 180);
            this.lstLog.TabIndex = 1;
            // 
            // lblLogTitle
            // 
            this.lblLogTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogTitle.ForeColor = System.Drawing.Color.White;
            this.lblLogTitle.Location = new System.Drawing.Point(0, 0);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblLogTitle.Size = new System.Drawing.Size(910, 20);
            this.lblLogTitle.TabIndex = 0;
            this.lblLogTitle.Text = "日誌訊息";
            this.lblLogTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(1264, 811);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStripMain);
            this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PCB Vision Builder";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerLeft.Panel1.ResumeLayout(false);
            this.splitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).EndInit();
            this.splitContainerLeft.ResumeLayout(false);
            this.tabToolbox.ResumeLayout(false);
            this.tabPageTools.ResumeLayout(false);
            this.grpProperties.ResumeLayout(false);
            this.pnlFlowControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSequence)).EndInit();
            this.panelCenter.ResumeLayout(false);
            this.panelImage.ResumeLayout(false);
            this.panelLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // Top
        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton btnTsNew;
        private System.Windows.Forms.ToolStripButton btnTsOpen;
        private System.Windows.Forms.ToolStripButton btnTsSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnTsRunOnce;
        private System.Windows.Forms.ToolStripButton btnTsRunLoop;
        private System.Windows.Forms.ToolStripButton btnTsStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnTsSettings;

        // Bottom
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusMain;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusTime;

        // Main Layout
        private System.Windows.Forms.SplitContainer splitContainerMain;
        
        // Left Panel (Toolbox & Sequence)
        private System.Windows.Forms.SplitContainer splitContainerLeft;
        private System.Windows.Forms.TabControl tabToolbox;
        private System.Windows.Forms.TabPage tabPageTools;
        private System.Windows.Forms.TreeView tvTools;
        
        private System.Windows.Forms.DataGridView dgvSequence;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.Panel pnlFlowControl;
        private System.Windows.Forms.Button btnRemoveTool;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;

        private System.Windows.Forms.GroupBox grpProperties;
        private System.Windows.Forms.PropertyGrid propertyGrid;

        // Center Panel (Results & Log)
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.Panel panelImage;
        // Replace PictureBox with Custom Control
        private PCBInspection.UI.Controls.InteractiveImageViewer imageViewer;
        
        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.Label lblLogTitle;
        private System.Windows.Forms.ListBox lstLog;

        // Add Toolbar Buttons
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnTsZoomIn;
        private System.Windows.Forms.ToolStripButton btnTsZoomOut;
        private System.Windows.Forms.ToolStripButton btnTsFit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnTsRoiRect;
        private System.Windows.Forms.ToolStripButton btnTsRoiCircle;
        private System.Windows.Forms.ToolStripButton btnTsRoiPoly;
        private System.Windows.Forms.ToolStripButton btnTsPointer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnTsUndo;
        private System.Windows.Forms.ToolStripButton btnTsRedo;
    }
}
