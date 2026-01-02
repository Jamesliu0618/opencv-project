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
            // TODO: launch main form
            Application.Run(new Form() { Text = "PCB Inspection UI (placeholder)" });
        }
    }
}