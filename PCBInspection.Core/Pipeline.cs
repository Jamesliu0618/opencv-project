using OpenCvSharp;
using PCBInspection.Core.Detectors;
using PCBInspection.Core.Interfaces;
using PCBInspection.Core.Models;
using PCBInspection.Core.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace PCBInspection.Core
{
	/// <summary>
	///     自動化檢測管線 (Inspection Pipeline)。
	///     負責串接相機擷取、演算法定位、瑕疵偵測、標註繪製、硬體 IO 訊號發送以及報表生成。
	/// </summary>
	public class Pipeline
	{
		private readonly string            _artifactsDir;
		private readonly ICameraAdapter    _camera;
		private readonly DefectAggregator  _defectAggregator;
		private readonly IAdvantechAdapter _io;

		public Pipeline(ICameraAdapter camera, IAdvantechAdapter io, string artifactsDir = "artifacts")
		{
			_camera           = camera;
			_io               = io;
			_artifactsDir     = artifactsDir;
			_defectAggregator = new DefectAggregator();
			Directory.CreateDirectory(_artifactsDir);
		}

		/// <summary>
		///     執行一次完整的檢測循環 (Capture -> Process -> Output)。
		/// </summary>
		/// <param name="pcbId">目前的 PCB 編號</param>
		/// <returns>彙整後的檢測結果</returns>
		public InspectionResult RunOnce(string pcbId = "pcb-000")
		{
			var sw    = Stopwatch.StartNew();
			var frame = _camera.CaptureFrame();

			// 1. 零件定位與量測：識別 PCB 上的各個元件並計算其物理尺寸
			var comps = Localization.DetectComponents(frame);

			foreach(var c in comps)
			{
				(double wmm, double hmm) = Measurement.ComponentSizeMm(c);
				c.WidthMm                = wmm;
				c.HeightMm               = hmm;
			}

			// 2. 綜合瑕疵偵測：執行電路、焊點、表面等多重偵測並進行結果聚合與 NMS 過濾
			AggregatedResult defectResult = _defectAggregator.Detect(frame, comps);

			// 3. 影像標註：將偵測到的元件與瑕疵資訊繪製到原圖上，生成可視化結果
			using(Mat annotated = AnnotationService.Annotate(frame, comps, defectResult.Defects, defectResult.Decision))
			{
				string annotatedPath = AnnotationService.SaveAnnotatedImage(annotated, _artifactsDir, pcbId);

				// 4. 建立檢測結果資料模型
				InspectionResult result = new InspectionResult
				{
					Id                 = Guid.NewGuid().ToString("N"),
					PcbId              = pcbId,
					Ok                 = defectResult.IsOk,
					AnnotatedImagePath = annotatedPath,
					CreatedAt          = DateTime.UtcNow,
					ModelVersion       = "1.0.0",
				};
				result.Components.AddRange(comps);
				result.Defects.AddRange(defectResult.Defects);

				// 5. 輸出控制：根據檢測結果發送正確的硬體 IO 訊號 (OK/NG/REVIEW)
				SendIoSignal(defectResult.Decision);
				sw.Stop();
				result.ProcessingTimeMs = (int)sw.ElapsedMilliseconds;

				// 6. 生成報告：輸出 JSON 與 HTML 格式的檢測報告
				InspectionReport report = ReportingService.GenerateReport(result);
				result.ReportPath = ReportingService.SaveJsonReport(report, _artifactsDir);
				return result;
			}
		}

		private void SendIoSignal(InspectionDecision decision)
		{
			try
			{
				switch(decision)
				{
					case InspectionDecision.OK:
						_io.WriteDigitalOutput("OUTPUT_OK", true);
						break;
					case InspectionDecision.NG:
						_io.WriteDigitalOutput("OUTPUT_NG", true);
						break;
					case InspectionDecision.REVIEW:
						_io.WriteDigitalOutput("OUTPUT_REVIEW", true);
						break;
				}
			}
			catch
			{
				// 記錄錯誤但繼續執行
			}
		}
	}
}