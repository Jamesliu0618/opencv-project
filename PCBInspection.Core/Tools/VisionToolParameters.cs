using System.ComponentModel;
using OpenCvSharp;

namespace PCBInspection.Core.Tools
{
    // ----- Preprocessing Tools -----

    public class GrayscaleParameters
    {
        // No parameters needed currently
    }

    public class BlurParameters
    {
        public enum BlurType { Gaussian, Median, Box }

        [DisplayName("模糊類型")]
        [Description("選擇模糊演算法: Gaussian (高斯), Median (中值), Box (均值)")]
        public BlurType Type { get; set; } = BlurType.Gaussian;

        [DisplayName("核心大小 (px)")]
        [Description("濾波核大小，必須為奇數 (e.g. 3, 5, 7, 9)")]
        public int KernelSize { get; set; } = 5;

        [DisplayName("Sigma X")]
        [Description("高斯核標準差 (僅用於 Gaussian)。設為 0 表示由核心大小自動計算。")]
        public double SigmaX { get; set; } = 0;
    }

    public class ThresholdParameters
    {
        public enum ThreshMethod { Binary, BinaryInv, Otsu, ToZero }

        [DisplayName("閾值方法")]
        [Description("選擇二值化方法")]
        public ThreshMethod Method { get; set; } = ThreshMethod.Binary;

        [DisplayName("閾值 (0-255)")]
        [Description("手動閾值。若使用 Otsu 則此值會被忽略。")]
        public double Threshold { get; set; } = 128;

        [DisplayName("最大值 (0-255)")]
        [Description("二值化後的最大值 (通常為 255)")]
        public double MaxVal { get; set; } = 255;
    }

    public class MorphologyParameters
    {
        public enum MorphOp { Erode, Dilate, Open, Close, Gradient }

        [DisplayName("運算類型")]
        [Description("形態學運算類型: 腐蝕, 膨脹, 開運算, 閉運算...")]
        public MorphOp Operation { get; set; } = MorphOp.Open;

        [DisplayName("核心形狀")]
        [Description("結構元素形狀")]
        public MorphShapes Shape { get; set; } = MorphShapes.Rect;

        [DisplayName("核心大小 (px)")]
        [Description("結構元素大小 (e.g. 3, 5)")]
        public int KernelSize { get; set; } = 3;

        [DisplayName("迭代次數")]
        [Description("運算重複執行的次數")]
        public int Iterations { get; set; } = 1;
    }

    // ----- Feature Extraction Tools -----

    public class EdgeDetectionParameters
    {
        public enum EdgeMethod { Canny, Sobel }

        [DisplayName("邊緣偵測方法")]
        public EdgeMethod Method { get; set; } = EdgeMethod.Canny;

        [DisplayName("閾值 1")]
        [Description("Canny: 低閾值 / Sobel: X Order")]
        public double Threshold1 { get; set; } = 50;

        [DisplayName("閾值 2")]
        [Description("Canny: 高閾值 / Sobel: Y Order")]
        public double Threshold2 { get; set; } = 150;

        [DisplayName("孔徑大小")]
        [Description("Sobel 運算子的孔徑大小 (3, 5, 7)")]
        public int ApertureSize { get; set; } = 3;
    }
}
