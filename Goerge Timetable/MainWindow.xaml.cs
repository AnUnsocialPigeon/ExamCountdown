using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Exam_Countdown {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string dir = Directory.GetCurrentDirectory() + @"\UserData.txt";
        private string dirDat = Directory.GetCurrentDirectory() + @"\Data.dat";
        private string[] FileData;
        private string[] ConfigFile;
        private string[] ConfigData;
        private List<string> Subjects = new List<string>();
        private List<DateTime> Dates = new List<DateTime>();

        private bool ShowHide { get; set; } = false;

        public MainWindow() {
            InitializeComponent();
            if (!File.Exists(dir))
                File.WriteAllLines(dir, new string[] {
                    "BackgroundURL: None",
                    "Background: 255,32,32,32",
                    "Foreground: 255,255,255,255",
                    "Button Background: 255,232,28,28",
                    "Button Foreground: 255,12,12,12",
                    "Font Size: 30",
                    "Exam 1 Name Here; 22/07/2022 13:41:00",
                    "Exam 2 Name Here; 23/07/2022 09:01:23"
                });
            if (!File.Exists(dirDat)) SaveConfigFile();

            try {
                ConfigFile = File.ReadAllLines(dirDat);
                ConfigData = ConfigFile.Select(x => x.Split(':')[1].Trim()).ToArray();
                ShowHide = ConfigData[0] != "False";
                Width = double.Parse(ConfigData[1]);
                Height = double.Parse(ConfigData[2]);
                Left = double.Parse(ConfigData[3]);
                Top = double.Parse(ConfigData[4]);
                MainTXTBlock.TextAlignment = (TextAlignment)int.Parse(ConfigData[5]);
            }
            catch {
                SaveConfigFile();
            }

            UpdateButtonVisability();

            FileData = File.ReadAllLines(dir);

            // Disecting
            string bgUrl = FileData[0].Split(':')[1].Trim();
            if (bgUrl == "None") {
                byte[] bg = FileData[1].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();
                Background = new SolidColorBrush(Color.FromArgb(bg[0], bg[1], bg[2], bg[3]));
            }
            else {
                MainGrid.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(bgUrl, UriKind.RelativeOrAbsolute)) };
            }

            byte[] fg = FileData[2].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();
            byte[] xb = FileData[3].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();
            byte[] xf = FileData[4].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();

            MainTXTBlock.Foreground = new SolidColorBrush(Color.FromArgb(fg[0], fg[1], fg[2], fg[3]));
            CloseBTN.Background = new SolidColorBrush(Color.FromArgb(xb[0], xb[1], xb[2], xb[3]));
            CloseBTN.Foreground = new SolidColorBrush(Color.FromArgb(xf[0], xf[1], xf[2], xf[3]));

            MainTXTBlock.FontSize = int.Parse(FileData[5].Split(':')[1].Trim());

            for (int line = 6; line < FileData.Length; line++) {
                Subjects.Add(FileData[line].Split(';')[0].Trim());
                Dates.Add(DateTime.Parse(FileData[line].Split(';')[1].Trim()));
            }
            Timer t = new Timer();
            t.Elapsed += TimerElapsedEvent;
            t.Interval = 1000;
            t.Start();
        }

        private void TimerElapsedEvent(object sender, ElapsedEventArgs e) {
            string Text = "";
            for (int i = 0; i < Dates.Count; i++) {
                TimeSpan d = Dates[i] - DateTime.Now;
                Text += $"{Subjects[i]}: {Math.Truncate(d.TotalDays)}d {Math.Truncate(d.TotalHours) % 24}h {Math.Truncate(d.TotalMinutes) % 60}m {Math.Truncate(d.TotalSeconds) % 60}s\n";
            }
            _ = Dispatcher.Invoke(() => MainTXTBlock.Text = Text);
        }

        private void CloseBTN_Click(object sender, RoutedEventArgs e) {
            SaveConfigFile();
            Environment.Exit(0);
            Close();
        }


        private void TextPosChange(object sender, RoutedEventArgs e) => MainTXTBlock.TextAlignment = (TextAlignment)int.Parse((sender as Button).Tag.ToString());

        private void ShowHideBTN_Click(object sender, RoutedEventArgs e) {
            ShowHide ^= true;
            UpdateButtonVisability();
            SaveConfigFile();
        }

        private void UpdateButtonVisability() {
            ShowHideBTN.Content = ShowHide ? "S" : "H";

            TextLeftBTN.IsEnabled = !ShowHide;
            TextCenterBTN.IsEnabled = !ShowHide;
            TextRightBTN.IsEnabled = !ShowHide;
            ResizeBTN.IsEnabled = !ShowHide;
            SaveBTN.IsEnabled = !ShowHide;
            CloseBTN.IsEnabled = !ShowHide;
            FileOpenerBTN.IsEnabled = !ShowHide;

            TextLeftBTN.Visibility = (Visibility)Convert.ToInt32(ShowHide);
            TextCenterBTN.Visibility = (Visibility)Convert.ToInt32(ShowHide);
            TextRightBTN.Visibility = (Visibility)Convert.ToInt32(ShowHide);
            ResizeBTN.Visibility = (Visibility)Convert.ToInt32(ShowHide);
            SaveBTN.Visibility = (Visibility)Convert.ToInt32(ShowHide);
            CloseBTN.Visibility = (Visibility)Convert.ToInt32(ShowHide);
            FileOpenerBTN.Visibility = (Visibility)Convert.ToInt32(ShowHide);
        }

        private void SaveConfigFile() {
            double windowLeft = Application.Current.MainWindow.Left == double.NaN ? 0 : Application.Current.MainWindow.Left;
            double windowTop = Application.Current.MainWindow.Top == double.NaN ? 0 : Application.Current.MainWindow.Top;

            File.WriteAllLines(dirDat, new string[] {
                $"Hidden: {ShowHide}",
                $"WindowWidth: {Width}",
                $"WindowHeight: {Height}",
                $"WindowLeft: {windowLeft}",
                $"WindowTop: {windowTop}",
                $"TextAllignment: {(int)MainTXTBlock.TextAlignment}"
            });
        }

        private void SaveBTN_Click(object sender, RoutedEventArgs e) => SaveConfigFile();



        #region moving
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //Attach this to the MouseDown event of your drag control to move the window in place of the title bar
        private void WindowDrag(object sender, MouseButtonEventArgs e) // MouseDown
        {
            ReleaseCapture();
            SendMessage(new WindowInteropHelper(this).Handle,
                0xA1, (IntPtr)0x2, (IntPtr)0);
        }

        //Attach this to the PreviewMousLeftButtonDown event of the grip control in the lower right corner of the form to resize the window
        private void WindowResize(object sender, MouseButtonEventArgs e) //PreviewMousLeftButtonDown
        {
            HwndSource hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)61448, IntPtr.Zero);
        }
        #endregion

        private void FileOpenerBTN_Click(object sender, RoutedEventArgs e) {
            try {
                using Process fileopener = new Process();

                fileopener.StartInfo.FileName = "notepad";
                fileopener.StartInfo.Arguments = dir;
                fileopener.Start();
            }
            catch { return; }
        }
    }
}
