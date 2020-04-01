using ChimitAnsi;
using ChimitRCON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterRCON
{
    public class ConsoleTabPage : TabPage
    {

        public ConsoleTabPage() : base()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        public void SendBTN_Click(object sender, EventArgs e) // Sends doesn't save
        {
            string txt = CMDInput.Text;
            historyStrings.Add(txt);
            historyPointer = historyStrings.Count;
            var answer3 = RCONClient.sendMessage(OtherRCON.RCONMessageType.Command, txt);
            Output.AppendText(AnsiOutput.Reset()); // reset colors
            Output.AppendText(txt + "\n");
            Output.AppendText(answer3);
            CMDInput.Text = "";
        }

        public void CMDInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.W || e.KeyCode == Keys.F4 || e.KeyCode == Keys.D))
            {
                TabControl tc = Parent as TabControl;
                if (null != tc)
                {
                    tc.TabPages.Remove(this);
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (historyPointer > 0)
                {
                    historyPointer--;
                    CMDInput.Text = historyStrings[historyPointer];
                }
            }
            if (e.KeyCode == Keys.Down)
            {
                if (historyPointer < historyStrings.Count - 1)
                {
                    historyPointer++;
                    CMDInput.Text = historyStrings[historyPointer];
                }
                else
                {
                    CMDInput.Text = "";
                }
            }
            if (e.KeyCode == Keys.Enter)
            {
                SendBTN.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        public OtherRCON RCONClient = new OtherRCON();
        public int historyPointer = 0;
        public List<string> historyStrings = new List<string>();

        public Control GetControl(string name)
        {
            foreach (Control c in this.Controls)
            {
                if (c.Name == name)
                {
                    return c;
                }
            }
            return null;
        }
        
        public Button SendBTN
        {
            get
            {
                return (Button)GetControl("SendBTN");
            }
        }
        public TextBox CMDInput
        {
            get
            {
                return (TextBox)GetControl("CMDInput");
            }
        }

        public AnsiTextBox Output
        {
            get
            {
                return (AnsiTextBox)GetControl("Output");
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            RCONClient.Dispose();
        }

        public static ConsoleTabPage Clone(TabPage rCON, string name)
        {
            ConsoleTabPage tab = (ConsoleTabPage)ControlFactory.CloneCtrl(rCON);
            tab.Text = name + "        ";
            tab.SendBTN.Click += new System.EventHandler(tab.SendBTN_Click);
            tab.CMDInput.KeyDown += new KeyEventHandler(tab.CMDInput_KeyDown);
            return tab;
        }
    }
}
