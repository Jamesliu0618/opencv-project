using System;
using System.Drawing;
using System.Windows.Forms;

namespace PCBInspection.UI.Controls
{
    public class ThumbnailItemControl : UserControl
    {
        private PictureBox pbImage;
        private Label lblTitle;
        private bool _isSelected;
        
        public object ItemTag { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                this.BackColor = _isSelected ? Color.Orange : Color.Transparent;
                this.Padding = _isSelected ? new Padding(3) : new Padding(0);
                this.Invalidate();
            }
        }

        public event EventHandler ItemClicked;

        public ThumbnailItemControl(string title, Bitmap image, object tag)
        {
            this.ItemTag = tag;
            InitializeComponent();
            SetData(title, image);
        }

        private void InitializeComponent()
        {
            this.Size = new Size(100, 100);
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;

            lblTitle = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft JhengHei", 8f),
                AutoEllipsis = true,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            pbImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            // Events forwarding
            pbImage.Click += (s, e) => OnClick(e);
            lblTitle.Click += (s, e) => OnClick(e);

            this.Controls.Add(pbImage);
            this.Controls.Add(lblTitle);
        }

        public void SetData(string title, Bitmap image)
        {
            lblTitle.Text = title;
            pbImage.Image = image; // Note: We reference the image, ownership remains outside usually. Or clone? 
                                   // For thumbnails, usually we want a smaller copy to save memory if original is huge.
                                   // But user might want to pass pre-generated thumbnail.
            // Tooltip
            var tt = new ToolTip();
            tt.SetToolTip(this, title);
            tt.SetToolTip(pbImage, title);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
