using Newtonsoft.Json;
using PCBInspection.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PCBInspection.Core.Services
{
	/// <summary>
	///     報告服務 - 生成 JSON 報告和統計匯出
	/// </summary>
	public static class ReportingService
	{
		/// <summary>
		///     根據檢測結果生成結構化的統計報表 (InspectionReport)。
		///     將原始影像處理數據轉換為更便於報表呈現與彙整的格式。
		/// </summary>
		public static InspectionReport GenerateReport(InspectionResult result, ReportOptions options = null)
		{
			options = options ?? new ReportOptions();

			var report = new InspectionReport
			{
				ReportId         = Guid.NewGuid().ToString("N"),
				GeneratedAt      = DateTime.UtcNow,
				PcbId            = result.PcbId,
				Decision         = result.Ok ? "OK" : "NG",
				ProcessingTimeMs = result.ProcessingTimeMs,
				ModelVersion     = result.ModelVersion ?? "1.0.0",
				// 彙整元件摘要資訊
				Components = result.Components?.Select(c => new ComponentSummary
				                   {
					                   Id       = c.Id,
					                   CenterMm = $"({c.CenterX_Mm:F2}, {c.CenterY_Mm:F2})",
					                   SizeMm   = $"{c.WidthMm:F2} x {c.HeightMm:F2}",
					                   AngleDeg = c.AngleDeg,
				                   })
				                   .ToList()
				          ?? new List<ComponentSummary>(),
				// 彙整缺陷摘要資訊
				Defects = result.Defects?.Select(d => new DefectSummary
				                {
					                Id         = d.Id,
					                Type       = d.Type,
					                Severity   = d.Severity,
					                Confidence = d.Confidence,
					                Location   = d.BoundingBox != null && d.BoundingBox.Length >= 4 ? $"({d.BoundingBox[0]}, {d.BoundingBox[1]})" : "N/A",
				                })
				                .ToList()
				       ?? new List<DefectSummary>(),
				// 計算統計指標 (如: 各類型缺陷計數、最大嚴重度等)
				Statistics = new ReportStatistics
				{
					TotalComponents = result.Components?.Count                                                      ?? 0,
					TotalDefects    = result.Defects?.Count                                                         ?? 0,
					MaxSeverity     = result.Defects?.Max(d => (int?)d.Severity)                                    ?? 0,
					DefectsByType   = result.Defects?.GroupBy(d => d.Type).ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<string, int>(),
				},
				Files = new ReportFiles
				{
					AnnotatedImage = result.AnnotatedImagePath,
					OriginalImage  = null, 
				},
			};
			return report;
		}

		/// <summary>
		///     儲存 JSON 報告
		/// </summary>
		public static string SaveJsonReport(InspectionReport report, string outputDir)
		{
			Directory.CreateDirectory(outputDir);
			var filename = $"{report.PcbId}_report_{report.GeneratedAt:yyyyMMddHHmmss}.json";
			var path     = Path.Combine(outputDir, filename);
			var json     = JsonConvert.SerializeObject(report, Formatting.Indented);
			File.WriteAllText(path, json, Encoding.UTF8);
			return path;
		}

		/// <summary>
		///     生成 CSV 統計匯出
		/// </summary>
		public static string ExportStatisticsCsv(IEnumerable<InspectionReport> reports, string outputPath)
		{
			var sb = new StringBuilder();
			sb.AppendLine("ReportId,PcbId,GeneratedAt,Decision,ProcessingTimeMs,TotalComponents,TotalDefects,MaxSeverity");

			foreach(var r in reports)
			{
				sb.AppendLine($"{r.ReportId},{r.PcbId},{r.GeneratedAt:yyyy-MM-dd HH:mm:ss}," + $"{r.Decision},{r.ProcessingTimeMs},{r.Statistics.TotalComponents}," + $"{r.Statistics.TotalDefects},{r.Statistics.MaxSeverity}");
			}
			Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
			File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
			return outputPath;
		}

		/// <summary>
		///     生成每日統計摘要
		/// </summary>
		public static DailySummary GenerateDailySummary(IEnumerable<InspectionReport> reports, DateTime date)
		{
			var dayReports = reports.Where(r => r.GeneratedAt.Date == date.Date).ToList();

			return new DailySummary
			{
				Date                    = date.Date,
				TotalInspections        = dayReports.Count,
				OkCount                 = dayReports.Count(r => r.Decision == "OK"),
				NgCount                 = dayReports.Count(r => r.Decision == "NG"),
				ReviewCount             = dayReports.Count(r => r.Decision == "REVIEW"),
				AverageProcessingTimeMs = dayReports.Any() ? (int)dayReports.Average(r => r.ProcessingTimeMs) : 0,
				DefectDistribution      = dayReports.SelectMany(r => r.Defects ?? new List<DefectSummary>()).GroupBy(d => d.Type).ToDictionary(g => g.Key, g => g.Count()),
			};
		}
	}

	#region Report Models

	public class InspectionReport
	{
		public string                 ReportId         { get; set; }
		public DateTime               GeneratedAt      { get; set; }
		public string                 PcbId            { get; set; }
		public string                 Decision         { get; set; }
		public int                    ProcessingTimeMs { get; set; }
		public string                 ModelVersion     { get; set; }
		public List<ComponentSummary> Components       { get; set; }
		public List<DefectSummary>    Defects          { get; set; }
		public ReportStatistics       Statistics       { get; set; }
		public ReportFiles            Files            { get; set; }
	}

	public class ComponentSummary
	{
		public string Id       { get; set; }
		public string CenterMm { get; set; }
		public string SizeMm   { get; set; }
		public double AngleDeg { get; set; }
	}

	public class DefectSummary
	{
		public string Id         { get; set; }
		public string Type       { get; set; }
		public int    Severity   { get; set; }
		public double Confidence { get; set; }
		public string Location   { get; set; }
	}

	public class ReportStatistics
	{
		public int                     TotalComponents { get; set; }
		public int                     TotalDefects    { get; set; }
		public int                     MaxSeverity     { get; set; }
		public Dictionary<string, int> DefectsByType   { get; set; }
	}

	public class ReportFiles
	{
		public string AnnotatedImage { get; set; }
		public string OriginalImage  { get; set; }
	}

	public class DailySummary
	{
		public DateTime                Date                    { get; set; }
		public int                     TotalInspections        { get; set; }
		public int                     OkCount                 { get; set; }
		public int                     NgCount                 { get; set; }
		public int                     ReviewCount             { get; set; }
		public int                     AverageProcessingTimeMs { get; set; }
		public Dictionary<string, int> DefectDistribution      { get; set; }
	}

	public class ReportOptions
	{
		public bool IncludeComponentDetails { get; set; } = true;
		public bool IncludeDefectDetails    { get; set; } = true;
		public bool IncludeStatistics       { get; set; } = true;
	}

	#endregion
}