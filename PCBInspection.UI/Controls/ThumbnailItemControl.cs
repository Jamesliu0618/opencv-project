using System;
using System.Drawing;
using System.Windows.Forms;

namespace PCBInspection.UI.Controls
{
	public class ThumbnailItemControl : UserControl
	{
		private bool       _isSelected;
		private Label      lblTitle;
		private PictureBox pbImage;

		public ThumbnailItemControl(string title, Bitmap image, object tag)
		{
			ItemTag = tag;
			InitializeComponent();
			SetData(title, image);
		}

		public object ItemTag { get; set; }

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				_isSelected = value;
				BackColor   = _isSelected ? Color.Orange : Color.Transparent;
				Padding     = _isSelected ? new Padding(3) : new Padding(0);
				Invalidate();
			}
		}

		public event EventHandler ItemClicked;

		private void InitializeComponent()
		{
			Size           = new Size(100, 100);
			Cursor         = Cursors.Hand;
			DoubleBuffered = true;

			lblTitle = new Label
			{
				Dock         = DockStyle.Bottom,
				Height       = 20,
				TextAlign    = ContentAlignment.MiddleCenter,
				Font         = new Font("Microsoft JhengHei", 8f),
				AutoEllipsis = true,
				BackColor    = Color.FromArgb(240, 240, 240),
			};

			pbImage = new PictureBox
			{
				Dock      = DockStyle.Fill,
				SizeMode  = PictureBoxSizeMode.Zoom,
				BackColor = Color.Black,
			};

			// Events forwarding
			pbImage.Click  += (s, e) => OnClick(e);
			lblTitle.Click += (s, e) => OnClick(e);
			Controls.Add(pbImage);
			Controls.Add(lblTitle);
		}

		public void SetData(string title, Bitmap image)
		{
			lblTitle.Text = title;
			pbImage.Image = image; // Note: We reference the image, ownership remains outside usually. Or clone? 

			// For thumbnails, usually we want a smaller copy to save memory if original is huge.
			// But user might want to pass pre-generated thumbnail.
			// Tooltip
			var tt = new ToolTip();
			tt.SetToolTip(this,    title);
			tt.SetToolTip(pbImage, title);
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			ItemClicked?.Invoke(this, EventArgs.Empty);
		}
	}
}