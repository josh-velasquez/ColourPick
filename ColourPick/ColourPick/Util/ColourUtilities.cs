using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ColourPick.Util
{
    internal class ColourUtilities
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);

        public Color GetPixel(Point position)
        {
            using (var bitmap = new Bitmap(1, 1))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(position, new Point(0, 0), new Size(1, 1));
                }
                return bitmap.GetPixel(0, 0);
            }
        }

        public Color SetCursorPosition()
        {
            Point pt = new Point();
            GetCursorPos(ref pt);
            return GetPixel(pt);
        }

        public Color GetRGB()
        {
            return SetCursorPosition();
        }

        public string GetHex()
        {
            return "#" + SetCursorPosition().R.ToString("X2") + SetCursorPosition().G.ToString("X2") + SetCursorPosition().B.ToString("X2");
        }

        public string GetPos()
        {
            Point pt = new Point();
            GetCursorPos(ref pt);
            // Primary screen returns the number of pixels of the monitor (so that the y coordinates start from bottom left of the window)
            return "X: " + pt.X.ToString() + " Y: " + (Screen.PrimaryScreen.Bounds.Height - pt.Y).ToString();
        }

        public string CheckColor(Color color)
        {
            var predefined = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var match = (from p in predefined where ((Color)p.GetValue(null, null)).ToArgb() == color.ToArgb() select (Color)p.GetValue(null, null));
            if (match.Any())
                return match.First().Name;
            return String.Empty;
        }

        public static System.Windows.Media.Brush ConvertHexToBrush(string hexValue)
        {
            var converter = new BrushConverter();
            var brush = (System.Windows.Media.Brush)converter.ConvertFromString(hexValue);
            return brush;
        }

        /// <summary>
        /// Gets the actual colour name of the colour
        /// </summary>
        /// <returns></returns>
        public string GetColorName()
        {
            return CheckColor(SetCursorPosition());
        }
    }
}