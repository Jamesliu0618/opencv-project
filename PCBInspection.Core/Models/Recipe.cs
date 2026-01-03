using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Models
{
	/// <summary>處理配方，包含一系列視覺工具步驟與設定</summary>
	public class Recipe
	{
		/// <summary>配方唯一識別碼</summary>
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		/// <summary>配方名稱</summary>
		public string Name { get; set; }

		/// <summary>配方描述</summary>
		public string Description { get; set; }

		/// <summary>作者名稱</summary>
		public string Author { get; set; }

		/// <summary>建立時間</summary>
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		/// <summary>最後修改時間</summary>
		public DateTime ModifiedAt { get; set; } = DateTime.Now;

		/// <summary>配方版本</summary>
		public string Version { get; set; } = "1.0.0";

		/// <summary>處理步驟清單</summary>
		public List<RecipeStep> Steps { get; set; } = new List<RecipeStep>();

		/// <summary>配方設定</summary>
		public RecipeSettings Settings { get; set; } = new RecipeSettings();
	}

	/// <summary>配方中的單一處理步驟</summary>
	public class RecipeStep
	{
		/// <summary>步驟順序 (從 0 開始)</summary>
		public int Order { get; set; }

		/// <summary>視覺工具名稱 (對應 VisionToolFactory 中的工具名稱)</summary>
		public string ToolName { get; set; }

		/// <summary>工具類別</summary>
		public string Category { get; set; }

		/// <summary>JSON 格式的參數字串 (序列化後的參數物件)</summary>
		public string ParametersJson { get; set; }

		/// <summary>參數類型全名 (用於反序列化)</summary>
		public string ParametersTypeName { get; set; }

		/// <summary>是否啟用此步驟</summary>
		public bool Enabled { get; set; } = true;

		/// <summary>步驟備註</summary>
		public string Notes { get; set; }
	}

	/// <summary>配方全域設定</summary>
	public class RecipeSettings
	{
		/// <summary>像素尺寸 (mm/pixel)</summary>
		public double PixelSizeMm { get; set; } = 0.01;

		/// <summary>ROI 定義清單</summary>
		public List<RoiDefinition> Rois { get; set; } = new List<RoiDefinition>();

		/// <summary>公差設定</summary>
		public ToleranceSettings Tolerances { get; set; } = new ToleranceSettings();

		/// <summary>自動儲存結果</summary>
		public bool AutoSaveResults { get; set; } = true;

		/// <summary>輸出資料夾路徑</summary>
		public string OutputFolder { get; set; }
	}

	/// <summary>ROI 定義 (用於配方儲存)</summary>
	public class RoiDefinition
	{
		/// <summary>ROI 名稱</summary>
		public string Name { get; set; }

		/// <summary>ROI 類型 (Rectangle/Circle/Polygon/Ellipse)</summary>
		public string Type { get; set; }

		/// <summary>是否啟用</summary>
		public bool Enabled { get; set; } = true;

		/// <summary>JSON 格式的座標資料</summary>
		public string GeometryJson { get; set; }
	}

	/// <summary>公差設定</summary>
	public class ToleranceSettings
	{
		/// <summary>面積上限 (像素²)</summary>
		public double MaxArea { get; set; } = 10000;

		/// <summary>面積下限 (像素²)</summary>
		public double MinArea { get; set; } = 100;

		/// <summary>最小圓度</summary>
		public double MinCircularity { get; set; } = 0.7;

		/// <summary>最小矩形度</summary>
		public double MinRectangularity { get; set; } = 0.8;

		/// <summary>尺寸公差 (%)</summary>
		public double SizeTolerancePercent { get; set; } = 10;
	}
}
