using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Drawing;

namespace PCBInspection.Core.Services
{
	/// <summary>缺陷標註資料模型</summary>
	public class LabelData
	{
		public string ImageName { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public List<LabelItem> Labels { get; set; } = new List<LabelItem>();
	}

	/// <summary>單一缺陷標註項目</summary>
	public class LabelItem
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Type { get; set; }
		public string Severity { get; set; }
		public int[] BoundingBox { get; set; } // [x, y, w, h]
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
