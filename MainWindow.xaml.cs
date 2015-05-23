using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace NWShelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // показывать окно справки по нажатию на F1
            CommandBinding helpBinding = new CommandBinding(ApplicationCommands.Help);
            helpBinding.Executed += f1pressed;
            CommandBindings.Add(helpBinding);

            // поиск установленного .NET Framework
            if (string.IsNullOrEmpty(Properties.Settings.Default.netPath_default))
            {
                RegistryKey installed_versions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
                string[] version_names = installed_versions.GetSubKeyNames();

                string netPath = string.Empty;
                foreach (string name in version_names)
                {
                    RegistryKey client = installed_versions.OpenSubKey(name).OpenSubKey("Client");
                    if (client == null)
                    {
                        netPath = installed_versions.OpenSubKey(name).GetValue("InstallPath", 0).ToString();
                        if (netPath != "0") { cb_netPath.Items.Add(netPath); }
                    }
                    else
                    {
                        netPath = client.GetValue("InstallPath", 0).ToString();
                        if (netPath != "0") { cb_netPath.Items.Add(netPath); }
                    }
                }
            }
            else // если в конфиге уже есть прописанный, то не искать, а взять его
            {
                cb_netPath.Items.Add(Properties.Settings.Default.netPath_default);
                cb_netPath.SelectedIndex = 0;
            }

            if (cb_netPath.Items.Count == 0)
            {
                addRecord2log("Couldn't find path to the installed .NET Framework. Set it in the .config file (variable netPath_default) and restart the application");
            }
        }

        private void f1pressed(object sender, ExecutedRoutedEventArgs e)
        {
            HelpWindow hlp = new HelpWindow();
            hlp.ShowDialog();
        }

        private void check_status(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_serviceName.Text.Trim()))
            {
                addRecord2log("You did not set the name of the service");
                return;
            }

            using (ServiceController sc = new ServiceController(tb_serviceName.Text.Trim()))
            {
                try
                {
                    addRecord2log(sc.Status.ToString());
                }
                catch (Exception ex)
                {
                    addRecord2log(ex.Message);
                }
            }
        }

        private void addRecord2log(string record)
        {
            log.Text += string.Format(
                "[{0}] {1}{2}-----{2}",
                DateTime.Now.ToString(),
                record,
                Environment.NewLine
                );
            log.ScrollToEnd();
        }

        private void setPath2service(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Service executable (*.exe)|*.exe";
            if ((bool)dialog.ShowDialog(this)) { tb_servicePath.Text = dialog.FileName; }
        }

        private void serviceName_changed(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tb_serviceName.Text.Trim()))
            {
                btn_status.Foreground = (Brush)new BrushConverter().ConvertFrom("Blue");
                btn_start.Foreground = (Brush)new BrushConverter().ConvertFrom("Green");
                btn_stop.Foreground = (Brush)new BrushConverter().ConvertFrom("Red");

                btn_status.IsEnabled = true;
                btn_start.IsEnabled = true;
                btn_stop.IsEnabled = true;
            }
            else
            {
                btn_status.Foreground = (Brush)new BrushConverter().ConvertFrom("Black");
                btn_start.Foreground = (Brush)new BrushConverter().ConvertFrom("Black");
                btn_stop.Foreground = (Brush)new BrushConverter().ConvertFrom("Black");

                btn_status.IsEnabled = false;
                btn_start.IsEnabled = false;
                btn_stop.IsEnabled = false;
            }
        }

        private void installService(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_servicePath.Text.Trim()))
            {
                addRecord2log("You did not set the path to the service's executable");
                return;
            }

            if (cb_netPath.SelectedValue == null)
            {
                addRecord2log("You did not set the path to the .NET Framework folder");
                return;
            }

            // установка
            using(Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = System.IO.Path.Combine(
                        cb_netPath.SelectedValue.ToString(), 
                        "InstallUtil.exe"
                        ),
                    Arguments = tb_servicePath.Text.Trim(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                addRecord2log(getProcessOutput(proc));
            }
        }

        private void startService(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_serviceName.Text.Trim()))
            {
                addRecord2log("You did not set the name of the service");
                return;
            }

            // запуск
            using (Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = "start " + tb_serviceName.Text.Trim(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                addRecord2log(getProcessOutput(proc));
            }
        }

        private void stopService(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_serviceName.Text.Trim()))
            {
                addRecord2log("You did not set the name of the service");
                return;
            }

            // остановка
            using (Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = "stop " + tb_serviceName.Text.Trim(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                addRecord2log(getProcessOutput(proc));
            }
        }

        private void deleteService(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_servicePath.Text.Trim()))
            {
                addRecord2log("You did not set the path to the service's executable");
                return;
            }

            if (cb_netPath.SelectedValue == null)
            {
                addRecord2log("You did not set the path to the .NET Framework folder");
                return;
            }

            // удаление
            using (Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = System.IO.Path.Combine(
                        cb_netPath.SelectedValue.ToString(),
                        "InstallUtil.exe"
                        ),
                    Arguments = "/u " + tb_servicePath.Text.Trim(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                addRecord2log(getProcessOutput(proc));
            }
        }

        private string getProcessOutput(Process proc)
        {
            StringBuilder output = new StringBuilder();
            while (!proc.StandardOutput.EndOfStream)
            {
                output.Append(proc.StandardOutput.ReadLine());
                output.Append(Environment.NewLine);
            }

            byte[] bytes = Encoding.Default.GetBytes(output.ToString());
            string outputEncoded = Encoding.GetEncoding(
                Properties.Settings.Default.consoleEncoding
                ).GetString(bytes);

            return outputEncoded;
        }
    }
}
