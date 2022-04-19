﻿#region License Information (GPL v3)

/*
    Copyright (c) Jaex

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ImageVisualizer
{
    public partial class ImageVisualizerForm : Form
    {
        private const int MaxZoom = 2;

        public Image Image { get; private set; }

        private double zoom = 0.1;

        public double Zoom
        {
            get
            {
                return zoom;
            }
            private set
            {
                double tempZoom = value.Between(0.1, MaxZoom);

                tsddbZoom.Text = $"Zoom: {tempZoom * 100}%";
                if (tempZoom != zoom)
                {
                    zoom = tempZoom;

                    UpdatePreview();
                }
            }
        }

        private Point dragStartPosition;
        private bool escapeKeyDown;

        public ImageVisualizerForm(Image img)
        {
            InitializeComponent();
            Image = img;
            Zoom = 1;
            UpdateControls();
        }

        private void UpdateControls()
        {
            tsMain.Renderer = new CustomToolStripProfessionalRenderer();
            tsddbZoom.HideImageMargin();

            for (double i = 0; i <= MaxZoom; i += 0.1)
            {
                double currentZoom = i + 0.1;
                ToolStripMenuItem tsmi = new ToolStripMenuItem(currentZoom * 100 + "%");

                //Todo if zoom ratio large 2, every time incress 1

                tsmi.Click += (sender, e) => Zoom = currentZoom;
                tsddbZoom.DropDownItems.Add(tsmi);
            }

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            UpdatePreview(Image);
        }

        private void UpdatePreview(Image img)
        {
            if (img != null)
            {
                tsslStatusWidth.Text = $"Width: {img.Width}px";
                tsslStatusHeight.Text = $"Height: {img.Height}px";
                tsslStatusPixelFormat.Text = $"Pixel format: {img.PixelFormat}";
                tsslStatusType.Text = $"Type: {img.GetType().Name}";

                Bitmap bmpPreview = RenderPreview(img);

                SetPreviewImage(bmpPreview);
            }
        }

        private Bitmap RenderPreview(Image img)
        {
            int lineSize = 2;
            int previewWidth = (int)Math.Round(img.Width * Zoom);
            int previewHeight = (int)Math.Round(img.Height * Zoom);

            Bitmap bmpPreview = new Bitmap(previewWidth + lineSize * 2, previewHeight + lineSize * 2, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(bmpPreview))
            {
                if (img.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    using (Image checkers = Helpers.DrawCheckers(previewWidth, previewHeight))
                    {
                        g.DrawImage(checkers, lineSize, lineSize, checkers.Width, checkers.Height);
                    }
                }

                g.DrawRectangle(Pens.White, 0, 0, bmpPreview.Width - 1, bmpPreview.Height - 1);
                g.DrawRectangle(Pens.Black, 1, 1, bmpPreview.Width - 3, bmpPreview.Height - 3);

                if (Zoom > 1)
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                }

                g.DrawImage(img, lineSize, lineSize, previewWidth, previewHeight);
            }

            return bmpPreview;
        }

        private void SetPreviewImage(Image img)
        {
            if (pbPreview.IsValidImage())
            {
                pbPreview.Image.Dispose();
            }

            pbPreview.Image = img;
        }

        private void ImageVisualizerForm_Shown(object sender, EventArgs e)
        {
            Activate();
        }

        private void ImageVisualizerForm_KeyDown(object sender, KeyEventArgs e)
        {
            escapeKeyDown = e.KeyCode == Keys.Escape;
        }

        private void ImageVisualizerForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (escapeKeyDown && e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void tsbCopyImage_Click(object sender, EventArgs e)
        {
            Helpers.CopyImage(Image);
        }

        private void tsbSaveImage_Click(object sender, EventArgs e)
        {
            Helpers.SaveImageAsFile(Image);
        }

        private void tsbOpenGitHub_Click(object sender, EventArgs e)
        {
            Helpers.OpenURL("https://github.com/Jaex/ImageVisualizer");
        }

        private void pbPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (pMain.HorizontalScroll.Visible || pMain.VerticalScroll.Visible))
            {
                Cursor = Cursors.SizeAll;
                dragStartPosition = e.Location;
            }
        }

        private void pbPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (pMain.HorizontalScroll.Visible || pMain.VerticalScroll.Visible))
            {
                Point scrollOffset = new Point(e.X - dragStartPosition.X, e.Y - dragStartPosition.Y);
                pMain.AutoScrollPosition = new Point(-pMain.AutoScrollPosition.X - scrollOffset.X, -pMain.AutoScrollPosition.Y - scrollOffset.Y);
            }
        }

        private void pbPreview_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Default;
        }
    }
}