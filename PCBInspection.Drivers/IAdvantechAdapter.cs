namespace PCBInspection.Drivers
{
    public interface IAdvantechAdapter
    {
        void Initialize();
        bool WriteDigitalOutput(string signalName, bool value, int pulseMs = 100);
        bool ReadDigitalInput(string signalName);
        void Dispose();
    }
}