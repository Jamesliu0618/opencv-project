using OpenCvSharp;

namespace PCBInspection.Drivers
{
    public interface ICameraAdapter
    {
        void Initialize(IDriverConfig config = null);
        Mat CaptureFrame();
        void Dispose();
    }

    public class IDriverConfig { }
}