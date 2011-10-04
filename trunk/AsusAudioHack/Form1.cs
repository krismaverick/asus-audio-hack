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

        private static string NOTFOUND = "not found";
        private string registryPathStr = NOTFOUND;
        private string audioGuidStr = NOTFOUND;
        private string instancePathStr = NOTFOUND;
        private string registryKeyStr = NOTFOUND;

        public Form1()
        {
            InitializeComponent();
            lblRegistryPath.Text = NOTFOUND;
            lblDeviceName.Text = NOTFOUND;                        
            this.label5.Text = 
                "Asus Audio Hack v1.0.1\n\n" + 
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
            if (!registryPathStr.Equals(NOTFOUND))
            {
                this.notifyIcon1.BalloonTipText = "Please wait for audio device to restart.";
                this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                this.notifyIcon1.ShowBalloonTip(1);
                RegistryKey hklm = Registry.LocalMachine;
                RegistryKey audioKey = hklm.OpenSubKey(registryPathStr, RegistryKeyPermissionCheck.ReadWriteSubTree);
                byte[] value = { 00 };
                audioKey.SetValue(registryKeyStr, value);
                audioKey.Close();
                try
                {
                    EnableAudio(false);
                    EnableAudio(true);
                }
                catch (Exception e)
                {
                    this.notifyIcon1.BalloonTipText = "Device failed to restart: " + e.Message;
                    this.notifyIcon1.ShowBalloonTip(1);
                }
            }
        }

        private void enableSpeakersMode()
        {
            if (!registryPathStr.Equals(NOTFOUND))
            {
                this.notifyIcon1.BalloonTipText = "Please wait for audio device to restart.";
                this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                this.notifyIcon1.ShowBalloonTip(1);
                RegistryKey hklm = Registry.LocalMachine;
                RegistryKey audioKey = hklm.OpenSubKey(registryPathStr, RegistryKeyPermissionCheck.ReadWriteSubTree);
                byte[] value = { 255 };
                audioKey.SetValue(registryKeyStr, value);
                audioKey.Close();
                try
                {
                    EnableAudio(false);
                    EnableAudio(true);
                }
                catch (Exception e)
                {
                    this.notifyIcon1.BalloonTipText = "Device failed to restart: " + e.Message;
                    this.notifyIcon1.ShowBalloonTip(1);
                }

            }
        }

        private void getCurrentAudioMode()
        {
            if (!registryPathStr.Equals(NOTFOUND))
            {
                RegistryKey hklm = Registry.LocalMachine;
                RegistryKey audioKey = hklm.OpenSubKey(registryPathStr, RegistryKeyPermissionCheck.ReadWriteSubTree);
                byte[] value = (byte[])audioKey.GetValue(registryKeyStr);
                if (value[0] == 0)
                {
                    this.Headphones.Checked = true;
                    this.Speakers.Checked = false;
                }
                else if (value[0] == 255)
                {
                    this.Headphones.Checked = false;
                    this.Speakers.Checked = true;
                }
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
            bool bFound = false;
            registryKeyStr = "ForceDisableJD";

            try
            {
                string[] searchVals = { "{4D36E96C-E325-11CE-BFC1-08002BE10318}" };
                foreach (string searchVal in searchVals)
                {

                    RegistryKey hklm = Registry.LocalMachine;
                    RegistryKey deviceKey = hklm.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\" + searchVal.ToUpper());
                    string[] deviceSubKeys = deviceKey.GetSubKeyNames();

                    foreach (string deviceStr in deviceSubKeys)
                    {
                        RegistryKey checkDeviceKey = deviceKey.OpenSubKey(deviceStr);
                        RegistryKey settingsKey = checkDeviceKey.OpenSubKey("Settings");
                        if (settingsKey != null)
                        {
                            string driverDesc = (string)checkDeviceKey.GetValue("DriverDesc");

                            RegistryValueKind regValKind = settingsKey.GetValueKind(registryKeyStr);  //try to find the ForceDisableJD key, if I find it, then this is the right device.
                            if (regValKind == RegistryValueKind.Binary)
                            {
                                instancePathStr = findInstancePath(driverDesc);
                                if (instancePathStr != null)
                                {
                                    registryPathStr = "SYSTEM\\CurrentControlSet\\Control\\Class\\" + searchVal + "\\" + deviceStr + "\\Settings";

                                    audioGuidStr = searchVal;

                                    lblRegistryPath.Text = "HKLM\\" + registryPathStr + "\\" + registryKeyStr;
                                    lblDeviceName.Text = driverDesc;
                                    lblDeviceGuid.Text = searchVal;
                                    lblDeviceInstance.Text = instancePathStr;
                                    bFound = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (bFound) break;
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
               //do nothing
            }

            getCurrentAudioMode();
        }

        private string findInstancePath(string deviceDesc)
        {
            string instancePathStr = null;

            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey hdaudioKey = hklm.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\HDAUDIO");
            string[] audioDevStrs = hdaudioKey.GetSubKeyNames();
            foreach (string audioDevStr in audioDevStrs)
            {
                RegistryKey hdaudioInstanceKey = hdaudioKey.OpenSubKey(audioDevStr);
                string[] audioSubDevStrs = hdaudioInstanceKey.GetSubKeyNames();
                foreach (string audioSubDevStr in audioSubDevStrs)
                {
                    RegistryKey hdaudioSubKey = hdaudioInstanceKey.OpenSubKey(audioSubDevStr);
                    string searchDeviceDesc = (string) hdaudioSubKey.GetValue("DeviceDesc");
                    if (searchDeviceDesc.Equals(deviceDesc))
                    {
                        instancePathStr = @"HDAUDIO\" + audioDevStr.ToUpper() + @"\" + audioSubDevStr.ToUpper();
                        //instancePathStr = @"HDAUDIO\FUNC_01&VEN_10EC&DEV_0663&SUBSYS_104319A3&REV_1000\4&10014B3F&0&0001";
                        return instancePathStr;
                    }
                }

            }



            return instancePathStr;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
            this.ShowInTaskbar = false;
        }

    }
}
