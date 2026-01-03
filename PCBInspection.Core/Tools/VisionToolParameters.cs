using OpenCvSharp;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using PCBInspection.Core.Models;

namespace PCBInspection.Core.Tools
{
	// ===== 01. 預處理 (Preprocessing) =====

	/// <summary>灰階化參數 (目前無可配置參數)</summary>
	public class GrayscaleParameters
	{
		// No parameters needed
	}

	/// <summary>濾波模糊參數</summary>
	public class BlurParameters
	{
		/// <summary>模糊演算法類型</summary>
		public enum BlurType
		{
			/// <summary>高斯模糊</summary>
			Gaussian,
			/// <summary>中值濾波</summary>
			Median,
			/// <summary>均值模糊</summary>
			Box,
			/// <summary>雙邊濾波</summary>
			Bilateral,
		}

		/// <summary>選擇模糊方案</summary>
		[DisplayName("模糊類型")] [Description("選擇模糊演算法:\n- Gaussian: 高斯模糊 (常用)\n- Median: 中值濾波 (去椒鹽雜訊)\n- Box: 均值模糊\n- Bilateral: 雙邊濾波 (保留邊緣)")]
		public BlurType Type { get; set; } = BlurType.Gaussian;

		/// <summary>核心大小 (像素)，必須為奇數</summary>
		[DisplayName("核心大小 (px)")] [Description("濾波核大小，必須為奇數 (e.g. 3, 5, 7, 9)")]
		public int KernelSize { get; set; } = 5;

		/// <summary>高斯/雙邊濾波的標準差</summary>
		[DisplayName("Sigma")] [Description("高斯/雙邊濾波的標準差。設為 0 表示自動計算。")]
		public double Sigma { get; set; } = 0;
	}

	/// <summary>二值化分割參數</summary>
	public class ThresholdParameters
	{
		/// <summary>二值化處理方法</summary>
		public enum ThreshMethod
		{
			/// <summary>標準二值化</summary>
			Binary,
			/// <summary>反向二值化</summary>
			BinaryInv,
			/// <summary>Otsu 自動閾值</summary>
			Otsu,
			/// <summary>自適應閾值</summary>
			Adaptive,
			/// <summary>低於閾值歸零</summary>
			ToZero,
		}

		/// <summary>選擇二值化方案</summary>
		[DisplayName("閾值方法")] [Description("選擇二值化方法:\n- Binary: 標準二值化\n- BinaryInv: 反向二值化\n- Otsu: 自動計算最佳閾值\n- Adaptive: 自適應閾值\n- ToZero: 低於閾值設為 0")]
		public ThreshMethod Method { get; set; } = ThreshMethod.Binary;

		/// <summary>手動設定之閾值 (0-255)</summary>
		[DisplayName("閾值 (0-255)")] [Description("手動閾值。若使用 Otsu/Adaptive 則此值會被忽略。")]
		public double Threshold { get; set; } = 128;

		/// <summary>二值化後的最大亮度值 (通常為 255)</summary>
		[DisplayName("最大值 (0-255)")] [Description("二值化後的最大值 (通常為 255)")]
		public double MaxVal { get; set; } = 255;

		/// <summary>自適應區塊大小，須為奇數</summary>
		[DisplayName("自適應區塊大小")] [Description("僅用於 Adaptive 方法，必須為奇數")]
		public int AdaptiveBlockSize { get; set; } = 11;

		/// <summary>自適應常數 C，從均值中減去之數值</summary>
		[DisplayName("自適應常數 C")] [Description("僅用於 Adaptive 方法，從均值中減去的常數")]
		public double AdaptiveC { get; set; } = 2;
	}

	/// <summary>形態學運算參數</summary>
	public class MorphologyParameters
	{
		/// <summary>形態學運算子類型</summary>
		public enum MorphOp
		{
			/// <summary>腐蝕</summary>
			Erode,
			/// <summary>膨脹</summary>
			Dilate,
			/// <summary>開運算</summary>
			Open,
			/// <summary>閉運算</summary>
			Close,
			/// <summary>形態學梯度</summary>
			Gradient,
			/// <summary>頂帽</summary>
			TopHat,
			/// <summary>黑帽</summary>
			BlackHat,
		}

		/// <summary>選擇形態學運算子</summary>
		[DisplayName("運算類型")] [Description("形態學運算:\n- Erode: 腐蝕\n- Dilate: 膨脹\n- Open: 開運算 (先腐後膨)\n- Close: 閉運算 (先膨後腐)\n- Gradient: 形態學梯度\n- TopHat: 頂帽\n- BlackHat: 黑帽")]
		public MorphOp Operation { get; set; } = MorphOp.Open;

		/// <summary>結構元素 (核心) 之形狀</summary>
		[DisplayName("核心形狀")] [Description("結構元素形狀: Rect (矩形), Cross (十字), Ellipse (橢圓)")]
		public MorphShapes Shape { get; set; } = MorphShapes.Rect;

		/// <summary>結構元素之大小 (像素)</summary>
		[DisplayName("核心大小 (px)")] [Description("結構元素大小")]
		public int KernelSize { get; set; } = 3;

		/// <summary>重疊執行運算的次數</summary>
		[DisplayName("迭代次數")] [Description("運算重複執行的次數")]
		public int Iterations { get; set; } = 1;
	}

	// ===== 02. 色彩處理 (Color Processing) =====

	/// <summary>色彩空間轉換參數</summary>
	public class ColorConvertParameters
	{
		/// <summary>轉換目標類型</summary>
		public enum ConversionType
		{
			/// <summary>BGR 轉灰階</summary>
			BGR2Gray,
			/// <summary>BGR 轉 HSV</summary>
			BGR2HSV,
			/// <summary>BGR 轉 Lab</summary>
			BGR2Lab,
			/// <summary>HSV 轉 BGR</summary>
			HSV2BGR,
			/// <summary>灰階轉 BGR (3通道)</summary>
			Gray2BGR,
		}

		/// <summary>選擇色彩轉換類型</summary>
		[DisplayName("轉換類型")] [Description("色彩空間轉換:\n- BGR2Gray: 彩色轉灰階\n- BGR2HSV: 轉 HSV (色相/飽和度/明度)\n- BGR2Lab: 轉 Lab 色彩空間\n- HSV2BGR: HSV 轉回 BGR\n- Gray2BGR: 灰階轉 BGR (3通道)")]
		public ConversionType Type { get; set; } = ConversionType.BGR2Gray;
	}

	/// <summary>直方圖均衡化參數</summary>
	public class HistogramEqualizeParameters
	{
		/// <summary>是否使用 CLAHE (對比度受限自適應直方圖均衡化)</summary>
		[DisplayName("使用 CLAHE")] [Description("使用對比度受限自適應直方圖均衡化 (CLAHE)，適用於局部對比度增強")]
		public bool UseCLAHE { get; set; } = false;

		/// <summary>CLAHE 對比度限制閾值</summary>
		[DisplayName("CLAHE Clip Limit")] [Description("對比度限制閾值 (僅用於 CLAHE)")]
		public double ClipLimit { get; set; } = 2.0;

		/// <summary>CLAHE 網格區塊大小</summary>
		[DisplayName("CLAHE 區塊大小")] [Description("區塊大小 (僅用於 CLAHE)")]
		public int TileGridSize { get; set; } = 8;
	}

	/// <summary>色彩範圍過濾參數 (In-Range 閾值)</summary>
	public class InRangeParameters
	{
		/// <summary>色彩範圍下界 - 第 1 通道 (H/B)</summary>
		[DisplayName("下界 H/B")]
		public int LowerH { get; set; } = 0;
		/// <summary>色彩範圍下界 - 第 2 通道 (S/G)</summary>
		[DisplayName("下界 S/G")]
		public int LowerS { get; set; } = 0;
		/// <summary>色彩範圍下界 - 第 3 通道 (V/R)</summary>
		[DisplayName("下界 V/R")]
		public int LowerV { get; set; } = 0;

		/// <summary>色彩範圍上界 - 第 1 通道 (H/B)</summary>
		[DisplayName("上界 H/B")]
		public int UpperH { get; set; } = 180;
		/// <summary>色彩範圍上界 - 第 2 通道 (S/G)</summary>
		[DisplayName("上界 S/G")]
		public int UpperS { get; set; } = 255;
		/// <summary>色彩範圍上界 - 第 3 通道 (V/R)</summary>
		[DisplayName("上界 V/R")]
		public int UpperV { get; set; } = 255;
	}

	// ===== 03. 特徵提取 (Feature Extraction) =====

	/// <summary>邊緣偵測參數</summary>
	public class EdgeDetectionParameters
	{
		/// <summary>邊緣偵測演算法類型</summary>
		public enum EdgeMethod
		{
			/// <summary>Canny 邊緣偵測 (雙閾值)</summary>
			Canny,
			/// <summary>Sobel 算子 (一階微分)</summary>
			Sobel,
			/// <summary>Laplacian 算子 (二階微分)</summary>
			Laplacian,
			/// <summary>Scharr 算子 (改進型 Sobel)</summary>
			Scharr,
		}

		/// <summary>選擇邊緣偵測方法</summary>
		[DisplayName("邊緣偵測方法")] [Description("- Canny: 最常用，雙閾值\n- Sobel: 一階微分\n- Laplacian: 二階微分\n- Scharr: 改進版 Sobel")]
		public EdgeMethod Method { get; set; } = EdgeMethod.Canny;

		/// <summary>Canny 低閾值 或 Sobel/Laplacian 核心大小</summary>
		[DisplayName("閾值 1 / Ksize")] [Description("Canny: 低閾值 / Sobel/Laplacian: 核心大小")]
		public double Threshold1 { get; set; } = 50;

		/// <summary>Canny 高閾值 (通常建議為低閾值的 2~3 倍)</summary>
		[DisplayName("閾值 2")] [Description("Canny: 高閾值 (建議為閾值1的2-3倍)")]
		public double Threshold2 { get; set; } = 150;
	}

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

	/// <summary>霍夫直線偵測參數</summary>
	public class HoughLinesParameters
	{
		/// <summary>是否使用概率霍夫變換 (HoughLinesP)</summary>
		[DisplayName("使用機率霍夫")] [Description("使用 HoughLinesP (機率霍夫) 而非標準霍夫")]
		public bool UseProbabilistic { get; set; } = true;

		/// <summary>是否自動套用 Canny 邊緣偵測</summary>
		[DisplayName("自動邊緣偵測")] [Description("自動套用 Canny 邊緣偵測 (建議啟用，除非輸入已為邊緣圖)")]
		public bool AutoCanny { get; set; } = true;

		/// <summary>Canny 低閾值</summary>
		[DisplayName("Canny 低閾值")] [Description("Canny 邊緣偵測的低閾值 (僅當自動邊緣偵測啟用時)")]
		public double CannyThreshold1 { get; set; } = 50;

		/// <summary>Canny 高閾值</summary>
		[DisplayName("Canny 高閾值")] [Description("Canny 邊緣偵測的高閾值 (建議為低閾值的 2-3 倍)")]
		public double CannyThreshold2 { get; set; } = 150;

		/// <summary>距離解析度 (像素)</summary>
		[DisplayName("Rho (px)")] [Description("累加器的距離解析度 (像素)")]
		public double Rho { get; set; } = 1;

		/// <summary>角度解析度 (度)</summary>
		[DisplayName("Theta (度)")] [Description("累加器的角度解析度 (度)")]
		public double ThetaDeg { get; set; } = 1;

		/// <summary>累加器閾值，大於此值的候選線段才會被保留</summary>
		[DisplayName("閾值")] [Description("累加器閾值，越高則線條越確定")]
		public int Threshold { get; set; } = 50;

		/// <summary>最短線段長度 (像素，僅適用機率霍夫)</summary>
		[DisplayName("最小線長 (px)")] [Description("機率霍夫的最小線段長度")]
		public double MinLineLength { get; set; } = 50;

		/// <summary>最大線段間隙 (像素，僅適用機率霍夫)</summary>
		[DisplayName("最大線間距 (px)")] [Description("機率霍夫的最大線段間距")]
		public double MaxLineGap { get; set; } = 10;

		/// <summary>繪製線條顏色</summary>
		[DisplayName("線條顏色 (BGR)")] [Description("繪製直線的顏色 (格式: B,G,R)")]
		public string LineColorBGR { get; set; } = "255,255,0";

		/// <summary>線條粗細</summary>
		[DisplayName("線條粗細 (px)")] [Description("繪製直線的粗細")]
		public int LineThickness { get; set; } = 2;

		/// <summary>最大輸出直線數量</summary>
		[DisplayName("最大直線數量")] [Description("限制輸出的直線數量。設為 0 表示不限制。")]
		public int MaxLines { get; set; } = 50;
	}


	/// <summary>霍夫圓形偵測參數</summary>
	public class HoughCirclesParameters
	{
		/// <summary>結果排序基準</summary>
		public enum SortType
		{
			/// <summary>依據確信度 (累加器數值)</summary>
			Confidence,
			/// <summary>依據半徑由小到大</summary>
			SmallestRadius,
			/// <summary>依據半徑由大到小</summary>
			LargestRadius,
			/// <summary>依據 X 軸位置由左至右</summary>
			XPosition,
		}

		// ===== 效能優化選項 =====

		/// <summary>啟用影像預縮放 (大幅提升效能)</summary>
		[Category("效能優化")]
		[DisplayName("啟用預縮放")] [Description("在偵測前先縮小影像，可大幅提升效能 (建議對 2000px 以上影像啟用)")]
		public bool EnablePreResize { get; set; } = false;

		/// <summary>預縮放比例 (0.25-1.0)</summary>
		[Category("效能優化")]
		[DisplayName("預縮放比例")] [Description("縮放比例，例如 0.5 表示縮小為原本的一半 (建議 0.25-0.5)")]
		public double PreResizeScale { get; set; } = 0.5;

		// ===== 偵測參數 =====

		/// <summary>累加器解析度與影像比例 (1 為相同)</summary>
		[DisplayName("dp")] [Description("累加器解析度與影像解析度的反比 (1 = 相同解析度，2 = 一半解析度加速運算)")]
		public double Dp { get; set; } = 1.5;

		/// <summary>偵測到圓心間的最小距離</summary>
		[DisplayName("最小圓心距離 (px)")] [Description("偵測到的圓心之間的最小距離 (越大越快)")]
		public double MinDist { get; set; } = 60;

		/// <summary>內部 Canny 高閾值</summary>
		[DisplayName("Canny 高閾值")] [Description("內部 Canny 邊緣偵測的高閾值")]
		public double Param1 { get; set; } = 100;

		/// <summary>累加器閾值，越小越容易偵測到圓形</summary>
		[DisplayName("累加器閾值")] [Description("圓心累加器閾值，越大越快但可能漏檢 (建議 40-80)")]
		public double Param2 { get; set; } = 50;

		/// <summary>最小偵測半徑 (像素)</summary>
		[DisplayName("最小半徑 (px)")] [Description("限制搜尋的半徑下界 (設定越精確越快)")]
		public int MinRadius { get; set; } = 10;

		/// <summary>最大偵測半徑 (像素)</summary>
		[DisplayName("最大半徑 (px)")] [Description("限制搜尋的半徑上界 (設定越精確越快)")]
		public int MaxRadius { get; set; } = 100;

		/// <summary>輸出數量限制，設為 0 表示不限制</summary>
		[DisplayName("最大圓形數量")] [Description("限制輸出的圓形個數，提早結束可加速。設為 0 表示不限制。")]
		public int MaxCircles { get; set; } = 10;

		/// <summary>輸出結果排序依據</summary>
		[DisplayName("排序依據")] [Description("選擇輸出的優先順序")]
		public SortType SortBy { get; set; } = SortType.Confidence;

		// ===== 結果過濾 =====

		/// <summary>後半徑過濾下界</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最小半徑 (px)")] [Description("結果過濾：只輸出半徑 >= 此值的圓形。設為 0 表示不限制。")]
		public int FilterMinRadius { get; set; } = 0;

		/// <summary>後半徑過濾上界</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最大半徑 (px)")] [Description("結果過濾：只輸出半徑 <= 此值的圓形。設為 0 表示不限制。")]
		public int FilterMaxRadius { get; set; } = 0;

		/// <summary>後面積過濾下界 (面積 = π × r²)</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最小面積 (px²)")] [Description("結果過濾：只輸出面積 >= 此值的圓形。設為 0 表示不限制。(面積 = π × 半徑²)")]
		public int FilterMinArea { get; set; } = 0;

		/// <summary>後面積過濾上界 (面積 = π × r²)</summary>
		[Category("結果過濾")]
		[DisplayName("輸出過濾: 最大面積 (px²)")] [Description("結果過濾：只輸出面積 <= 此值的圓形。設為 0 表示不限制。(面積 = π × 半徑²)")]
		public int FilterMaxArea { get; set; } = 0;
	}

	/// <summary>模板匹配參數</summary>
	public class TemplateMatchParameters
	{
		/// <summary>匹配演算法類型</summary>
		public enum MatchMethod
		{
			/// <summary>平方差匹配</summary>
			SqDiff,
			/// <summary>正規化平方差匹配</summary>
			SqDiffNormed,
			/// <summary>相關性匹配</summary>
			CCorr,
			/// <summary>正規化相關性匹配</summary>
			CCorrNormed,
			/// <summary>相關係數匹配</summary>
			CCoeff,
			/// <summary>正規化相關係數匹配 (推薦)</summary>
			CCoeffNormed,
		}

		/// <summary>選擇匹配演算法</summary>
		[DisplayName("匹配方法")] [Description("- SqDiff: 平方差 (越小越好)\n- CCorr: 相關性 (越大越好)\n- CCoeff: 相關係數 (越大越好)\n- *Normed: 正規化版本")]
		public MatchMethod Method { get; set; } = MatchMethod.CCoeffNormed;

		/// <summary>模板影像之磁碟路徑</summary>
		[DisplayName("模板路徑")] [Description("模板影像檔案的完整路徑")]
		public string TemplatePath { get; set; } = "";

		/// <summary>匹配確信度閾值 (適用於 Normed 方法，範圍 0-1)</summary>
		[DisplayName("匹配閾值")] [Description("匹配分數閾值 (0-1 for Normed methods)")]
		public double MatchThreshold { get; set; } = 0.8;
	}

	// ===== 04. 繪圖與標註 (Drawing) =====

	/// <summary>繪製文字參數</summary>
	public class DrawTextParameters
	{
		/// <summary>欲繪製之字串</summary>
		[DisplayName("文字內容")]
		public string Text { get; set; } = "Sample";

		/// <summary>繪製起點 X 座標</summary>
		[DisplayName("X 座標")]
		public int X { get; set; } = 10;

		/// <summary>繪製起點 Y 座標</summary>
		[DisplayName("Y 座標")]
		public int Y { get; set; } = 30;

		/// <summary>字體比例因子</summary>
		[DisplayName("字體大小")]
		public double FontScale { get; set; } = 1.0;

		/// <summary>文字顏色 (格式: B,G,R)</summary>
		[DisplayName("顏色 (BGR)")] [Description("格式: B,G,R (例如 255,0,0 為藍色)")]
		public string ColorBGR { get; set; } = "0,255,0";

		/// <summary>筆觸粗細 (像素)</summary>
		[DisplayName("線條粗細")]
		public int Thickness { get; set; } = 2;
	}

	// ===== 05. 幾何變換 (Geometric Transform) =====

	/// <summary>影像旋轉與翻轉參數</summary>
	public class RotateFlipParameters
	{
		/// <summary>影像翻轉類型</summary>
		public enum FlipType
		{
			/// <summary>不翻轉</summary>
			None,
			/// <summary>水平翻轉</summary>
			Horizontal,
			/// <summary>垂直翻轉</summary>
			Vertical,
			/// <summary>兩者皆翻轉</summary>
			Both,
		}

		/// <summary>影像旋轉類型</summary>
		public enum RotationType
		{
			/// <summary>不旋轉</summary>
			None,
			/// <summary>順時針 90 度</summary>
			Rotate90CW,
			/// <summary>180 度</summary>
			Rotate180,
			/// <summary>逆時針 90 度</summary>
			Rotate90CCW,
		}

		/// <summary>設定旋轉角度</summary>
		[DisplayName("旋轉")] [Description("順時針旋轉角度")]
		public RotationType Rotation { get; set; } = RotationType.None;

		/// <summary>設定翻轉方向</summary>
		[DisplayName("翻轉")] [Description("影像翻轉方向")]
		public FlipType Flip { get; set; } = FlipType.None;
	}

	/// <summary>影像縮放參數</summary>
	public class ResizeParameters
	{
		/// <summary>縮放模式</summary>
		public enum ResizeMode
		{
			/// <summary>依比例縮放</summary>
			ByScale,
			/// <summary>指定目標尺寸</summary>
			BySize,
		}

		/// <summary>選擇縮放模式</summary>
		[DisplayName("縮放模式")]
		public ResizeMode Mode { get; set; } = ResizeMode.ByScale;

		/// <summary>縮放比例 (當模式為 ByScale 時使用)</summary>
		[DisplayName("縮放比例")] [Description("當模式為 ByScale 時使用 (例如 0.5 = 縮小一半)")]
		public double Scale { get; set; } = 1.0;

		/// <summary>目標寬度 (當模式為 BySize 時使用)</summary>
		[DisplayName("目標寬度 (px)")] [Description("當模式為 BySize 時使用")]
		public int TargetWidth { get; set; } = 640;

		/// <summary>目標高度 (當模式為 BySize 時使用)</summary>
		[DisplayName("目標高度 (px)")] [Description("當模式為 BySize 時使用")]
		public int TargetHeight { get; set; } = 480;

		/// <summary>影像插值方法</summary>
		[DisplayName("插值方法")] [Description("影像插值方法")]
		public InterpolationFlags Interpolation { get; set; } = InterpolationFlags.Linear;
	}

	/// <summary>影像裁切參數</summary>
	public class CropParameters
	{
		/// <summary>裁切區域左上角 X 座標</summary>
		[DisplayName("X 起點")]
		public int X { get; set; } = 0;

		/// <summary>裁切區域左上角 Y 座標</summary>
		[DisplayName("Y 起點")]
		public int Y { get; set; } = 0;

		/// <summary>裁切區域寬度</summary>
		[DisplayName("寬度")]
		public int Width { get; set; } = 100;

		/// <summary>裁切區域高度</summary>
		[DisplayName("高度")]
		public int Height { get; set; } = 100;
	}

	// ===== 06. 降噪 (Denoising) =====

	/// <summary>去雜訊參數 (Non-local Means Denoising)</summary>
	public class DenoiseParameters
	{
		/// <summary>去雜訊強度 (h 值)，越大效果越強但細節損失越多</summary>
		[DisplayName("濾波強度")] [Description("去雜訊強度 (h 值)，越大去雜訊效果越強 but 細節損失越多")]
		public float FilterStrength { get; set; } = 10;

		/// <summary>模板區塊大小，必須為奇數</summary>
		[DisplayName("模板視窗大小")] [Description("模板區塊大小，必須為奇數")]
		public int TemplateWindowSize { get; set; } = 7;

		/// <summary>搜尋區域大小，必須為奇數</summary>
		[DisplayName("搜尋視窗大小")] [Description("搜尋區域大小，必須為奇數")]
		public int SearchWindowSize { get; set; } = 21;
	}

	// ===== 07. 特徵點 (Keypoints) =====

	/// <summary>特徵點偵測參數</summary>
	public class FeatureDetectParameters
	{
		/// <summary>特徵點偵測器類型</summary>
		public enum DetectorType
		{
			/// <summary>ORB (快速且免費)</summary>
			ORB,
			/// <summary>SIFT (高度精確)</summary>
			SIFT,
			/// <summary>FAST (極速偵測)</summary>
			FAST,
			/// <summary>BRISK (二進制描述子)</summary>
			BRISK,
			/// <summary>AKAZE (非線性尺度空間)</summary>
			AKAZE,
		}

		/// <summary>選擇偵測器算法</summary>
		[DisplayName("偵測器類型")] [Description("- ORB: 快速、免費\n- SIFT: 精確、需 contrib\n- FAST: 極快速\n- BRISK: 二進制描述子\n- AKAZE: 非線性尺度空間")]
		public DetectorType Detector { get; set; } = DetectorType.ORB;

		/// <summary>保留的最大特徵點數量</summary>
		[DisplayName("最大特徵點數")] [Description("保留的最大特徵點數量")]
		public int MaxFeatures { get; set; } = 500;

		/// <summary>過濾小於此大小的特徵點</summary>
		[DisplayName("最小特徵大小")] [Description("過濾小於此大小的特徵點。設為 0 表示不限制。")]
		public float MinSize { get; set; } = 0;

		/// <summary>過濾大於此大小的特徵點</summary>
		[DisplayName("最大特徵大小")] [Description("過濾大於此大小的特徵點。設為 0 表示不限制。")]
		public float MaxSize { get; set; } = 0;

		/// <summary>是否在輸出影像上標註特徵點</summary>
		[DisplayName("繪製特徵點")] [Description("是否在輸出影像上繪製特徵點")]
		public bool DrawKeypoints { get; set; } = true;
	}

	// ===== 08. 相機校正 (Camera Calibration) =====

	/// <summary>相機校正參數</summary>
	public class CameraCalibrationParameters
	{
		/// <summary>棋盤格內角點的水平數量</summary>
		[DisplayName("棋盤格寬度 (格數)")] [Description("棋盤格內角點的水平數量 (通常為 9)")]
		public int PatternWidth { get; set; } = 9;

		/// <summary>棋盤格內角點的垂直數量</summary>
		[DisplayName("棋盤格高度 (格數)")] [Description("棋盤格內角點的垂直數量 (通常為 6)")]
		public int PatternHeight { get; set; } = 6;

		/// <summary>棋盤格每個方格的實際物理邊長 (毫米)</summary>
		[DisplayName("方格大小 (mm)")] [Description("每個方格的實際邊長 (毫米)")]
		public float SquareSize { get; set; } = 25.0f;

		/// <summary>存放校正用標定影像的資料夾路徑</summary>
		[DisplayName("校正影像資料夾")] [Description("包含多張棋盤格影像的資料夾路徑")]
		public string CalibrationImagesFolder { get; set; } = "";

		/// <summary>相機內參矩陣儲存路徑 (.xml)</summary>
		[DisplayName("輸出相機矩陣路徑")] [Description("儲存相機內參矩陣的檔案路徑 (.xml)")]
		public string OutputCameraMatrixPath { get; set; } = "camera_matrix.xml";

		/// <summary>相機畸變係數儲存路徑 (.xml)</summary>
		[DisplayName("輸出畸變係數路徑")] [Description("儲存畸變係數的檔案路徑 (.xml)")]
		public string OutputDistCoeffsPath { get; set; } = "dist_coeffs.xml";

		/// <summary>是否在 Log 中輸出重投影誤差</summary>
		[DisplayName("顯示重投影誤差")] [Description("是否在 Log 中顯示校正的重投影誤差值")]
		public bool ShowReprojectionError { get; set; } = true;
	}

	/// <summary>影像畸變矯正參數</summary>
	public class UndistortParameters
	{
		/// <summary>載入相機內參矩陣的檔案路徑 (.xml)</summary>
		[DisplayName("相機矩陣檔案路徑")] [Description("載入相機內參矩陣的檔案路徑 (.xml)")]
		public string CameraMatrixPath { get; set; } = "camera_matrix.xml";

		/// <summary>載入畸變係數的檔案路徑 (.xml)</summary>
		[DisplayName("畸變係數檔案路徑")] [Description("載入畸變係數的檔案路徑 (.xml)")]
		public string DistCoeffsPath { get; set; } = "dist_coeffs.xml";

		/// <summary>是否自動裁剪掉矯正後產生的黑邊</summary>
		[DisplayName("自動裁剪黑邊")] [Description("矯正後自動裁剪邊緣的黑色區域")]
		public bool AutoCropBlackBorder { get; set; } = true;
	}

	/// <summary>透視變換參數 (四點矯正)</summary>
	public class PerspectiveTransformParameters
	{
		/// <summary>插值方法類型</summary>
		public enum InterpolationType
		{
			/// <summary>最近鄰插值</summary>
			Nearest,
			/// <summary>線性插值</summary>
			Linear,
			/// <summary>三次插值</summary>
			Cubic,
			/// <summary>Lanczos 插值</summary>
			Lanczos4,
		}

		/// <summary>源影像左上角 X 座標</summary>
		[DisplayName("左上角 X")] [Description("源影像的左上角點 X 座標")]
		public float SrcTopLeftX { get; set; } = 0;

		/// <summary>源影像左上角 Y 座標</summary>
		[DisplayName("左上角 Y")] [Description("源影像的左上角點 Y 座標")]
		public float SrcTopLeftY { get; set; } = 0;

		/// <summary>源影像右上角 X 座標</summary>
		[DisplayName("右上角 X")] [Description("源影像的右上角點 X 座標")]
		public float SrcTopRightX { get; set; } = 100;

		/// <summary>源影像右上角 Y 座標</summary>
		[DisplayName("右上角 Y")] [Description("源影像的右上角點 Y 座標")]
		public float SrcTopRightY { get; set; } = 0;

		/// <summary>源影像右下角 X 座標</summary>
		[DisplayName("右下角 X")] [Description("源影像的右下角點 X 座標")]
		public float SrcBottomRightX { get; set; } = 100;

		/// <summary>源影像右下角 Y 座標</summary>
		[DisplayName("右下角 Y")] [Description("源影像的右下角點 Y 座標")]
		public float SrcBottomRightY { get; set; } = 100;

		/// <summary>源影像左下角 X 座標</summary>
		[DisplayName("左下角 X")] [Description("源影像的左下角點 X 座標")]
		public float SrcBottomLeftX { get; set; } = 0;

		/// <summary>源影像左下角 Y 座標</summary>
		[DisplayName("左下角 Y")] [Description("源影像的左下角點 Y 座標")]
		public float SrcBottomLeftY { get; set; } = 100;

		/// <summary>輸出影像寬度 (px)</summary>
		[DisplayName("輸出寬度 (px)")] [Description("變換後輸出影像的寬度")]
		public int OutputWidth { get; set; } = 640;

		/// <summary>輸出影像高度 (px)</summary>
		[DisplayName("輸出高度 (px)")] [Description("變換後輸出影像的高度")]
		public int OutputHeight { get; set; } = 480;

		/// <summary>影像變換的插值方法</summary>
		[DisplayName("插值方法")] [Description("影像變換的插值演算法")]
		public InterpolationType Interpolation { get; set; } = InterpolationType.Linear;
	}

	// ===== 09. 測量與分析 (Measurement & Analysis) =====

	/// <summary>幾何測量參數</summary>
	public class MeasurementToolParameters
	{
		/// <summary>測量類型</summary>
		public enum MeasureType
		{
			/// <summary>點對點距離</summary>
			Distance,
			/// <summary>夾角測量</summary>
			Angle,
			/// <summary>封閉區域面積</summary>
			Area,
			/// <summary>周長</summary>
			Perimeter,
			/// <summary>圓形直徑</summary>
			Diameter,
		}

		/// <summary>每像素對應的實際長度 (毫米/像素)</summary>
		[DisplayName("像素比例 (mm/px)")] [Description("每像素對應的實際長度 (毫米/像素)")]
		public double PixelScale { get; set; } = 0.1;

		/// <summary>測量目標類型</summary>
		[DisplayName("測量類型")] [Description("選擇測量的幾何量類型")]
		public MeasureType Type { get; set; } = MeasureType.Distance;

		/// <summary>結果顯示單位 (mm, um, cm...)</summary>
		[DisplayName("顯示單位")] [Description("測量結果顯示的單位 (mm, μm, cm)")]
		public string DisplayUnit { get; set; } = "mm";

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")] [Description("測量結果顯示的小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>測量起點 X 座標</summary>
		[DisplayName("起點 X")] [Description("測量起點的 X 座標")]
		public int StartX { get; set; } = 0;

		/// <summary>測量起點 Y 座標</summary>
		[DisplayName("起點 Y")] [Description("測量起點的 Y 座標")]
		public int StartY { get; set; } = 0;

		/// <summary>測量終點 X 座標</summary>
		[DisplayName("終點 X")] [Description("測量終點的 X 座標")]
		public int EndX { get; set; } = 100;

		/// <summary>測量終點 Y 座標</summary>
		[DisplayName("終點 Y")] [Description("測量終點的 Y 座標")]
		public int EndY { get; set; } = 100;

		/// <summary>是否在輸出影像上標註測量結果</summary>
		[DisplayName("顯示結果於影像")] [Description("是否將測量結果標註在輸出影像上")]
		public bool DrawOnImage { get; set; } = true;

		/// <summary>是否使用物件編號做為測量參考點</summary>
		[Category("物件參考")]
		[DisplayName("使用物件參考")] [Description("啟用後將使用指定編號的物件中心點進行測量，而非手動座標")]
		public bool UseObjectReference { get; set; } = false;

		/// <summary>起點參考的物件編號 (例如: "1")</summary>
		[Category("物件參考")]
		[DisplayName("起點物件編號")] [Description("測量起點參考的物件編號 (需搭配前置的物件分析步驟)")]
		public string StartObjectId { get; set; } = "1";

		/// <summary>終點參考的物件編號 (例如: "2")</summary>
		[Category("物件參考")]
		[DisplayName("終點物件編號")] [Description("測量終點參考的物件編號")]
		public string EndObjectId { get; set; } = "2";

		/// <summary>上下文缺陷列表 (執行時自動注入，不顯示)</summary>
		[Browsable(false)]
		[XmlIgnore]
		public List<Defect> ContextDefects { get; set; }
	}

	/// <summary>物件特徵分析參數</summary>
	public class ObjectAnalysisParameters
	{
		/// <summary>是否計算物件水平外接矩形</summary>
		[DisplayName("計算外接矩形")] [Description("計算並繪製物件的水平外接矩形")]
		public bool ComputeBoundingRect { get; set; } = true;

		/// <summary>是否計算物件最小旋轉外接矩形</summary>
		[DisplayName("計算最小外接矩形")] [Description("計算並繪製物件的最小旋轉外接矩形")]
		public bool ComputeMinAreaRect { get; set; } = true;

		/// <summary>是否計算物件最小外接圓</summary>
		[DisplayName("計算外接圓")] [Description("計算並繪製物件的最小外接圓")]
		public bool ComputeMinEnclosingCircle { get; set; } = false;

		/// <summary>是否計算物件凸包廓</summary>
		[DisplayName("計算凸包")] [Description("計算並繪製物件的凸包輪廓")]
		public bool ComputeConvexHull { get; set; } = false;

		/// <summary>是否計算圓度、矩形度等細節形狀特徵</summary>
		[DisplayName("計算形狀特徵")] [Description("計算面積、周長、圓度、長寬比、矩形度等形狀特徵")]
		public bool ComputeShapeFeatures { get; set; } = true;

		/// <summary>是否在影像上標註物件序號 (Index)</summary>
		[DisplayName("標註物件編號")] [Description("在每個物件上標註序號")]
		public bool LabelObjectIndex { get; set; } = true;

		/// <summary>是否將分析結果匯出為 DataTable</summary>
		[DisplayName("輸出結果到資料表")] [Description("將分析結果輸出為 DataTable 格式")]
		public bool OutputToDataTable { get; set; } = true;

		/// <summary>面積過濾下界 (px2)</summary>
		[DisplayName("最小面積過濾 (px²)")] [Description("過濾面積小於此值的物件")]
		public double FilterMinArea { get; set; } = 100;

		/// <summary>面積過濾上界 (px2)，設為 0 表示不限制</summary>
		[DisplayName("最大面積過濾 (px²)")] [Description("過濾面積大於此值的物件。設為 0 表示不限制。")]
		public double FilterMaxArea { get; set; } = 0;
	}

	/// <summary>影像直方圖分析參數</summary>
	public class HistogramAnalysisParameters
	{
		/// <summary>直方圖取樣類型</summary>
		public enum HistogramType
		{
			/// <summary>單通道灰階</summary>
			Grayscale,
			/// <summary>三通道 RGB 彩色</summary>
			RGB,
			/// <summary>HSV 色彩空間</summary>
			HSV,
		}

		/// <summary>是否開啟獨立視窗顯示直方圖</summary>
		[DisplayName("顯示直方圖視窗")] [Description("以獨立視窗顯示直方圖")]
		public bool ShowHistogramWindow { get; set; } = true;

		/// <summary>是否計算並顯示均值、標準差等統計量</summary>
		[DisplayName("計算統計值")] [Description("計算並顯示均值、標準差、最大最小值")]
		public bool ComputeStatistics { get; set; } = true;

		/// <summary>選擇分析的直方圖色彩空間</summary>
		[DisplayName("直方圖類型")] [Description("選擇直方圖的色彩通道類型")]
		public HistogramType Type { get; set; } = HistogramType.Grayscale;

		/// <summary>是否將直方圖小圖繪製於輸出影像上</summary>
		[DisplayName("繪製於影像上")] [Description("將直方圖繪製在輸出影像的右下角")]
		public bool DrawOnImage { get; set; } = false;
	}

	/// <summary>灰階剖面線分析參數</summary>
	public class ProfileLineParameters
	{
		/// <summary>剖面線起點 X 座標</summary>
		[DisplayName("起點 X")] [Description("剖面線起點的 X 座標")]
		public int StartX { get; set; } = 0;

		/// <summary>剖面線起點 Y 座標</summary>
		[DisplayName("起點 Y")] [Description("剖面線起點的 Y 座標")]
		public int StartY { get; set; } = 0;

		/// <summary>剖面線終點 X 座標</summary>
		[DisplayName("終點 X")] [Description("剖面線終點的 X 座標")]
		public int EndX { get; set; } = 100;

		/// <summary>剖面線終點 Y 座標</summary>
		[DisplayName("終點 Y")] [Description("剖面線終點的 Y 座標")]
		public int EndY { get; set; } = 0;

		/// <summary>是否開啟獨立視窗顯示剖面曲線圖 (Intensity Profile)</summary>
		[DisplayName("顯示灰階剖面圖")] [Description("以獨立視窗顯示灰階剖面曲線圖")]
		public bool ShowProfileWindow { get; set; } = true;

		/// <summary>剖面取樣的線條寬度 (像素平均)</summary>
		[DisplayName("剖面線寬度 (px)")] [Description("取樣時平均的線條寬度")]
		public int LineWidth { get; set; } = 1;

		/// <summary>是否將剖面數據匯出為 CSV 檔案</summary>
		[DisplayName("輸出數據到 CSV")] [Description("將剖面數據輸出為 CSV 檔案")]
		public bool OutputToCsv { get; set; } = false;

		/// <summary>CSV 檔案儲存路徑</summary>
		[DisplayName("CSV 輸出路徑")] [Description("CSV 檔案的儲存路徑")]
		public string CsvOutputPath { get; set; } = "profile_data.csv";
	}

	// ===== 10. 影像品質評估 (Quality Assessment) =====

	/// <summary>影像品質評估參數</summary>
	public class QualityAssessmentParameters
	{
		/// <summary>品質評估指標</summary>
		public enum QualityMetric
		{
			/// <summary>峰值信噪比</summary>
			PSNR,
			/// <summary>結構相似性</summary>
			SSIM,
			/// <summary>均方誤差</summary>
			MSE,
		}

		/// <summary>選擇品質計算方法</summary>
		[DisplayName("品質指標")] [Description("選擇影像品質評估的計算方式:\\n- PSNR: 峰值信噪比\\n- SSIM: 結構相似性\\n- MSE: 均方誤差")]
		public QualityMetric Metric { get; set; } = QualityMetric.PSNR;

		/// <summary>作為比對基礎的參考影像 (標準影像) 路徑</summary>
		[DisplayName("參考影像路徑")] [Description("作為品質比較基準的參考影像檔案路徑")]
		public string ReferenceImagePath { get; set; } = "";

		/// <summary>是否在輸出影像上顯示計算的分數</summary>
		[DisplayName("顯示品質分數")] [Description("在輸出影像上顯示計算的品質分數")]
		public bool ShowScore { get; set; } = true;

		/// <summary>是否將詳細品質評估結果輸出至 Log</summary>
		[DisplayName("輸出品質報告")] [Description("將品質評估結果輸出到 Log")]
		public bool OutputReport { get; set; } = true;

		/// <summary>是否視覺化 SSIM 差異熱圖</summary>
		[DisplayName("SSIM 視覺化")] [Description("顯示 SSIM 差異熱圖 (僅適用於 SSIM 指標)")]
		public bool VisualizeSSIM { get; set; } = false;
	}

	// ===== 11. 顏色分析 (Color Analysis) =====

	/// <summary>色彩分析參數</summary>
	public class ColorAnalysisParameters
	{
		/// <summary>是否使用 K-Means 演算法計算主要顏色成分</summary>
		[DisplayName("計算主要顏色")] [Description("使用 K-Means 聚類提取主要顏色")]
		public bool ComputeDominantColors { get; set; } = true;

		/// <summary>提取主要顏色的數量 (K 值)</summary>
		[DisplayName("顏色數量")] [Description("K-Means 聚類的顏色數量 (K 值)")]
		public int ColorCount { get; set; } = 5;

		/// <summary>是否顯示顏色分布色塊圖</summary>
		[DisplayName("顯示顏色分布圖")] [Description("以色塊方式顯示提取的主要顏色")]
		public bool ShowColorDistribution { get; set; } = true;

		/// <summary>是否計算與標準色間的 ΔE 色差</summary>
		[DisplayName("計算色差")] [Description("與標準色計算 ΔE 色差值")]
		public bool ComputeColorDifference { get; set; } = false;

		/// <summary>標準參考色的 Lab 數值 (格式: L,a,b)</summary>
		[DisplayName("標準色 Lab 值")] [Description("參考標準色的 Lab 值 (格式: L,a,b)")]
		public string StandardLabColor { get; set; } = "50,0,0";

		/// <summary>判斷色差合格的 ΔE 閾值</summary>
		[DisplayName("色差容許範圍 ΔE")] [Description("允許的最大色差值")]
		public double DeltaEThreshold { get; set; } = 5.0;
	}

	// ===== 12. 背景處理 (Background Processing) =====

	/// <summary>背景相減參數</summary>
	public class BackgroundSubtractionParameters
	{
		/// <summary>背景建模演算法類型</summary>
		public enum SubtractionMethod
		{
			/// <summary>高斯混合模型 MOG2</summary>
			MOG2,
			/// <summary>K-近鄰演算法 KNN</summary>
			KNN,
		}

		/// <summary>選擇背景分割演算法</summary>
		[DisplayName("方法")] [Description("背景分割演算法:\\n- MOG2: 高斯混合模型\\n- KNN: K近鄰演算法")]
		public SubtractionMethod Method { get; set; } = SubtractionMethod.MOG2;

		/// <summary>背景模型學習率 (0~1)，-1 為自動</summary>
		[DisplayName("學習率")] [Description("背景模型的更新速度 (0-1)，設為 -1 表示自動")]
		public double LearningRate { get; set; } = 0.01;

		/// <summary>用於建立背景模型的歷史影像幀數</summary>
		[DisplayName("歷史幀數")] [Description("用於建立背景模型的歷史幀數")]
		public int History { get; set; } = 500;

		/// <summary>像素方差閾值 (用於 MOG2)</summary>
		[DisplayName("方差閾值")] [Description("MOG2 的像素方差閾值")]
		public double VarThreshold { get; set; } = 16;

		/// <summary>是否偵測並標註影像中的陰影區域</summary>
		[DisplayName("偵測陰影")] [Description("是否偵測並標記陰影區域")]
		public bool DetectShadows { get; set; } = false;

		/// <summary>是否對前景遮罩進行形態學去噪後處理</summary>
		[DisplayName("形態學後處理")] [Description("對前景遮罩進行形態學開運算去除雜訊")]
		public bool MorphologicalPostProcess { get; set; } = true;

		/// <summary>形態學後處理的核心大小 (像素)</summary>
		[DisplayName("核心大小 (px)")] [Description("形態學後處理的核心大小")]
		public int MorphKernelSize { get; set; } = 3;
	}

	// ===== 13. 缺陷檢測 (Defect Detection) =====

	/// <summary>通用缺陷檢測參數</summary>
	public class DefectDetectionParameters
	{
		/// <summary>缺陷分類類型</summary>
		public enum DefectType
		{
			/// <summary>不限類型</summary>
			Any,
			/// <summary>刮傷</summary>
			Scratch,
			/// <summary>污點</summary>
			Stain,
			/// <summary>凹陷</summary>
			Dent,
			/// <summary>缺件 (漏打)</summary>
			Missing,
			/// <summary>多件 (多餘物件)</summary>
			Extra,
		}

		/// <summary>缺陷檢測運算模式</summary>
		public enum DetectionMode
		{
			/// <summary>模板差異比對 (最常用)</summary>
			TemplateDiff,
			/// <summary>基於邊緣的異常檢測</summary>
			EdgeBased,
			/// <summary>基於色彩的異常檢測</summary>
			ColorBased,
		}

		/// <summary>選擇缺陷檢測模式</summary>
		[DisplayName("檢測模式")] [Description("缺陷檢測的演算法模式:\\n- TemplateDiff: 與參考影像差異比對\\n- EdgeBased: 邊緣異常檢測\\n- ColorBased: 色彩異常檢測")]
		public DetectionMode Mode { get; set; } = DetectionMode.TemplateDiff;

		/// <summary>存放無缺陷標準樣本影像的路徑</summary>
		[DisplayName("參考樣本路徑")] [Description("無缺陷的標準參考影像路徑")]
		public string ReferenceSamplePath { get; set; } = "";

		/// <summary>最小缺陷判定面積 (px2)</summary>
		[DisplayName("最小缺陷面積 (px²)")] [Description("過濾小於此面積的缺陷")]
		public double MinDefectArea { get; set; } = 50;

		/// <summary>最大缺陷判定面積 (px2)，設為 0 表示不限制</summary>
		[DisplayName("最大缺陷面積 (px²)")] [Description("過濾大於此面積的缺陷。設為 0 表示不限制。")]
		public double MaxDefectArea { get; set; } = 0;

		/// <summary>影像差異比對的靈敏度閾值 (0~255)</summary>
		[DisplayName("差異閾值 (0-255)")] [Description("差異影像的二值化閾值")]
		public double DifferenceThreshold { get; set; } = 30;

		/// <summary>是否在輸出影像上高亮框選缺陷</summary>
		[DisplayName("高亮顯示缺陷")] [Description("在輸出影像上以紅色框標示缺陷位置")]
		public bool HighlightDefects { get; set; } = true;

		/// <summary>欲檢測的缺陷類型過濾</summary>
		[DisplayName("缺陷類型標註")] [Description("指定要檢測的缺陷類型")]
		public DefectType TypeFilter { get; set; } = DefectType.Any;

		/// <summary>是否匯出缺陷統計報告至 Log</summary>
		[DisplayName("輸出缺陷報告")] [Description("將缺陷統計資訊輸出到 Log")]
		public bool OutputDefectReport { get; set; } = true;

		/// <summary>是否將缺陷清單匯出為 CSV 檔案</summary>
		[DisplayName("缺陷座標輸出到 CSV")] [Description("將所有缺陷的座標與資訊輸出為 CSV")]
		public bool OutputToCsv { get; set; } = false;

		/// <summary>缺陷報告 CSV 的儲存路徑</summary>
		[DisplayName("CSV 輸出路徑")] [Description("CSV 檔案的儲存路徑")]
		public string CsvOutputPath { get; set; } = "defect_report.csv";
	}

	// ===== 14. 幾何量測工具 (Geometry Measurement - AISYS OVK 對應) =====

	/// <summary>角度量測參數 (對應 AxAngleMsr)</summary>
	public class AngleMeasurementParameters
	{
		/// <summary>量測模式</summary>
		public enum AngleMode
		{
			/// <summary>三點夾角 (頂點 + 兩端點)</summary>
			ThreePoints,
			/// <summary>兩線夾角</summary>
			TwoLines,
		}

		/// <summary>選擇角度量測模式</summary>
		[DisplayName("量測模式")] [Description("三點模式使用頂點+兩端點計算夾角；兩線模式使用直線方程式計算")]
		public AngleMode Mode { get; set; } = AngleMode.ThreePoints;

		// 三點模式參數
		/// <summary>頂點 X 座標</summary>
		[Category("三點模式")] [DisplayName("頂點 X")]
		public int VertexX { get; set; } = 100;
		/// <summary>頂點 Y 座標</summary>
		[Category("三點模式")] [DisplayName("頂點 Y")]
		public int VertexY { get; set; } = 100;
		/// <summary>第一端點 X 座標</summary>
		[Category("三點模式")] [DisplayName("端點1 X")]
		public int Point1X { get; set; } = 50;
		/// <summary>第一端點 Y 座標</summary>
		[Category("三點模式")] [DisplayName("端點1 Y")]
		public int Point1Y { get; set; } = 50;
		/// <summary>第二端點 X 座標</summary>
		[Category("三點模式")] [DisplayName("端點2 X")]
		public int Point2X { get; set; } = 150;
		/// <summary>第二端點 Y 座標</summary>
		[Category("三點模式")] [DisplayName("端點2 Y")]
		public int Point2Y { get; set; } = 50;

		// 兩線模式參數 (線1: 點A到點B, 線2: 點C到點D)
		/// <summary>線1 起點 X</summary>
		[Category("兩線模式")] [DisplayName("線1 起點 X")]
		public int Line1StartX { get; set; } = 0;
		/// <summary>線1 起點 Y</summary>
		[Category("兩線模式")] [DisplayName("線1 起點 Y")]
		public int Line1StartY { get; set; } = 0;
		/// <summary>線1 終點 X</summary>
		[Category("兩線模式")] [DisplayName("線1 終點 X")]
		public int Line1EndX { get; set; } = 100;
		/// <summary>線1 終點 Y</summary>
		[Category("兩線模式")] [DisplayName("線1 終點 Y")]
		public int Line1EndY { get; set; } = 0;
		/// <summary>線2 起點 X</summary>
		[Category("兩線模式")] [DisplayName("線2 起點 X")]
		public int Line2StartX { get; set; } = 0;
		/// <summary>線2 起點 Y</summary>
		[Category("兩線模式")] [DisplayName("線2 起點 Y")]
		public int Line2StartY { get; set; } = 0;
		/// <summary>線2 終點 X</summary>
		[Category("兩線模式")] [DisplayName("線2 終點 X")]
		public int Line2EndX { get; set; } = 0;
		/// <summary>線2 終點 Y</summary>
		[Category("兩線模式")] [DisplayName("線2 終點 Y")]
		public int Line2EndY { get; set; } = 100;

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;
	}

	/// <summary>直線交點計算參數 (對應 AxIntersectionMsr)</summary>
	public class LineIntersectionParameters
	{
		/// <summary>線1 起點 X</summary>
		[DisplayName("線1 起點 X")]
		public int Line1StartX { get; set; } = 0;
		/// <summary>線1 起點 Y</summary>
		[DisplayName("線1 起點 Y")]
		public int Line1StartY { get; set; } = 0;
		/// <summary>線1 終點 X</summary>
		[DisplayName("線1 終點 X")]
		public int Line1EndX { get; set; } = 100;
		/// <summary>線1 終點 Y</summary>
		[DisplayName("線1 終點 Y")]
		public int Line1EndY { get; set; } = 100;
		/// <summary>線2 起點 X</summary>
		[DisplayName("線2 起點 X")]
		public int Line2StartX { get; set; } = 100;
		/// <summary>線2 起點 Y</summary>
		[DisplayName("線2 起點 Y")]
		public int Line2StartY { get; set; } = 0;
		/// <summary>線2 終點 X</summary>
		[DisplayName("線2 終點 X")]
		public int Line2EndX { get; set; } = 0;
		/// <summary>線2 終點 Y</summary>
		[DisplayName("線2 終點 Y")]
		public int Line2EndY { get; set; } = 100;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;

		/// <summary>交點標記半徑</summary>
		[DisplayName("標記半徑 (px)")]
		public int MarkerRadius { get; set; } = 5;
	}

	/// <summary>點到線距離計算參數 (對應 AxPointLineDistanceMsr)</summary>
	public class PointLineDistanceParameters
	{
		/// <summary>點座標 X</summary>
		[DisplayName("點 X")]
		public int PointX { get; set; } = 50;
		/// <summary>點座標 Y</summary>
		[DisplayName("點 Y")]
		public int PointY { get; set; } = 50;
		/// <summary>線起點 X</summary>
		[DisplayName("線起點 X")]
		public int LineStartX { get; set; } = 0;
		/// <summary>線起點 Y</summary>
		[DisplayName("線起點 Y")]
		public int LineStartY { get; set; } = 0;
		/// <summary>線終點 X</summary>
		[DisplayName("線終點 X")]
		public int LineEndX { get; set; } = 100;
		/// <summary>線終點 Y</summary>
		[DisplayName("線終點 Y")]
		public int LineEndY { get; set; } = 0;

		/// <summary>每像素對應的實際長度 (毫米/像素)</summary>
		[DisplayName("像素比例 (mm/px)")]
		public double PixelScale { get; set; } = 0.1;

		/// <summary>結果顯示單位</summary>
		[DisplayName("顯示單位")]
		public string DisplayUnit { get; set; } = "mm";

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;
	}

	/// <summary>迴歸直線參數 (對應 AxLineRegression)</summary>
	public class LineRegressionParameters
	{
		/// <summary>點群座標 (格式: x1,y1;x2,y2;...)</summary>
		[DisplayName("點群座標")] [Description("格式: x1,y1;x2,y2;x3,y3;... 例如: 10,20;30,40;50,55")]
		public string PointsData { get; set; } = "10,20;30,40;50,60;70,80;90,100";

		/// <summary>是否使用偵測到的輪廓邊緣點</summary>
		[DisplayName("使用輪廓邊緣點")] [Description("自動從輸入二值影像提取邊緣點，忽略手動輸入的點群座標")]
		public bool UseContourPoints { get; set; } = false;

		/// <summary>擬合線延伸長度 (像素)</summary>
		[DisplayName("線延伸長度 (px)")]
		public int LineExtension { get; set; } = 500;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;

		/// <summary>是否標記原始點</summary>
		[DisplayName("標記原始點")]
		public bool DrawPoints { get; set; } = true;

		/// <summary>點標記半徑</summary>
		[DisplayName("點標記半徑 (px)")]
		public int PointRadius { get; set; } = 3;
	}

	/// <summary>迴歸圓參數 (對應 AxCircleRegression)</summary>
	public class CircleRegressionParameters
	{
		/// <summary>點群座標 (格式: x1,y1;x2,y2;...)</summary>
		[DisplayName("點群座標")] [Description("格式: x1,y1;x2,y2;x3,y3;... 例如: 100,50;150,100;100,150;50,100")]
		public string PointsData { get; set; } = "100,50;150,100;100,150;50,100";

		/// <summary>是否使用偵測到的輪廓邊緣點</summary>
		[DisplayName("使用輪廓邊緣點")] [Description("自動從輸入二值影像提取邊緣點，忽略手動輸入的點群座標")]
		public bool UseContourPoints { get; set; } = false;

		/// <summary>每像素對應的實際長度 (毫米/像素)</summary>
		[DisplayName("像素比例 (mm/px)")]
		public double PixelScale { get; set; } = 0.1;

		/// <summary>結果顯示單位</summary>
		[DisplayName("顯示單位")]
		public string DisplayUnit { get; set; } = "mm";

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;

		/// <summary>是否標記原始點</summary>
		[DisplayName("標記原始點")]
		public bool DrawPoints { get; set; } = true;

		/// <summary>點標記半徑</summary>
		[DisplayName("點標記半徑 (px)")]
		public int PointRadius { get; set; } = 3;
	}

	/// <summary>兩線間距參數 (對應 AxLineLineGapMsr)</summary>
	public class LineLineGapParameters
	{
		/// <summary>線1 起點 X</summary>
		[DisplayName("線1 起點 X")]
		public int Line1StartX { get; set; } = 0;
		/// <summary>線1 起點 Y</summary>
		[DisplayName("線1 起點 Y")]
		public int Line1StartY { get; set; } = 0;
		/// <summary>線1 終點 X</summary>
		[DisplayName("線1 終點 X")]
		public int Line1EndX { get; set; } = 100;
		/// <summary>線1 終點 Y</summary>
		[DisplayName("線1 終點 Y")]
		public int Line1EndY { get; set; } = 0;
		/// <summary>線2 起點 X</summary>
		[DisplayName("線2 起點 X")]
		public int Line2StartX { get; set; } = 0;
		/// <summary>線2 起點 Y</summary>
		[DisplayName("線2 起點 Y")]
		public int Line2StartY { get; set; } = 50;
		/// <summary>線2 終點 X</summary>
		[DisplayName("線2 終點 X")]
		public int Line2EndX { get; set; } = 100;
		/// <summary>線2 終點 Y</summary>
		[DisplayName("線2 終點 Y")]
		public int Line2EndY { get; set; } = 50;

		/// <summary>每像素對應的實際長度 (毫米/像素)</summary>
		[DisplayName("像素比例 (mm/px)")]
		public double PixelScale { get; set; } = 0.1;

		/// <summary>結果顯示單位</summary>
		[DisplayName("顯示單位")]
		public string DisplayUnit { get; set; } = "mm";

		/// <summary>結果顯示的小數位數</summary>
		[DisplayName("小數位數")]
		public int DecimalPlaces { get; set; } = 2;

		/// <summary>是否在影像上繪製結果</summary>
		[DisplayName("繪製結果")]
		public bool DrawOnImage { get; set; } = true;
	}

	// ===== 15. 影像處理擴充工具 (Image Processing - AISYS OVK 對應) =====

	/// <summary>算術邏輯運算參數 (對應 AxImageALops)</summary>
	public class ImageArithmeticParameters
	{
		/// <summary>運算類型</summary>
		public enum OperationType
		{
			/// <summary>加法</summary>
			Add,
			/// <summary>減法</summary>
			Subtract,
			/// <summary>乘法</summary>
			Multiply,
			/// <summary>除法</summary>
			Divide,
			/// <summary>位元 AND</summary>
			BitwiseAnd,
			/// <summary>位元 OR</summary>
			BitwiseOr,
			/// <summary>位元 XOR</summary>
			BitwiseXor,
			/// <summary>位元 NOT</summary>
			BitwiseNot,
			/// <summary>絕對差值</summary>
			AbsDiff,
			/// <summary>最大值</summary>
			Max,
			/// <summary>最小值</summary>
			Min,
		}

		/// <summary>選擇運算類型</summary>
		[DisplayName("運算類型")] [Description("影像算術邏輯運算類型")]
		public OperationType Operation { get; set; } = OperationType.Add;

		/// <summary>第二張影像路徑 (用於雙運算元運算)</summary>
		[DisplayName("第二影像路徑")] [Description("用於雙運算元運算的第二張影像 (BitwiseNot 除外)")]
		public string SecondImagePath { get; set; } = "";

		/// <summary>純量值 (用於影像+純量運算)</summary>
		[DisplayName("純量值")] [Description("用於 Add/Subtract 時的純量值 (0~255)")]
		public int ScalarValue { get; set; } = 50;

		/// <summary>是否使用純量而非第二影像</summary>
		[DisplayName("使用純量")] [Description("使用純量進行運算，而非第二張影像")]
		public bool UseScalar { get; set; } = true;
	}

	/// <summary>對比亮度調整參數 (對應 AxImageGainOffset)</summary>
	public class GainOffsetParameters
	{
		/// <summary>對比度 (Gain/Alpha)，1.0 為原始</summary>
		[DisplayName("對比度 (Gain)")] [Description("對比度調整，1.0 為原始，大於 1 增加對比")]
		public double Gain { get; set; } = 1.0;

		/// <summary>亮度偏移 (Offset/Beta)</summary>
		[DisplayName("亮度 (Offset)")] [Description("亮度偏移，0 為原始，正值調亮，負值調暗")]
		public double Offset { get; set; } = 0;
	}

	/// <summary>LUT 色彩轉換參數 (對應 AxImageLut)</summary>
	public class LutParameters
	{
		/// <summary>預設 LUT 類型</summary>
		public enum LutPreset
		{
			/// <summary>反相</summary>
			Invert,
			/// <summary>Gamma 校正</summary>
			Gamma,
			/// <summary>對數</summary>
			Log,
			/// <summary>自訂</summary>
			Custom,
		}

		/// <summary>選擇 LUT 預設類型</summary>
		[DisplayName("LUT 類型")]
		public LutPreset Preset { get; set; } = LutPreset.Invert;

		/// <summary>Gamma 值 (用於 Gamma 校正)</summary>
		[DisplayName("Gamma 值")] [Description("Gamma 校正值，小於 1 調亮，大於 1 調暗")]
		public double GammaValue { get; set; } = 1.0;

		/// <summary>自訂 LUT 表格 (256 個值，以分號分隔)</summary>
		[DisplayName("自訂 LUT")] [Description("256 個值 (0~255)，以分號分隔")]
		public string CustomLut { get; set; } = "";
	}

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

	/// <summary>對焦評估參數 (對應 AxImageFocusRatio)</summary>
	public class FocusRatioParameters
	{
		/// <summary>評估方法</summary>
		public enum FocusMethod
		{
			/// <summary>Laplacian 方差</summary>
			LaplacianVariance,
			/// <summary>Sobel 梯度</summary>
			SobelGradient,
			/// <summary>Tenengrad (Sobel 平方和)</summary>
			Tenengrad,
		}

		/// <summary>選擇對焦評估方法</summary>
		[DisplayName("評估方法")]
		public FocusMethod Method { get; set; } = FocusMethod.LaplacianVariance;

		/// <summary>是否在影像上顯示對焦分數</summary>
		[DisplayName("顯示分數")]
		public bool ShowScore { get; set; } = true;
	}

	/// <summary>色版分離參數 (對應 AxImageRgbSeparator 等)</summary>
	public class ChannelSeparatorParameters
	{
		/// <summary>色彩空間類型</summary>
		public enum ColorSpaceType
		{
			/// <summary>RGB</summary>
			RGB,
			/// <summary>HSV</summary>
			HSV,
			/// <summary>HSI</summary>
			HSI,
			/// <summary>Lab</summary>
			Lab,
			/// <summary>Luv</summary>
			Luv,
			/// <summary>XYZ</summary>
			XYZ,
			/// <summary>YCrCb</summary>
			YCrCb,
		}

		/// <summary>選擇色彩空間</summary>
		[DisplayName("色彩空間")]
		public ColorSpaceType ColorSpace { get; set; } = ColorSpaceType.RGB;

		/// <summary>選擇輸出通道 (0/1/2)</summary>
		[DisplayName("輸出通道")] [Description("選擇輸出的通道索引 (0=第一通道)")]
		public int ChannelIndex { get; set; } = 0;
	}

	/// <summary>RGB 色版合成參數 (對應 AxImageRgbComposer)</summary>
	public class RgbComposerParameters
	{
		/// <summary>紅色通道影像路徑</summary>
		[DisplayName("R 通道影像")]
		public string RedChannelPath { get; set; } = "";

		/// <summary>綠色通道影像路徑</summary>
		[DisplayName("G 通道影像")]
		public string GreenChannelPath { get; set; } = "";

		/// <summary>藍色通道影像路徑</summary>
		[DisplayName("B 通道影像")]
		public string BlueChannelPath { get; set; } = "";
	}
}