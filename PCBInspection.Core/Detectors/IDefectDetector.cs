namespace PCBInspection.Core.Detectors
{
    using PCBInspection.Core.Models;
    using OpenCvSharp;
    using System.Collections.Generic;

    /// <summary>
    /// 瑕疵偵測器介面
    /// </summary>
    public interface IDefectDetector
    {
        /// <summary>
        /// 偵測影像中的瑕疵
        /// </summary>
        List<Defect> Detect(Mat image, List<Component> components = null);

        /// <summary>
        /// 偵測器名稱
        /// </summary>
        string Name { get; }
    }
}
