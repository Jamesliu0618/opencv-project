using Newtonsoft.Json;
using PCBInspection.Core.Models;
using PCBInspection.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PCBInspection.UI
{
	/// <summary>配方管理器對話框</summary>
	public partial class RecipeManagerForm : Form
	{
		private readonly RecipeService _recipeService = new RecipeService();
		private          Recipe        _selectedRecipe;
		private readonly string        _recipesFolder;

		/// <summary>選取的配方（供外部使用）</summary>
		public Recipe SelectedRecipe => _selectedRecipe;

		/// <summary>所有配方清單</summary>
		public List<Recipe> Recipes { get; } = new List<Recipe>();

		public RecipeManagerForm()
		{
			_recipesFolder = Path.Combine(Application.StartupPath, "Recipes");
			Directory.CreateDirectory(_recipesFolder);

			InitializeComponent();
			SetupEvents();
			LoadRecipes();
		}

		private void InitializeComponent()
		{
			this.Text            = "配方管理器";
			this.Size            = new Size(750, 550);
			this.StartPosition   = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;

			// 左側配方列表
			var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 250 };

			lstRecipes = new ListBox
			{
				Dock         = DockStyle.Fill,
				IntegralHeight = false,
				DisplayMember = "Name"
			};

			var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 35, FlowDirection = FlowDirection.LeftToRight };
			btnNew    = new Button { Text = "新增", Width = 55 };
			btnCopy   = new Button { Text = "複製", Width = 55 };
			btnDelete = new Button { Text = "刪除", Width = 55 };
			btnImport = new Button { Text = "匯入", Width = 55 };
			btnExport = new Button { Text = "匯出", Width = 55 };
			pnlButtons.Controls.AddRange(new Control[] { btnNew, btnCopy, btnDelete, btnImport, btnExport });

			pnlLeft.Controls.Add(lstRecipes);
			pnlLeft.Controls.Add(pnlButtons);

			// 右側詳細資訊
			var pnlRight = new Panel { Dock = DockStyle.Fill };

			var grpInfo = new GroupBox { Text = "配方資訊", Dock = DockStyle.Top, Height = 130 };
			var tblInfo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
			tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
			tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

			txtName        = new TextBox { Dock = DockStyle.Fill };
			txtDescription = new TextBox { Dock = DockStyle.Fill, Multiline = true };
			txtAuthor      = new TextBox { Dock = DockStyle.Fill };
			lblVersion     = new Label { Dock = DockStyle.Fill, AutoSize = false };

			tblInfo.Controls.Add(new Label { Text = "名稱:", AutoSize = true }, 0, 0);
			tblInfo.Controls.Add(txtName, 1, 0);
			tblInfo.Controls.Add(new Label { Text = "描述:", AutoSize = true }, 0, 1);
			tblInfo.Controls.Add(txtDescription, 1, 1);
			tblInfo.Controls.Add(new Label { Text = "作者:", AutoSize = true }, 0, 2);
			tblInfo.Controls.Add(txtAuthor, 1, 2);
			tblInfo.Controls.Add(new Label { Text = "版本:", AutoSize = true }, 0, 3);
			tblInfo.Controls.Add(lblVersion, 1, 3);
			grpInfo.Controls.Add(tblInfo);

			// 步驟列表
			var grpSteps = new GroupBox { Text = "處理步驟", Dock = DockStyle.Fill };
			dgvSteps = new DataGridView
			{
				Dock               = DockStyle.Fill,
				AutoGenerateColumns = false,
				AllowUserToAddRows = false,
				RowHeadersVisible  = false,
				SelectionMode      = DataGridViewSelectionMode.FullRowSelect
			};
			dgvSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "Order", HeaderText = "#", Width = 30 });
			dgvSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "ToolName", HeaderText = "工具名稱", Width = 150 });
			dgvSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "類別", Width = 80 });
			dgvSteps.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = "啟用", Width = 50 });
			dgvSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "備註", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
			grpSteps.Controls.Add(dgvSteps);

			pnlRight.Controls.Add(grpSteps);
			pnlRight.Controls.Add(grpInfo);

			// 底部按鈕
			var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 45 };
			btnSave   = new Button { Text = "儲存修改", Location = new Point(420, 8), Size = new Size(100, 28), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
			btnSelect = new Button { Text = "選取此配方", Location = new Point(530, 8), Size = new Size(100, 28), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
			btnClose  = new Button { Text = "關閉", Location = new Point(640, 8), Size = new Size(80, 28), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
			pnlBottom.Controls.AddRange(new Control[] { btnSave, btnSelect, btnClose });

			this.Controls.Add(pnlRight);
			this.Controls.Add(pnlLeft);
			this.Controls.Add(pnlBottom);
		}

		private void SetupEvents()
		{
			lstRecipes.SelectedIndexChanged += (s, e) => LoadSelectedRecipe();
			btnNew.Click    += (s, e) => CreateNewRecipe();
			btnCopy.Click   += (s, e) => CopyRecipe();
			btnDelete.Click += (s, e) => DeleteRecipe();
			btnImport.Click += (s, e) => ImportRecipe();
			btnExport.Click += (s, e) => ExportRecipe();
			btnSave.Click   += (s, e) => SaveRecipe();
			btnSelect.Click += (s, e) => SelectAndClose();
			btnClose.Click  += (s, e) => Close();
		}

		private void LoadRecipes()
		{
			Recipes.Clear();

			// 載入內建配方
			Recipes.AddRange(_recipeService.GetBuiltInRecipes());

			// 載入使用者配方
			foreach (var file in Directory.GetFiles(_recipesFolder, "*.json"))
			{
				try
				{
					var recipe = _recipeService.LoadRecipe(file);
					Recipes.Add(recipe);
				}
				catch { }
			}

			RefreshRecipeList();
		}

		private void RefreshRecipeList()
		{
			lstRecipes.Items.Clear();
			foreach (var r in Recipes)
				lstRecipes.Items.Add(r);

			if (lstRecipes.Items.Count > 0)
				lstRecipes.SelectedIndex = 0;
		}

		private void LoadSelectedRecipe()
		{
			if (lstRecipes.SelectedItem is Recipe recipe)
			{
				_selectedRecipe      = recipe;
				txtName.Text         = recipe.Name;
				txtDescription.Text  = recipe.Description;
				txtAuthor.Text       = recipe.Author;
				lblVersion.Text      = $"{recipe.Version} (建立於: {recipe.CreatedAt:yyyy-MM-dd})";

				dgvSteps.Rows.Clear();
				foreach (var step in recipe.Steps)
				{
					dgvSteps.Rows.Add(step.Order + 1, step.ToolName, step.Category, step.Enabled, step.Notes);
				}
			}
		}

		private void CreateNewRecipe()
		{
			var recipe = new Recipe
			{
				Name        = "新配方",
				Description = "",
				Author      = Environment.UserName,
				Steps       = new List<RecipeStep>()
			};
			Recipes.Add(recipe);
			RefreshRecipeList();
			lstRecipes.SelectedItem = recipe;
		}

		private void CopyRecipe()
		{
			if (_selectedRecipe == null) return;

			var json = JsonConvert.SerializeObject(_selectedRecipe);
			var copy = JsonConvert.DeserializeObject<Recipe>(json);
			copy.Id        = Guid.NewGuid().ToString("N");
			copy.Name      = _selectedRecipe.Name + " (副本)";
			copy.CreatedAt = DateTime.Now;

			Recipes.Add(copy);
			RefreshRecipeList();
			lstRecipes.SelectedItem = copy;
		}

		private void DeleteRecipe()
		{
			if (_selectedRecipe == null) return;

			if (MessageBox.Show($"確定要刪除配方 '{_selectedRecipe.Name}'?", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				Recipes.Remove(_selectedRecipe);

				// 刪除檔案
				var filePath = Path.Combine(_recipesFolder, $"{_selectedRecipe.Id}.json");
				if (File.Exists(filePath))
					File.Delete(filePath);

				_selectedRecipe = null;
				RefreshRecipeList();
			}
		}

		private void ImportRecipe()
		{
			using (var dlg = new OpenFileDialog { Filter = "配方檔案|*.json" })
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					try
					{
						var recipe = _recipeService.LoadRecipe(dlg.FileName);
						Recipes.Add(recipe);
						RefreshRecipeList();
						lstRecipes.SelectedItem = recipe;
					}
					catch (Exception ex)
					{
						MessageBox.Show($"匯入失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void ExportRecipe()
		{
			if (_selectedRecipe == null) return;

			using (var dlg = new SaveFileDialog { Filter = "配方檔案|*.json", FileName = $"{_selectedRecipe.Name}.json" })
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					try
					{
						_recipeService.SaveRecipe(_selectedRecipe, dlg.FileName);
						MessageBox.Show("匯出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"匯出失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void SaveRecipe()
		{
			if (_selectedRecipe == null) return;

			_selectedRecipe.Name        = txtName.Text;
			_selectedRecipe.Description = txtDescription.Text;
			_selectedRecipe.Author      = txtAuthor.Text;
			_selectedRecipe.ModifiedAt  = DateTime.Now;

			var filePath = Path.Combine(_recipesFolder, $"{_selectedRecipe.Id}.json");
			_recipeService.SaveRecipe(_selectedRecipe, filePath);

			RefreshRecipeList();
			MessageBox.Show("儲存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void SelectAndClose()
		{
			if (_selectedRecipe != null)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		#region Controls

		private ListBox      lstRecipes;
		private Button       btnNew;
		private Button       btnCopy;
		private Button       btnDelete;
		private Button       btnImport;
		private Button       btnExport;
		private TextBox      txtName;
		private TextBox      txtDescription;
		private TextBox      txtAuthor;
		private Label        lblVersion;
		private DataGridView dgvSteps;
		private Button       btnSave;
		private Button       btnSelect;
		private Button       btnClose;

		#endregion
	}
}
