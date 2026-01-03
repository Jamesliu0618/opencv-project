using System;
using System.Collections.Generic;

namespace PCBInspection.Core.Models
{
	/// <summary>缺陷細節資訊</summary>
	public class Defect
	{
		/// <summary>缺陷識別號</summary>
		public string Id          { get; set; }
		/// <summary>缺陷類型 (例如: 刮傷, 缺件)</summary>
		public string Type        { get; set; }
		/// <summary>嚴重程度 (通常為 0-100)</summary>
		public int    Severity    { get; set; }
		/// <summary>置信度或特徵值 (例如: 面積, 圓度)</summary>
		public double Confidence  { get; set; }
		/// <summary>邊界框 [x, y, width, height]</summary>
		public int[]  BoundingBox { get; set; } // [x, y, width, height]
		
		/// <summary>圓度 (0-1)</summary>
		public double Circularity { get; set; }
		/// <summary>矩形度 (0-1)</summary>
		public double Rectangularity { get; set; }
		/// <summary>角度 (度)</summary>
		public double Angle { get; set; }
		
		/// <summary>面積 (像素平方)</summary>
		public double Area { get; set; }
		/// <summary>重心 X 座標</summary>
		public double CenterX { get; set; }
		/// <summary>重心 Y 座標</summary>
		public double CenterY { get; set; }
	}

	/// <summary>完整檢測結果報告</summary>
	public class InspectionResult
	{
		/// <summary>報告 ID</summary>
		public string          Id                 { get; set; }
		/// <summary>PCB 序號或標識</summary>
		public string          PcbId              { get; set; }
		/// <summary>是否通過檢測 (OK/NG)</summary>
		public bool            Ok                 { get; set; }
		/// <summary>偵測到的缺陷列表</summary>
		public List<Defect>    Defects            { get; set; } = new List<Defect>();
		/// <summary>定位到的元件列表</summary>
		public List<Component> Components         { get; set; } = new List<Component>();
		/// <summary>註記後的影像存檔路徑</summary>
		public string          AnnotatedImagePath { get; set; }
		/// <summary>報告檔案路徑</summary>
		public string          ReportPath         { get; set; }
		/// <summary>使用的模型版本</summary>
		public string          ModelVersion       { get; set; }
		/// <summary>處理總耗時 (毫秒)</summary>
		public int             ProcessingTimeMs   { get; set; }
		/// <summary>結果產生時間</summary>
		public DateTime        CreatedAt          { get; set; }
	}
}