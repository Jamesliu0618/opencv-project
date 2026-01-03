using System;
using System.Windows.Forms;

namespace PCBInspection.UI
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var args = Environment.GetCommandLineArgs();

			if(args.Length > 1 && args[1].ToLowerInvariant().Contains("calibrate"))
			{
				Application.Run(new CalibrationWizardForm());
			}
			else
			{
				Application.Run(new MainForm());
			}
		}
	}
}