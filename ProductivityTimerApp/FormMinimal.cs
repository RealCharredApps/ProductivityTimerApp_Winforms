using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using ProductivityTimerApp;


namespace ProductivityTimerApp
{
    public class MinimalForm : Form
    {
        private Label lblTime;
        private Label lblTitle;
        private string timeText = "00:00:00";
        private string titleText = "";


        public MinimalForm()
        {
            Text = "Minimal Timer";
            this.TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(260, 130);
            MinimumSize = new Size(260, 130);
            MaximumSize = new Size(400, 200);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();


            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Toggle Borderless Mode", null, (s, e) =>
            {
                FormBorderStyle = FormBorderStyle == FormBorderStyle.None
                    ? FormBorderStyle.SizableToolWindow
                    : FormBorderStyle.None;
            });
            contextMenu.Items.Add("Close", null, (s, e) => Close());
            ContextMenuStrip = contextMenu;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 2,
            };
            foreach (Control ctrl in Controls)
                DoubleBufferUtility.Enable(ctrl);

            layout.SetDoubleBuffered(true);

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));


            lblTime = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                //Text = "00:00:00",
                AutoSize = false
            };

            lblTitle = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.TopCenter,
                //Margin = new Padding(0, 0, 0, 5),
                AutoSize = true,
                Visible = true, // ✅ ensure it’s not conditionally hidden
            };


            layout.Controls.Add(lblTime, 0, 0);
            layout.Controls.Add(lblTitle, 0, 1);
            Controls.Add(layout);

            Shown += (s, e) =>
            {
                PerformLayout();
                Invalidate(); // force redraw so title fits
            };

            Resize += (s, e) => AdjustFontSizes();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var client = ClientRectangle;
            var topHalf = new Rectangle(0, 0, client.Width, client.Height / 2);
            var bottomHalf = new Rectangle(0, client.Height / 2, client.Width, client.Height / 2);

            using var timeFont = FitFont(e.Graphics, timeText, FontFamily.GenericMonospace, topHalf.Size, FontStyle.Bold);
            using var titleFont = FitFont(e.Graphics, titleText, FontFamily.GenericSansSerif, bottomHalf.Size, FontStyle.Regular);

            TextRenderer.DrawText(e.Graphics, timeText, timeFont, topHalf, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(e.Graphics, titleText, titleFont, bottomHalf, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
        }

        public static class DoubleBufferUtility
        {
            public static void Enable(Control control)
            {
                if (SystemInformation.TerminalServerSession) return;

                PropertyInfo prop = typeof(Control)
                    .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);

                prop?.SetValue(control, true, null);
            }
        }

        private Font FitFont(Graphics g, string text, FontFamily family, Size area, FontStyle style)
        {
            for (int size = 72; size >= 8; size--)
            {
                var font = new Font(family, size, style);
                var sz = TextRenderer.MeasureText(g, text, font);
                if (sz.Width <= area.Width && sz.Height <= area.Height)
                    return font;
            }
            return new Font(family, 8, style);
        }




        public void UpdateTimeDisplay(string time, string title)
        {
            if (lblTime.InvokeRequired)
            {
                lblTime.Invoke(() => lblTime.Text = time);
                lblTitle.Invoke(() => lblTitle.Text = title);
            }
            else
            {
                lblTime.Text = time;
                lblTitle.Text = title;
            }
        }


        private void AdjustFontSizes()
        {
            lblTime.Font = FitText(lblTime, "00:00:00", "Consolas", 72, 12, FontStyle.Bold);
            lblTitle.Font = FitText(lblTitle, lblTitle.Text, "Segoe UI", 24, 8, FontStyle.Regular);
        }

        private Font FitText(Label label, string text, string fontName, int max, int min, FontStyle style)
        {
            using (Graphics g = label.CreateGraphics())
            {
                for (int size = max; size >= min; size--)
                {
                    var testFont = new Font(fontName, size, style);
                    var sizeNeeded = TextRenderer.MeasureText(g, text, testFont);
                    if (sizeNeeded.Width <= label.Width && sizeNeeded.Height <= label.Height)
                        return testFont;
                }
            }
            return new Font(fontName, min, style);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCAPTION = 0x2;

            if (m.Msg == WM_NCHITTEST && FormBorderStyle == FormBorderStyle.None)
            {
                m.Result = (IntPtr)HTCAPTION;
                return;
            }
            base.WndProc(ref m);
        }
    }

    internal class DoubleBuffered
    {
    }
}