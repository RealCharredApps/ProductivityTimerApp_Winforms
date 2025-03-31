using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace ProductivityTimerApp
{
    public class TimerAlertForm : Form
    {
        private Label message;
        private Button dismissButton;
        private SoundPlayer player;

        public TimerAlertForm(string title)
        {
            Text = title;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(350, 150);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ControlBox = false; // removes X button

            message = new Label
            {
                Text = "Time for a break???",
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            dismissButton = new Button
            {
                Text = "Dismiss",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            dismissButton.Click += (s, e) =>
            {
                player?.Stop();
                Close();
            };

            Controls.Add(message);
            Controls.Add(dismissButton);

            player = new SoundPlayer("Assets/alert.wav");
            player.PlayLooping(); // play until dismissed
        }
    }
}
