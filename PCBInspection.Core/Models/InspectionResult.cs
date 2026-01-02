using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Models
{
    public class Defect
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int Severity { get; set; }
        public double Confidence { get; set; }
    }

    public class InspectionResult
    {
        public string Id { get; set; }
        public string PcbId { get; set; }
        public bool Ok { get; set; }
        public List<Defect> Defects { get; set; } = new List<Defect>();
        public string AnnotatedImagePath { get; set; }
        public string ReportPath { get; set; }
        public string ModelVersion { get; set; }
        public int ProcessingTimeMs { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}