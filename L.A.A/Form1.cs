using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AutoItX3Lib;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using Microsoft.Win32;

namespace L.A.A
{
    public partial class Form1 : Form
    {
        private CheckBox checkBoxStartup;
        private IntPtr handle;
        private string WINDOW_NAME = "League of Legends";
        private AutoItX3 au3 = new AutoItX3();
        private RECT rect;

        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT IpRect);

        private int col;
        private bool isAutoAcceptEnabled = false;
        private Button buttonStart;
        private Button buttonMinimize;
        private Button buttonClose;
        private bool isCtrlPressed;

        private Point lastCursorPosition;
        private bool isMovingWindow;

        public Form1()
        {
            InitializeComponent();
            InitializeUIComponents();
            LoadCheckboxState();
        }

        private void InitializeUIComponents()
        {
            // Set up the form properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.ClientSize = new Size(600, 400);
            this.BackColor = Color.White;

            string imagePath = "Images\\bg.jpg"; // Replace with the actual path to your image
            if (File.Exists(imagePath))
            {
                this.BackgroundImage = Image.FromFile(imagePath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }


            // Set up the custom close button
            buttonClose = new Button();
            buttonClose.Text = "x";
            buttonClose.Font = new Font("Arial", 12, FontStyle.Bold);
            buttonClose.ForeColor = Color.White;
            buttonClose.BackColor = Color.Red;
            buttonClose.FlatStyle = FlatStyle.Flat;
            buttonClose.FlatAppearance.BorderSize = 0;
            buttonClose.Size = new Size(30, 30);
            buttonClose.Location = new Point(this.Width - buttonClose.Width - 10, 10);
            buttonClose.Click += ButtonClose_Click;
            this.Controls.Add(buttonClose);

            // Set up the custom minimize button
            buttonMinimize = new Button();
            buttonMinimize.Text = "-";
            buttonMinimize.Font = new Font("Arial", 12, FontStyle.Bold);
            buttonMinimize.ForeColor = Color.White;
            buttonMinimize.BackColor = Color.FromArgb(64, 64, 64);
            buttonMinimize.FlatStyle = FlatStyle.Flat;
            buttonMinimize.FlatAppearance.BorderSize = 0;
            buttonMinimize.Size = new Size(30, 30);
            buttonMinimize.Location = new Point(this.Width - buttonClose.Width - buttonMinimize.Width - 10, 10);
            buttonMinimize.Click += ButtonMinimize_Click;
            this.Controls.Add(buttonMinimize);

            // Set up the startup checkbox
            checkBoxStartup = new CheckBox();
            checkBoxStartup.Text = "Start on Windows Startup";
            checkBoxStartup.Font = new Font("Arial", 12, FontStyle.Regular);
            checkBoxStartup.ForeColor = Color.White;
            checkBoxStartup.BackColor = Color.Transparent;
            checkBoxStartup.AutoSize = true;
            checkBoxStartup.Location = new Point(10, this.Height - checkBoxStartup.Height - 10);
            checkBoxStartup.CheckedChanged += CheckBoxStartup_CheckedChanged;
            this.Controls.Add(checkBoxStartup);

            // Set up the start button
            buttonStart = new Button();
            buttonStart.Text = "Start";
            buttonStart.Font = new Font("Arial", 12, FontStyle.Bold);
            buttonStart.ForeColor = Color.White;
            buttonStart.BackColor = Color.FromArgb(64, 64, 64);
            buttonStart.FlatStyle = FlatStyle.Flat;
            buttonStart.FlatAppearance.BorderSize = 0;
            buttonStart.Size = new Size(80, 30);
            buttonStart.Location = new Point((this.Width - buttonStart.Width) / 2, (this.Height - buttonStart.Height) / 2);
            buttonStart.Click += ButtonStart_Click;
            this.Controls.Add(buttonStart);

            // Register event handlers for moving the window
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            isAutoAcceptEnabled = !isAutoAcceptEnabled;
            buttonStart.Text = isAutoAcceptEnabled ? "Stop" : "Start";
        }

        private void ButtonMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;  // Set the initial window state to minimized

            // Start a new thread for the AutoAccept method
            Thread AA = new Thread(AutoAccept) { IsBackground = true };
            AA.Start();

            LoadCheckboxState();  // Load the checkbox state

            // Hide the form and show in system tray
            this.Hide();
            this.ShowInTaskbar = false;

            // Set up NotifyIcon
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Restore the form when double-clicking on the notify icon
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void CheckBoxStartup_CheckedChanged(object sender, EventArgs e)
        {
            SetStartupOnWindowsStartup(checkBoxStartup.Checked);
        }

        private void SetStartupOnWindowsStartup(bool startOnStartup)
        {
            string appName = "L.A.A";
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, true))
            {
                if (startOnStartup)
                {
                    key.SetValue(appName, Application.ExecutablePath);
                }
                else
                {
                    key.DeleteValue(appName, false);
                }
            }
        }

        private void AutoAccept()
        {
            while (true)
            {
                if (isAutoAcceptEnabled)
                {
                    GetWindowRectangle();
                    col = au3.PixelGetColor(rect.left + 584, rect.top + 555);
                    if (col == 0x1E252A)
                    {
                        Thread.Sleep(20);
                        au3.MouseMove(rect.left + 584, rect.top + 555, 1);
                        au3.MouseClick("LEFT");
                        Thread.Sleep(20);
                        au3.MouseClick("LEFT");
                        Thread.Sleep(5000);
                    }
                }
                Thread.Sleep(20);
            }
        }

        private void GetWindowRectangle()
        {
            handle = FindWindow(null, WINDOW_NAME);
            GetWindowRect(handle, out rect);
        }

        // Window movement event handlers

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastCursorPosition = e.Location;
                isMovingWindow = true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMovingWindow)
            {
                this.Left += e.X - lastCursorPosition.X;
                this.Top += e.Y - lastCursorPosition.Y;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isMovingWindow = false;
        }

        // Saving and loading checkbox state

        private void LoadCheckboxState()
        {
            bool startOnStartup = GetStartupOnWindowsStartup();
            checkBoxStartup.Checked = startOnStartup;
        }

        private bool GetStartupOnWindowsStartup()
        {
            string appName = "L.A.A";
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey))
            {
                return key.GetValue(appName) != null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            SaveCheckboxState();
        }

        private void SaveCheckboxState()
        {
            bool startOnStartup = checkBoxStartup.Checked;
            SetStartupOnWindowsStartup(startOnStartup);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                isCtrlPressed = true;
            }
            else if (isCtrlPressed && e.KeyCode == Keys.L)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.BringToFront();
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                isCtrlPressed = false;
            }





        }
    }
}
