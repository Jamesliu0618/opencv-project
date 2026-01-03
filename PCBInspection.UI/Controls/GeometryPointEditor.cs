using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using PCBInspection.Core.Models;
using System.Collections.Generic;

namespace PCBInspection.UI.Controls
{
	public class GeometryPointEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService editorService = provider?.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			if (editorService != null)
			{
				// 取得當前的 GeoPoint (若為 null 則新建)
				GeoPoint currentPoint = value as GeoPoint ?? new GeoPoint();

				// 建立選擇器控件
				var selector = new GeometryPointSelector
				{
					X = currentPoint.X,
					Y = currentPoint.Y
				};

				// 從 MainForm 取得檢測物件列表 (透過靜態屬性或方法)
				// 這裡假設 MainForm 有一個靜態方法或屬性可以存取
				// 若 MainForm 無法直接存取，需透過 context.Instance 取得 (如果有傳遞參考)
				// 暫時使用 MainForm.Instance (需確認是否已實作)
				if (MainForm.Instance != null)
				{
					selector.UpdateObjectList(MainForm.Instance.GetDetectedObjects());
				}

				// 訂閱事件以便在選擇後自動關閉下拉視窗 (可選)
				// selector.CoordinateChanged += (s, e) => { /* do nothing, wait for close */ };

				// 顯示下拉視窗
				editorService.DropDownControl(selector);

				// 更新值
				currentPoint.X = (int)selector.X;
				currentPoint.Y = (int)selector.Y;

				return currentPoint;
			}

			return base.EditValue(context, provider, value);
		}
	}
}
