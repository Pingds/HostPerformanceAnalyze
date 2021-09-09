using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HostPerformanceAnalyze
{
    public class GPUDataHandler : BaseDataHandler
    {
        public GPUDataHandler(Process process) : base(process)
        {
            DataType = DataType.GPU;
            CategoryName = "GPU Engine";
            CounterName = "Utilization Percentage";
            ExtensionName = "phys_0_eng_0_engtype_3D";
        }

        public override void WriteData()
        {
            BaseDataModel baseData = new BaseDataModel();
            baseData.Time = DateTime.Now;
            foreach (var item in Counters)
            {
                var value = item.NextValue();
                if (value > 0)
                    baseData.Value = value;
            }
            Data.Add(baseData);
        }
    }

    public class CPUDataHandler : BaseDataHandler
    {
        private PerformanceCounter cpuCounter = null;
        public CPUDataHandler(Process process) : base(process)
        {
            DataType = DataType.CPU;
            CategoryName = "Process";
            CounterName = "% Processor Time";
        }

        public override void InitPerformanceCounter()
        {
            try
            {

                cpuCounter = new PerformanceCounter(CategoryName, CounterName, InternalProcess.ProcessName);
            }
            catch (Exception ex)
            {

            }

        }

        public Process Process;

        public override void WriteData()
        {
            BaseDataModel baseData = new BaseDataModel();
            baseData.Time = DateTime.Now;
            if (cpuCounter != null)
            {
                baseData.Value = cpuCounter.NextValue() / Environment.ProcessorCount;
                Debug.WriteLine($"Process {DataType.ToString()}: {baseData.Value}");
            }
            else
            {
                baseData.Value = 0;
            }
            Data.Add(baseData);
        }
    }

    public class MemoryDataHandler : BaseDataHandler
    {
        private PerformanceCounter memoryCounter = null;
        public MemoryDataHandler(Process process) : base(process)
        {
            DataType = DataType.Memory;
            CategoryName = "Process";
            CounterName = "Working Set - Private";
        }

        public override void InitPerformanceCounter()
        {
            memoryCounter = new PerformanceCounter(CategoryName, CounterName, InternalProcess.ProcessName);

        }

        //private void getallmemoryusage()
        //{
        //    /*
        //     * 
        //     * The correspondences are as below:

        //    Task Manager                               Performance Monitor
        //    ---------------------------------        -----------------------
        //    Memory(Private Working Set) -->  Working Set-Private
        //    Working Set(Memory)            -->  Working Set
        //    Peak Working Set(Memory)    -->  Working Set Peak

        //    PrivateMemorySize
        //    The number of bytes that the associated process has allocated that cannot be shared with other processes.
        //    PeakVirtualMemorySize
        //    The maximum amount of virtual memory that the process has requested.
        //    PeakPagedMemorySize
        //    The maximum amount of memory that the associated process has allocated that could be written to the virtual paging file.
        //    PagedSystemMemorySize
        //    The amount of memory that the system has allocated on behalf of the associated process that can be written to the virtual memory paging file.
        //    PagedMemorySize
        //    The amount of memory that the associated process has allocated that can be written to the virtual memory paging file.
        //    NonpagedSystemMemorySize
        //    The amount of memory that the system has allocated on behalf of the associated process that cannot be written to the virtual memory paging file.
        //    */
        //    double f = 1024.0;

        //    InternalProcess.Refresh();

        //    Debug.WriteLine($"Private memory size64: {(InternalProcess.PrivateMemorySize64 / f).ToString("N2")} K");
        //    Debug.WriteLine($"Working Set size64: {InternalProcess.WorkingSet64 / f}");
        //    Debug.WriteLine($"Peak virtual memory size64: {InternalProcess.PeakVirtualMemorySize64 / f}");
        //    Debug.WriteLine($"Peak paged memory size64: {InternalProcess.PeakPagedMemorySize64 / f}");
        //    Debug.WriteLine($"Paged system memory size64: {InternalProcess.PagedSystemMemorySize64 / f}");
        //    Debug.WriteLine($"Paged memory size64: {InternalProcess.PagedMemorySize64 / f}");
        //    Debug.WriteLine($"Nonpaged system memory size64: {InternalProcess.NonpagedSystemMemorySize64 / f}");

        //}

        private double f = 1024.0 * 1024.0;
        public override void WriteData()
        {
            BaseDataModel baseData = new BaseDataModel();
            baseData.Time = DateTime.Now;
            InternalProcess.Refresh();
            if (memoryCounter != null)
            {
                baseData.Value = memoryCounter.NextValue() / f;
                Debug.WriteLine($"Process {DataType.ToString()}: {baseData.Value}");
            }
            else
            {
                baseData.Value = 0;
            }

            Data.Add(baseData);
        }
    }

    public class HandlesDataHandler : BaseDataHandler
    {
        [DllImport("user32.dll")]
        private static extern uint GetGuiResources(IntPtr hProcess, uint uiFlags);

        private enum ResourceType
        {
            Gdi = 0,
            User = 1
        }

        public static int GetWindowHandlesForCurrentProcess(IntPtr processHandle)
        {
            uint gdiObjects = GetGuiResources(processHandle, (uint)ResourceType.Gdi);
            uint userObjects = GetGuiResources(processHandle, (uint)ResourceType.User);

            return Convert.ToInt32(gdiObjects + userObjects);
        }
        public HandlesDataHandler(Process process) : base(process)
        {
            DataType = DataType.Handle;
            //CategoryName = "Processor Performance";
            //CounterName = "Processor Frequency";
            //ExtensionName = "_Total";
        }

        public override void WriteData()
        {
            BaseDataModel baseData = new BaseDataModel();
            baseData.Time = DateTime.Now;
            InternalProcess.Refresh();
            baseData.Value = InternalProcess.HandleCount;
            Debug.WriteLine($"Process {DataType.ToString()}: {baseData.Value}");
            Data.Add(baseData);
        }
    }

    public class ThreadsDataHandler : BaseDataHandler
    {
        public ThreadsDataHandler(Process process) : base(process)
        {
            DataType = DataType.Thread;
        }

        public override void WriteData()
        {
            BaseDataModel baseData = new BaseDataModel();
            baseData.Time = DateTime.Now;
            InternalProcess.Refresh();
            baseData.Value = InternalProcess.Threads.Count;
            Debug.WriteLine($"Process {DataType.ToString()}: {baseData.Value}");
            Data.Add(baseData);
        }
    }

    public class NetworkDataHandler : BaseDataHandler
    {
        public string NetworkCard = string.Empty;
        public NetworkDataHandler(Process process) : base(process)
        {
            DataType = DataType.Network;
            CategoryName = "Network Interface";
            CounterName = "Current Bandwidth";
        }

        public override void InitPerformanceCounter()
        {

            PerformanceCounter bandwidthCounter = new PerformanceCounter(CategoryName, CounterName, NetworkCard);
            float bandwidth = bandwidthCounter.NextValue();

            PerformanceCounter dataSentCounter = new PerformanceCounter(CategoryName, "Bytes Sent/sec", NetworkCard);

            PerformanceCounter dataReceivedCounter = new PerformanceCounter(CategoryName, "Bytes Received/sec", NetworkCard);

            //const int numberOfIterations = 10;



            //float sendSum = 0;
            //float receiveSum = 0;

            //for (int index = 0; index < numberOfIterations; index++)
            //{
            //    sendSum += dataSentCounter.NextValue();
            //    receiveSum += dataReceivedCounter.NextValue();
            //}
            //float dataSent = sendSum;
            //float dataReceived = receiveSum;


            //double utilization = (8 * (dataSent + dataReceived)) / (bandwidth * numberOfIterations) * 100;
            //return utilization;
            //base.InitPerformanceCounter();
        }


        //public static List<string> GetNetworkCards()
        //{
        //    //PerformanceCounterCategory category = new PerformanceCounterCategory(CategoryName);
        //    //string[] instancename = category.GetInstanceNames();

        //    return instancename.ToList();
        //}


        public override void WriteData()
        {

        }
    }

    public abstract class BaseDataHandler
    {
        public string CategoryName { get; set; }

        public string CounterName { get; set; }
        public string ExtensionName { get; set; }

        public string ProcessName { get { return InternalProcess?.ProcessName; } }

        public int ProcessId
        {
            get
            {
                if (InternalProcess == null)
                    return -1;
                return InternalProcess.Id;
            }
        }

        public DataType DataType { get; set; }

        public List<BaseDataModel> Data { get; set; }

        protected List<PerformanceCounter> Counters = new List<PerformanceCounter>();

        protected Process InternalProcess;

        public BaseDataHandler(Process process)
        {
            InternalProcess = process;
            Data = new List<BaseDataModel>();
        }

        protected static PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories();

        public virtual void InitPerformanceCounter()
        {
            try
            {
                //var tempItem = categories.Where(s => s.CategoryName.Contains(CategoryName));
                PerformanceCounterCategory category = new PerformanceCounterCategory(CategoryName);
                var instances = category.GetInstanceNames().Where(s => s.Contains(ProcessId.ToString()));
                if (!string.IsNullOrEmpty(ExtensionName))
                    instances = instances.Where(s => s.Contains(ExtensionName));
                foreach (var instance in instances)
                {
                    var counters = category.GetCounters(instance).Where(s => s.CounterName == CounterName);
                    Counters.AddRange(counters);
                }
            }
            catch (Exception ex)
            {

            }

        }

        public abstract void WriteData();
    }
}
