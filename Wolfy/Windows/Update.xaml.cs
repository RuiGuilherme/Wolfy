﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wolfy.Classes;

namespace Wolfy.Windows {
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window {

        // ----------------| Variables |---------------- //
        private static WebClient Client;
        private static DispatcherTimer Timer;
        private static String LatestLink;

        // Time in Seconds
        private static int InstallTime = 5;

        public Update(String _LatestLink) {
            InitializeComponent();

            // Variables
            Client = new WebClient();
            Timer = new DispatcherTimer();
            LatestLink = _LatestLink;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Init text
            this.Title = String.Format(Langs.Get("update_downloading"), 0, 0, 0);
            InstallBtn.Content = String.Format(Langs.Get("update_install"), "..");
            CancelBtn.Content = Langs.Get("cancel");
            // Init
            Client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadFileCompleted);
            Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            InstallTime++;
            // Timer
            Timer.Tick += new EventHandler(ElapsedTime);
            Timer.Interval = new TimeSpan(0, 0, 1);
            // Download
            Uri _Uri = new Uri(LatestLink);
            Client.DownloadFileAsync(_Uri, Reference.TempUpdateZip);
        }

        #region Download

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            // Start timer (install)
            if (!e.Cancelled) {
                Timer.Start();
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            // Variables
            double _Received = double.Parse(e.BytesReceived.ToString());
            double _Total = double.Parse(e.TotalBytesToReceive.ToString());
            double _Percentage = _Received / _Total * 100;
            // Show
            this.Title = String.Format(Langs.Get("update_downloading"), Utils.BytesToString(e.BytesReceived), Utils.BytesToString(e.TotalBytesToReceive), Math.Truncate(_Percentage));
            ProgressBar.Value = int.Parse(Math.Truncate(_Percentage).ToString());
        }

        private void ElapsedTime(object sender, EventArgs e) {
            // Text
            InstallTime--;
            InstallBtn.Content = String.Format(Langs.Get("update_install"), InstallTime);
            // Install
            Install();
        }

        #endregion

        #region Window

        private void InstallBtn_Click(object sender, RoutedEventArgs e) {
            // Force install
            InstallTime = 0;
            Install();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e) {
            Client.CancelAsync();
            Utils.RestartApplication();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Utils.RestartApplication();
        }

        #endregion

        private void Install() {
            if (InstallTime == 0) {
                // Stop timer
                Timer.Stop();

                try {
                    // Extract files
                    ZipFile.ExtractToDirectory(Reference.TempUpdateZip, Reference.TempUpdatePath);
                    // Start install
                    if (File.Exists(Reference.TempUpdateFile)) {

                        Process _Update = new Process();
                        _Update.StartInfo.FileName = Reference.TempUpdateFile;
                        _Update.StartInfo.Arguments = String.Format(" /sp- /silent /nocancel /norestart /closeapplications /restartapplications /dir=\"{0}\"", Reference.AppPath);
                        _Update.Start();

                    }
                    else {
                        MessageBox.Show(Langs.Get("update_install_error"), Reference.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                        Utils.CloseApplication();
                    }
                }
                catch {
                    MessageBox.Show(Langs.Get("update_install_error"), Reference.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally {
                    Utils.CloseApplication();
                }
            }
        }

        // Translation event
        public void OnTranslation() {
        }
    }
}
