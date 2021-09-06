using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Path = System.IO.Path;

namespace HostPerformanceAnalyze
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<BaseDataHandler> dataHandlers = new List<BaseDataHandler>();
        private Process hostProcess = null;
        private Process dwmProcess = null;
        private bool isInited = false;
        private bool isMonitoring = false;

        public MainWindow()
        {
            InitializeComponent();

            if (CheckHostProcessStatus())
            {
                InitHandlers();
            }
        }

        private bool CheckHostProcessStatus()
        {
            try
            {
                hostProcess = Process.GetProcessesByName("RoomsHost").FirstOrDefault();
                dwmProcess = Process.GetProcessesByName("dwm").FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"detect RoomsHost failed. \r\n {ex}");
            }

            if (hostProcess == null)
            {
                MessageBox.Show($"No RoomsHost launching, please launch RoomsHost first!");
                return false;
            }
            else
                return true;
        }


        private void InitHandlers()
        {
            var gpuDataHandler = new GPUDataHandler(hostProcess);
            gpuDataHandler.InitPerformanceCounter();
            dataHandlers.Add(gpuDataHandler);


            var dwmGpuDataHandler = new GPUDataHandler(dwmProcess);
            dwmGpuDataHandler.InitPerformanceCounter();
            dataHandlers.Add(dwmGpuDataHandler);

            var cpuDataHandler = new CPUDataHandler(hostProcess);
            cpuDataHandler.InitPerformanceCounter();
            dataHandlers.Add(cpuDataHandler);

            var handlesDataHandler = new HandlesDataHandler(hostProcess);
            dataHandlers.Add(handlesDataHandler);

            var threadsDataHandler = new ThreadsDataHandler(hostProcess);
            dataHandlers.Add(threadsDataHandler);

            var memoryDataHandler = new MemoryDataHandler(hostProcess);
            memoryDataHandler.InitPerformanceCounter();
            dataHandlers.Add(memoryDataHandler);

            isInited = true;
        }

        private void CollectData(int interval = 2000)
        {
            Task.Run(() =>
            {
                while (isMonitoring)
                {
                    Thread.Sleep(interval);
                    foreach (var dataHandler in dataHandlers)
                    {
                        dataHandler.WriteData();
                    }
                }
            });
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!isInited)
            {
                if (CheckHostProcessStatus())
                {
                    InitHandlers();
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (!isMonitoring)
                {
                    isMonitoring = true;
                    CollectData();
                }
            }

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            isMonitoring = false;

            var fileFullName = ExcelHelper.SaveCSV(dataHandlers);
            if (!string.IsNullOrEmpty(fileFullName))
            {
                OpenFolder(fileFullName);
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            OpenFolder(basePath);
        }

        private void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath) || File.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            };
        }
    }
}
