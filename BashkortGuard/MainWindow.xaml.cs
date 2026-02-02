using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BashkortGuard
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

         
            string wgPath = @"C:\Program Files\WireGuard\wireguard.exe";
            if (!File.Exists(wgPath))
            {
                MessageBox.Show(
                    "WireGuard не установлен.\n\nСкачайте с https://www.wireguard.com/install",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

           
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            timer.Tick += (s, e) => UpdateStatus();
            timer.Start();

            UpdateStatus(); 
        }

     
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string confPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default.conf");

            if (!File.Exists(confPath))
            {
                MessageBox.Show("Файл default.conf не найден в папке программы!", "Ошибка");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\WireGuard\wireguard.exe",
                    Arguments = $"/installtunnelservice \"{confPath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(psi);
                string err = p.StandardError.ReadToEnd();
                p.WaitForExit();

                if (p.ExitCode == 0)
                    MessageBox.Show("Подключение выполнено", "Успех");
                else if (err.Contains("already installed and running"))
                    MessageBox.Show("VPN уже подключён", "Инфо");
                else
                    MessageBox.Show("Ошибка подключения:\n" + err, "Ошибка");

                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключить:\n" + ex.Message, "Ошибка");
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string confPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default.conf");
            string tunnelName = Path.GetFileNameWithoutExtension(confPath); // ← ключевое изменение

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\WireGuard\wireguard.exe",
                    Arguments = $"/uninstalltunnelservice \"{tunnelName}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(psi);
                string err = p.StandardError.ReadToEnd();
                p.WaitForExit();

                if (p.ExitCode == 0)
                {
                    MessageBox.Show("VPN успешно отключён", "Готово");
                }
                else if (err.Contains("does not exist as an installed service") ||
                         err.Contains("not installed") ||
                         err.Contains("no such service"))
                {
                    MessageBox.Show("VPN уже отключён или не был запущен", "Информация");
                }
                else
                {
                    MessageBox.Show("Ошибка при отключении:\n" + err, "Ошибка");
                }

                System.Threading.Thread.Sleep(1000);
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось отключить:\n" + ex.Message, "Ошибка");
            }
        }


        private bool IsTunnelActive()
        {
            string confPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default.conf");
            string tunnelName = Path.GetFileNameWithoutExtension(confPath);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\WireGuard\wg.exe",
                    Arguments = $"show {tunnelName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();

                if (error.Contains("interface not found") ||
                    error.Contains("No such device") ||
                    error.Contains("Unable to access interface"))
                    return false;

                return output.Contains("latest handshake:");
            }
            catch
            {
                return false;
            }
        }


        private void UpdateStatus()
        {
            bool connected = IsTunnelActive();

            Title = connected
                ? "BashkortGuard — Подключено"
                : "BashkortGuard — Отключено";

            
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}