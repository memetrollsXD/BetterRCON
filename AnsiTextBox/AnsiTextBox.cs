using libVT100;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BetterRCON
{
    public class AnsiTextBox : RichTextBox, IAnsiDecoderClient
    {
        public AnsiTextBox() : base()
        {
            vt100 = new AnsiDecoder();
            screenS = new ScreenStream();
            screenS.InjectTo = vt100;
            vt100.Encoding = System.Text.Encoding.GetEncoding("ASCII");
            vt100.Subscribe(this);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            currentForegroundColor = ForeColor;
            currentBackgroundColor = BackColor;
        }

        public new void AppendText(string str)
        {
            /*
            byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(str);
            */
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; ++i)
            {
                bytes[i] = (byte)str[i];
            }
            screenS.Write(bytes, 0, bytes.Length);
        }

        public void SetTab(IAnsiDecoder _sender)
        {
        }

        public void ClearTab(IAnsiDecoder _sender, bool ClearAll)
        {
        }

        public void Characters(IAnsiDecoder _sender, char[] _chars)
        {
            if (null == currentForegroundColor)
            {
                currentForegroundColor = ForeColor;
            }
            if (null == currentBackgroundColor)
            {
                currentBackgroundColor = BackColor;
            }
            SelectionStart = TextLength;
            SelectionLength = 0;
            SelectionColor = currentForegroundColor.GetValueOrDefault();
            SelectionBackColor = currentBackgroundColor.GetValueOrDefault();
            base.AppendText(new string(_chars));
        }

        public void SaveCursor(IAnsiDecoder _sernder)
        {
        }

        public void RestoreCursor(IAnsiDecoder _sender)
        {
        }

        public Size GetSize(IAnsiDecoder _sender)
        {
            return this.Size;
        }

        public void MoveCursor(IAnsiDecoder _sender, Direction _direction, int _amount, bool scroll)
        {
        }

        public void MoveCursorToBeginningOfLineBelow(IAnsiDecoder _sender, int _lineNumberRelativeToCurrentLine, bool scroll)
        {
        }

        public void MoveCursorToBeginningOfLineAbove(IAnsiDecoder _sender, int _lineNumberRelativeToCurrentLine, bool scroll)
        {
        }

        public void MoveCursorToColumn(IAnsiDecoder _sender, int _columnNumber)
        {
        }

        public void MoveCursorTo(IAnsiDecoder _sender, Point _position)
        {
        }

        public void ClearScreen(IAnsiDecoder _sender, ClearDirection _direction)
        {
            this.Text = "";
        }

        public void ClearLine(IAnsiDecoder _sender, ClearDirection _direction)
        {
        }

        public void ScrollPageUpwards(IAnsiDecoder _sender, int _linesToScroll)
        {
        }

        public void ScrollPageDownwards(IAnsiDecoder _sender, int _linesToScroll)
        {
        }

        public Point GetCursorPosition(IAnsiDecoder _sender)
        {
            return new Point(0, 0); // todo
        }

        public void SetGraphicRendition(IAnsiDecoder _sender, GraphicRendition[] _commands)
        {
            foreach (GraphicRendition g in _commands)
            {
                switch (g)
                {
                    /// all attributes off
                    case GraphicRendition.Reset:
                        break;
                    /// Intensity: Bold
                    case GraphicRendition.Bold:
                        break;
                    /// Intensity: Faint     not widely supported
                    case GraphicRendition.Faint:
                        break;
                    /// Italic: on     not widely supported. Sometimes treated as inverse.
                    case GraphicRendition.Italic:
                        break;
                    /// Underline: Single     not widely supported
                    case GraphicRendition.Underline:
                        break;
                    /// Blink: Slow     less than 150 per minute
                    case GraphicRendition.BlinkSlow:
                        break;
                    /// Blink: Rapid     MS-DOS ANSI.SYS; 150 per minute or more
                    case GraphicRendition.BlinkRapid:
                        break;
                    /// Image: Negative     inverse or reverse; swap foreground and background
                    case GraphicRendition.Inverse:
                        break;
                    /// Conceal     not widely supported
                    case GraphicRendition.Conceal:
                        break;
                    /// Font selection (not sure which)
                    case GraphicRendition.Font1:
                        break;
                    /// Underline: Double
                    case GraphicRendition.UnderlineDouble:
                        break;
                    /// Intensity: Normal     not bold and not faint
                    case GraphicRendition.NormalIntensity:
                        break;
                    /// Underline: None     
                    case GraphicRendition.NoUnderline:
                        break;
                    /// Blink: off     
                    case GraphicRendition.NoBlink:
                        break;
                    /// Image: Positive
                    ///
                    /// Not sure what this is supposed to be, the opposite of inverse???
                    case GraphicRendition.Positive:
                        break;
                    /// Reveal,     conceal off
                    case GraphicRendition.Reveal:
                        break;
                    /// Set foreground color, normal intensity
                    case GraphicRendition.ForegroundNormalBlack:
                        currentForegroundColor = Color.Black;
                        break;
                    case GraphicRendition.ForegroundNormalRed:
                        currentForegroundColor = Color.Red;
                        break;
                    case GraphicRendition.ForegroundNormalGreen:
                        currentForegroundColor = Color.Green;
                        break;
                    case GraphicRendition.ForegroundNormalYellow:
                        currentForegroundColor = Color.Yellow;
                        break;
                    case GraphicRendition.ForegroundNormalBlue:
                        currentForegroundColor = Color.Blue;
                        break;
                    case GraphicRendition.ForegroundNormalMagenta:
                        currentForegroundColor = Color.Magenta;
                        break;
                    case GraphicRendition.ForegroundNormalCyan:
                        currentForegroundColor = Color.Cyan;
                        break;
                    case GraphicRendition.ForegroundNormalWhite:
                        currentForegroundColor = Color.White;
                        break;
                    case GraphicRendition.ForegroundNormalReset:
                        currentForegroundColor = ForeColor;
                        break;
                    /// Set background color, normal intensity
                    case GraphicRendition.BackgroundNormalBlack:
                        currentBackgroundColor = Color.Black;
                        break;
                    case GraphicRendition.BackgroundNormalRed:
                        currentBackgroundColor = Color.Red;
                        break;
                    case GraphicRendition.BackgroundNormalGreen:
                        currentBackgroundColor = Color.Green;
                        break;
                    case GraphicRendition.BackgroundNormalYellow:
                        currentBackgroundColor = Color.Yellow;
                        break;
                    case GraphicRendition.BackgroundNormalBlue:
                        currentBackgroundColor = Color.Blue;
                        break;
                    case GraphicRendition.BackgroundNormalMagenta:
                        currentBackgroundColor = Color.Magenta;
                        break;
                    case GraphicRendition.BackgroundNormalCyan:
                        currentBackgroundColor = Color.Cyan;
                        break;
                    case GraphicRendition.BackgroundNormalWhite:
                        currentBackgroundColor = Color.White;
                        break;
                    case GraphicRendition.BackgroundNormalReset:
                        currentBackgroundColor = BackColor;
                        break;
                    /// Set foreground color, high intensity (aixtem)
                    case GraphicRendition.ForegroundBrightBlack:
                        break;
                    case GraphicRendition.ForegroundBrightRed:
                        break;
                    case GraphicRendition.ForegroundBrightGreen:
                        break;
                    case GraphicRendition.ForegroundBrightYellow:
                        break;
                    case GraphicRendition.ForegroundBrightBlue:
                        break;
                    case GraphicRendition.ForegroundBrightMagenta:
                        break;
                    case GraphicRendition.ForegroundBrightCyan:
                        break;
                    case GraphicRendition.ForegroundBrightWhite:
                        break;
                    case GraphicRendition.ForegroundBrightReset:
                        break;
                    /// Set background color, high intensity (aixterm)
                    case GraphicRendition.BackgroundBrightBlack:
                        break;
                    case GraphicRendition.BackgroundBrightRed:
                        break;
                    case GraphicRendition.BackgroundBrightGreen:
                        break;
                    case GraphicRendition.BackgroundBrightYellow:
                        break;
                    case GraphicRendition.BackgroundBrightBlue:
                        break;
                    case GraphicRendition.BackgroundBrightMagenta:
                        break;
                    case GraphicRendition.BackgroundBrightCyan:
                        break;
                    case GraphicRendition.BackgroundBrightWhite:
                        break;
                    case GraphicRendition.BackgroundBrightReset:
                        break;
                }
            }
        }

        public void ModeChanged(IAnsiDecoder _sender, AnsiMode _mode)
        {
        }

        public void SetProperty(IAnsiDecoder _sender, PropertyTypes type, string value)
        {
        }

        private IAnsiDecoder vt100;
        private ScreenStream screenS;
        private Color? currentBackgroundColor = null;
        private Color? currentForegroundColor = null;
    }
}