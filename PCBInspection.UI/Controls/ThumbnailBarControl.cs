using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PCBInspection.UI.Controls
{
    public class ThumbnailBarControl : UserControl
    {
        private FlowLayoutPanel flowPanel;
        private List<ThumbnailItemControl> _items = new List<ThumbnailItemControl>();

        public event EventHandler<object> ThumbnailClicked;

        public ThumbnailBarControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Bottom;
            this.Height = 110;
            this.BackColor = Color.FromArgb(60, 60, 60);

            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false, // Horizontal scroll
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            this.Controls.Add(flowPanel);
        }

        public void AddThumbnail(string title, Bitmap image, object tag)
        {
            // Make a small copy for display to save memory?
            // User responsibility or ours? 
            // Let's create a thumbnail copy to allow original disposal if needed (though Tag might hold it).
            // For now, assume image is managed externally or lightweight enough.
            // If image is 20MB, we should scale it down.
            
            Bitmap thumbImg = null;
            if (image != null)
            {
                // Resize to fit ~80x80
                thumbImg = new Bitmap(image, new Size(80, 80)); 
            }

            var item = new ThumbnailItemControl(title, thumbImg, tag);
            item.ItemClicked += (s, e) =>
            {
                foreach (var it in _items) it.IsSelected = false;
                item.IsSelected = true;
                ThumbnailClicked?.Invoke(this, item.ItemTag);
            };

            _items.Add(item);
            flowPanel.Controls.Add(item);
        }

        public void Clear()
        {
            flowPanel.Controls.Clear();
            foreach(var item in _items)
            {
                // Dispose thumbnail images created
                if (item.Controls.Count > 0 && item.Controls[0] is PictureBox pb && pb.Image != null)
                {
                    pb.Image.Dispose();
                }
                item.Dispose();
            }
            _items.Clear();
        }

        public void SelectItem(object tag)
        {
            foreach (var item in _items)
            {
                bool match = item.ItemTag == tag;
                item.IsSelected = match;
                if (match)
                {
                    flowPanel.ScrollControlIntoView(item);
                }
            }
        }
    }
}
