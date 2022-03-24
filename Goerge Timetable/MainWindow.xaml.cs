using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace Goerge_Timetable {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string dir = Directory.GetCurrentDirectory() + @"\UserData.txt";
        private string[] FileData;
        private List<string> Subjects = new List<string>();
        private List<DateTime> Dates = new List<DateTime>();

        public MainWindow() {
            InitializeComponent();
            if (!System.IO.File.Exists(dir))
                System.IO.File.WriteAllLines(dir, new string[] {
                    "Background: 255,32,32,32",
                    "Foreground: 255,255,255,255",
                    "X Background: 255,232,28,28",
                    "X Foreground: 255,12,12,12",
                    "Exam 1 Name Here; 22/07/2022 13:41:00",
                    "Exam 2 Name Here; 23/07/2022 09:01:23"
                });

            FileData = File.ReadAllLines(dir);

            // Disecting
            byte[] bg = FileData[0].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();
            byte[] fg = FileData[1].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();
            byte[] xb = FileData[2].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();
            byte[] xf = FileData[3].Split(':')[1].Trim().Split(',').Select(byte.Parse).ToArray();

            Background = new SolidColorBrush(Color.FromArgb(bg[0], bg[1], bg[2], bg[3]));
            MainTXTBlock.Foreground = new SolidColorBrush(Color.FromArgb(fg[0], fg[1], fg[2], fg[3]));
            CloseBTN.Background = new SolidColorBrush(Color.FromArgb(xb[0], xb[1], xb[2], xb[3]));
            CloseBTN.Foreground = new SolidColorBrush(Color.FromArgb(xf[0], xf[1], xf[2], xf[3]));
            
            for (int line = 4; line < FileData.Length; line++) {
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

        private void Button_Click(object sender, RoutedEventArgs e) {
            Environment.Exit(0);
            Close();
        }
    }
}
