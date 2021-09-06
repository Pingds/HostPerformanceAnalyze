using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostPerformanceAnalyze
{
    public enum DataType
    {
        CPU = 0,
        GPU = 1,
        DwmGPU = 2,
        Memory = 3,
        Handle = 4,
        Thread = 5,
        GMemory = 6
    }
    public class BaseDataModel
    {
        //cpu\ gpu\memory\handle\thread\dwmGPU

        public DateTime Time { get; set; }
        public double Value { get; set; }

        public override string ToString()
        {
            return $"{Time.ToShortTimeString()}-{Value:N2}";
        }
    }

    public class GPUDataModel : BaseDataModel
    {
        public double DwmGPUValue { get; set; }
        public double AllGPU { get { return Value + DwmGPUValue; } }
    }
}
