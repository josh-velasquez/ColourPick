using ColourPick.Util;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace ColourPick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            colourTimer = new DispatcherTimer();
            colourTimer.Tick += ColorTimerTick;
            colourTimer.Interval = TimeSpan.FromMilliseconds(1);
            MainWindow.MouseAction += new EventHandler(Event);
        }

        private DispatcherTimer colourTimer;
        private ColourUtilities colour = new ColourUtilities();

        private void ColorTimerTick(object sender, EventArgs e)
        {
            InsertItems();
        }

        private void Event(object sender, EventArgs e)
        {
            InsertItems();
        }

        private void InsertItems()
        {
            rgbListBox.Items.Insert(0, colour.GetRGB());
            hexListBox.Items.Insert(0, colour.GetHex());
            positionListBox.Items.Insert(0, colour.GetPos());
            UpdateShowColour(colour.GetHex());
        }

        #region Custom item background

        //private void InsertItems()
        //{
        //    rgbListBox.Items.Insert(0, new ListBoxItem { Content = colour.GetRGB(), Background = GetRGBColour(colour.GetRGB()), Foreground = GetForeground(colour.GetRGB()) });
        //    hexListBox.Items.Insert(0, new ListBoxItem { Content = colour.GetHex(), Background = GetRGBColour(colour.GetRGB()), Foreground = GetForeground(colour.GetRGB()) });
        //    positionListBox.Items.Insert(0, colour.GetPos());
        //}

        //private SolidColorBrush GetForeground(System.Drawing.Color colour)
        //{
        //    return new SolidColorBrush(Color.FromArgb(255, colour.R, colour.G, colour.B));
        //}

        //private SolidColorBrush GetRGBColour(System.Drawing.Color colour)
        //{
        //    return new SolidColorBrush(Color.FromArgb(255, colour.R, colour.G, colour.B));
        //}

        #endregion Custom item background

        private void OnColourClick(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);   // Stop any instance first
            clickButton.IsEnabled = false;
            this.Topmost = true;
            clearButton.IsEnabled = false;
            hoverButton.IsEnabled = false;
            _hookID = SetHook(_proc);
        }

        private void OnHoverClick(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            hoverButton.IsEnabled = false;
            this.Topmost = true;
            clearButton.IsEnabled = false;
            clickButton.IsEnabled = false;
            colourTimer.Start();
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            clearButton.IsEnabled = true;
            if (rgbListBox.Items.Count > 0)
            {
                rgbListBox.Items.RemoveAt(0);
                hexListBox.Items.RemoveAt(0);
                positionListBox.Items.RemoveAt(0);
            }
            try
            {
                colourTimer.Stop();
                UnhookWindowsHookEx(_hookID);
            }
            catch (Exception)
            {
            }

            hoverButton.IsEnabled = true;
            clickButton.IsEnabled = true;
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            rgbListBox.Items.Clear();
            hexListBox.Items.Clear();
            positionListBox.Items.Clear();
            ShowColourTextBox.Text = "#FFF";
            UpdateShowColour("#FFF");
        }

        #region Mouse Handler

        public static event EventHandler MouseAction = delegate { };

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
                if (hook == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
                return hook;
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
          int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                MouseAction(null, new EventArgs());
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
          LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
          IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private void OnShowColour(object sender, RoutedEventArgs e)
        {
            var hexColour = ShowColourTextBox.Text;
            UpdateShowColour(hexColour);
        }

        private void UpdateShowColour(string hexValue)
        {
            colourPanel.Fill = ColourUtilities.ConvertHexToBrush(hexValue);
        }
    }

    #endregion Mouse Handler
}