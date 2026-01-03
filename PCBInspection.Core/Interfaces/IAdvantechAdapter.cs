namespace PCBInspection.Core.Interfaces
{
	/// <summary>研華 IO 卡適配器介面，用於控制數位輸出與讀取數位輸入。</summary>
	public interface IAdvantechAdapter
	{
		/// <summary>初始化 IO 卡硬體資源</summary>
		void Initialize();
		/// <summary>寫入數位輸出訊號</summary>
		bool WriteDigitalOutput(string signalName, bool value, int pulseMs = 100);
		/// <summary>讀取數位輸入訊號</summary>
		bool ReadDigitalInput(string   signalName);
		/// <summary>釋放硬體資源</summary>
		void Dispose();
	}
}