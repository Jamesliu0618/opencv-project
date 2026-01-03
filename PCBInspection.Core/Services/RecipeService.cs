using Newtonsoft.Json;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PCBInspection.Core.Services
{
	/// <summary>配方服務，提供配方的載入、儲存與管理功能</summary>
	public class RecipeService
	{
		private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
		{
			Formatting            = Formatting.Indented,
			TypeNameHandling      = TypeNameHandling.Auto,
			NullValueHandling     = NullValueHandling.Ignore,
			DateFormatString      = "yyyy-MM-dd HH:mm:ss",
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		/// <summary>從檔案載入配方</summary>
		public Recipe LoadRecipe(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("找不到配方檔案", filePath);

			string json   = File.ReadAllText(filePath, Encoding.UTF8);
			var    recipe = JsonConvert.DeserializeObject<Recipe>(json, JsonSettings);

			if (recipe == null)
				throw new InvalidOperationException("配方檔案格式錯誤");

			return recipe;
		}

		/// <summary>儲存配方至檔案</summary>
		public void SaveRecipe(Recipe recipe, string filePath)
		{
			if (recipe == null)
				throw new ArgumentNullException(nameof(recipe));

			recipe.ModifiedAt = DateTime.Now;

			string directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			string json = JsonConvert.SerializeObject(recipe, JsonSettings);
			File.WriteAllText(filePath, json, Encoding.UTF8);
		}

		/// <summary>從工具序列建立新配方</summary>
		public Recipe CreateFromToolSequence(IEnumerable<ToolSequenceItem> tools, string name)
		{
			var recipe = new Recipe
			{
				Name      = name,
				CreatedAt = DateTime.Now,
				Steps     = tools.Select((t, i) => new RecipeStep
				{
					Order              = i,
					ToolName           = t.ToolName,
					Category           = t.Category,
					ParametersJson     = t.Parameters != null ? JsonConvert.SerializeObject(t.Parameters, JsonSettings) : null,
					ParametersTypeName = t.Parameters?.GetType().AssemblyQualifiedName,
					Enabled            = t.Enabled
				}).ToList()
			};

			return recipe;
		}

		/// <summary>將配方套用至工具序列</summary>
		public List<ToolSequenceItem> ApplyRecipeToSequence(Recipe recipe, List<VisionToolFactory.ToolDefinition> availableTools)
		{
			var result = new List<ToolSequenceItem>();

			foreach (var step in recipe.Steps.OrderBy(s => s.Order))
			{
				var toolDef = availableTools.FirstOrDefault(t => t.Name == step.ToolName);
				if (toolDef == null) continue;

				object parameters = toolDef.DefaultParameters;

				if (!string.IsNullOrEmpty(step.ParametersJson) && !string.IsNullOrEmpty(step.ParametersTypeName))
				{
					try
					{
						var paramType = Type.GetType(step.ParametersTypeName);
						if (paramType != null)
							parameters = JsonConvert.DeserializeObject(step.ParametersJson, paramType, JsonSettings);
					}
					catch
					{
						// 使用預設參數
					}
				}

				result.Add(new ToolSequenceItem
				{
					ToolName   = step.ToolName,
					Category   = step.Category,
					Parameters = parameters,
					Enabled    = step.Enabled,
					Action     = toolDef.Action
				});
			}

			return result;
		}

		/// <summary>取得內建預設配方清單</summary>
		public List<Recipe> GetBuiltInRecipes()
		{
			return new List<Recipe>
			{
				CreatePcbInspectionRecipe(),
				CreateQrCodeRecipe(),
				CreateDimensionMeasureRecipe()
			};
		}

		/// <summary>驗證配方是否有效</summary>
		public RecipeValidationResult ValidateRecipe(Recipe recipe, List<VisionToolFactory.ToolDefinition> availableTools)
		{
			var result = new RecipeValidationResult { IsValid = true };

			if (recipe == null)
			{
				result.IsValid = false;
				result.Errors.Add("配方為 null");
				return result;
			}

			if (string.IsNullOrWhiteSpace(recipe.Name))
			{
				result.Warnings.Add("配方名稱為空");
			}

			if (recipe.Steps == null || recipe.Steps.Count == 0)
			{
				result.Warnings.Add("配方不包含任何處理步驟");
			}
			else
			{
				foreach (var step in recipe.Steps)
				{
					var toolDef = availableTools.FirstOrDefault(t => t.Name == step.ToolName);
					if (toolDef == null)
					{
						result.Warnings.Add($"找不到工具: {step.ToolName}");
					}
				}
			}

			return result;
		}

		#region 內建配方

		private Recipe CreatePcbInspectionRecipe()
		{
			return new Recipe
			{
				Id          = "builtin-pcb-inspection",
				Name        = "PCB 檢測配方",
				Description = "標準 PCB 瑕疵檢測流程：灰階化 → 濾波 → 邊緣偵測 → 輪廓搜尋",
				Author      = "System",
				Steps = new List<RecipeStep>
				{
					new RecipeStep { Order = 0, ToolName = "灰階化",   Category = "預處理",   Enabled = true },
					new RecipeStep { Order = 1, ToolName = "高斯模糊", Category = "預處理",   Enabled = true },
					new RecipeStep { Order = 2, ToolName = "Canny",   Category = "特徵提取", Enabled = true },
					new RecipeStep { Order = 3, ToolName = "輪廓搜尋", Category = "特徵提取", Enabled = true }
				}
			};
		}

		private Recipe CreateQrCodeRecipe()
		{
			return new Recipe
			{
				Id          = "builtin-qrcode",
				Name        = "QR Code 辨識配方",
				Description = "QR Code 辨識流程：灰階化 → 自適應二值化 → 輪廓搜尋",
				Author      = "System",
				Steps = new List<RecipeStep>
				{
					new RecipeStep { Order = 0, ToolName = "灰階化",       Category = "預處理", Enabled = true },
					new RecipeStep { Order = 1, ToolName = "自適應閾值",   Category = "預處理", Enabled = true },
					new RecipeStep { Order = 2, ToolName = "輪廓搜尋",     Category = "特徵提取", Enabled = true }
				}
			};
		}

		private Recipe CreateDimensionMeasureRecipe()
		{
			return new Recipe
			{
				Id          = "builtin-dimension",
				Name        = "尺寸量測配方",
				Description = "尺寸量測流程：灰階化 → 邊緣偵測 → 霍夫直線偵測",
				Author      = "System",
				Steps = new List<RecipeStep>
				{
					new RecipeStep { Order = 0, ToolName = "灰階化",     Category = "預處理",   Enabled = true },
					new RecipeStep { Order = 1, ToolName = "Canny",     Category = "特徵提取", Enabled = true },
					new RecipeStep { Order = 2, ToolName = "霍夫直線偵測", Category = "特徵提取", Enabled = true }
				}
			};
		}

		#endregion
	}

	/// <summary>工具序列項目 (UI 層使用)</summary>
	public class ToolSequenceItem
	{
		/// <summary>工具名稱</summary>
		public string ToolName { get; set; }

		/// <summary>工具類別</summary>
		public string Category { get; set; }

		/// <summary>參數物件</summary>
		public object Parameters { get; set; }

		/// <summary>是否啟用</summary>
		public bool Enabled { get; set; } = true;

		/// <summary>執行動作</summary>
		public VisionToolFactory.VisionAction Action { get; set; }
	}

	/// <summary>配方驗證結果</summary>
	public class RecipeValidationResult
	{
		/// <summary>是否有效</summary>
		public bool IsValid { get; set; }

		/// <summary>錯誤清單</summary>
		public List<string> Errors { get; set; } = new List<string>();

		/// <summary>警告清單</summary>
		public List<string> Warnings { get; set; } = new List<string>();
	}
}
