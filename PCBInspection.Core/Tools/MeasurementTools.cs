using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PCBInspection.Core.Tools
{
	/// <summary>測量工具類型列舉</summary>
	public enum MeasurementType
	{
		Distance,
		Angle,
		Polyline,
		Area,
		CircleCenter,
		Parallelism,
		Perpendicularity
	}

	/// <summary>測量工具基底類別</summary>
	public abstract class MeasurementToolBase
	{
		/// <summary>測量識別碼</summary>
		public string Id { get; } = Guid.NewGuid().ToString("N");

		/// <summary>測量名稱</summary>
		public string Name { get; set; }

		/// <summary>測量類型</summary>
		public abstract MeasurementType Type { get; }

		/// <summary>測量結果</summary>
		public MeasurementResult Result { get; protected set; }

		/// <summary>執行測量</summary>
		public abstract void Execute();

		/// <summary>繪製測量標記</summary>
		public abstract void Draw(Graphics g, float scale, float offsetX, float offsetY);
	}

	/// <summary>點到點距離測量</summary>
	public class DistanceMeasurement : MeasurementToolBase
	{
		public override MeasurementType Type => MeasurementType.Distance;

		/// <summary>起點</summary>
		public PointF Point1 { get; set; }

		/// <summary>終點</summary>
		public PointF Point2 { get; set; }

		/// <summary>像素尺寸 (mm/pixel)</summary>
		public double PixelSizeMm { get; set; } = 1.0;

		public override void Execute()
		{
			double distPx = Math.Sqrt(Math.Pow(Point2.X - Point1.X, 2) + Math.Pow(Point2.Y - Point1.Y, 2));
			double distMm = distPx * PixelSizeMm;

			Result = new MeasurementResult
			{
				MeasurementId = Id,
				Type          = "Distance",
				Value         = distMm,
				ValuePixel    = distPx,
				Unit          = "mm"
			};
		}

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			var p1 = new PointF(Point1.X * scale + offsetX, Point1.Y * scale + offsetY);
			var p2 = new PointF(Point2.X * scale + offsetX, Point2.Y * scale + offsetY);

			using (var pen = new Pen(Color.Cyan, 2))
			{
				g.DrawLine(pen, p1, p2);
				g.FillEllipse(Brushes.Cyan, p1.X - 4, p1.Y - 4, 8, 8);
				g.FillEllipse(Brushes.Cyan, p2.X - 4, p2.Y - 4, 8, 8);
			}

			if (Result != null)
			{
				var midPt = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2 - 15);
				g.DrawString($"{Result.Value:F2} {Result.Unit}", SystemFonts.DefaultFont, Brushes.Yellow, midPt);
			}
		}
	}

	/// <summary>三點角度測量</summary>
	public class AngleMeasurement : MeasurementToolBase
	{
		public override MeasurementType Type => MeasurementType.Angle;

		/// <summary>角度起點</summary>
		public PointF Point1 { get; set; }

		/// <summary>角度頂點</summary>
		public PointF Vertex { get; set; }

		/// <summary>角度終點</summary>
		public PointF Point2 { get; set; }

		public override void Execute()
		{
			double v1x = Point1.X - Vertex.X;
			double v1y = Point1.Y - Vertex.Y;
			double v2x = Point2.X - Vertex.X;
			double v2y = Point2.Y - Vertex.Y;

			double dot   = v1x * v2x + v1y * v2y;
			double mag1  = Math.Sqrt(v1x * v1x + v1y * v1y);
			double mag2  = Math.Sqrt(v2x * v2x + v2y * v2y);
			double angle = Math.Acos(dot / (mag1 * mag2)) * 180.0 / Math.PI;

			Result = new MeasurementResult
			{
				MeasurementId = Id,
				Type          = "Angle",
				Value         = angle,
				ValuePixel    = angle,
				Unit          = "°"
			};
		}

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			var p1 = new PointF(Point1.X * scale + offsetX, Point1.Y * scale + offsetY);
			var v  = new PointF(Vertex.X * scale + offsetX, Vertex.Y * scale + offsetY);
			var p2 = new PointF(Point2.X * scale + offsetX, Point2.Y * scale + offsetY);

			using (var pen = new Pen(Color.Orange, 2))
			{
				g.DrawLine(pen, v, p1);
				g.DrawLine(pen, v, p2);
				g.FillEllipse(Brushes.Orange, v.X - 5, v.Y - 5, 10, 10);
			}

			if (Result != null)
			{
				g.DrawString($"{Result.Value:F1}°", SystemFonts.DefaultFont, Brushes.Yellow, v.X + 10, v.Y - 20);
			}
		}
	}

	/// <summary>多段線長度測量（累計測量）</summary>
	public class PolylineMeasurement : MeasurementToolBase
	{
		public override MeasurementType Type => MeasurementType.Polyline;

		/// <summary>點集合</summary>
		public List<PointF> Points { get; set; } = new List<PointF>();

		/// <summary>像素尺寸 (mm/pixel)</summary>
		public double PixelSizeMm { get; set; } = 1.0;

		public override void Execute()
		{
			double totalPx = 0;
			for (int i = 1; i < Points.Count; i++)
			{
				totalPx += Math.Sqrt(
					Math.Pow(Points[i].X - Points[i - 1].X, 2) +
					Math.Pow(Points[i].Y - Points[i - 1].Y, 2));
			}

			Result = new MeasurementResult
			{
				MeasurementId = Id,
				Type          = "Polyline",
				Value         = totalPx * PixelSizeMm,
				ValuePixel    = totalPx,
				Unit          = "mm"
			};
		}

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			if (Points.Count < 2) return;

			var screenPoints = Points.Select(p =>
				new PointF(p.X * scale + offsetX, p.Y * scale + offsetY)).ToArray();

			using (var pen = new Pen(Color.Lime, 2))
			{
				g.DrawLines(pen, screenPoints);
				foreach (var sp in screenPoints)
				{
					g.FillEllipse(Brushes.Lime, sp.X - 3, sp.Y - 3, 6, 6);
				}
			}

			if (Result != null)
			{
				var lastPt = screenPoints.Last();
				g.DrawString($"Σ {Result.Value:F2} {Result.Unit}", SystemFonts.DefaultFont, Brushes.Yellow, lastPt.X + 5, lastPt.Y - 15);
			}
		}
	}

	/// <summary>多邊形面積測量</summary>
	public class AreaMeasurement : MeasurementToolBase
	{
		public override MeasurementType Type => MeasurementType.Area;

		/// <summary>多邊形頂點</summary>
		public List<PointF> PolygonPoints { get; set; } = new List<PointF>();

		/// <summary>像素尺寸 (mm/pixel)</summary>
		public double PixelSizeMm { get; set; } = 1.0;

		public override void Execute()
		{
			if (PolygonPoints.Count < 3)
			{
				Result = new MeasurementResult { MeasurementId = Id, Type = "Area", Value = 0, Unit = "mm²" };
				return;
			}

			// Shoelace formula
			double areaPx = 0;
			int    n      = PolygonPoints.Count;
			for (int i = 0; i < n; i++)
			{
				int j = (i + 1) % n;
				areaPx += PolygonPoints[i].X * PolygonPoints[j].Y;
				areaPx -= PolygonPoints[j].X * PolygonPoints[i].Y;
			}
			areaPx = Math.Abs(areaPx) / 2.0;

			double areaMm = areaPx * PixelSizeMm * PixelSizeMm;

			Result = new MeasurementResult
			{
				MeasurementId = Id,
				Type          = "Area",
				Value         = areaMm,
				ValuePixel    = areaPx,
				Unit          = "mm²"
			};
		}

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			if (PolygonPoints.Count < 3) return;

			var screenPoints = PolygonPoints.Select(p =>
				new PointF(p.X * scale + offsetX, p.Y * scale + offsetY)).ToArray();

			using (var pen   = new Pen(Color.Magenta, 2))
			using (var brush = new SolidBrush(Color.FromArgb(50, Color.Magenta)))
			{
				g.FillPolygon(brush, screenPoints);
				g.DrawPolygon(pen, screenPoints);
			}

			if (Result != null)
			{
				var center = new PointF(
					screenPoints.Average(p => p.X),
					screenPoints.Average(p => p.Y));
				g.DrawString($"{Result.Value:F2} {Result.Unit}", SystemFonts.DefaultFont, Brushes.Yellow, center);
			}
		}
	}

	/// <summary>圓心距測量</summary>
	public class CircleCenterDistanceMeasurement : MeasurementToolBase
	{
		public override MeasurementType Type => MeasurementType.CircleCenter;

		/// <summary>圓1 中心</summary>
		public PointF Center1 { get; set; }

		/// <summary>圓1 半徑 (顯示用)</summary>
		public float Radius1 { get; set; }

		/// <summary>圓2 中心</summary>
		public PointF Center2 { get; set; }

		/// <summary>圓2 半徑 (顯示用)</summary>
		public float Radius2 { get; set; }

		/// <summary>像素尺寸 (mm/pixel)</summary>
		public double PixelSizeMm { get; set; } = 1.0;

		public override void Execute()
		{
			double distPx = Math.Sqrt(Math.Pow(Center2.X - Center1.X, 2) + Math.Pow(Center2.Y - Center1.Y, 2));

			Result = new MeasurementResult
			{
				MeasurementId = Id,
				Type          = "CircleCenterDistance",
				Value         = distPx * PixelSizeMm,
				ValuePixel    = distPx,
				Unit          = "mm"
			};
		}

		public override void Draw(Graphics g, float scale, float offsetX, float offsetY)
		{
			var c1 = new PointF(Center1.X * scale + offsetX, Center1.Y * scale + offsetY);
			var c2 = new PointF(Center2.X * scale + offsetX, Center2.Y * scale + offsetY);
			var r1 = Radius1 * scale;
			var r2 = Radius2 * scale;

			using (var pen = new Pen(Color.DeepPink, 2))
			{
				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
				g.DrawEllipse(pen, c1.X - r1, c1.Y - r1, r1 * 2, r1 * 2);
				g.DrawEllipse(pen, c2.X - r2, c2.Y - r2, r2 * 2, r2 * 2);

				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
				g.DrawLine(pen, c1, c2);

				g.FillEllipse(Brushes.DeepPink, c1.X - 4, c1.Y - 4, 8, 8);
				g.FillEllipse(Brushes.DeepPink, c2.X - 4, c2.Y - 4, 8, 8);
			}

			if (Result != null)
			{
				var midPt = new PointF((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2 - 15);
				g.DrawString($"{Result.Value:F2} {Result.Unit}", SystemFonts.DefaultFont, Brushes.Yellow, midPt);
			}
		}
	}

	/// <summary>測量結果</summary>
	public class MeasurementResult
	{
		/// <summary>測量識別碼</summary>
		public string MeasurementId { get; set; }

		/// <summary>測量類型</summary>
		public string Type { get; set; }

		/// <summary>測量值 (含單位換算)</summary>
		public double Value { get; set; }

		/// <summary>像素值</summary>
		public double ValuePixel { get; set; }

		/// <summary>單位</summary>
		public string Unit { get; set; }

		/// <summary>公差上限</summary>
		public double? ToleranceMax { get; set; }

		/// <summary>公差下限</summary>
		public double? ToleranceMin { get; set; }

		/// <summary>是否在公差範圍內</summary>
		public bool IsWithinTolerance
		{
			get
			{
				if (!ToleranceMin.HasValue && !ToleranceMax.HasValue) return true;
				if (ToleranceMin.HasValue && Value < ToleranceMin.Value) return false;
				if (ToleranceMax.HasValue && Value > ToleranceMax.Value) return false;
				return true;
			}
		}
	}

	/// <summary>測量管理器</summary>
	public class MeasurementManager
	{
		/// <summary>所有測量項目</summary>
		public List<MeasurementToolBase> Measurements { get; } = new List<MeasurementToolBase>();

		/// <summary>新增測量</summary>
		public void Add(MeasurementToolBase measurement)
		{
			measurement.Execute();
			Measurements.Add(measurement);
		}

		/// <summary>移除測量</summary>
		public bool Remove(string id)
		{
			var item = Measurements.FirstOrDefault(m => m.Id == id);
			if (item != null)
			{
				Measurements.Remove(item);
				return true;
			}
			return false;
		}

		/// <summary>清除所有測量</summary>
		public void Clear()
		{
			Measurements.Clear();
		}

		/// <summary>繪製所有測量標記</summary>
		public void DrawAll(Graphics g, float scale, float offsetX, float offsetY)
		{
			foreach (var m in Measurements)
			{
				m.Draw(g, scale, offsetX, offsetY);
			}
		}

		/// <summary>取得所有測量結果</summary>
		public List<MeasurementResult> GetAllResults()
		{
			return Measurements.Where(m => m.Result != null).Select(m => m.Result).ToList();
		}

		/// <summary>匯出測量結果為 CSV</summary>
		public string ExportToCsv()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendLine("Id,Name,Type,Value,Unit,IsOk");

			foreach (var m in Measurements.Where(x => x.Result != null))
			{
				sb.AppendLine($"{m.Id},{m.Name},{m.Result.Type},{m.Result.Value:F4},{m.Result.Unit},{m.Result.IsWithinTolerance}");
			}

			return sb.ToString();
		}
	}
}
