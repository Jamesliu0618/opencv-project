using System;
using System.IO;
using Newtonsoft.Json;
using OpenCvSharp;
using PCBInspection.Core.Models;
using PCBInspection.Core.Interfaces;

namespace PCBInspection.Core
{
    public class Pipeline
    {
        private readonly ICameraAdapter _camera;
        private readonly IAdvantechAdapter _io;
        private readonly string _artifactsDir;

        public Pipeline(ICameraAdapter camera, IAdvantechAdapter io, string artifactsDir = "artifacts")
        {
            _camera = camera;
            _io = io;
            _artifactsDir = artifactsDir;
            Directory.CreateDirectory(_artifactsDir);
        }

        public InspectionResult RunOnce(string pcbId = "pcb-000")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var frame = _camera.CaptureFrame();

            // Detect components and annotate frame with measurements
            var comps = Localization.DetectComponents(frame);
            foreach (var c in comps)
            {
                var (wmm, hmm) = Measurement.ComponentSizeMm(c);
                c.WidthMm = wmm; c.HeightMm = hmm;

                var tl = new Point((int)(c.CenterX_Px - c.SizeW_Px / 2.0), (int)(c.CenterY_Px - c.SizeH_Px / 2.0));
                var br = new Point((int)(c.CenterX_Px + c.SizeW_Px / 2.0), (int)(c.CenterY_Px + c.SizeH_Px / 2.0));
                var rect = new Rect(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);
                Cv2.Rectangle(frame, rect, Scalar.Green, 2);
                Cv2.PutText(frame, $"{wmm:F2}x{hmm:F2} mm", new Point(tl.X, tl.Y - 6), HersheyFonts.HersheySimplex, 0.5, Scalar.Blue, 1);
            }

            var annotatedPath = Path.Combine(_artifactsDir, $"{pcbId}_annotated_{DateTime.UtcNow:yyyyMMddHHmmss}.png");
            Cv2.PutText(frame, "Annotated", new Point(10, 30), HersheyFonts.HersheySimplex, 1.0, Scalar.Red, 2);
            Cv2.ImWrite(annotatedPath, frame);

            // Basic decision logic (no defects)
            var result = new InspectionResult
            {
                Id = Guid.NewGuid().ToString("N"),
                PcbId = pcbId,
                Ok = true,
                AnnotatedImagePath = annotatedPath,
                CreatedAt = DateTime.UtcNow
            };
            result.Components.AddRange(comps);

            // Send OK signal
            try
            {
                _io.WriteDigitalOutput("OUTPUT_OK", true, 100);
            }
            catch
            {
                // log and continue
            }

            sw.Stop();
            result.ProcessingTimeMs = (int)sw.ElapsedMilliseconds;

            // Write report
            var reportPath = Path.Combine(_artifactsDir, $"{pcbId}_report_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
            result.ReportPath = reportPath;
            File.WriteAllText(reportPath, JsonConvert.SerializeObject(result, Formatting.Indented));

            return result;
        }
    }
}