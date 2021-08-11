using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinuxQueueApi.Model
{
    public class Disk
    {
        public string Name { get; set; }
        public long AvailableFreeSpace { get; set; }
        public long TotalSize { get; set; }
    }
}
