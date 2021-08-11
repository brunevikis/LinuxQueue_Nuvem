using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinuxQueue40 {
    public enum QueueFolderEnum {
        Queue,
        Running,
        Finished,
        Unknown
    }
    public static class QueueFolders {

        //static QueueFolders ff = null;

        public static void RegisterFolder(string rootPath) {
            //ff= new QueueFolders() {
            QueueFolders.rootPath = rootPath;
            //};
        }

        //public static QueueFolders Instance() {
        //    return ff;
        //}

        public static string rootPath = "";
        public static string queueFolder { get { return System.IO.Path.Combine(rootPath, "queue\\"); } }
        public static string runningFolder { get { return System.IO.Path.Combine(rootPath, "running\\"); } }
        public static string finishedFolder { get { return System.IO.Path.Combine(rootPath, "finished\\"); } }
        public static string runlogFolder { get { return System.IO.Path.Combine(rootPath, "run_log\\"); } }

        public static QueueFolderEnum FolderToType(string folder) {
            if (folder.Equals(queueFolder, StringComparison.OrdinalIgnoreCase)) {
                return QueueFolderEnum.Queue;
            } else if (folder.Equals(runningFolder, StringComparison.OrdinalIgnoreCase)) {
                return QueueFolderEnum.Running;
            } else if (folder.Equals(finishedFolder, StringComparison.OrdinalIgnoreCase)) {
                return QueueFolderEnum.Finished;
            } else
                return QueueFolderEnum.Unknown;
        }
    }
}
