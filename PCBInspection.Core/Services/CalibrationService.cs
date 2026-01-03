using Newtonsoft.Json;
using System;
using System.IO;

namespace PCBInspection.Core.Services
{
	/// <summary>相機校正元資料，紀錄像素與實際長度的轉換關係。</summary>
	public class CalibrationMetadata
	{
		/// <summary>像素比 (Pixels per mm)</summary>
		public double   PixelsPerMm { get; set; }
		/// <summary>旋轉校正角度 (度)</summary>
		public double   RotationDeg { get; set; }
		/// <summary>建立時間</summary>
		public DateTime CreatedAt   { get; set; }
	}

	/// <summary>校正服務：負責校正參數的儲存、載入與 runtime 套用。</summary>
	public static class CalibrationService
	{
		/// <summary>
		/// 儲存當前校正參數至指定格式檔案 (JSON)。
		/// </summary>
		public static void SaveCalibration(string path)
		{
			// 1. 封裝當前全域校正狀態
			var meta = new CalibrationMetadata
			{
				PixelsPerMm = Calibration.GetPixelsPerMm(),
				RotationDeg = Calibration.GetRotationDeg(),
				CreatedAt   = DateTime.UtcNow,
			};

			// 2. 建立目錄並寫入檔案
			var dir = Path.GetDirectoryName(path);
			if(!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			File.WriteAllText(path, JsonConvert.SerializeObject(meta, Formatting.Indented));
		}

		public static CalibrationMetadata LoadCalibration(string path)
		{
			if(!File.Exists(path))
			{
				throw new FileNotFoundException("Calibration file not found", path);
			}
			var txt  = File.ReadAllText(path);
			var meta = JsonConvert.DeserializeObject<CalibrationMetadata>(txt);

			if(meta == null)
			{
				throw new InvalidDataException("Invalid calibration file content");
			}

			// apply to runtime calibration
			Calibration.SetManualScale(meta.PixelsPerMm);
			return meta;
		}
	}
}