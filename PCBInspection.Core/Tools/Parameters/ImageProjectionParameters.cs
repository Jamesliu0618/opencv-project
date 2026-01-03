using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>影像投影參數 (對應 AxImageProjector)</summary>
	public class ImageProjectionParameters
	{
		/// <summary>投影方向</summary>
		public enum ProjectionDirection
		{
			/// <summary>水平投影 (沿 X 軸累加，輸出列向量)</summary>
			Horizontal,
			/// <summary>垂直投影 (沿 Y 軸累加，輸出行向量)</summary>
			Vertical,
		}

		/// <summary>選擇投影方向</summary>
		[DisplayName("投影方向")]
		public ProjectionDirection Direction { get; set; } = ProjectionDirection.Horizontal;

		/// <summary>累加方式</summary>
		public enum ReduceType
		{
			/// <summary>總和</summary>
			Sum,
			/// <summary>平均</summary>
			Average,
			/// <summary>最大值</summary>
			Max,
			/// <summary>最小值</summary>
			Min,
		}

		/// <summary>選擇累加方式</summary>
		[DisplayName("累加方式")]
		public ReduceType Type { get; set; } = ReduceType.Average;

		/// <summary>是否繪製投影圖表</summary>
		[DisplayName("繪製投影圖")] [Description("在影像邊緣繪製投影結果圖表")]
		public bool DrawProjection { get; set; } = true;
	}
}
