﻿using ChimitAnsi;
using ChimitRCON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterRCON
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (Properties.Settings.Default.IsPotato.Equals(true))
            {
                MessageBox.Show("No potatoes allowed!");
            }
            if (Properties.Settings.Default.FirstTime.Equals(true))
            {
                MessageBox.Show("Welcome to BetterRCON!");
            }
            //var rcon = RCONClient.INSTANCE;
            //var answer = rcon.sendMessage(RCONMessageType.Command, "echo RCON Connection Established");
            // var answer2 = rcon.sendMessage(RCONMessageType.Command, "list");
            // note: dont remove the color codes anymore - we have a cool color richtext control
            //Output.AppendText(answer.RemoveColorCodes() + "\r\n");
            // Output.AppendText(answer2.RemoveColorCodes() + "\r\n");

            // ah yes just as in the simulations. I have a form
            // i can't open the designer
            IPTextBox.Text = Properties.Settings.Default.IP;
            PortTextBox.Text = Properties.Settings.Default.Port;
            PasswordTextBox.Text = Properties.Settings.Default.Password;
        }

        private void SaveBtn_Click(object sender, EventArgs e) // Saves doesn't send
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (Properties.Settings.Default.FirstTime.Equals(true))
            {
                Properties.Settings.Default.FirstTime = false;
                Directory.CreateDirectory(path + "\\BetterRCON");
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = path + "\\BetterRCON";
            dialog.CheckPathExists = true;
            dialog.ValidateNames = true;
            dialog.DefaultExt = ".rcon";
            dialog.Filter = "RCON|*.rcon";
            dialog.AddExtension = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                byte[] array = Serialize(new ConnectionData(IPTextBox.Text, int.Parse(PortTextBox.Text), PasswordTextBox.Text));
                File.WriteAllBytes(dialog.FileName, array);
                Properties.Settings.Default.HasConnected = true;
                Properties.Settings.Default.FirstTime = false;
                Properties.Settings.Default.IP = IPTextBox.Text;
                Properties.Settings.Default.Port = PortTextBox.Text;
                Properties.Settings.Default.Password = PasswordTextBox.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            dialog.CheckFileExists = true;
            dialog.ValidateNames = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ConnectionData data = Deserialize(File.ReadAllBytes(dialog.FileName));
                // I dont understand any of this
                IPTextBox.Text = data.IP;
                PortTextBox.Text = data.Port.ToString();
                PasswordTextBox.Text = data.Password;
            }
        }

        private void PortTextBox_TextChanged(object sender, EventArgs e)
        {
            // MessageBox.Show("Other ports are currently unsupported!");
        }

        private byte[] Serialize(ConnectionData thing)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, thing);
                return ms.ToArray();
            }
        }

        private static ConnectionData Deserialize(byte[] source)
        {
            using (var ms = new MemoryStream(source))
            {
                var formatter = new BinaryFormatter();
                return (ConnectionData)formatter.Deserialize(ms);
            }
        }

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            ConsoleTabPage tab = ConsoleTabPage.Clone(RCON, IPTextBox.Text);
            Tabs.TabPages.Add(tab);
            Tabs.SelectedIndex = Tabs.TabCount - 1;
            tab.Output.AppendText(AnsiOutput.cls());
            int x = Int32.Parse(PortTextBox.Text);
            if (!tab.RCONClient.setupStream(IPTextBox.Text, x, PasswordTextBox.Text, OtherRCON.RCONColorMode.ANSI))
            {
                tab.Output.AppendText(AnsiOutput.red("Error connecting.") + " IP settings incorrect?\n");
                return;
            }
            string answer = tab.RCONClient.sendMessage(OtherRCON.RCONMessageType.Command, "echo RCON Connection Established");
            if (String.IsNullOrEmpty(answer))
            {
                tab.Output.AppendText(AnsiOutput.red("Error connecting.") + " Password incorrect?\n");
                return;
            }
            string answer2 = tab.RCONClient.sendMessage(OtherRCON.RCONMessageType.Command, "list");
            tab.Output.AppendText(answer);
            tab.Output.AppendText(answer2);
            Properties.Settings.Default.HasConnected = true;
            Properties.Settings.Default.FirstTime = false;
            Properties.Settings.Default.IP = IPTextBox.Text;
            Properties.Settings.Default.Port = PortTextBox.Text;
            Properties.Settings.Default.Password = PasswordTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            IPTextBox.Text = "";
            PortTextBox.Text = "";
            PasswordTextBox.Text = "";
            Properties.Settings.Default.IP = IPTextBox.Text;
            Properties.Settings.Default.Port = PortTextBox.Text;
            Properties.Settings.Default.Password = PasswordTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            Tabs.TabPages.Remove(RCON);
            Tabs.SelectedIndex = 0;
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConsoleTabPage tab = Tabs.SelectedTab as ConsoleTabPage;
            if (null != tab)
            {
                tab.CMDInput.Focus();
            }
        }
    }
}
