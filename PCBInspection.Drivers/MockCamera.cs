using System;
using System.IO;
using OpenCvSharp;

namespace PCBInspection.Drivers
{
    public class MockCamera : ICameraAdapter
    {
        private readonly string _fixturesDir;
        private readonly string[] _images;
        private int _index = 0;

        public MockCamera(string fixturesDir)
        {
            _fixturesDir = fixturesDir;
            if (Directory.Exists(_fixturesDir))
                _images = Directory.GetFiles(_fixturesDir, "*.png");
            else
                _images = Array.Empty<string>();
        }

        public void Initialize(IDriverConfig config = null)
        {
            // noop for mock
        }

        public Mat CaptureFrame()
        {
            if (_images.Length == 0)
            {
                // generate a synthetic image
                var mat = new Mat(new Size(640, 480), MatType.CV_8UC3, Scalar.White);
                Cv2.PutText(mat, DateTime.UtcNow.ToString("o"), new Point(10, 30), HersheyFonts.HersheySimplex, 0.6, Scalar.Black);
                return mat;
            }

            var path = _images[_index % _images.Length];
            _index++;
            return Cv2.ImRead(path);
        }

        public void Dispose()
        {
            // nothing to clean up
        }
    }
}