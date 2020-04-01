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
                Image img = new Bitmap(Properties.Resources.closebutton);
                Rectangle r = e.Bounds;
                r = GetTabRect(e.Index);
                r.Offset(2, 2);
                Brush TitleBrush = new SolidBrush(Color.Black);
                Font f = this.Font;
                string title = tab.Text;
                if (e.Index >= 1)
                {
                    e.Graphics.DrawImage(img, new Point(r.X, r.Y));
                }
                else
                {
                    r.Offset(-1 * img.Width, 0);
                }
                e.Graphics.DrawString(title, f, TitleBrush, new PointF(r.X + img.Width, r.Y));
            }
            catch (Exception) { }
        }

        private void Tabs_MouseClick(object sender, MouseEventArgs e)
        {
            Point p = e.Location;
            Rectangle r = GetTabRect(SelectedIndex);
            Image img = new Bitmap(Properties.Resources.closebutton);
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
