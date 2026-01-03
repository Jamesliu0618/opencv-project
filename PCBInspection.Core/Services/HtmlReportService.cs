using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PCBInspection.Core.Models;

namespace PCBInspection.Core.Services
{
	public class HtmlReportService
	{
		public void GenerateReport(string filePath, List<InspectionResult> results, string batchName)
		{
			var sb = new StringBuilder();

			// Header
			sb.AppendLine("<!DOCTYPE html>");
			sb.AppendLine("<html>");
			sb.AppendLine("<head>");
			sb.AppendLine("<meta charset='UTF-8'>");
			sb.AppendLine("<title>PCB Inspection Report</title>");
			sb.AppendLine("<style>");
			sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }");
			sb.AppendLine("h1 { color: #333; }");
			sb.AppendLine(".card { background: white; padding: 20px; margin-bottom: 20px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }");
			sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
			sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
			sb.AppendLine("th { background-color: #f2f2f2; }");
			sb.AppendLine(".status-ok { color: green; font-weight: bold; }");
			sb.AppendLine(".status-ng { color: red; font-weight: bold; }");
			sb.AppendLine(".summary-box { display: flex; gap: 20px; }");
			sb.AppendLine(".stat-item { flex: 1; background: #e9ecef; padding: 15px; border-radius: 5px; text-align: center; }");
			sb.AppendLine(".stat-value { font-size: 24px; font-weight: bold; color: #007bff; }");
			sb.AppendLine("</style>");
			sb.AppendLine("</head>");
			sb.AppendLine("<body>");

			// Title
			sb.AppendLine($"<h1>PCB 檢測報告: {batchName}</h1>");
			sb.AppendLine($"<p>生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

			// Statistics
			int total = results.Count;
			int ok = results.Count(r => r.Ok);
			int ng = total - ok;
			double yieldRate = total > 0 ? (double)ok / total * 100.0 : 0;

			int totalDefects = results.Sum(r => r.Defects.Count);

			sb.AppendLine("<div class='card'>");
			sb.AppendLine("<h2>統計摘要</h2>");
			sb.AppendLine("<div class='summary-box'>");
			
			sb.AppendLine(CreateStatItem("檢測總數", total.ToString()));
			sb.AppendLine(CreateStatItem("合格數量 (OK)", ok.ToString(), "green"));
			sb.AppendLine(CreateStatItem("不良數量 (NG)", ng.ToString(), "red"));
			sb.AppendLine(CreateStatItem("良率", $"{yieldRate:F1}%"));
			sb.AppendLine(CreateStatItem("缺陷總數", totalDefects.ToString()));

			sb.AppendLine("</div>");
			sb.AppendLine("</div>");

			// Detail Table
			sb.AppendLine("<div class='card'>");
			sb.AppendLine("<h2>詳細檢測結果</h2>");
			sb.AppendLine("<table>");
			sb.AppendLine("<thead><tr>");
			sb.AppendLine("<th>序號 (ID)</th>");
			sb.AppendLine("<th>PCB 識別碼</th>");
			sb.AppendLine("<th>結果</th>");
			sb.AppendLine("<th>缺陷數</th>");
			sb.AppendLine("<th>處理時間 (ms)</th>");
			sb.AppendLine("<th>影像連結</th>");
			sb.AppendLine("</tr></thead>");
			sb.AppendLine("<tbody>");

			foreach(var res in results)
			{
				string statusClass = res.Ok ? "status-ok" : "status-ng";
				string statusText = res.Ok ? "OK" : "NG";
				string imgLink = string.IsNullOrEmpty(res.AnnotatedImagePath) ? "-" : $"<a href='{GetRelativePath(res.AnnotatedImagePath, filePath)}' target='_blank'>查看影像</a>";

				sb.AppendLine("<tr>");
				sb.AppendLine($"<td>{res.Id}</td>");
				sb.AppendLine($"<td>{res.PcbId ?? "-"}</td>");
				sb.AppendLine($"<td class='{statusClass}'>{statusText}</td>");
				sb.AppendLine($"<td>{res.Defects.Count}</td>");
				sb.AppendLine($"<td>{res.ProcessingTimeMs}</td>");
				sb.AppendLine($"<td>{imgLink}</td>");
				sb.AppendLine("</tr>");
			}

			sb.AppendLine("</tbody>");
			sb.AppendLine("</table>");
			sb.AppendLine("</div>");

			sb.AppendLine("</body>");
			sb.AppendLine("</html>");

			File.WriteAllText(filePath, sb.ToString());
		}

		private string CreateStatItem(string label, string value, string color = "#007bff")
		{
			return $"<div class='stat-item'><div>{label}</div><div class='stat-value' style='color:{color}'>{value}</div></div>";
		}

		private string GetRelativePath(string fullPath, string basePath)
		{
			// Simple relative path calculation
			// Assuming both are absolute
			try 
			{
				Uri pathUri = new Uri(fullPath);
				// Folders must end in a slash
				if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					basePath += Path.DirectorySeparatorChar;
				}
				Uri folderUri = new Uri(Path.GetDirectoryName(basePath) + Path.DirectorySeparatorChar);
				return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
			}
			catch
			{
				return fullPath;
			}
		}
	}
}
