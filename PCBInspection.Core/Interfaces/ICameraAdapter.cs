using OpenCvSharp;

namespace PCBInspection.Core.Interfaces
{
	/// <summary>相機適配器介面，提供抽象的影像擷取能力。</summary>
	public interface ICameraAdapter
	{
		/// <summary>初始化相機參數 (如曝光、增益、IP 位址等)</summary>
		void Initialize(object config = null);
		/// <summary>同步擷取一幀影像</summary>
		Mat  CaptureFrame();
		/// <summary>斷開連線並釋放資源</summary>
		void Dispose();
	}
}