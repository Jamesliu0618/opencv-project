using System.ComponentModel;
using OpenCvSharp;

namespace PCBInspection.Core.Tools
{
    // ===== 01. 預處理 (Preprocessing) =====

    public class GrayscaleParameters
    {
        // No parameters needed
    }

    public class BlurParameters
    {
        public enum BlurType { Gaussian, Median, Box, Bilateral }

        [DisplayName("模糊類型")]
        [Description("選擇模糊演算法:\n- Gaussian: 高斯模糊 (常用)\n- Median: 中值濾波 (去椒鹽雜訊)\n- Box: 均值模糊\n- Bilateral: 雙邊濾波 (保留邊緣)")]
        public BlurType Type { get; set; } = BlurType.Gaussian;

        [DisplayName("核心大小 (px)")]
        [Description("濾波核大小，必須為奇數 (e.g. 3, 5, 7, 9)")]
        public int KernelSize { get; set; } = 5;

        [DisplayName("Sigma")]
        [Description("高斯/雙邊濾波的標準差。設為 0 表示自動計算。")]
        public double Sigma { get; set; } = 0;
    }

    public class ThresholdParameters
    {
        public enum ThreshMethod { Binary, BinaryInv, Otsu, Adaptive, ToZero }

        [DisplayName("閾值方法")]
        [Description("選擇二值化方法:\n- Binary: 標準二值化\n- BinaryInv: 反向二值化\n- Otsu: 自動計算最佳閾值\n- Adaptive: 自適應閾值\n- ToZero: 低於閾值設為 0")]
        public ThreshMethod Method { get; set; } = ThreshMethod.Binary;

        [DisplayName("閾值 (0-255)")]
        [Description("手動閾值。若使用 Otsu/Adaptive 則此值會被忽略。")]
        public double Threshold { get; set; } = 128;

        [DisplayName("最大值 (0-255)")]
        [Description("二值化後的最大值 (通常為 255)")]
        public double MaxVal { get; set; } = 255;

        [DisplayName("自適應區塊大小")]
        [Description("僅用於 Adaptive 方法，必須為奇數")]
        public int AdaptiveBlockSize { get; set; } = 11;

        [DisplayName("自適應常數 C")]
        [Description("僅用於 Adaptive 方法，從均值中減去的常數")]
        public double AdaptiveC { get; set; } = 2;
    }

    public class MorphologyParameters
    {
        public enum MorphOp { Erode, Dilate, Open, Close, Gradient, TopHat, BlackHat }

        [DisplayName("運算類型")]
        [Description("形態學運算:\n- Erode: 腐蝕\n- Dilate: 膨脹\n- Open: 開運算 (先腐後膨)\n- Close: 閉運算 (先膨後腐)\n- Gradient: 形態學梯度\n- TopHat: 頂帽\n- BlackHat: 黑帽")]
        public MorphOp Operation { get; set; } = MorphOp.Open;

        [DisplayName("核心形狀")]
        [Description("結構元素形狀: Rect (矩形), Cross (十字), Ellipse (橢圓)")]
        public MorphShapes Shape { get; set; } = MorphShapes.Rect;

        [DisplayName("核心大小 (px)")]
        [Description("結構元素大小")]
        public int KernelSize { get; set; } = 3;

        [DisplayName("迭代次數")]
        [Description("運算重複執行的次數")]
        public int Iterations { get; set; } = 1;
    }

    // ===== 02. 色彩處理 (Color Processing) =====

    public class ColorConvertParameters
    {
        public enum ConversionType { BGR2Gray, BGR2HSV, BGR2Lab, HSV2BGR, Gray2BGR }

        [DisplayName("轉換類型")]
        [Description("色彩空間轉換:\n- BGR2Gray: 彩色轉灰階\n- BGR2HSV: 轉 HSV (色相/飽和度/明度)\n- BGR2Lab: 轉 Lab 色彩空間\n- HSV2BGR: HSV 轉回 BGR\n- Gray2BGR: 灰階轉 BGR (3通道)")]
        public ConversionType Type { get; set; } = ConversionType.BGR2Gray;
    }

    public class HistogramEqualizeParameters
    {
        [DisplayName("使用 CLAHE")]
        [Description("使用對比度受限自適應直方圖均衡化 (CLAHE)，適用於局部對比度增強")]
        public bool UseCLAHE { get; set; } = false;

        [DisplayName("CLAHE Clip Limit")]
        [Description("對比度限制閾值 (僅用於 CLAHE)")]
        public double ClipLimit { get; set; } = 2.0;

        [DisplayName("CLAHE 區塊大小")]
        [Description("區塊大小 (僅用於 CLAHE)")]
        public int TileGridSize { get; set; } = 8;
    }

    public class InRangeParameters
    {
        [DisplayName("下界 H/B")]
        public int LowerH { get; set; } = 0;
        [DisplayName("下界 S/G")]
        public int LowerS { get; set; } = 0;
        [DisplayName("下界 V/R")]
        public int LowerV { get; set; } = 0;

        [DisplayName("上界 H/B")]
        public int UpperH { get; set; } = 180;
        [DisplayName("上界 S/G")]
        public int UpperS { get; set; } = 255;
        [DisplayName("上界 V/R")]
        public int UpperV { get; set; } = 255;
    }

    // ===== 03. 特徵提取 (Feature Extraction) =====

    public class EdgeDetectionParameters
    {
        public enum EdgeMethod { Canny, Sobel, Laplacian, Scharr }

        [DisplayName("邊緣偵測方法")]
        [Description("- Canny: 最常用，雙閾值\n- Sobel: 一階微分\n- Laplacian: 二階微分\n- Scharr: 改進版 Sobel")]
        public EdgeMethod Method { get; set; } = EdgeMethod.Canny;

        [DisplayName("閾值 1 / Ksize")]
        [Description("Canny: 低閾值 / Sobel/Laplacian: 核心大小")]
        public double Threshold1 { get; set; } = 50;

        [DisplayName("閾值 2")]
        [Description("Canny: 高閾值 (建議為閾值1的2-3倍)")]
        public double Threshold2 { get; set; } = 150;
    }

    public class ContourFindParameters
    {
        public enum ContourModeType { External, List, CComp, Tree }
        public enum ContourApproxType { None, Simple, TC89_L1, TC89_KCOS }

        [DisplayName("輪廓模式")]
        [Description("- External: 僅最外層輪廓\n- List: 所有輪廓 (無階層)\n- CComp: 兩層結構\n- Tree: 完整階層")]
        public ContourModeType Mode { get; set; } = ContourModeType.External;

        [DisplayName("近似方法")]
        [Description("- None: 保留所有點\n- Simple: 壓縮水平/垂直/對角線段")]
        public ContourApproxType ApproxMethod { get; set; } = ContourApproxType.Simple;

        [DisplayName("最小面積")]
        [Description("過濾小於此面積的輪廓 (像素數)")]
        public double MinArea { get; set; } = 100;

        [DisplayName("最大面積")]
        [Description("過濾大於此面積的輪廓 (像素數)。設為 0 表示不限制。")]
        public double MaxArea { get; set; } = 0;

        [DisplayName("繪製輪廓")]
        [Description("是否在輸出影像上繪製輪廓")]
        public bool DrawContours { get; set; } = true;
    }

    public class HoughLinesParameters
    {
        [DisplayName("使用機率霍夫")]
        [Description("使用 HoughLinesP (機率霍夫) 而非標準霍夫")]
        public bool UseProbabilistic { get; set; } = true;

        [DisplayName("Rho (px)")]
        [Description("累加器的距離解析度 (像素)")]
        public double Rho { get; set; } = 1;

        [DisplayName("Theta (度)")]
        [Description("累加器的角度解析度 (度)")]
        public double ThetaDeg { get; set; } = 1;

        [DisplayName("閾值")]
        [Description("累加器閾值，越高則線條越確定")]
        public int Threshold { get; set; } = 50;

        [DisplayName("最小線長 (px)")]
        [Description("機率霍夫的最小線段長度")]
        public double MinLineLength { get; set; } = 50;

        [DisplayName("最大線間距 (px)")]
        [Description("機率霍夫的最大線段間距")]
        public double MaxLineGap { get; set; } = 10;
    }

    public class HoughCirclesParameters
    {
        [DisplayName("dp")]
        [Description("累加器解析度與影像解析度的反比 (1 = 相同解析度)")]
        public double Dp { get; set; } = 1;

        [DisplayName("最小圓心距離 (px)")]
        [Description("偵測到的圓心之間的最小距離")]
        public double MinDist { get; set; } = 40;

        [DisplayName("Canny 高閾值")]
        [Description("內部 Canny 邊緣偵測的高閾值")]
        public double Param1 { get; set; } = 100;

        [DisplayName("累加器閾值")]
        [Description("圓心累加器閾值，越小越敏感 (越大找越少圓)")]
        public double Param2 { get; set; } = 30;

        [DisplayName("最小半徑 (px)")]
        public int MinRadius { get; set; } = 0;

        [DisplayName("最大半徑 (px)")]
        public int MaxRadius { get; set; } = 150;

        [DisplayName("最大圓形數量")]
        [Description("限制輸出的圓形個數。設為 0 表示不限制。")]
        public int MaxCircles { get; set; } = 10;

        public enum SortType { Confidence, SmallestRadius, LargestRadius, XPosition }

        [DisplayName("排序依據")]
        [Description("選擇輸出的優先順序")]
        public SortType SortBy { get; set; } = SortType.Confidence;

        [DisplayName("輸出過濾: 最小半徑 (px)")]
        [Description("結果過濾：只輸出半徑 >= 此值的圓形。設為 0 表示不限制。")]
        public int FilterMinRadius { get; set; } = 0;

        [DisplayName("輸出過濾: 最大半徑 (px)")]
        [Description("結果過濾：只輸出半徑 <= 此值的圓形。設為 0 表示不限制。")]
        public int FilterMaxRadius { get; set; } = 0;

        [DisplayName("輸出過濾: 最小面積 (px²)")]
        [Description("結果過濾：只輸出面積 >= 此值的圓形。設為 0 表示不限制。(面積 = π × 半徑²)")]
        public int FilterMinArea { get; set; } = 0;

        [DisplayName("輸出過濾: 最大面積 (px²)")]
        [Description("結果過濾：只輸出面積 <= 此值的圓形。設為 0 表示不限制。(面積 = π × 半徑²)")]
        public int FilterMaxArea { get; set; } = 0;
    }

    public class TemplateMatchParameters
    {
        public enum MatchMethod { SqDiff, SqDiffNormed, CCorr, CCorrNormed, CCoeff, CCoeffNormed }

        [DisplayName("匹配方法")]
        [Description("- SqDiff: 平方差 (越小越好)\n- CCorr: 相關性 (越大越好)\n- CCoeff: 相關係數 (越大越好)\n- *Normed: 正規化版本")]
        public MatchMethod Method { get; set; } = MatchMethod.CCoeffNormed;

        [DisplayName("模板路徑")]
        [Description("模板影像檔案的完整路徑")]
        public string TemplatePath { get; set; } = "";

        [DisplayName("匹配閾值")]
        [Description("匹配分數閾值 (0-1 for Normed methods)")]
        public double MatchThreshold { get; set; } = 0.8;
    }

    // ===== 04. 繪圖與標註 (Drawing) =====

    public class DrawTextParameters
    {
        [DisplayName("文字內容")]
        public string Text { get; set; } = "Sample";

        [DisplayName("X 座標")]
        public int X { get; set; } = 10;

        [DisplayName("Y 座標")]
        public int Y { get; set; } = 30;

        [DisplayName("字體大小")]
        public double FontScale { get; set; } = 1.0;

        [DisplayName("顏色 (BGR)")]
        [Description("格式: B,G,R (例如 255,0,0 為藍色)")]
        public string ColorBGR { get; set; } = "0,255,0";

        [DisplayName("線條粗細")]
        public int Thickness { get; set; } = 2;
    }

    // ===== 05. 幾何變換 (Geometric Transform) =====

    public class RotateFlipParameters
    {
        public enum RotationType { None, Rotate90CW, Rotate180, Rotate90CCW }
        public enum FlipType { None, Horizontal, Vertical, Both }

        [DisplayName("旋轉")]
        [Description("順時針旋轉角度")]
        public RotationType Rotation { get; set; } = RotationType.None;

        [DisplayName("翻轉")]
        [Description("影像翻轉方向")]
        public FlipType Flip { get; set; } = FlipType.None;
    }

    public class ResizeParameters
    {
        public enum ResizeMode { ByScale, BySize }

        [DisplayName("縮放模式")]
        public ResizeMode Mode { get; set; } = ResizeMode.ByScale;

        [DisplayName("縮放比例")]
        [Description("當模式為 ByScale 時使用 (例如 0.5 = 縮小一半)")]
        public double Scale { get; set; } = 1.0;

        [DisplayName("目標寬度 (px)")]
        [Description("當模式為 BySize 時使用")]
        public int TargetWidth { get; set; } = 640;

        [DisplayName("目標高度 (px)")]
        [Description("當模式為 BySize 時使用")]
        public int TargetHeight { get; set; } = 480;

        [DisplayName("插值方法")]
        [Description("影像插值方法")]
        public InterpolationFlags Interpolation { get; set; } = InterpolationFlags.Linear;
    }

    public class CropParameters
    {
        [DisplayName("X 起點")]
        public int X { get; set; } = 0;

        [DisplayName("Y 起點")]
        public int Y { get; set; } = 0;

        [DisplayName("寬度")]
        public int Width { get; set; } = 100;

        [DisplayName("高度")]
        public int Height { get; set; } = 100;
    }

    // ===== 06. 降噪 (Denoising) =====

    public class DenoiseParameters
    {
        [DisplayName("濾波強度")]
        [Description("去雜訊強度 (h 值)，越大去雜訊效果越強但細節損失越多")]
        public float FilterStrength { get; set; } = 10;

        [DisplayName("模板視窗大小")]
        [Description("模板區塊大小，必須為奇數")]
        public int TemplateWindowSize { get; set; } = 7;

        [DisplayName("搜尋視窗大小")]
        [Description("搜尋區域大小，必須為奇數")]
        public int SearchWindowSize { get; set; } = 21;
    }

    // ===== 07. 特徵點 (Keypoints) =====

    public class FeatureDetectParameters
    {
        public enum DetectorType { ORB, SIFT, FAST, BRISK, AKAZE }

        [DisplayName("偵測器類型")]
        [Description("- ORB: 快速、免費\n- SIFT: 精確、需 contrib\n- FAST: 極快速\n- BRISK: 二進制描述子\n- AKAZE: 非線性尺度空間")]
        public DetectorType Detector { get; set; } = DetectorType.ORB;

        [DisplayName("最大特徵點數")]
        [Description("保留的最大特徵點數量")]
        public int MaxFeatures { get; set; } = 500;

        [DisplayName("最小特徵大小")]
        [Description("過濾小於此大小的特徵點。設為 0 表示不限制。")]
        public float MinSize { get; set; } = 0;

        [DisplayName("最大特徵大小")]
        [Description("過濾大於此大小的特徵點。設為 0 表示不限制。")]
        public float MaxSize { get; set; } = 0;

        [DisplayName("繪製特徵點")]
        [Description("是否在輸出影像上繪製特徵點")]
        public bool DrawKeypoints { get; set; } = true;
    }
}
