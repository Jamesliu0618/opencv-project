using System.ComponentModel;

namespace PCBInspection.Core.Tools
{
	/// <summary>輪廓搜尋參數</summary>
	public class ContourFindParameters
	{
		/// <summary>輪廓近似方法</summary>
		public enum ContourApproxType
		{
			/// <summary>保留所有點</summary>
			None,
			/// <summary>壓縮水平、垂直或對角線段，僅保留端點</summary>
			Simple,
			/// <summary>Teh-Chin 連接演算法 L1</summary>
			TC89_L1,
			/// <summary>Teh-Chin 連接演算法 KCOS</summary>
			TC89_KCOS,
		}

		/// <summary>輪廓檢索模式</summary>
		public enum ContourModeType
		{
			/// <summary>僅檢索最外層輪廓</summary>
			External,
			/// <summary>檢索所有輪廓且不建立階層關係</summary>
			List,
			/// <summary>檢索所有輪廓並組織成兩層階層</summary>
			CComp,
			/// <summary>檢索所有輪廓並建立完整的樹狀階層</summary>
			Tree,
		}

		/// <summary>選擇輪廓檢索模式</summary>
		[DisplayName("輪廓模式")] [Description("- External: 僅最外層輪廓\n- List: 所有輪廓 (無階層)\n- CComp: 兩層結構\n- Tree: 完整階層")]
		public ContourModeType Mode { get; set; } = ContourModeType.External;

		/// <summary>選擇輪廓點近似方法</summary>
		[DisplayName("近似方法")] [Description("- None: 保留所有點\n- Simple: 壓縮水平/垂直/對角線段")]
		public ContourApproxType ApproxMethod { get; set; } = ContourApproxType.Simple;

		/// <summary>最小輪廓面積過濾 (像素)</summary>
		[DisplayName("最小面積")] [Description("過濾小於此面積的輪廓 (像素數)")]
		public double MinArea { get; set; } = 100;

		/// <summary>最大輪廓面積過濾 (像素)，設為 0 表示不限制</summary>
		[DisplayName("最大面積")] [Description("過濾大於此面積的輪廓 (像素數)。設為 0 表示不限制。")]
		public double MaxArea { get; set; } = 0;

		/// <summary>是否在結果影像中繪製輪廓線</summary>
		[DisplayName("繪製輪廓")] [Description("是否在輸出影像上繪製輪廓")]
		public bool DrawContours { get; set; } = true;
	}
}
