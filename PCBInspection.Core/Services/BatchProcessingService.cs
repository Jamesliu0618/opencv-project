using PCBInspection.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCBInspection.Core.Services
{
	/// <summary>批次處理服務，支援多執行緒平行處理多張影像</summary>
	public class BatchProcessingService
	{
		private CancellationTokenSource _cts;
		private readonly RecipeService  _recipeService;

		/// <summary>進度變更事件</summary>
		public event Action<BatchProgress> ProgressChanged;

		/// <summary>處理完成事件</summary>
		public event Action<BatchResult> Completed;

		/// <summary>單一檔案處理完成事件</summary>
		public event Action<FileProcessResult> FileProcessed;

		public BatchProcessingService()
		{
			_recipeService = new RecipeService();
		}

		/// <summary>非同步處理資料夾中的所有影像</summary>
		public async Task ProcessFolderAsync(string inputFolder, Recipe recipe, BatchOptions options)
		{
			if (!Directory.Exists(inputFolder))
				throw new DirectoryNotFoundException($"找不到輸入資料夾: {inputFolder}");

			_cts = new CancellationTokenSource();

			var imageFiles = GetImageFiles(inputFolder, options.SearchSubfolders);
			if (imageFiles.Count == 0)
			{
				Completed?.Invoke(new BatchResult
				{
					TotalProcessed    = 0,
					OutputFolder      = options.OutputFolder,
					ProcessingTimeMs  = 0
				});
				return;
			}

			// 準備輸出目錄結構
			string outputFolder = options.OutputFolder ?? Path.Combine(inputFolder, "Output");
			string okFolder     = Path.Combine(outputFolder, "OK");
			string ngFolder     = Path.Combine(outputFolder, "NG");

			Directory.CreateDirectory(okFolder);
			Directory.CreateDirectory(ngFolder);

			var stopwatch     = Stopwatch.StartNew();
			var results       = new ConcurrentBag<FileProcessResult>(); // 執行緒安全的結果收集箱
			int processedCount = 0;
			int totalCount     = imageFiles.Count;

			// 配置多執行緒選項
			var parallelOptions = new ParallelOptions
			{
				// 若未指定平行數，則使用 CPU 核心數 - 1 (保留一核給 UI)
				MaxDegreeOfParallelism = options.MaxParallelism > 0
					? options.MaxParallelism
					: Math.Max(1, Environment.ProcessorCount - 1),
				CancellationToken = _cts.Token
			};

			try
			{
				// 在背景執行緒啟動平行迴圈，避免阻塞 UI
				await Task.Run(() =>
				{
					Parallel.ForEach(imageFiles, parallelOptions, (filePath, state) =>
					{
						// 檢查是否中途取消
						if (_cts.Token.IsCancellationRequested)
						{
							state.Stop();
							return;
						}

						// 處理單一影像檔案
						var result = ProcessSingleFile(filePath, recipe, okFolder, ngFolder, options);
						results.Add(result);

						// 更新進度計數 (原子操作)
						int current = Interlocked.Increment(ref processedCount);

						// 計算預估剩餘時間 (以當前平均速度推算)
						double elapsedMs     = stopwatch.ElapsedMilliseconds;
						double avgTimePerFile = elapsedMs / current;
						double remainingMs    = avgTimePerFile * (totalCount - current);

						// 觸發進度變更事件
						ProgressChanged?.Invoke(new BatchProgress
						{
							Current            = current,
							Total              = totalCount,
							CurrentFile        = Path.GetFileName(filePath),
							Elapsed            = TimeSpan.FromMilliseconds(elapsedMs),
							EstimatedRemaining = TimeSpan.FromMilliseconds(remainingMs),
							PercentComplete    = (double)current / totalCount * 100
						});

						// 觸發單檔完成事件
						FileProcessed?.Invoke(result);
					});
				}, _cts.Token);
			}
			catch (OperationCanceledException)
			{
				// 使用者取消
			}

			stopwatch.Stop();

			// 生成批次報表
			var resultList = results.ToList();
			string reportPath = null;

			if (options.GenerateCsvReport)
			{
				reportPath = GenerateBatchReport(resultList, outputFolder);
			}

			if (options.GenerateHtmlReport)
			{
				try
				{
					var inspectionResults = resultList.Select(r => new InspectionResult
					{
						Id               = Path.GetFileNameWithoutExtension(r.FileName),
						PcbId            = r.FileName,
						Ok               = r.IsOk,
						Defects          = r.Defects ?? new List<Defect>(),
						AnnotatedImagePath = r.OutputPath,
						ProcessingTimeMs = r.ProcessingTimeMs,
						CreatedAt        = r.EndTime
					}).ToList();

					var htmlService = new HtmlReportService();
					string htmlPath = Path.Combine(outputFolder, $"BatchReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
					htmlService.GenerateReport(htmlPath, inspectionResults, $"Batch Process {DateTime.Now:yyyy-MM-dd HH:mm}");
					
					if (reportPath == null) reportPath = htmlPath;
				}
				catch (Exception ex)
				{
					// Log error?
					Console.WriteLine($"Error generating HTML report: {ex.Message}");
				}
			}

			Completed?.Invoke(new BatchResult
			{
				TotalProcessed   = resultList.Count,
				OkCount          = resultList.Count(r => r.IsOk),
				NgCount          = resultList.Count(r => !r.IsOk && !r.HasError),
				ErrorCount       = resultList.Count(r => r.HasError),
				FailedFiles      = resultList.Where(r => r.HasError).Select(r => r.FilePath).ToList(),
				OutputFolder     = outputFolder,
				ReportPath       = reportPath,
				ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
				WasCancelled     = _cts.Token.IsCancellationRequested
			});
		}

		/// <summary>取消批次處理</summary>
		public void Cancel()
		{
			_cts?.Cancel();
		}

		/// <summary>處理單一檔案</summary>
		private FileProcessResult ProcessSingleFile(string filePath, Recipe recipe, string okFolder, string ngFolder, BatchOptions options)
		{
			var result = new FileProcessResult
			{
				FilePath  = filePath,
				FileName  = Path.GetFileName(filePath),
				StartTime = DateTime.Now
			};

			try
			{
				using (var mat = OpenCvSharp.Cv2.ImRead(filePath))
				{
					if (mat.Empty())
					{
						result.HasError     = true;
						result.ErrorMessage = "無法讀取影像檔案";
						return result;
					}

					// 取得所有可用工具並套用配方 (依配方順序建立執行步驟)
					var availableTools = VisionToolFactory.GetAllTools();
					var sequence       = _recipeService.ApplyRecipeToSequence(recipe, availableTools);

					OpenCvSharp.Mat current = mat.Clone();
					bool isOk = true;
					var defects = new List<Defect>();

					// 依序執行各個檢測步驟
					foreach (var step in sequence.Where(s => s.Enabled))
					{
						var (stepOk, resultMat, stepDefects) = step.Action(current, step.Parameters);

						// 更新當前影像 (用於流程串接)
						if (resultMat != null && resultMat != current)
						{
							current.Dispose();
							current = resultMat;
						}

						// 只要有一個步驟 NG，整張影像即視為 NG
						if (!stepOk) isOk = false;
						if (stepDefects != null) defects.AddRange(stepDefects);
					}

					result.IsOk        = isOk;
					result.DefectCount = defects.Count;
					result.Defects     = defects;

					// 儲存結果影像 (依據 OK/NG 狀態存入不同目錄)
					string destFolder = isOk ? okFolder : ngFolder;
					string destPath   = Path.Combine(destFolder, Path.GetFileName(filePath));

					if (options.SaveProcessedImages)
					{
						OpenCvSharp.Cv2.ImWrite(destPath, current);
						result.OutputPath = destPath;
					}

					current.Dispose();
				}
			}
			catch (Exception ex)
			{
				result.HasError     = true;
				result.ErrorMessage = ex.Message;
			}

			result.EndTime         = DateTime.Now;
			result.ProcessingTimeMs = (int)(result.EndTime - result.StartTime).TotalMilliseconds;

			return result;
		}

		/// <summary>取得資料夾中的所有影像檔案</summary>
		private List<string> GetImageFiles(string folder, bool searchSubfolders)
		{
			var searchOption = searchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var extensions   = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };

			return Directory.GetFiles(folder, "*.*", searchOption)
			                .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
			                .OrderBy(f => f)
			                .ToList();
		}

		/// <summary>生成批次處理 CSV 報表</summary>
		private string GenerateBatchReport(List<FileProcessResult> results, string outputFolder)
		{
			string reportPath = Path.Combine(outputFolder, $"BatchReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

			using (var writer = new StreamWriter(reportPath, false, System.Text.Encoding.UTF8))
			{
				writer.WriteLine("FileName,IsOk,DefectCount,ProcessingTimeMs,ErrorMessage,OutputPath");

				foreach (var r in results.OrderBy(x => x.FileName))
				{
					string errorMsg = r.ErrorMessage?.Replace(",", ";").Replace("\n", " ") ?? "";
					writer.WriteLine($"{r.FileName},{r.IsOk},{r.DefectCount},{r.ProcessingTimeMs},{errorMsg},{r.OutputPath ?? ""}");
				}
			}

			return reportPath;
		}
	}

	#region 批次處理相關資料模型

	/// <summary>批次處理進度</summary>
	public class BatchProgress
	{
		/// <summary>目前處理的檔案數</summary>
		public int Current { get; set; }

		/// <summary>總檔案數</summary>
		public int Total { get; set; }

		/// <summary>目前處理的檔案名稱</summary>
		public string CurrentFile { get; set; }

		/// <summary>已經過時間</summary>
		public TimeSpan Elapsed { get; set; }

		/// <summary>預估剩餘時間</summary>
		public TimeSpan EstimatedRemaining { get; set; }

		/// <summary>完成百分比</summary>
		public double PercentComplete { get; set; }

		/// <summary>格式化的進度文字 (例如: "15/100")</summary>
		public string ProgressText => $"{Current}/{Total}";
	}

	/// <summary>批次處理結果</summary>
	public class BatchResult
	{
		/// <summary>總處理數量</summary>
		public int TotalProcessed { get; set; }

		/// <summary>OK 數量</summary>
		public int OkCount { get; set; }

		/// <summary>NG 數量</summary>
		public int NgCount { get; set; }

		/// <summary>錯誤數量</summary>
		public int ErrorCount { get; set; }

		/// <summary>失敗的檔案清單</summary>
		public List<string> FailedFiles { get; set; } = new List<string>();

		/// <summary>輸出資料夾</summary>
		public string OutputFolder { get; set; }

		/// <summary>報表路徑</summary>
		public string ReportPath { get; set; }

		/// <summary>總處理時間 (毫秒)</summary>
		public int ProcessingTimeMs { get; set; }

		/// <summary>是否被取消</summary>
		public bool WasCancelled { get; set; }

		/// <summary>良率 (%)</summary>
		public double YieldRate => TotalProcessed > 0 ? (double)OkCount / TotalProcessed * 100 : 0;
	}

	/// <summary>批次處理選項</summary>
	public class BatchOptions
	{
		/// <summary>輸出資料夾 (null 則使用輸入資料夾下的 Output)</summary>
		public string OutputFolder { get; set; }

		/// <summary>搜尋子資料夾</summary>
		public bool SearchSubfolders { get; set; } = false;

		/// <summary>最大平行處理數 (0 = 自動)</summary>
		public int MaxParallelism { get; set; } = 0;

		/// <summary>生成 CSV 報表</summary>
		public bool GenerateCsvReport { get; set; } = true;

		/// <summary>儲存處理後的影像</summary>
		public bool SaveProcessedImages { get; set; } = true;

		/// <summary>生成 HTML 報表</summary>
		public bool GenerateHtmlReport { get; set; } = false;
	}

	/// <summary>單一檔案處理結果</summary>
	public class FileProcessResult
	{
		/// <summary>檔案路徑</summary>
		public string FilePath { get; set; }

		/// <summary>檔案名稱</summary>
		public string FileName { get; set; }

		/// <summary>是否 OK</summary>
		public bool IsOk { get; set; }

		/// <summary>缺陷數量</summary>
		public int DefectCount { get; set; }

		/// <summary>缺陷列表</summary>
		public List<Defect> Defects { get; set; }

		/// <summary>是否有錯誤</summary>
		public bool HasError { get; set; }

		/// <summary>錯誤訊息</summary>
		public string ErrorMessage { get; set; }

		/// <summary>輸出路徑</summary>
		public string OutputPath { get; set; }

		/// <summary>開始時間</summary>
		public DateTime StartTime { get; set; }

		/// <summary>結束時間</summary>
		public DateTime EndTime { get; set; }

		/// <summary>處理時間 (毫秒)</summary>
		public int ProcessingTimeMs { get; set; }
	}

	#endregion
}
