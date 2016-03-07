﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using NVIDIASurroundToggle.Native;
using NVIDIASurroundToggle.Native.Enums;
using NVIDIASurroundToggle.Resources;

namespace NVIDIASurroundToggle
{
    public partial class FrmSplash : Form
    {
        private static FrmSplash _instance;

        private readonly Action _action;

        private readonly bool _showButtons;

        private int _time = 5;

        // Debug FrmSplash Mode
        private bool debug = true;

        public FrmSplash(Action action = null, bool showButtons = true)
        {
            InitializeComponent();
            FrmSplashLocationChanged(null, null);
            _action = action;
            _showButtons = showButtons;
            Instance = this;
        }

        public static FrmSplash Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception(Language.FrmSplash_There_is_no_active_splash_screen_);
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        private void FrmSplashLoad(object sender, EventArgs e)
        {
            if (!debug)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
            Utility.ToggleTaskbar(false);
            if (!_showButtons)
            {
                _time = 0;
            }
            HiderTick(null, null);
        }

        private async void HiderTick(object sender, EventArgs e)
        {
            _time--;
            if (_time > 0)
            {
                lbl_message.Text = string.Format(Language.FrmSplash_Starting_in__0__seconds____, _time);
                if (!t_hider.Enabled)
                {
                    t_hider.Start();
                }
            }
            else
            {
                Cursor.Hide();
                lbl_message.Text = Language.FrmSplash_Please_wait____;
                t_hider.Stop();
                btn_options.Visible = false;
                btn_tools.Visible = false;
                if (_action != null)
                {
                    t_killer.Start();
                    try
                    {
                        await Dispatcher.CurrentDispatcher.BeginInvoke(_action, null);
                    }
                    catch
                    {
                        // ignored
                    }
                }
                Close();
            }
        }

        private void FrmSplashLocationChanged(object sender, EventArgs e)
        {            
            if (!debug) {
                Size = new Size(
                    Methods.GetSystemMetrics(SystemMetric.WidthVirtualScreen),
                    Methods.GetSystemMetrics(SystemMetric.HeightVirtualScreen)); // DEBUG ONLY
                Location = new Point(
                     Methods.GetSystemMetrics(SystemMetric.XVirtualScreen),
                     Methods.GetSystemMetrics(SystemMetric.YVirtualScreen));
            } else {
                Size = new Size(500, 200);
                Location = new Point(
                    Methods.GetSystemMetrics(SystemMetric.XVirtualScreen)/2,
                    Methods.GetSystemMetrics(SystemMetric.YVirtualScreen)/2);
            }
        }

        private void FrmSplashFormClosed(object sender, FormClosedEventArgs e)
        {
            t_hider.Stop();
            t_killer.Stop();
            Utility.ToggleTaskbar(true);
            Cursor.Show();
        }

        private void FrmSplashPaint(object sender, PaintEventArgs e)
        {
            FrmSplashLocationChanged(null, null);
        }

        private void FrmSplashKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape && t_hider.Enabled)
            {
                Close();
            }
        }

        private void KillerTick(object sender, EventArgs e)
        {
            lbl_message.Text = Language.FrmSplash_Failed__Closing____;
            Application.DoEvents();
            Thread.Sleep(5000);
            Close();
            Application.Exit();
        }

        private void BtnToolsClick(object sender, EventArgs e)
        {
            new FrmTools().Show();
            Close();
        }

        private void BtnOptionsClick(object sender, EventArgs e)
        {
            new FrmOptions().Show();
            Close();
        }
    }
}