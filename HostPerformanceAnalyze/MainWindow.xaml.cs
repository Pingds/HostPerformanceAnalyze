using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
        private int dataCount = 0;
        private DispatcherTimer checkHostTimer;


        public ObservableCollection<LogModel> LogInfos
        {
            get { return (ObservableCollection<LogModel>)GetValue(LogInfosProperty); }
            set { SetValue(LogInfosProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogInfos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogInfosProperty =
            DependencyProperty.Register("LogInfos", typeof(ObservableCollection<LogModel>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<LogModel>()));



        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            btnStop.IsEnabled = false;
            btnStart.IsEnabled = false;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //firstly, check counters environment
                    if (!CountersHelper.CheckCounterEnv())
                    {
                        var consoleData = CountersHelper.ResetCounterEnv();
                        AddLog(consoleData);
                    }

                    //secondly, check process
                    if (CheckHostProcessStatus())
                    {
                        InitHandlers();
                    }
                    else
                    {
                        //timer start to check host process
                        checkHostTimer = new DispatcherTimer(new TimeSpan(0, 0, 5), DispatcherPriority.Background, (ee, ss) =>
                        {
                            if (CheckHostProcessStatus())
                            {
                                InitHandlers();
                                checkHostTimer.Stop();
                                checkHostTimer = null;
                            }
                        }, Dispatcher);
                        checkHostTimer.Start();
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"Error:{ex.Message}");
                    this.Dispatcher.Invoke(() => { btnStart.IsEnabled = true; });
                }
            });
        }

        private bool CheckHostProcessStatus()
        {
            try
            {
                //hostProcess = Process.GetProcessesByName("RoomsHost").FirstOrDefault();
                //dwmProcess = Process.GetProcessesByName("dwm").FirstOrDefault();
                hostProcess = Process.GetProcessesByName("RoomsHost").FirstOrDefault();
                dwmProcess = Process.GetProcessesByName("dwm").FirstOrDefault();
            }
            catch (Exception ex)
            {
                AddLog($"Error:detect RoomsHost failed. \r\n {ex}");
            }

            if (hostProcess == null)
            {
                AddLog($"Error:No RoomsHost launching, please launch RoomsHost first!");
                return false;
            }
            else
                return true;
        }


        private void InitHandlers()
        {
            AddLog($"System Init....Start");
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

            //var networkDataHandler = new NetworkDataHandler(hostProcess);
            //networkDataHandler.InitPerformanceCounter();
            //dataHandlers.Add(networkDataHandler);

            isInited = true;

            this.Dispatcher.Invoke(() => { btnStart.IsEnabled = true; });
            AddLog($"System Init....Finish");
        }

        private void CollectData(int interval = 2000)
        {
            Task.Factory.StartNew(() =>
            {
                while (isMonitoring)
                {
                    Thread.Sleep(interval);
                    foreach (var dataHandler in dataHandlers)
                    {
                        dataCount++;
                        dataHandler.WriteData();
                        AddLog($"WriteData Count={dataCount}");
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
                    ClearLog();
                    isMonitoring = true;
                    btnStart.IsEnabled = false;
                    btnStop.IsEnabled = true;
                    CollectData();
                }
            }

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            isMonitoring = false;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
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

        private void AddLog(string info)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.LogInfos.Add(new LogModel(info));
                logListBox.SelectedItem = LogInfos[LogInfos.Count - 1];
                logListBox.ScrollIntoView(LogInfos[LogInfos.Count - 1]);
            }));
        }

        private void ClearLog()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.LogInfos.Clear();
                this.LogInfos.Add(new LogModel($"ClearLog"));
                logListBox.SelectedItem = LogInfos[LogInfos.Count - 1];
                logListBox.ScrollIntoView(LogInfos[LogInfos.Count - 1]);
            }));
        }
    }
}
