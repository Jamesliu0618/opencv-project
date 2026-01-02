using OpenCvSharp;

namespace PCBInspection.Core.Interfaces
{
    public interface ICameraAdapter
    {
        void Initialize(object config = null);
        Mat CaptureFrame();
        void Dispose();
    }
}