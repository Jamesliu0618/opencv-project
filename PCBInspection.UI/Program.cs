using System;
using System.Windows.Forms;

namespace PCBInspection.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Launch main UI or calibration wizard via CLI flag
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1].ToLowerInvariant().Contains("calibrate"))
            {
                Application.Run(new CalibrationWizardForm());
            }
            else
            {
                Application.Run(new Form() { Text = "PCB Inspection UI (placeholder)" });
            }
        }
    }
}