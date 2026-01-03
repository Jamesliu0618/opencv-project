using System;
using System.IO;
using Newtonsoft.Json;
using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Interfaces;
using PCBInspection.Core.Detectors;
using PCBInspection.Core.Services;

namespace PCBInspection.Core
{
    public class Pipeline
    {
        private readonly ICameraAdapter _camera;
        private readonly IAdvantechAdapter _io;
        private readonly string _artifactsDir;
        private readonly DefectAggregator _defectAggregator;

        public Pipeline(ICameraAdapter camera, IAdvantechAdapter io, string artifactsDir = "artifacts")
        {
            _camera = camera;
            _io = io;
            _artifactsDir = artifactsDir;
            _defectAggregator = new DefectAggregator();
            Directory.CreateDirectory(_artifactsDir);
        }

        public InspectionResult RunOnce(string pcbId = "pcb-000")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var frame = _camera.CaptureFrame();

            // 1. 零件定位與測量
            var comps = Localization.DetectComponents(frame);
            foreach (var c in comps)
            {
                var (wmm, hmm) = Measurement.ComponentSizeMm(c);
                c.WidthMm = wmm;
                c.HeightMm = hmm;
            }

            // 2. 瑕疵偵測
            var defectResult = _defectAggregator.Detect(frame, comps);

            // 3. 建立標註影像
            using (var annotated = AnnotationService.Annotate(frame, comps, defectResult.Defects, defectResult.Decision))
            {
                var annotatedPath = AnnotationService.SaveAnnotatedImage(annotated, _artifactsDir, pcbId);

                // 4. 建立檢測結果
                var result = new InspectionResult
                {
                    Id = Guid.NewGuid().ToString("N"),
                    PcbId = pcbId,
                    Ok = defectResult.IsOk,
                    AnnotatedImagePath = annotatedPath,
                    CreatedAt = DateTime.UtcNow,
                    ModelVersion = "1.0.0"
                };
                result.Components.AddRange(comps);
                result.Defects.AddRange(defectResult.Defects);

                // 5. 發送 IO 訊號
                SendIoSignal(defectResult.Decision);

                sw.Stop();
                result.ProcessingTimeMs = (int)sw.ElapsedMilliseconds;

                // 6. 生成報告
                var report = ReportingService.GenerateReport(result);
                result.ReportPath = ReportingService.SaveJsonReport(report, _artifactsDir);

                return result;
            }
        }

        private void SendIoSignal(InspectionDecision decision)
        {
            try
            {
                switch (decision)
                {
                    case InspectionDecision.OK:
                        _io.WriteDigitalOutput("OUTPUT_OK", true, 100);
                        break;
                    case InspectionDecision.NG:
                        _io.WriteDigitalOutput("OUTPUT_NG", true, 100);
                        break;
                    case InspectionDecision.REVIEW:
                        _io.WriteDigitalOutput("OUTPUT_REVIEW", true, 100);
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
