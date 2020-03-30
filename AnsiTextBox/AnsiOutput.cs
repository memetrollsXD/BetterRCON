using System;

namespace BetterRCON
{
    public class AnsiOutput
    {

        public enum ColorCode
        {
            FG_BLACK = 30,
            FG_RED = 31,
            FG_GREEN = 32,
            FG_YELLOW = 33,
            FG_BLUE = 34,
            FG_MAGENTA = 35,
            FG_CYAN = 36,
            FG_WHITE = 37,
            FG_DEFAULT = 39,
            BG_BLACK = 40,
            BG_RED = 41,
            BG_GREEN = 42,
            BG_YELLOW = 43,
            BG_BLUE = 44,
            BG_MAGENTA = 45,
            BG_CYAN = 46,
            BG_WHITE = 47,
            BG_DEFAULT = 49
        };

        public static string cls()
        {
            string cls_code = "\x1b[2J\x1b[1;1H"; // todo: reset?
            return cls_code + Reset();
        }

        public static string color(ColorCode code, string str)
        {
            string res = String.Format("\x1b[{0}m", (int)code);
            res += Reset();
            return res;
        }

        public static string Reset()
        {
            return "\u001b[0m";
        }

        public static string red(string str)
        {
            return color(ColorCode.FG_RED, str);
        }

        public static string green(string str)
        {
            return color(ColorCode.FG_GREEN, str);
        }

        public static string blue(string str)
        {
            return color(ColorCode.FG_BLUE, str);
        }

        public static string yellow(string str)
        {
            return color(ColorCode.FG_YELLOW, str);
        }

        public static string red(long num)
        {
            return red(String.Format("{0}", num));
        }

        public static string green(long num)
        {
            return green(String.Format("{0}", num));
        }

        public static string blue(long num)
        {
            return blue(String.Format("{0}", num));
        }

        public static string yellow(long num)
        {
            return yellow(String.Format("{0}", num));
        }

        public static string defc(string str)
        {
            return color(ColorCode.FG_DEFAULT, str);
        }

        public static string defc(long num)
        {
            return defc(String.Format("{0}", num));
        }

        public static string SetTitle(string title)
        {
            return String.Format("", "\x1b]2;{0}\007", title);
        }
    }
}