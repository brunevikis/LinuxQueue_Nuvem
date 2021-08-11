using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinuxQueueGUI
{
    public static class Tools
    {

        private static string FindExecutable(string path)
        {
            var executable = new StringBuilder(1024);
            var result = FindExecutable(path, string.Empty, executable);
            return result >= 32 ? executable.ToString() : string.Empty;
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll", EntryPoint = "FindExecutable")]
        private static extern long FindExecutable(string lpFile, string lpDirectory, StringBuilder lpResult);

        internal static void OpenText(string textFile)
        {

            var npp = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Notepad++", "notepad++.exe");
            if (System.IO.File.Exists(npp))
            {
                System.Diagnostics.Process.Start(npp, textFile);

            }
            else
            {
                var exec = FindExecutable("dummy.dat");

                if (exec != string.Empty)
                {
                    System.Diagnostics.Process.Start(exec, textFile);
                }
                else
                {
                    System.Diagnostics.Process.Start(textFile);
                }
            }


        }


        internal static float GetUsedSpace(string drive)
        {





            System.IO.DriveInfo c = new System.IO.DriveInfo(drive);

            var free = ((long)c.TotalFreeSpace / 1073741824.0);
            var total = ((long)c.TotalSize / 1073741824.0);

            return (float)((total - free) * 100 / (total));
        }
    }

    public class Disk
    {
        public string Name { get; set; }
        public float UsedPercentage
        {
            get
            {
                return
TotalSize > 0 ?
(float)((100d - AvailableFreeSpace * 100d / TotalSize))
: 100f
;
            }
            set
            {
                TotalSize = 100;
                AvailableFreeSpace = (long)(100f - value);


            }
        }
        public long AvailableFreeSpace { get; set; }
        public long TotalSize { get; set; }
    }

}
