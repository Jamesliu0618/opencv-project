using System;
using System.IO;
using System.Windows.Forms;
using PCBInspection.Core.Services;
using PCBInspection.Core;

namespace PCBInspection.UI
{
    public class CalibrationWizardForm : Form
    {
        private TextBox txtPatternCols;
        private TextBox txtPatternRows;
        private TextBox txtSquareMm;
        private TextBox txtImagesDir;
        private Button btnBrowse;
        private Button btnRun;
        private Button btnSave;
        private Label lblResult;

        public CalibrationWizardForm()
        {
            Text = "Calibration Wizard";
            Width = 520; Height = 220;

            var lblCols = new Label { Text = "Pattern cols (inner corners)", Left = 10, Top = 10, Width = 180 };
            txtPatternCols = new TextBox { Left = 200, Top = 10, Width = 60, Text = "7" };

            var lblRows = new Label { Text = "Pattern rows (inner corners)", Left = 270, Top = 10, Width = 180 };
            txtPatternRows = new TextBox { Left = 450, Top = 10, Width = 40, Text = "5" };

            var lblSquare = new Label { Text = "Square size (mm)", Left = 10, Top = 40, Width = 180 };
            txtSquareMm = new TextBox { Left = 200, Top = 40, Width = 60, Text = "2.0" };

            var lblImages = new Label { Text = "Images directory", Left = 10, Top = 80, Width = 180 };
            txtImagesDir = new TextBox { Left = 200, Top = 80, Width = 240 };
            btnBrowse = new Button { Text = "Browse...", Left = 450, Top = 78, Width = 60 };
            btnBrowse.Click += BtnBrowse_Click;

            btnRun = new Button { Text = "Run Calibration", Left = 10, Top = 120, Width = 140 };
            btnRun.Click += BtnRun_Click;

            btnSave = new Button { Text = "Save Calibration", Left = 160, Top = 120, Width = 140, Enabled = false };
            btnSave.Click += BtnSave_Click;

            lblResult = new Label { Text = "Result: (not run)", Left = 10, Top = 160, Width = 480 };

            Controls.Add(lblCols);
            Controls.Add(txtPatternCols);
            Controls.Add(lblRows);
            Controls.Add(txtPatternRows);
            Controls.Add(lblSquare);
            Controls.Add(txtSquareMm);
            Controls.Add(lblImages);
            Controls.Add(txtImagesDir);
            Controls.Add(btnBrowse);
            Controls.Add(btnRun);
            Controls.Add(btnSave);
            Controls.Add(lblResult);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK) txtImagesDir.Text = dlg.SelectedPath;
            }
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            try
            {
                int cols = int.Parse(txtPatternCols.Text);
                int rows = int.Parse(txtPatternRows.Text);
                double sq = double.Parse(txtSquareMm.Text);
                var dir = txtImagesDir.Text;
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) { MessageBox.Show("Please select a valid images directory"); return; }
                var images = Directory.GetFiles(dir, "*.png");
                if (images.Length == 0) { MessageBox.Show("No PNG images found in directory"); return; }

                Calibration.ComputeCalibration(images, cols, rows, sq);
                var ppm = Calibration.GetPixelsPerMm();
                lblResult.Text = $"Result: pixelsPerMm = {ppm:F4} px/mm";
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Calibration failed: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "calibration.json");
                CalibrationService.SaveCalibration(savePath);
                MessageBox.Show("Calibration saved to " + savePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message);
            }
        }
    }
}