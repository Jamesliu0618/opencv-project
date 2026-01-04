using OpenCvSharp;
using PCBInspection.Core.ROI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCBInspection.Core.Services
{
	/// <summary>ROI 管理服務，提供全域 ROI 清單與編號存取</summary>
	public static class RoiManager
	{
		private static readonly List<RoiBase> _rois = new List<RoiBase>();
		private static int _nextIndex = 1;

		/// <summary>取得目前所有已註冊的 ROI</summary>
		public static IReadOnlyList<RoiBase> Rois => _rois.AsReadOnly();

		/// <summary>清除並重新匯入 ROI 清單 (通常由 UI 呼叫)</summary>
		public static void SetRois(IEnumerable<RoiBase> rois)
		{
			_rois.Clear();
			_nextIndex = 1;
			foreach(var roi in rois)
			{
				roi.Index = _nextIndex++;
				_rois.Add(roi);
			}
		}

		/// <summary>取得指定編號的 ROI (編號從 1 開始)</summary>
		public static RoiBase GetByIndex(int index)
		{
			return _rois.FirstOrDefault(r => r.Index == index);
		}

		/// <summary>取得指定名稱的 ROI</summary>
		public static RoiBase GetByName(string name)
		{
			return _rois.FirstOrDefault(r => r.Name == name);
		}

		/// <summary>將 RoiBase 轉換為 OpenCV Rect</summary>
		public static Rect? GetRectFromRoi(RoiBase roi)
		{
			if(roi == null) return null;

			if(roi is RectangleRoi rect)
			{
				return new Rect((int)rect.Rect.X, (int)rect.Rect.Y, (int)rect.Rect.Width, (int)rect.Rect.Height);
			}
			else if(roi is CircleRoi circ)
			{
				int x = (int)(circ.Center.X - circ.Radius);
				int y = (int)(circ.Center.Y - circ.Radius);
				int d = (int)(circ.Radius * 2);
				return new Rect(x, y, d, d);
			}
			// For Polygon/Ellipse, use bounding box
			return null;
		}

		/// <summary>依編號取得 ROI 並轉換為 Rect</summary>
		public static Rect? GetRectByIndex(int index)
		{
			var roi = GetByIndex(index);
			return GetRectFromRoi(roi);
		}
	}
}
