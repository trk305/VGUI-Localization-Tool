using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VGUILocalizationTool
{
    public partial class Form1: Form
    {
        private bool isDarkMode = false;
        public Form1()
        {
            InitializeComponent();
            this.Text = "Application Settings";
            this.ClientSize = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;

            // Create header label
            Label headerLabel = new Label();
            headerLabel.Text = "Better VGUI Localization Tool Made by trk305";
            headerLabel.Font = new Font(headerLabel.Font, FontStyle.Bold);
            headerLabel.Dock = DockStyle.Top;
            headerLabel.Height = 40;
            headerLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Create theme toggle button (now below the header)
            Button btnToggleTheme = new Button();
            btnToggleTheme.Text = "Toggle Dark Mode";
            btnToggleTheme.Dock = DockStyle.Top;
            btnToggleTheme.Height = 40;
            btnToggleTheme.Margin = new Padding(10);
            btnToggleTheme.Click += BtnToggleTheme_Click;

            // Add sample controls (optional)
            Label sampleLabel = new Label();
            sampleLabel.Text = "Settings will appear here";
            sampleLabel.Dock = DockStyle.Fill;
            sampleLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Add controls to form in correct order
            this.Controls.Add(sampleLabel);  // Background content
            this.Controls.Add(btnToggleTheme); // Button (middle)
            this.Controls.Add(headerLabel);  // Header (top)

            // Set initial theme
            ApplyTheme();
        }

        private void BtnToggleTheme_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();

            if (Application.OpenForms["MainForm"] is MainForm mainForm)
            {
                mainForm.ApplyTheme(isDarkMode);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            this.ForeColor = isDarkMode ? Color.White : Color.Black;

            foreach (Control control in this.Controls)
            {
                // Skip special theming for the button itself
                if (control is Button btn)
                {
                    btn.Text = isDarkMode ? "Toggle Light Mode" : "Toggle Dark Mode";
                    btn.BackColor = isDarkMode ? Color.FromArgb(60, 60, 60) : SystemColors.Control;
                    btn.ForeColor = isDarkMode ? Color.White : Color.Black;
                    continue;
                }

                // Standard theming for other controls
                control.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
                control.ForeColor = isDarkMode ? Color.White : Color.Black;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
