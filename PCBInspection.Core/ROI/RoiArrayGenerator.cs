using System;
using System.Collections.Generic;
using System.Drawing;

namespace PCBInspection.Core.ROI
{
	/// <summary>ROI 陣列產生器，用於批次建立重複排列的 ROI</summary>
	public class RoiArrayGenerator
	{
		/// <summary>產生網格陣列 ROI</summary>
		public List<RoiBase> GenerateGrid(RoiBase template, int rows, int cols, float spacingX, float spacingY)
		{
			var result = new List<RoiBase>();

			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					float offsetX = col * spacingX;
					float offsetY = row * spacingY;

					var clone = CloneRoi(template, offsetX, offsetY);
					clone.Name = $"{template.Name}_{row}_{col}";
					result.Add(clone);
				}
			}

			return result;
		}

		/// <summary>產生線性陣列 ROI</summary>
		public List<RoiBase> GenerateLinear(RoiBase template, int count, float spacing, float angleDeg)
		{
			var    result  = new List<RoiBase>();
			double radians = angleDeg * Math.PI / 180.0;

			for (int i = 0; i < count; i++)
			{
				float offsetX = (float)(i * spacing * Math.Cos(radians));
				float offsetY = (float)(i * spacing * Math.Sin(radians));

				var clone = CloneRoi(template, offsetX, offsetY);
				clone.Name = $"{template.Name}_{i}";
				result.Add(clone);
			}

			return result;
		}

		/// <summary>產生圓形陣列 ROI</summary>
		public List<RoiBase> GenerateCircular(RoiBase template, PointF center, float radius, int count)
		{
			var result       = new List<RoiBase>();
			var templateRect = GetRoiBounds(template);

			for (int i = 0; i < count; i++)
			{
				double angle = 2 * Math.PI * i / count;
				float  x     = center.X + (float)(radius * Math.Cos(angle)) - templateRect.Width / 2;
				float  y     = center.Y + (float)(radius * Math.Sin(angle)) - templateRect.Height / 2;

				float offsetX = x - templateRect.X;
				float offsetY = y - templateRect.Y;

				var clone = CloneRoi(template, offsetX, offsetY);
				clone.Name = $"{template.Name}_{i}";
				result.Add(clone);
			}

			return result;
		}

		/// <summary>複製 ROI 並套用位移</summary>
		private RoiBase CloneRoi(RoiBase template, float offsetX, float offsetY)
		{
			switch (template)
			{
				case RectangleRoi rect:
					return new RectangleRoi(
						rect.Rect.X + offsetX,
						rect.Rect.Y + offsetY,
						rect.Rect.Width,
						rect.Rect.Height)
					{
						Color = rect.Color
					};

				case CircleRoi circle:
					return new CircleRoi(
						new PointF(circle.Center.X + offsetX, circle.Center.Y + offsetY),
						circle.Radius)
					{
						Color = circle.Color
					};

				case EllipseRoi ellipse:
					return new EllipseRoi(
						new PointF(ellipse.Center.X + offsetX, ellipse.Center.Y + offsetY),
						ellipse.RadiusX,
						ellipse.RadiusY,
						ellipse.Rotation)
					{
						Color = ellipse.Color
					};

				case PolygonRoi polygon:
					var newPolygon = new PolygonRoi { Color = polygon.Color };
					foreach (var pt in polygon.Points)
					{
						newPolygon.Points.Add(new PointF(pt.X + offsetX, pt.Y + offsetY));
					}
					return newPolygon;

				default:
					throw new NotSupportedException($"不支援的 ROI 類型: {template.GetType().Name}");
			}
		}

		/// <summary>取得 ROI 的邊界矩形</summary>
		private RectangleF GetRoiBounds(RoiBase roi)
		{
			switch (roi)
			{
				case RectangleRoi rect:
					return rect.Rect;

				case CircleRoi circle:
					return new RectangleF(
						circle.Center.X - circle.Radius,
						circle.Center.Y - circle.Radius,
						circle.Radius * 2,
						circle.Radius * 2);

				case EllipseRoi ellipse:
					return new RectangleF(
						ellipse.Center.X - ellipse.RadiusX,
						ellipse.Center.Y - ellipse.RadiusY,
						ellipse.RadiusX * 2,
						ellipse.RadiusY * 2);

				case PolygonRoi polygon:
					if (polygon.Points.Count == 0)
						return RectangleF.Empty;

					float minX = float.MaxValue, minY = float.MaxValue;
					float maxX = float.MinValue, maxY = float.MinValue;

					foreach (var pt in polygon.Points)
					{
						minX = Math.Min(minX, pt.X);
						minY = Math.Min(minY, pt.Y);
						maxX = Math.Max(maxX, pt.X);
						maxY = Math.Max(maxY, pt.Y);
					}

					return new RectangleF(minX, minY, maxX - minX, maxY - minY);

				default:
					return RectangleF.Empty;
			}
		}
	}
}
