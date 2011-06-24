using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Management;

namespace AsusAudioHack
{
    public partial class Form1 : Form
    {
        private bool bExitClicked = false;

        private string registryPathStr;
        private string audioGuidStr;
        private string instancePathStr;
        private string registryKeyStr;

        public Form1()
        {
            InitializeComponent();
            this.label5.Text = 
                "Asus Audio Hack\n\n" + 
                "This system tray application allows you to " +
                "change the registry setting for the realtek " +
                "audio card in Asus G50/G60 laptops to get around\n" + 
                "the broken headphone jack problem plaguing " +
                "these laptops.  The audio device will " +
                "disable/enable to enable the change.";

            findAudioDevice();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }

        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            bExitClicked = true;
            this.Close();
        }

        private void Speakers_Click(object sender, EventArgs e)
        {
            enableSpeakersMode();
            this.Headphones.Checked = false;
//            this.notifyIcon1.Icon = new Icon(GetType(), "speakers.ico");
        }

        private void Headphones_Click(object sender, EventArgs e)
        {
            enableHeadphonesMode();
            this.Speakers.Checked = false;
//            this.notifyIcon1.Icon = new Icon(GetType(), "headphones.ico");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bExitClicked)
            {
                e.Cancel = true;
            }
            this.Hide();
            this.ShowInTaskbar = false;

        }

        private void enableHeadphonesMode()
        {
            this.notifyIcon1.BalloonTipText = "Please wait for audio device to restart.";
            this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            this.notifyIcon1.ShowBalloonTip(1); 
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey audioKey = hklm.OpenSubKey(registryPathStr, RegistryKeyPermissionCheck.ReadWriteSubTree);
            byte[] value = {00};
            audioKey.SetValue(registryKeyStr, value);
            audioKey.Close();
            EnableAudio(false);
            EnableAudio(true);
        }

        private void enableSpeakersMode()
        {
            this.notifyIcon1.BalloonTipText = "Please wait for audio device to restart.";
            this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            this.notifyIcon1.ShowBalloonTip(1);
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey audioKey = hklm.OpenSubKey(registryPathStr, RegistryKeyPermissionCheck.ReadWriteSubTree);
            byte[] value = { 255 };
            audioKey.SetValue(registryKeyStr, value);
            audioKey.Close();
            EnableAudio(false);
            EnableAudio(true);
        }

        private void getCurrentAudioMode()
        {
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey audioKey = hklm.OpenSubKey(registryPathStr, RegistryKeyPermissionCheck.ReadWriteSubTree);
            byte[] value = (byte[]) audioKey.GetValue(registryKeyStr);
            if(value[0] == 0) 
            {
                this.Headphones.Checked = true;
                this.Speakers.Checked = false;
            }
            else if(value[0] == 255)
            {
                this.Headphones.Checked = false;
                this.Speakers.Checked = true;
            }
        }

        private void EnableAudio(bool enable)
        {
            Guid audioGuid = new Guid(audioGuidStr);
            string instancePath = instancePathStr;
            DeviceHelper.SetDeviceEnabled(audioGuid, instancePath, enable);
        }

        private void About_Click(object sender, EventArgs e)
        {
            notifyIcon1_DoubleClick(sender, e);
        }

        private void findAudioDevice()
        {
            registryPathStr = "SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E96C-E325-11CE-BFC1-08002BE10318}\\0000\\Settings";
            registryKeyStr = "ForceDisableJD";
            audioGuidStr = "{4D36E96C-E325-11CE-BFC1-08002BE10318}";
            instancePathStr = @"HDAUDIO\FUNC_01&VEN_10EC&DEV_0663&SUBSYS_104319A3&REV_1000\4&10014B3F&0&0001";

            lblDeviceGuid.Text = audioGuidStr;
            lblDeviceInstance.Text = instancePathStr;
            lblRegistryPath.Text = "HKLM\\" + registryPathStr + "\\" + registryKeyStr;
            lblDeviceName.Text = "Realtek High Definition Audio";

            getCurrentAudioMode();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
            this.ShowInTaskbar = false;
        }

    }
}
