using PCBInspection.Core.Models;
using PCBInspection.Core.Services;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace PCBInspection.UI
{
	/// <summary>批次處理對話框</summary>
	public partial class BatchProcessingForm : Form
	{
		private readonly BatchProcessingService _batchService = new BatchProcessingService();
		private readonly RecipeService          _recipeService = new RecipeService();
		private          Recipe                 _currentRecipe;
		private          bool                   _isRunning;

		public BatchProcessingForm()
		{
			InitializeComponent();
			SetupEvents();
		}

		private void InitializeComponent()
		{
			this.Text            = "批次處理";
			this.Size            = new System.Drawing.Size(600, 580);
			this.StartPosition   = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox     = false;
			this.MinimizeBox     = false;

			// 輸入資料夾群組
			var grpInput = new GroupBox
			{
				Text     = "輸入設定",
				Location = new System.Drawing.Point(12, 12),
				Size     = new System.Drawing.Size(560, 100),
				Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			var lblInputFolder = new Label { Text = "輸入資料夾:", Location = new System.Drawing.Point(10, 25), AutoSize = true };
			txtInputFolder = new TextBox { Location = new System.Drawing.Point(90, 22), Size = new System.Drawing.Size(380, 23) };
			btnBrowseInput = new Button { Text = "...", Location = new System.Drawing.Point(480, 21), Size = new System.Drawing.Size(60, 25) };

			var lblRecipe = new Label { Text = "配方:", Location = new System.Drawing.Point(10, 60), AutoSize = true };
			cboRecipe = new ComboBox { Location = new System.Drawing.Point(90, 57), Size = new System.Drawing.Size(260, 23), DropDownStyle = ComboBoxStyle.DropDownList };
			btnLoadRecipe = new Button { Text = "載入...", Location = new System.Drawing.Point(360, 56), Size = new System.Drawing.Size(70, 25) };

			grpInput.Controls.AddRange(new Control[] { lblInputFolder, txtInputFolder, btnBrowseInput, lblRecipe, cboRecipe, btnLoadRecipe });

			// 輸出設定群組
			var grpOutput = new GroupBox
			{
				Text     = "輸出設定",
				Location = new System.Drawing.Point(12, 118),
				Size     = new System.Drawing.Size(560, 100),
				Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			var lblOutputFolder = new Label { Text = "輸出資料夾:", Location = new System.Drawing.Point(10, 25), AutoSize = true };
			txtOutputFolder = new TextBox { Location = new System.Drawing.Point(90, 22), Size = new System.Drawing.Size(380, 23) };
			btnBrowseOutput = new Button { Text = "...", Location = new System.Drawing.Point(480, 21), Size = new System.Drawing.Size(60, 25) };

			chkGenerateCsv     = new CheckBox { Text = "生成 CSV", Location = new System.Drawing.Point(10, 55), AutoSize = true, Checked = true };
			chkGenerateHtml    = new CheckBox { Text = "生成 HTML", Location = new System.Drawing.Point(100, 55), AutoSize = true, Checked = true };
			chkSaveImages      = new CheckBox { Text = "儲存影像", Location = new System.Drawing.Point(200, 55), AutoSize = true, Checked = true };
			chkSearchSubfolder = new CheckBox { Text = "搜尋子資料夾", Location = new System.Drawing.Point(300, 55), AutoSize = true };

			var lblThreads = new Label { Text = "執行緒數:", Location = new System.Drawing.Point(10, 78), AutoSize = true };
			numThreads = new NumericUpDown { Location = new System.Drawing.Point(90, 75), Size = new System.Drawing.Size(60, 23), Minimum = 1, Maximum = 32, Value = Environment.ProcessorCount - 1 };

			grpOutput.Controls.AddRange(new Control[] { lblOutputFolder, txtOutputFolder, btnBrowseOutput, chkGenerateCsv, chkGenerateHtml, chkSaveImages, chkSearchSubfolder, lblThreads, numThreads });

			// 進度群組
			var grpProgress = new GroupBox
			{
				Text     = "處理進度",
				Location = new System.Drawing.Point(12, 224),
				Size     = new System.Drawing.Size(560, 150),
				Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			progressBar = new ProgressBar { Location = new System.Drawing.Point(10, 25), Size = new System.Drawing.Size(540, 25), Style = ProgressBarStyle.Continuous };
			lblProgress = new Label { Text = "就緒", Location = new System.Drawing.Point(10, 55), AutoSize = true };
			lblElapsed  = new Label { Text = "已用時間: --", Location = new System.Drawing.Point(10, 75), AutoSize = true };
			lblRemaining = new Label { Text = "預估剩餘: --", Location = new System.Drawing.Point(200, 75), AutoSize = true };
			lblRate      = new Label { Text = "處理速度: --", Location = new System.Drawing.Point(400, 75), AutoSize = true };

			lstResults = new ListBox { Location = new System.Drawing.Point(10, 95), Size = new System.Drawing.Size(540, 45) };

			grpProgress.Controls.AddRange(new Control[] { progressBar, lblProgress, lblElapsed, lblRemaining, lblRate, lstResults });

			// 結果群組
			var grpResult = new GroupBox
			{
				Text     = "處理結果",
				Location = new System.Drawing.Point(12, 380),
				Size     = new System.Drawing.Size(560, 60),
				Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			lblResultOk    = new Label { Text = "OK: -", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = System.Drawing.Color.Green };
			lblResultNg    = new Label { Text = "NG: -", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = System.Drawing.Color.Red };
			lblResultError = new Label { Text = "錯誤: -", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = System.Drawing.Color.Orange };
			lblYield       = new Label { Text = "良率: -", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font(Font, System.Drawing.FontStyle.Bold) };

			var tblResult = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 4,
				RowCount = 1,
				Padding = new Padding(5)
			};
			tblResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			tblResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			tblResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
			tblResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

			tblResult.Controls.Add(lblResultOk, 0, 0);
			tblResult.Controls.Add(lblResultNg, 1, 0);
			tblResult.Controls.Add(lblResultError, 2, 0);
			tblResult.Controls.Add(lblYield, 3, 0);

			grpResult.Controls.Add(tblResult);

			// 按鈕
			btnStart  = new Button { Text = "▶ 開始處理", Location = new System.Drawing.Point(350, 500), Size = new System.Drawing.Size(100, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
			btnCancel = new Button { Text = "取消", Location = new System.Drawing.Point(460, 500), Size = new System.Drawing.Size(100, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Enabled = false };

			this.Controls.AddRange(new Control[] { grpInput, grpOutput, grpProgress, grpResult, btnStart, btnCancel });
		}

		private void SetupEvents()
		{
			btnBrowseInput.Click  += (s, e) => BrowseFolder(txtInputFolder);
			btnBrowseOutput.Click += (s, e) => BrowseFolder(txtOutputFolder);
			btnLoadRecipe.Click   += (s, e) => LoadRecipeFromFile();
			btnStart.Click        += async (s, e) => await StartBatchProcessingAsync();
			btnCancel.Click       += (s, e) => CancelProcessing();

			FormClosing += (s, e) =>
			{
				if (_isRunning)
				{
					e.Cancel = true;
					MessageBox.Show("請先停止處理再關閉視窗", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			};

			Load += (s, e) => LoadBuiltInRecipes();
		}

		private void LoadBuiltInRecipes()
		{
			cboRecipe.Items.Clear();
			foreach (var recipe in _recipeService.GetBuiltInRecipes())
			{
				cboRecipe.Items.Add(recipe);
			}
			cboRecipe.DisplayMember = "Name";
			if (cboRecipe.Items.Count > 0)
				cboRecipe.SelectedIndex = 0;
		}

		private void LoadRecipeFromFile()
		{
			using (var dlg = new OpenFileDialog { Filter = "配方檔案|*.json|所有檔案|*.*" })
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					try
					{
						var recipe = _recipeService.LoadRecipe(dlg.FileName);
						cboRecipe.Items.Add(recipe);
						cboRecipe.SelectedItem = recipe;
					}
					catch (Exception ex)
					{
						MessageBox.Show($"載入配方失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void BrowseFolder(TextBox target)
		{
			using (var dlg = new FolderBrowserDialog())
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					target.Text = dlg.SelectedPath;
				}
			}
		}

		private async System.Threading.Tasks.Task StartBatchProcessingAsync()
		{
			if (string.IsNullOrWhiteSpace(txtInputFolder.Text) || !Directory.Exists(txtInputFolder.Text))
			{
				MessageBox.Show("請選擇有效的輸入資料夾", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (cboRecipe.SelectedItem == null)
			{
				MessageBox.Show("請選擇配方", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			_currentRecipe = (Recipe)cboRecipe.SelectedItem;
			_isRunning     = true;

			btnStart.Enabled  = false;
			btnCancel.Enabled = true;
			progressBar.Value = 0;
			lstResults.Items.Clear();

			var options = new BatchOptions
			{
				OutputFolder        = string.IsNullOrWhiteSpace(txtOutputFolder.Text) ? null : txtOutputFolder.Text,
				GenerateCsvReport   = chkGenerateCsv.Checked,
				GenerateHtmlReport  = chkGenerateHtml.Checked,
				SaveProcessedImages = chkSaveImages.Checked,
				SearchSubfolders    = chkSearchSubfolder.Checked,
				MaxParallelism      = (int)numThreads.Value
			};

			_batchService.ProgressChanged += OnProgressChanged;
			_batchService.FileProcessed   += OnFileProcessed;
			_batchService.Completed       += OnCompleted;

			try
			{
				await _batchService.ProcessFolderAsync(txtInputFolder.Text, _currentRecipe, options);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"處理失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				_batchService.ProgressChanged -= OnProgressChanged;
				_batchService.FileProcessed   -= OnFileProcessed;
				_batchService.Completed       -= OnCompleted;
				_isRunning = false;
				btnStart.Enabled  = true;
				btnCancel.Enabled = false;
			}
		}

		private void CancelProcessing()
		{
			_batchService.Cancel();
			lblProgress.Text = "正在取消...";
		}

		private void OnProgressChanged(BatchProgress progress)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<BatchProgress>(OnProgressChanged), progress);
				return;
			}

			progressBar.Maximum = progress.Total;
			progressBar.Value   = Math.Min(progress.Current, progress.Total);
			lblProgress.Text    = $"處理中 {progress.ProgressText}: {progress.CurrentFile}";
			lblElapsed.Text     = $"已用時間: {progress.Elapsed:hh\\:mm\\:ss}";
			lblRemaining.Text   = $"預估剩餘: {progress.EstimatedRemaining:hh\\:mm\\:ss}";

			if (progress.Current > 0 && progress.Elapsed.TotalSeconds > 0)
			{
				double rate = progress.Current / progress.Elapsed.TotalSeconds;
				lblRate.Text = $"處理速度: {rate:F1} 張/秒";
			}
		}

		private void OnFileProcessed(FileProcessResult result)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<FileProcessResult>(OnFileProcessed), result);
				return;
			}

			string status = result.HasError ? "錯誤" : (result.IsOk ? "OK" : "NG");
			lstResults.Items.Insert(0, $"[{status}] {result.FileName} ({result.ProcessingTimeMs}ms)");
			if (lstResults.Items.Count > 100)
				lstResults.Items.RemoveAt(lstResults.Items.Count - 1);
		}

		private void OnCompleted(BatchResult result)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<BatchResult>(OnCompleted), result);
				return;
			}

			progressBar.Value    = progressBar.Maximum;
			lblProgress.Text     = result.WasCancelled ? "已取消" : "處理完成";
			lblResultOk.Text     = $"OK: {result.OkCount}";
			lblResultNg.Text     = $"NG: {result.NgCount}";
			lblResultError.Text  = $"錯誤: {result.ErrorCount}";
			lblYield.Text        = $"良率: {result.YieldRate:F1}%";

			if (!result.WasCancelled && !string.IsNullOrEmpty(result.ReportPath))
			{
				if (MessageBox.Show($"處理完成！\n\n總處理: {result.TotalProcessed}\n良率: {result.YieldRate:F1}%\n\n是否開啟報表?", 
					"完成", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				{
					System.Diagnostics.Process.Start(result.ReportPath);
				}
			}
		}

		#region Controls

		private TextBox        txtInputFolder;
		private TextBox        txtOutputFolder;
		private Button         btnBrowseInput;
		private Button         btnBrowseOutput;
		private ComboBox       cboRecipe;
		private Button         btnLoadRecipe;
		private CheckBox       chkGenerateCsv;
		private CheckBox       chkGenerateHtml;
		private CheckBox       chkSaveImages;
		private CheckBox       chkSearchSubfolder;
		private NumericUpDown  numThreads;
		private ProgressBar    progressBar;
		private Label          lblProgress;
		private Label          lblElapsed;
		private Label          lblRemaining;
		private Label          lblRate;
		private ListBox        lstResults;
		private Label          lblResultOk;
		private Label          lblResultNg;
		private Label          lblResultError;
		private Label          lblYield;
		private Button         btnStart;
		private Button         btnCancel;

		#endregion
	}
}
