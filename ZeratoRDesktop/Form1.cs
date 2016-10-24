using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using Microsoft.Win32;

namespace ZeratoRDesktop
{
    public partial class Form1 : Form
    {
        private IniFile mConfigFile;
        private System.Timers.Timer mTimer = new System.Timers.Timer(60000 * 60);
        private BackgroundWorker mWorker = new BackgroundWorker();
        RegistryKey mRegistery = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public Form1()
        {
            InitializeComponent();

            if (Program.FROM_STARTUP) Hide();

            Text = "ZeratoR Desktop v" + Program.VERSION;

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;

            // Is in startup?
            launchOnRebootCheckBox.Checked = mRegistery.GetValue("ZeratoRDesktop") != null;

            mConfigFile = new IniFile("Config.ini");
            bool AutoSwitch = false;
            if (!mConfigFile.KeyExists("auto", "BackgroundSwitcher"))
            {
                AutoSwitch = false;
                mConfigFile.Write("auto", "false", "BackgroundSwitcher");
            }
            else
            {
                AutoSwitch = mConfigFile.Read("auto", "BackgroundSwitcher") == "true";
            }

            mWorker.DoWork += new DoWorkEventHandler(updateBackgroundDelegate);

            autoCheckBox.Checked = AutoSwitch;
            update_Timer();
            if (autoCheckBox.Checked) updateBackground();
        }

        private void updateNextBackgroundUpdate(double nextUpdate)
        {
            notifyIcon1.Text = "ZeratoR Desktop - Prochaine MàJ: " + DateTime.Now.AddSeconds(nextUpdate);
        }

        private void start_Timer()
        {
            updateNextBackgroundUpdate(60 * 60);
            mTimer.AutoReset = true;
            mTimer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            mTimer.Start();
        }

        private void update_Timer()
        {
            if (autoCheckBox.Checked)
            {
                start_Timer();
            }
            else
            {
                mTimer.Stop();
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            updateNextBackgroundUpdate(60 * 60);
            updateBackground();
        }

        private void updateBackgroundDelegate(object o, DoWorkEventArgs args)
        {
            Action a = () => changeBackgroundButton.Enabled = false;
            changeBackgroundButton.Invoke(a);
            try
            {
                String URL = BackgroundManager.getBackgroundURL();
                Image background = BackgroundManager.downloadBackground(URL);
                BackgroundManager.saveBackground(background);
                BackgroundManager.setBackground(background, BackgroundManager.PicturePosition.Fill);
            }
            catch (Exception e)
            {
                if (Visible)
                {
                    MessageBox.Show("Le background n'a pas pu être mis à jour.\nErreur: " + e.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                }
            }
            a = () => changeBackgroundButton.Enabled = true;
            changeBackgroundButton.Invoke(a);
        }

        private void updateBackground()
        {
            if (mWorker.IsBusy)
            {
                MessageBox.Show("Le background est déjà en cours de téléchargement.",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                return;
            }
            mWorker.RunWorkerAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            updateBackground();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://twitter.com/AlexMog_FR");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://zerator.tv/");
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState) Hide();
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void autoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            update_Timer();
        }

        private void launchOnRebootCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (launchOnRebootCheckBox.Checked)
            {
                mRegistery.SetValue("ZeratoRDesktop", "\"" + Application.ExecutablePath + "\" startup");
            }
            else
            {
                mRegistery.DeleteValue("ZeratoRDesktop");
            }
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
