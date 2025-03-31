using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Reflection;


namespace ProductivityTimerApp
{

    public static class ControlExtensions
    {
        public static void SetDoubleBuffered(this Control control, bool enabled)
        {
            var prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(control, enabled, null);
        }
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
    public partial class Form1 : Form
    {
        private Label lblTime;
        private TextBox txtTime;
        private TextBox txtTitle;
        private TextBox txtNotes;
        private Button btnStart;
        private Button btnPause;
        private Button btnReset;
        private ComboBox templateSelector;

        private System.Windows.Forms.Timer countdownTimer;
        private TimeSpan remainingTime;
        private bool isPaused = false;
        private MinimalForm minimalForm;

        private string settingsFile = "settings.json";

        public Form1()
        {
            Text = "Productivity Timer";
            ClientSize = new Size(400, 330);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
         ControlStyles.UserPaint |
         ControlStyles.DoubleBuffer, true);
            UpdateStyles();

            InitializeComponents();
            LoadAppState();
        }

        private void InitializeComponents()
        {
            lblTime = new Label
            {
                Text = "00:00:00",
                Font = new Font("Consolas", 32, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            txtTime = new TextBox
            {
                Text = "00:01:00",
                Location = new Point(20, 90),
                Width = 100,
                Font = new Font("Consolas", 10, FontStyle.Regular),
                TextAlign = HorizontalAlignment.Center
            };
            txtTime.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    StartTimer(s, e);
                    e.SuppressKeyPress = true;
                }
            };

            txtTitle = new TextBox
            {
                PlaceholderText = "Task title...",
                Location = new Point(20, 130),
                Width = 340
            };

            txtNotes = new TextBox
            {
                PlaceholderText = "Optional notes...",
                Location = new Point(20, 160),
                Width = 340
            };

            templateSelector = new ComboBox
            {
                Location = new Point(140, 90),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            templateSelector.Items.AddRange(new[] { "Select Template", "Quick Minute", "Short Task (15 min)", "Pomodoro (25/5)", "Pomodoro (take 5)", "Focused (30/10)", "Focused (take 10)" });
            templateSelector.SelectedIndex = 0;
            templateSelector.SelectedIndexChanged += (s, e) =>
            {
                if (templateSelector.SelectedIndex == 1)
                    txtTime.Text = "00:01:00";
                else if (templateSelector.SelectedIndex == 2)
                    txtTime.Text = "00:15:00";
                else if (templateSelector.SelectedIndex == 3)
                    txtTime.Text = "00:25:00";
                else if (templateSelector.SelectedIndex == 4)
                    txtTime.Text = "00:05:00";
                else if (templateSelector.SelectedIndex == 5)
                    txtTime.Text = "00:30:00";
                else if (templateSelector.SelectedIndex == 6)
                    txtTime.Text = "00:10:00";
                else if (templateSelector.SelectedIndex == 7)
                    txtTime.Text = "00:05:00";
                else if (templateSelector.SelectedIndex == 8)
                    txtTime.Text = "00:30:00";
                else if (templateSelector.SelectedIndex == 9)
                    txtTime.Text = "00:10:00";
            };

            btnStart = new Button { Text = "Start", Location = new Point(20, 200), AutoSize = true };
            btnPause = new Button { Text = "Pause", Location = new Point(140, 200), AutoSize = true };
            btnReset = new Button { Text = "Reset", Location = new Point(260, 200), AutoSize = true };

            var btnMinimalView = new Button { Text = "Minimal View", Location = new Point(20, 240), AutoSize = true };

            btnStart.Click += StartTimer;
            btnPause.Click += PauseTimer;
            btnReset.Click += ResetTimer;

            btnMinimalView.Click += (s, e) =>
            {
                if (minimalForm != null && !minimalForm.IsDisposed && minimalForm.Visible)
                {
                    minimalForm.Close();
                    minimalForm = null;
                }
                else
                {
                    minimalForm = new MinimalForm
                    {
                        StartPosition = FormStartPosition.Manual
                    };

                    if (File.Exists(settingsFile))
                    {
                        var saved = JsonSerializer.Deserialize<AppState>(File.ReadAllText(settingsFile));
                        if (saved?.MinimalViewLocation.HasValue == true)
                        {
                            minimalForm.Location = saved.MinimalViewLocation.Value;
                        }
                    }

                    minimalForm.Show();
                    UpdateMinimalForm(lblTime.Text, txtTitle.Text);
                }
            };




            Controls.Add(lblTime);
            Controls.Add(txtTime);
            Controls.Add(templateSelector);
            Controls.Add(txtTitle);
            Controls.Add(txtNotes);
            Controls.Add(btnStart);
            Controls.Add(btnPause);
            Controls.Add(btnReset);
            Controls.Add(btnMinimalView);

            countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            countdownTimer.Tick += TimerTick;
        }

        private void StartTimer(object sender, EventArgs e)
        {
            if (!countdownTimer.Enabled || isPaused)
            {
                if (TimeSpan.TryParse(txtTime.Text, out remainingTime))
                {
                    countdownTimer.Start();
                    isPaused = false;
                    btnPause.Text = "Pause";
                    btnStart.Text = "Restart";
                }
                else
                {
                    MessageBox.Show("Invalid time format. Use HH:MM:SS.");
                }
            }
        }

        private void PauseTimer(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                countdownTimer.Stop();
                btnPause.Text = "Continue";
                isPaused = true;
            }
            else
            {
                countdownTimer.Start();
                btnPause.Text = "Pause";
                isPaused = false;
            }
        }

        private void ResetTimer(object sender, EventArgs e)
        {
            countdownTimer.Stop();
            remainingTime = TimeSpan.Zero;
            lblTime.Text = "00:00:00";
            btnPause.Text = "Pause";
            btnStart.Text = "Start";
            UpdateMinimalForm("00:00:00", "");
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (remainingTime.TotalSeconds > 0)
            {
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
                var displayTime = remainingTime.ToString(@"hh\:mm\:ss");
                lblTime.Text = displayTime;
                btnStart.Text = "Restart";
                UpdateMinimalForm(displayTime, txtTitle.Text);
            }
            else
            {
                countdownTimer.Stop();
                lblTime.Text = "00:00:00";
                UpdateMinimalForm("00:00:00", txtTitle.Text);

                try
                {
                    new SoundPlayer("Assets/alert.wav").Play();
                }
                catch { }

                var alert = new TimerAlertForm(txtTitle.Text);
                alert.Show();
            }
        }

        private void UpdateMinimalForm(string time, string title)
        {
            if (minimalForm == null || minimalForm.IsDisposed || !minimalForm.Visible)
                return;

            minimalForm.BeginInvoke(new Action(() =>
            {
                minimalForm.UpdateTimeDisplay(time, title);
            }));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveAppState();
            base.OnFormClosing(e);
        }

        private void SaveAppState()
        {
            try
            {
                var state = new AppState
                {
                    //LastTime = txtTime.Text,
                    LastTitle = txtTitle.Text,
                    LastNotes = txtNotes.Text,
                    MinimalViewOpen = minimalForm != null && minimalForm.Visible && !minimalForm.IsDisposed,
                    MinimalViewLocation = minimalForm != null && !minimalForm.IsDisposed ? minimalForm.Location : null
                };
                File.WriteAllText(settingsFile, JsonSerializer.Serialize(state));
            }
            catch (JsonException)
            {
                // no json data, just write new data
                File.WriteAllText(settingsFile, "{}");
            }
        }

        private void LoadAppState()
        {
            try
            {
                if (File.Exists(settingsFile))
                {
                    var content = File.ReadAllText(settingsFile);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var saved = JsonSerializer.Deserialize<AppState>(content);
                        if (saved != null)
                        {
                            //txtTime.Text = saved.LastTime;
                            txtTitle.Text = saved.LastTitle;
                            txtNotes.Text = saved.LastNotes;

                            if (saved.MinimalViewOpen && saved.MinimalViewLocation.HasValue)
                            {
                                var location = saved.MinimalViewLocation.Value;
                                bool onScreen = Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(location));
                                if (onScreen)
                                {
                                    minimalForm = new MinimalForm
                                    {
                                        StartPosition = FormStartPosition.Manual,
                                        Location = location
                                    };
                                    minimalForm.Show();
                                }
                            }
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Optional: Log or debug the issue
                Console.WriteLine($"Error loading JSON: {ex.Message}");
            }

            // If we reach here, rewrite clean settings
            SaveAppState();
        }



        public class AppState
        {
            //public string LastTime { get; set; }
            public string LastTitle { get; set; }
            public string LastNotes { get; set; }
            public bool MinimalViewOpen { get; set; }
            public Point? MinimalViewLocation { get; set; }
        }
    }
}
