using Newtonsoft.Json;
using System;
using System.IO;

namespace PCBInspection.Core.Services
{
	public class CalibrationMetadata
	{
		public double   PixelsPerMm { get; set; }
		public double   RotationDeg { get; set; }
		public DateTime CreatedAt   { get; set; }
	}

	public static class CalibrationService
	{
		public static void SaveCalibration(string path)
		{
			var meta = new CalibrationMetadata
			{
				PixelsPerMm = Calibration.GetPixelsPerMm(),
				RotationDeg = Calibration.GetRotationDeg(),
				CreatedAt   = DateTime.UtcNow,
			};
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