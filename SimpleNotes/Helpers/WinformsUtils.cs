using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace SimpleNotes.Helpers
{
    public static class WinformsUtils
    {
        class Wpf32Window : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public Wpf32Window(Window wpfWindow)
                => Handle = new WindowInteropHelper(wpfWindow).Handle;
        }

        public static System.Windows.Forms.IWin32Window ToWinformsWindow(this Window parent) => new Wpf32Window(parent);

        public static FontInfo ToWpfFont(this Font winformsFont)
        {
            return new FontInfo()
            {
                FontFamily = new System.Windows.Media.FontFamily(winformsFont.Name),
                FontSize = winformsFont.Size * 96.0 / 72.0,
                FontWeight = winformsFont.Bold ? FontWeights.Bold : FontWeights.Regular,
                FontStyle = winformsFont.Italic ? FontStyles.Italic : FontStyles.Normal,
            };
        }

        public static Font ToWinformsFont(this FontInfo wpfFont)
        {
            System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
            if (wpfFont.FontStyle == FontStyles.Italic) style |= System.Drawing.FontStyle.Italic;
            if (wpfFont.FontWeight == FontWeights.Bold) style |= System.Drawing.FontStyle.Bold;
            return new Font(wpfFont.FontFamily.Source, (float)wpfFont.FontSize * 72.0f / 96.0f, style);
        }

        public class FontInfo
        {
            public System.Windows.Media.FontFamily FontFamily { get; set; }
            public double FontSize { get; set; }
            public System.Windows.FontWeight FontWeight { get; set; }
            public System.Windows.FontStyle FontStyle { get; set; }
        }
    }
}
