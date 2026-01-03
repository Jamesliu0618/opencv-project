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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelImageContainer = new System.Windows.Forms.Panel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.panelZoomInfo = new System.Windows.Forms.Panel();
            this.lblZoomLevel = new System.Windows.Forms.Label();
            this.btnZoomReset = new System.Windows.Forms.Button();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblResult = new System.Windows.Forms.Label();
            this.panelControls = new System.Windows.Forms.Panel();
            this.grpFeatures = new System.Windows.Forms.GroupBox();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lblSequence = new System.Windows.Forms.Label();
            this.lblAvailable = new System.Windows.Forms.Label();
            this.lstSequence = new System.Windows.Forms.ListBox();
            this.lstAvailable = new System.Windows.Forms.ListBox();
            this.grpInspection = new System.Windows.Forms.GroupBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnSingleShot = new System.Windows.Forms.Button();
            this.grpInfo = new System.Windows.Forms.GroupBox();
            this.lblProcessTime = new System.Windows.Forms.Label();
            this.lblDefectCount = new System.Windows.Forms.Label();
            this.lblComponentCount = new System.Windows.Forms.Label();
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.btnCalibration = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.txtImagePath = new System.Windows.Forms.TextBox();
            this.lblImagePath = new System.Windows.Forms.Label();
            this.lstDefects = new System.Windows.Forms.ListBox();
            this.lblDefectList = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripZoomLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelImageContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.panelZoomInfo.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.panelControls.SuspendLayout();
            this.grpFeatures.SuspendLayout();
            this.grpInspection.SuspendLayout();
            this.grpInfo.SuspendLayout();
            this.grpConfig.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panelImageContainer);
            this.splitContainer.Panel1.Controls.Add(this.panelZoomInfo);
            this.splitContainer.Panel1.Controls.Add(this.panelStatus);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panelControls);
            this.splitContainer.Size = new System.Drawing.Size(1024, 668);
            this.splitContainer.SplitterDistance = 720;
            this.splitContainer.TabIndex = 0;
            // 
            // panelImageContainer
            // 
            this.panelImageContainer.AutoScroll = true;
            this.panelImageContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.panelImageContainer.Controls.Add(this.picPreview);
            this.panelImageContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelImageContainer.Location = new System.Drawing.Point(0, 30);
            this.panelImageContainer.Name = "panelImageContainer";
            this.panelImageContainer.Size = new System.Drawing.Size(720, 558);
            this.panelImageContainer.TabIndex = 2;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(720, 558);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // panelZoomInfo
            // 
            this.panelZoomInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.panelZoomInfo.Controls.Add(this.lblZoomLevel);
            this.panelZoomInfo.Controls.Add(this.btnZoomReset);
            this.panelZoomInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelZoomInfo.Location = new System.Drawing.Point(0, 0);
            this.panelZoomInfo.Name = "panelZoomInfo";
            this.panelZoomInfo.Size = new System.Drawing.Size(720, 30);
            this.panelZoomInfo.TabIndex = 3;
            // 
            // lblZoomLevel
            // 
            this.lblZoomLevel.AutoSize = true;
            this.lblZoomLevel.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblZoomLevel.ForeColor = System.Drawing.Color.White;
            this.lblZoomLevel.Location = new System.Drawing.Point(10, 7);
            this.lblZoomLevel.Name = "lblZoomLevel";
            this.lblZoomLevel.Size = new System.Drawing.Size(80, 15);
            this.lblZoomLevel.TabIndex = 0;
            this.lblZoomLevel.Text = "縮放: 100%";
            // 
            // btnZoomReset
            // 
            this.btnZoomReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomReset.ForeColor = System.Drawing.Color.White;
            this.btnZoomReset.Location = new System.Drawing.Point(120, 3);
            this.btnZoomReset.Name = "btnZoomReset";
            this.btnZoomReset.Size = new System.Drawing.Size(80, 24);
            this.btnZoomReset.TabIndex = 1;
            this.btnZoomReset.Text = "重置縮放";
            this.btnZoomReset.UseVisualStyleBackColor = true;
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.panelStatus.Controls.Add(this.lblResult);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 588);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(720, 80);
            this.panelStatus.TabIndex = 1;
            // 
            // lblResult
            // 
            this.lblResult.BackColor = System.Drawing.Color.Gray;
            this.lblResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResult.Font = new System.Drawing.Font("Microsoft JhengHei UI", 36F, System.Drawing.FontStyle.Bold);
            this.lblResult.ForeColor = System.Drawing.Color.White;
            this.lblResult.Location = new System.Drawing.Point(0, 0);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(720, 80);
            this.lblResult.TabIndex = 0;
            this.lblResult.Text = "待機中";
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelControls
            // 
            this.panelControls.Controls.Add(this.lstDefects);
            this.panelControls.Controls.Add(this.lblDefectList);
            this.panelControls.Controls.Add(this.grpFeatures);
            this.panelControls.Controls.Add(this.grpInspection);
            this.panelControls.Controls.Add(this.grpInfo);
            this.panelControls.Controls.Add(this.grpConfig);
            this.panelControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControls.Location = new System.Drawing.Point(0, 0);
            this.panelControls.Name = "panelControls";
            this.panelControls.Padding = new System.Windows.Forms.Padding(10);
            this.panelControls.Size = new System.Drawing.Size(300, 668);
            this.panelControls.TabIndex = 0;
            // 
            // grpFeatures
            // 
            this.grpFeatures.Controls.Add(this.propertyGrid);
            this.grpFeatures.Controls.Add(this.btnMoveDown);
            this.grpFeatures.Controls.Add(this.btnMoveUp);
            this.grpFeatures.Controls.Add(this.btnRemove);
            this.grpFeatures.Controls.Add(this.btnAdd);
            this.grpFeatures.Controls.Add(this.lblSequence);
            this.grpFeatures.Controls.Add(this.lblAvailable);
            this.grpFeatures.Controls.Add(this.lstSequence);
            this.grpFeatures.Controls.Add(this.lstAvailable);
            this.grpFeatures.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpFeatures.Location = new System.Drawing.Point(10, 335);
            this.grpFeatures.Name = "grpFeatures";
            this.grpFeatures.Size = new System.Drawing.Size(280, 240);
            this.grpFeatures.TabIndex = 5;
            this.grpFeatures.TabStop = false;
            this.grpFeatures.Text = "檢測項目設定";
            // 
            // propertyGrid
            // 
            this.propertyGrid.HelpVisible = true;
            this.propertyGrid.Location = new System.Drawing.Point(6, 140);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(268, 94);
            this.propertyGrid.TabIndex = 8;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Font = new System.Drawing.Font("Microsoft JhengHei UI", 8F);
            this.btnMoveDown.Location = new System.Drawing.Point(234, 100);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(40, 23);
            this.btnMoveDown.TabIndex = 7;
            this.btnMoveDown.Text = "↓";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Font = new System.Drawing.Font("Microsoft JhengHei UI", 8F);
            this.btnMoveUp.Location = new System.Drawing.Point(234, 40);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(40, 23);
            this.btnMoveUp.TabIndex = 6;
            this.btnMoveUp.Text = "↑";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(115, 80);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(40, 23);
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "<";
            this.btnRemove.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(115, 50);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(40, 23);
            this.btnAdd.TabIndex = 4;
            this.btnAdd.Text = ">";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // lblSequence
            // 
            this.lblSequence.AutoSize = true;
            this.lblSequence.Font = new System.Drawing.Font("Microsoft JhengHei UI", 8F);
            this.lblSequence.Location = new System.Drawing.Point(158, 20);
            this.lblSequence.Name = "lblSequence";
            this.lblSequence.Size = new System.Drawing.Size(51, 14);
            this.lblSequence.TabIndex = 3;
            this.lblSequence.Text = "執行順序";
            // 
            // lblAvailable
            // 
            this.lblAvailable.AutoSize = true;
            this.lblAvailable.Font = new System.Drawing.Font("Microsoft JhengHei UI", 8F);
            this.lblAvailable.Location = new System.Drawing.Point(6, 20);
            this.lblAvailable.Name = "lblAvailable";
            this.lblAvailable.Size = new System.Drawing.Size(51, 14);
            this.lblAvailable.TabIndex = 2;
            this.lblAvailable.Text = "可用項目";
            // 
            // lstSequence
            // 
            this.lstSequence.FormattingEnabled = true;
            this.lstSequence.ItemHeight = 15;
            this.lstSequence.Location = new System.Drawing.Point(161, 37);
            this.lstSequence.Name = "lstSequence";
            this.lstSequence.Size = new System.Drawing.Size(70, 94);
            this.lstSequence.TabIndex = 1;
            // 
            // lstAvailable
            // 
            this.lstAvailable.FormattingEnabled = true;
            this.lstAvailable.ItemHeight = 15;
            this.lstAvailable.Location = new System.Drawing.Point(6, 37);
            this.lstAvailable.Name = "lstAvailable";
            this.lstAvailable.Size = new System.Drawing.Size(103, 94);
            this.lstAvailable.TabIndex = 0;
            // 
            // grpInspection
            // 
            this.grpInspection.Controls.Add(this.btnStop);
            this.grpInspection.Controls.Add(this.btnStart);
            this.grpInspection.Controls.Add(this.btnSingleShot);
            this.grpInspection.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpInspection.Location = new System.Drawing.Point(10, 225);
            this.grpInspection.Name = "grpInspection";
            this.grpInspection.Size = new System.Drawing.Size(280, 110);
            this.grpInspection.TabIndex = 2;
            this.grpInspection.TabStop = false;
            this.grpInspection.Text = "檢測控制";
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnStop.Enabled = false;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(145, 65);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 35);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = false;
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(180)))), ((int)(((byte)(80)))));
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Location = new System.Drawing.Point(15, 65);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 35);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "連續檢測";
            this.btnStart.UseVisualStyleBackColor = false;
            // 
            // btnSingleShot
            // 
            this.btnSingleShot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(120)))), ((int)(((byte)(200)))));
            this.btnSingleShot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSingleShot.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSingleShot.ForeColor = System.Drawing.Color.White;
            this.btnSingleShot.Location = new System.Drawing.Point(15, 25);
            this.btnSingleShot.Name = "btnSingleShot";
            this.btnSingleShot.Size = new System.Drawing.Size(250, 35);
            this.btnSingleShot.TabIndex = 0;
            this.btnSingleShot.Text = "單次檢測";
            this.btnSingleShot.UseVisualStyleBackColor = false;
            // 
            // grpInfo
            // 
            this.grpInfo.Controls.Add(this.lblProcessTime);
            this.grpInfo.Controls.Add(this.lblDefectCount);
            this.grpInfo.Controls.Add(this.lblComponentCount);
            this.grpInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpInfo.Location = new System.Drawing.Point(10, 130);
            this.grpInfo.Name = "grpInfo";
            this.grpInfo.Size = new System.Drawing.Size(280, 95);
            this.grpInfo.TabIndex = 1;
            this.grpInfo.TabStop = false;
            this.grpInfo.Text = "檢測資訊";
            // 
            // lblProcessTime
            // 
            this.lblProcessTime.AutoSize = true;
            this.lblProcessTime.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblProcessTime.Location = new System.Drawing.Point(15, 70);
            this.lblProcessTime.Name = "lblProcessTime";
            this.lblProcessTime.Size = new System.Drawing.Size(100, 15);
            this.lblProcessTime.TabIndex = 2;
            this.lblProcessTime.Text = "處理時間: -- ms";
            // 
            // lblDefectCount
            // 
            this.lblDefectCount.AutoSize = true;
            this.lblDefectCount.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblDefectCount.Location = new System.Drawing.Point(15, 48);
            this.lblDefectCount.Name = "lblDefectCount";
            this.lblDefectCount.Size = new System.Drawing.Size(85, 15);
            this.lblDefectCount.TabIndex = 1;
            this.lblDefectCount.Text = "瑕疵數量: --";
            // 
            // lblComponentCount
            // 
            this.lblComponentCount.AutoSize = true;
            this.lblComponentCount.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblComponentCount.Location = new System.Drawing.Point(15, 26);
            this.lblComponentCount.Name = "lblComponentCount";
            this.lblComponentCount.Size = new System.Drawing.Size(85, 15);
            this.lblComponentCount.TabIndex = 0;
            this.lblComponentCount.Text = "零件數量: --";
            // 
            // grpConfig
            // 
            this.grpConfig.Controls.Add(this.btnCalibration);
            this.grpConfig.Controls.Add(this.btnLoadImage);
            this.grpConfig.Controls.Add(this.txtImagePath);
            this.grpConfig.Controls.Add(this.lblImagePath);
            this.grpConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpConfig.Location = new System.Drawing.Point(10, 10);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(280, 120);
            this.grpConfig.TabIndex = 0;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "設定";
            // 
            // btnCalibration
            // 
            this.btnCalibration.Location = new System.Drawing.Point(15, 80);
            this.btnCalibration.Name = "btnCalibration";
            this.btnCalibration.Size = new System.Drawing.Size(250, 30);
            this.btnCalibration.TabIndex = 3;
            this.btnCalibration.Text = "開啟校正精靈";
            this.btnCalibration.UseVisualStyleBackColor = true;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Location = new System.Drawing.Point(220, 45);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(45, 23);
            this.btnLoadImage.TabIndex = 2;
            this.btnLoadImage.Text = "...";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            // 
            // txtImagePath
            // 
            this.txtImagePath.Location = new System.Drawing.Point(15, 45);
            this.txtImagePath.Name = "txtImagePath";
            this.txtImagePath.Size = new System.Drawing.Size(200, 23);
            this.txtImagePath.TabIndex = 1;
            // 
            // lblImagePath
            // 
            this.lblImagePath.AutoSize = true;
            this.lblImagePath.Location = new System.Drawing.Point(15, 25);
            this.lblImagePath.Name = "lblImagePath";
            this.lblImagePath.Size = new System.Drawing.Size(103, 15);
            this.lblImagePath.TabIndex = 0;
            this.lblImagePath.Text = "影像檔案/資料夾:";
            // 
            // lstDefects
            // 
            this.lstDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDefects.FormattingEnabled = true;
            this.lstDefects.ItemHeight = 15;
            this.lstDefects.Location = new System.Drawing.Point(10, 575);
            this.lstDefects.Name = "lstDefects";
            this.lstDefects.Size = new System.Drawing.Size(280, 93);
            this.lstDefects.TabIndex = 3;
            // 
            // lblDefectList
            // 
            this.lblDefectList.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDefectList.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDefectList.Location = new System.Drawing.Point(10, 575);
            this.lblDefectList.Name = "lblDefectList";
            this.lblDefectList.Size = new System.Drawing.Size(280, 20);
            this.lblDefectList.TabIndex = 4;
            this.lblDefectList.Text = "瑕疵列表";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripZoomLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 668);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1024, 22);
            this.statusStrip.TabIndex = 1;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(55, 17);
            this.toolStripStatusLabel.Text = "就緒";
            // 
            // toolStripZoomLabel
            // 
            this.toolStripZoomLabel.Name = "toolStripZoomLabel";
            this.toolStripZoomLabel.Size = new System.Drawing.Size(80, 17);
            this.toolStripZoomLabel.Text = "| 縮放: 100%";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 690);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PCB 視覺檢測系統";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelImageContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.panelZoomInfo.ResumeLayout(false);
            this.panelZoomInfo.PerformLayout();
            this.panelStatus.ResumeLayout(false);
            this.panelControls.ResumeLayout(false);
            this.grpFeatures.ResumeLayout(false);
            this.grpFeatures.PerformLayout();
            this.grpInspection.ResumeLayout(false);
            this.grpInfo.ResumeLayout(false);
            this.grpInfo.PerformLayout();
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelImageContainer;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Panel panelZoomInfo;
        private System.Windows.Forms.Label lblZoomLevel;
        private System.Windows.Forms.Button btnZoomReset;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.GroupBox grpFeatures;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Label lblSequence;
        private System.Windows.Forms.Label lblAvailable;
        private System.Windows.Forms.ListBox lstSequence;
        private System.Windows.Forms.ListBox lstAvailable;
        private System.Windows.Forms.GroupBox grpInspection;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnSingleShot;
        private System.Windows.Forms.GroupBox grpInfo;
        private System.Windows.Forms.Label lblProcessTime;
        private System.Windows.Forms.Label lblDefectCount;
        private System.Windows.Forms.Label lblComponentCount;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Button btnCalibration;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.TextBox txtImagePath;
        private System.Windows.Forms.Label lblImagePath;
        private System.Windows.Forms.ListBox lstDefects;
        private System.Windows.Forms.Label lblDefectList;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripZoomLabel;
    }
}
