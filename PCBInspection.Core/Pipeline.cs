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

		public InspectionResult RunOnce(string pcbId = "pcb-000")
		{
			var sw    = Stopwatch.StartNew();
			var frame = _camera.CaptureFrame();

			// 1. 零件定位與測量
			var comps = Localization.DetectComponents(frame);

			foreach(var c in comps)
			{
				(double wmm, double hmm) = Measurement.ComponentSizeMm(c);
				c.WidthMm                = wmm;
				c.HeightMm               = hmm;
			}

			// 2. 瑕疵偵測
			AggregatedResult defectResult = _defectAggregator.Detect(frame, comps);

			// 3. 建立標註影像
			using(Mat annotated = AnnotationService.Annotate(frame, comps, defectResult.Defects, defectResult.Decision))
			{
				string annotatedPath = AnnotationService.SaveAnnotatedImage(annotated, _artifactsDir, pcbId);

				// 4. 建立檢測結果
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

				// 5. 發送 IO 訊號
				SendIoSignal(defectResult.Decision);
				sw.Stop();
				result.ProcessingTimeMs = (int)sw.ElapsedMilliseconds;

				// 6. 生成報告
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