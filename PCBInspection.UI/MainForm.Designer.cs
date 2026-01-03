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
            this.components = new System.ComponentModel.Container();
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
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsZoomIn = new System.Windows.Forms.ToolStripButton();
            this.btnTsZoomOut = new System.Windows.Forms.ToolStripButton();
            this.btnTsFit = new System.Windows.Forms.ToolStripButton();
            this.btnTsSplit = new System.Windows.Forms.ToolStripButton(); // New Button
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsPointer = new System.Windows.Forms.ToolStripButton();
            this.btnTsRoiRect = new System.Windows.Forms.ToolStripButton();
            this.btnTsRoiCircle = new System.Windows.Forms.ToolStripButton();
            this.btnTsRoiPoly = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsUndo = new System.Windows.Forms.ToolStripButton();
            this.btnTsRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTsBatch = new System.Windows.Forms.ToolStripButton();
            this.btnTsRecipe = new System.Windows.Forms.ToolStripButton();
            
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatusMain = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusTime = new System.Windows.Forms.ToolStripStatusLabel();
            
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerLeft = new System.Windows.Forms.SplitContainer();
            this.splitContainerCenterRight = new System.Windows.Forms.SplitContainer();
            this.splitContainerImages = new System.Windows.Forms.SplitContainer(); // New SplitContainer
            this.imageViewerRef = new PCBInspection.UI.Controls.InteractiveImageViewer(); // New Viewer
            this.imgListToolbox = new System.Windows.Forms.ImageList(this.components);
            this.tabToolbox = new System.Windows.Forms.TabControl();
            this.tabPageTools = new System.Windows.Forms.TabPage();
            this.tabPageHistory = new System.Windows.Forms.TabPage(); // New TabPage
            this.tabPageLabeling = new System.Windows.Forms.TabPage(); // [Fix] Initialize here
            this.defectLabelingControl1 = new PCBInspection.UI.Controls.DefectLabelingControl();
            this.lstHistory = new System.Windows.Forms.ListBox(); // New ListBox
            this.tvTools = new System.Windows.Forms.TreeView();
            this.dgvSequence = new System.Windows.Forms.DataGridView();
            this.colStepName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRun = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colLoop = new System.Windows.Forms.DataGridViewButtonColumn();
            this.pnlFlowControl = new System.Windows.Forms.Panel();
            this.btnRemoveTool = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.grpProperties = new System.Windows.Forms.GroupBox();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            
            this.panelCenter = new System.Windows.Forms.Panel();
            this.panelImage = new System.Windows.Forms.Panel();
            this.imageViewer = new PCBInspection.UI.Controls.InteractiveImageViewer();
            this.panelLog = new System.Windows.Forms.Panel();
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.infoPanel = new PCBInspection.UI.Controls.InfoPanelControl();
            this.thumbnailBar = new PCBInspection.UI.Controls.ThumbnailBarControl();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCenterRight)).BeginInit();
            this.splitContainerCenterRight.Panel1.SuspendLayout();
            this.splitContainerCenterRight.Panel2.SuspendLayout();
            this.splitContainerCenterRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).BeginInit();
            this.splitContainerLeft.Panel1.SuspendLayout();
            this.splitContainerLeft.Panel2.SuspendLayout();
            this.splitContainerLeft.SuspendLayout();
            this.tabToolbox.SuspendLayout();
            this.tabPageTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSequence)).BeginInit();
            this.pnlFlowControl.SuspendLayout();
            this.grpProperties.SuspendLayout();
            this.panelCenter.SuspendLayout();
            this.panelImage.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.toolStripMain.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // 
            // toolStripMain
            // 
            this.toolStripMain.BackColor = System.Drawing.Color.WhiteSmoke;
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
            this.btnTsSplit,
            this.toolStripSeparator4,
            this.btnTsPointer,
            this.btnTsRoiRect,
            this.btnTsRoiCircle,
            this.btnTsRoiPoly,
            this.toolStripSeparator5,
            this.btnTsUndo,
            this.btnTsRedo,
            this.toolStripSeparator6,
            this.btnTsBatch,
            this.btnTsRecipe});
            this.toolStripMain.Location = new System.Drawing.Point(0, 0);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Size = new System.Drawing.Size(1264, 45);
            this.toolStripMain.TabIndex = 0;
            this.toolStripMain.Font = new System.Drawing.Font("微軟正黑體", 12f);
            this.toolStripMain.ImageScalingSize = new System.Drawing.Size(32, 32);

            // Define Texts/Icons for new buttons
            this.btnTsZoomIn.Text = "+";
            this.btnTsZoomIn.ForeColor = System.Drawing.Color.Black;
            this.btnTsZoomOut.Text = "-";
            this.btnTsZoomOut.ForeColor = System.Drawing.Color.Black;
            this.btnTsFit.Text = "[ ]";
            this.btnTsFit.ForeColor = System.Drawing.Color.Black;
            
            this.btnTsSplit.Text = "◫";
            this.btnTsSplit.ToolTipText = "切換比對模式";
            this.btnTsSplit.ForeColor = System.Drawing.Color.Black;
            
            this.btnTsPointer.Text = "指標";
            this.btnTsPointer.ForeColor = System.Drawing.Color.Black;
            this.btnTsRoiRect.Text = "□";
            this.btnTsRoiRect.ForeColor = System.Drawing.Color.Black;
            this.btnTsRoiCircle.Text = "○";
            this.btnTsRoiCircle.ForeColor = System.Drawing.Color.Black;
            this.btnTsRoiPoly.Text = "⬡";
            this.btnTsRoiPoly.ForeColor = System.Drawing.Color.Black;

            this.btnTsUndo.Text = "↩";
            this.btnTsUndo.ForeColor = System.Drawing.Color.Black;
            this.btnTsRedo.Text = "↪";
            this.btnTsRedo.ForeColor = System.Drawing.Color.Black;

            this.btnTsBatch.Text = "▦ 批次";
            this.btnTsBatch.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnTsBatch.ToolTipText = "批次處理";
            this.btnTsRecipe.Text = "☰ 配方";
            this.btnTsRecipe.ForeColor = System.Drawing.Color.DarkGreen;
            this.btnTsRecipe.ToolTipText = "配方管理器";

            // 
            // btnTsNew
            // 
            this.btnTsNew.ForeColor = System.Drawing.Color.Black;
            this.btnTsNew.Name = "btnTsNew";
            this.btnTsNew.Size = new System.Drawing.Size(43, 32);
            this.btnTsNew.Text = "新建";

            // (Next items simplified but all must be initialized)
            this.btnTsOpen.ForeColor = System.Drawing.Color.Black;
            this.btnTsOpen.Text = "開啟";
            this.btnTsSave.ForeColor = System.Drawing.Color.Black;
            this.btnTsSave.Text = "儲存";
            this.btnTsRunOnce.ForeColor = System.Drawing.Color.Green;
            this.btnTsRunOnce.Text = "► 執行";
            this.btnTsRunLoop.ForeColor = System.Drawing.Color.Green;
            this.btnTsRunLoop.Text = "↻ 循環";
            this.btnTsStop.ForeColor = System.Drawing.Color.Red;
            this.btnTsStop.Text = "■ 停止";
            this.btnTsSettings.ForeColor = System.Drawing.Color.Black;
            this.btnTsSettings.Text = "設定";

            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.WhiteSmoke;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusMain,
            this.lblStatusTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 789);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1264, 22);
            this.statusStrip.TabIndex = 1;

            this.lblStatusMain.ForeColor = System.Drawing.Color.Black;
            this.lblStatusMain.Text = "就緒";

            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 35);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerLeft);
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerCenterRight);
            this.splitContainerMain.Size = new System.Drawing.Size(1264, 754);
            this.splitContainerMain.SplitterDistance = 350;
            this.splitContainerMain.TabIndex = 2;

            // 
            // splitContainerCenterRight
            // 
            this.splitContainerCenterRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerCenterRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerCenterRight.Name = "splitContainerCenterRight";
            this.splitContainerCenterRight.Panel1.Controls.Add(this.splitContainerImages);
            this.splitContainerCenterRight.Panel1.Controls.Add(this.thumbnailBar);
            this.thumbnailBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.thumbnailBar.BringToFront();

            // 
            // splitContainerImages
            // 
            this.splitContainerImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerImages.Location = new System.Drawing.Point(0, 0);
            this.splitContainerImages.Name = "splitContainerImages";
            // 
            // splitContainerImages.Panel1
            // 
            this.splitContainerImages.Panel1.Controls.Add(this.imageViewerRef);
            this.splitContainerImages.Panel1Collapsed = true;
            // 
            // splitContainerImages.Panel2
            // 
            this.splitContainerImages.Panel2.Controls.Add(this.imageViewer);
            this.splitContainerImages.Size = new System.Drawing.Size(580, 754); // Based on SplitterDistance
            this.splitContainerImages.SplitterDistance = 290;
            this.splitContainerImages.TabIndex = 0;

            // 
            // imageViewerRef
            // 
            this.imageViewerRef.BackColor = System.Drawing.Color.Black;
            this.imageViewerRef.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewerRef.Location = new System.Drawing.Point(0, 0);
            this.imageViewerRef.Name = "imageViewerRef";
            this.imageViewerRef.Size = new System.Drawing.Size(290, 754);
            this.imageViewerRef.TabIndex = 0;
            
            // 
            // imageViewer
            // 
            this.imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            // ...

            this.splitContainerCenterRight.Panel2.Controls.Add(this.infoPanel);
            this.splitContainerCenterRight.Size = new System.Drawing.Size(910, 754);
            this.splitContainerCenterRight.SplitterDistance = 580;
            this.splitContainerCenterRight.Panel2MinSize = 280;

            // 
            // splitContainerLeft
            // 
            this.splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLeft.Name = "splitContainerLeft";
            this.splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainerLeft.Panel1.Controls.Add(this.tabToolbox);
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
            this.tabToolbox.Controls.Add(this.tabPageLabeling); // 新增標註分頁
            this.tabToolbox.Controls.Add(this.tabPageHistory);
            this.tabToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabToolbox.Location = new System.Drawing.Point(0, 0);
            this.tabToolbox.Name = "tabToolbox";
            this.tabToolbox.Size = new System.Drawing.Size(350, 300);

            this.tabPageTools.BackColor = System.Drawing.Color.White;
            this.tabPageTools.Controls.Add(this.tvTools);
            this.tabPageTools.Text = "工具箱";

            // 
            // tabPageHistory
            // 
            this.tabPageHistory.Controls.Add(this.lstHistory);
            this.tabPageHistory.Location = new System.Drawing.Point(4, 25); // Just default
            this.tabPageHistory.Name = "tabPageHistory";
            this.tabPageHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHistory.Size = new System.Drawing.Size(342, 271);
            this.tabPageHistory.TabIndex = 1;
            this.tabPageHistory.Text = "歷史紀錄";
            this.tabPageHistory.UseVisualStyleBackColor = true;

            // 
            // tabPageLabeling
            // 
            // 
            // tabPageLabeling
            // 
            this.tabPageLabeling.BackColor = System.Drawing.Color.White;
            this.tabPageLabeling.Controls.Add(this.defectLabelingControl1);
            this.tabPageLabeling.Location = new System.Drawing.Point(4, 25);
            this.tabPageLabeling.Name = "tabPageLabeling";
            this.tabPageLabeling.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLabeling.Size = new System.Drawing.Size(342, 271);
            this.tabPageLabeling.TabIndex = 2;
            this.tabPageLabeling.Text = "手動標註";
            this.tabPageLabeling.UseVisualStyleBackColor = true;

            // 
            // defectLabelingControl1
            // 
            this.defectLabelingControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defectLabelingControl1.Location = new System.Drawing.Point(3, 3);
            this.defectLabelingControl1.Name = "defectLabelingControl1";
            this.defectLabelingControl1.Size = new System.Drawing.Size(336, 265);
            this.defectLabelingControl1.TabIndex = 0;

            // 
            // lstHistory
            // 
            this.lstHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstHistory.FormattingEnabled = true;
            this.lstHistory.ItemHeight = 16;
            this.lstHistory.Location = new System.Drawing.Point(3, 3);
            this.lstHistory.Name = "lstHistory";
            this.lstHistory.Size = new System.Drawing.Size(336, 265);
            this.lstHistory.TabIndex = 0;

            this.tvTools.BackColor = System.Drawing.Color.White;
            this.tvTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTools.ForeColor = System.Drawing.Color.Black;
            this.tvTools.ImageList = this.imgListToolbox;
            this.tvTools.ImageIndex = 0;
            this.tvTools.SelectedImageIndex = 0;
            this.tvTools.ItemHeight = 24;
            this.tvTools.Indent = 20;
            this.tvTools.ShowLines = true;
            this.tvTools.BorderStyle = System.Windows.Forms.BorderStyle.None;

            // 
            // dgvSequence
            // 
            this.dgvSequence.AllowUserToAddRows = false;
            this.dgvSequence.BackgroundColor = System.Drawing.Color.White;
            this.dgvSequence.ColumnHeadersHeight = 30;
            this.dgvSequence.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStepName,
            this.colTime,
            this.colStatus,
            this.colRun,
            this.colLoop});
            this.dgvSequence.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvSequence.Height = 194;
            this.dgvSequence.RowHeadersVisible = false;
            this.dgvSequence.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSequence.ForeColor = System.Drawing.Color.Black;
            this.dgvSequence.DefaultCellStyle.BackColor = System.Drawing.Color.White;

            this.colStepName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStepName.HeaderText = "步驟名稱";
            this.colStepName.Name = "colStepName";
            this.colTime.HeaderText = "耗時";
            this.colTime.Name = "colTime";
            this.colTime.Width = 60;
            this.colStatus.HeaderText = "狀態";
            this.colStatus.Name = "colStatus";
            this.colStatus.Width = 60;
            this.colRun.Name = "colRun";
            this.colRun.HeaderText = "";
            this.colRun.Text = "▶";
            this.colRun.UseColumnTextForButtonValue = true;
            this.colRun.Width = 35;
            this.colRun.FlatStyle = System.Windows.Forms.FlatStyle.Standard;

            this.colLoop.Name = "colLoop";
            this.colLoop.HeaderText = "";
            this.colLoop.Text = "↻";
            this.colLoop.ToolTipText = "循環執行 (動態調參)";
            this.colLoop.UseColumnTextForButtonValue = true;
            this.colLoop.Width = 35;
            this.colLoop.FlatStyle = System.Windows.Forms.FlatStyle.Standard;

            // 
            // pnlFlowControl
            // 
            this.pnlFlowControl.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFlowControl.Controls.Add(this.btnRemoveTool);
            this.pnlFlowControl.Controls.Add(this.btnMoveDown);
            this.pnlFlowControl.Controls.Add(this.btnMoveUp);
            this.pnlFlowControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFlowControl.Location = new System.Drawing.Point(0, 194);
            this.pnlFlowControl.Size = new System.Drawing.Size(350, 30);

            this.btnRemoveTool.Text = "-"; 
            this.btnRemoveTool.Location = new System.Drawing.Point(298, 3);
            this.btnRemoveTool.Size = new System.Drawing.Size(40, 23);
            this.btnMoveDown.Text = "↓";
            this.btnMoveDown.Location = new System.Drawing.Point(50, 3);
            this.btnMoveDown.Size = new System.Drawing.Size(40, 23);
            this.btnMoveUp.Text = "↑";
            this.btnMoveUp.Location = new System.Drawing.Point(4, 3);
            this.btnMoveUp.Size = new System.Drawing.Size(40, 23);

            // 
            // grpProperties
            // 
            this.grpProperties.Controls.Add(this.propertyGrid);
            this.grpProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpProperties.ForeColor = System.Drawing.Color.Black;
            this.grpProperties.Location = new System.Drawing.Point(0, 224);
            this.grpProperties.Text = "參數設定";

            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.ViewBackColor = System.Drawing.Color.White;
            this.propertyGrid.ViewForeColor = System.Drawing.Color.Black;
            this.propertyGrid.HelpVisible = true;

            // 
            // panelCenter
            // 
            this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewer.BackColor = System.Drawing.Color.Silver;

            this.panelLog.Controls.Add(this.lstLog);
            this.panelLog.Controls.Add(this.lblLogTitle);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLog.Height = 200;

            this.lstLog.BackColor = System.Drawing.Color.White;
            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstLog.ForeColor = System.Drawing.Color.Black;

            this.lblLogTitle.BackColor = System.Drawing.Color.LightGray;
            this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogTitle.Text = "日誌訊息";
            this.lblLogTitle.Height = 20;

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1264, 811);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStripMain);
            this.Name = "MainForm";
            this.Text = "PCB Vision Builder";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerCenterRight.Panel1.ResumeLayout(false);
            this.splitContainerCenterRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCenterRight)).EndInit();
            this.splitContainerCenterRight.ResumeLayout(false);
            this.splitContainerImages.Panel1.ResumeLayout(false);
            this.splitContainerImages.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerImages)).EndInit();
            this.splitContainerImages.ResumeLayout(false);
            this.splitContainerLeft.Panel1.ResumeLayout(false);
            this.splitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).EndInit();
            this.splitContainerLeft.ResumeLayout(false);
            this.tabToolbox.ResumeLayout(false);
            this.tabPageTools.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSequence)).EndInit();
            this.pnlFlowControl.ResumeLayout(false);
            this.grpProperties.ResumeLayout(false);
            this.panelCenter.ResumeLayout(false);
            this.panelImage.ResumeLayout(false);
            this.panelLog.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnTsZoomIn;
        private System.Windows.Forms.ToolStripButton btnTsZoomOut;
        private System.Windows.Forms.ToolStripButton btnTsFit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnTsPointer;
        private System.Windows.Forms.ToolStripButton btnTsRoiRect;
        private System.Windows.Forms.ToolStripButton btnTsRoiCircle;
        private System.Windows.Forms.ToolStripButton btnTsRoiPoly;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnTsUndo;
        private System.Windows.Forms.ToolStripButton btnTsRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton btnTsBatch;
        private System.Windows.Forms.ToolStripButton btnTsRecipe;

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusMain;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusTime;

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerLeft;
        private System.Windows.Forms.TabControl tabToolbox;
        private System.Windows.Forms.TabPage tabPageTools;
        private System.Windows.Forms.TreeView tvTools;
        private System.Windows.Forms.DataGridView dgvSequence;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewButtonColumn colRun;
        private System.Windows.Forms.DataGridViewButtonColumn colLoop;
        private System.Windows.Forms.Panel pnlFlowControl;
        private System.Windows.Forms.Button btnRemoveTool;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.GroupBox grpProperties;
        private System.Windows.Forms.PropertyGrid propertyGrid;

        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.Panel panelImage;
        private PCBInspection.UI.Controls.InteractiveImageViewer imageViewer;
        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.Label lblLogTitle;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.SplitContainer splitContainerCenterRight;
        private PCBInspection.UI.Controls.InfoPanelControl infoPanel;
        private PCBInspection.UI.Controls.ThumbnailBarControl thumbnailBar;
        private System.Windows.Forms.ToolStripButton btnTsSplit;
        private System.Windows.Forms.SplitContainer splitContainerImages;
        private PCBInspection.UI.Controls.InteractiveImageViewer imageViewerRef;
        private System.Windows.Forms.TabPage tabPageHistory;
        private System.Windows.Forms.ListBox lstHistory;
        private System.Windows.Forms.TabPage tabPageLabeling;
        private PCBInspection.UI.Controls.DefectLabelingControl defectLabelingControl1;
        private System.Windows.Forms.ImageList imgListToolbox;
    }
}
