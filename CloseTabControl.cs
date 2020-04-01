using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterRCON
{
    public class CloseTabControl : TabControl
    {
        public CloseTabControl()
        {
            DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Tabs_DrawItem);
            MouseClick += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseClick);
        }

        private void Tabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            Point _imageLocation = new Point(0, 0);
            try
            {
                TabPage tab = TabPages[e.Index];
                Image closeicon = new Bitmap(Properties.Resources.closebutton);
                Rectangle r = GetTabRect(e.Index);
                // r will be 
                r.Offset(2, 2);
                Brush TitleBrush = new SolidBrush(Color.Black);
                Font f = tab.Font;
                string title = tab.Text;
                if (e.Index >= 1) // only on the second or more tab
                {
                    // we paint the close icon on the tab title here
                    e.Graphics.DrawImage(closeicon, new Point(r.X + r.Width - closeicon.Width - 5, r.Y));
                }
                // here, we draw the title. Since the control is in OwnerDrawFixed mode, we need to draw it ourselves.
                // The control doesn't do it automatically anymore.
                e.Graphics.DrawString(title, f, TitleBrush, new PointF(r.X, r.Y));
            }
            catch (Exception) { }
        }

        private void Tabs_MouseClick(object sender, MouseEventArgs e)
        {
            Point p = e.Location;
            Rectangle r = GetTabRect(SelectedIndex);
            Image img = new Bitmap(Properties.Resources.closebutton);
            r.X = r.X + r.Width - img.Width - 5;
            r.Width = img.Width;
            r.Height = img.Height;
            if (SelectedIndex >= 1)
            {
                if (r.Contains(p))
                {
                    TabPage TabP = (TabPage)TabPages[SelectedIndex];
                    TabPages.Remove(TabP);
                }
            }
        }
    }
}
