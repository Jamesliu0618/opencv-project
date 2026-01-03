using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Drawing;

namespace PCBInspection.Core.Services
{
	/// <summary>
	/// 缺陷標註資料模型 (JSON 持久化格式)。
	/// 紀錄單張影像的所有標註項目與原始尺寸。
	/// </summary>
	public class LabelData
	{
		/// <summary>對應的影像名稱</summary>
		public string ImageName { get; set; }
		/// <summary>標註時的影像寬度 (Px)</summary>
		public int Width { get; set; }
		/// <summary>標註時的影像高度 (Px)</summary>
		public int Height { get; set; }
		/// <summary>標註項目列表</summary>
		public List<LabelItem> Labels { get; set; } = new List<LabelItem>();
	}

	/// <summary>單一缺陷標註項目，支援矩形框與多邊形描述。</summary>
	public class LabelItem
	{
		/// <summary>唯一識別碼 (GUID)</summary>
		public string Id { get; set; } = Guid.NewGuid().ToString();
		/// <summary>標註類型 (如: 刮傷, 汙點)</summary>
		public string Type { get; set; }
		/// <summary>嚴重度級別</summary>
		public string Severity { get; set; }
		/// <summary>邊界框 [x, y, w, h]</summary>
		public int[] BoundingBox { get; set; } 
		/// <summary>多邊形座標點清單 (若有)</summary>
		public List<Point> Polygon { get; set; }
	}

	/// <summary>標註服務：負責讀寫標註檔案</summary>
	public class LabelingService
	{
		/// <summary>儲存標註資料</summary>
		/// <param name="imagePath">影像檔案完整路徑</param>
		/// <param name="labels">標註項目列表</param>
		/// <param name="imageSize">影像尺寸</param>
		public void SaveLabels(string imagePath, List<LabelItem> labels, Size imageSize)
		{
			if (string.IsNullOrEmpty(imagePath)) return;

			string jsonPath = Path.ChangeExtension(imagePath, ".json");
			
			var data = new LabelData
			{
				ImageName = Path.GetFileName(imagePath),
				Width     = imageSize.Width,
				Height    = imageSize.Height,
				Labels    = labels ?? new List<LabelItem>()
			};

			string json = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(jsonPath, json);
		}

		/// <summary>載入標註資料</summary>
		/// <param name="imagePath">影像檔案完整路徑</param>
		/// <returns>標註項目列表，若無檔案則回傳空列表</returns>
		public List<LabelItem> LoadLabels(string imagePath)
		{
			if (string.IsNullOrEmpty(imagePath)) return new List<LabelItem>();

			string jsonPath = Path.ChangeExtension(imagePath, ".json");
			if (!File.Exists(jsonPath)) return new List<LabelItem>();

			try
			{
				string json = File.ReadAllText(jsonPath);
				var data = JsonConvert.DeserializeObject<LabelData>(json);
				return data?.Labels ?? new List<LabelItem>();
			}
			catch (Exception)
			{
				return new List<LabelItem>();
			}
		}
	}
}
