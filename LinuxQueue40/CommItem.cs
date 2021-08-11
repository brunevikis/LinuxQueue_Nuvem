using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinuxQueue40 {
    public class CommItem //: ListViewItem 
    {
        public static CommItem Create(string file, string folder) {
            var result = new CommItem();
            result.Folder = folder;
            result.CommandName = System.IO.Path.GetFileName(file);

            result.GetDetails();

            return result;
        }

        public void Update(string currentfolder = null) {            
            this.Folder = currentfolder ?? this.Folder;

            GetDetails();
        }

        public CommItem() : base() { }

        public CommItem(string file, string folder/*, string cluster*/)
            : this() {

            this.Folder = folder;
            this.CommandName = System.IO.Path.GetFileName(file);
            //this.Cluster = cluster;
            //base.Name = this.CommandName;
            GetDetails();
        }

        protected void GetDetails() {
            var content = System.IO.File.ReadAllText(System.IO.Path.Combine(this.Folder, this.CommandName));

            var lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines) {
                if (line.StartsWith("dir=", StringComparison.OrdinalIgnoreCase)) {
                    WorkingDirectory = Delinuxize(line.Replace("dir=", "").Trim());
                } else
                    if (line.StartsWith("cluster=", StringComparison.OrdinalIgnoreCase)) {

                        //Cluster = new Cluster() { Host = line.Replace("cluster=", "").Trim() };

                        Cluster = QueueController.GetCluster(line.Replace("cluster=", "").Trim());

                    } else
                        if (line.StartsWith("cmd=", StringComparison.OrdinalIgnoreCase)) {
                            Command = Delinuxize(line.Replace("cmd=", "").Trim());
                        } else
                            if (line.StartsWith("ign=", StringComparison.OrdinalIgnoreCase)) {
                                IgnoreQueue = bool.TrueString == line.Replace("ign=", "").Trim() ? true : false;
                            } else
                                if (line.StartsWith("usr=", StringComparison.OrdinalIgnoreCase)) {
                                    User = line.Replace("usr=", "").Trim();
                                } else
                                    if (line.StartsWith("PID=", StringComparison.OrdinalIgnoreCase)) {
                                        Pid = line.Replace("PID=", "").Trim();
                                    } else
                                        if (line.StartsWith("ord=", StringComparison.OrdinalIgnoreCase)) {
                                            int ord;
                                            if (int.TryParse(line.Replace("ord=", "").Trim(), out ord))
                                                Order = ord;
                                            else
                                                Order = 99;
                                        } else
                                            if (line.StartsWith("STIME=", StringComparison.OrdinalIgnoreCase)) {
                                                SDate = line.Replace("STIME=", "").Trim();// x;
                                            } else
                                                if (line.StartsWith("ETIME=", StringComparison.OrdinalIgnoreCase)) {
                                                    EDate = line.Replace("ETIME=", "").Trim();// x;
                                                } else
                                                    if (line.StartsWith("KTIME=", StringComparison.OrdinalIgnoreCase)) {
                                                        HasError = true;
                                                    } else
                                                        if (line.StartsWith("EXITCODE=", StringComparison.OrdinalIgnoreCase)) {
                                                            int exc;
                                                            if (int.TryParse(line.Replace("EXITCODE=", "").Trim(), out exc))
                                                                ExitCode = exc;
                                                        }
            }
        }

        public string CommandName { get; set; }
        public string WorkingDirectory { get; set; }
        public string Command { get; set; }
        public string Pid { get; set; }

        public int Order { get; set; }

        public string ToCommFile() {
            var content = new StringBuilder();

            content.Append("ord=");
            content.Append(Order.ToString());
            content.Append("\n");

            content.Append("cluster=");
            content.Append(
                Cluster != null ? Cluster.Host : "null");
            content.Append("\n");

            content.Append("usr=");
            content.Append(User);
            content.Append("\n");

            content.Append("dir=");
            content.Append(Linuxize(WorkingDirectory));
            content.Append("\n");

            content.Append("cmd=");
            content.Append(Linuxize(Command));
            content.Append("\n");

            content.Append("ign=");
            content.Append(IgnoreQueue.ToString());
            content.Append("\n");

            return content.ToString();
        }

        public int? ExitCode { get; set; }

        public string Folder { get; set; }
        public QueueFolderEnum FolderType {
            get {
                return QueueFolders.FolderToType(Folder);
            }
        }
        public string LogFile { get { return System.IO.Path.Combine(QueueFolders.runlogFolder, CommandName) + ".run"; } }

        public string User { get; set; }

        private string Linuxize(string path) {
            return path.Replace("\\", "/").Replace("Z:", "/home/compass/sacompass/previsaopld").Replace("L:", "/home/producao/PrevisaoPLD");
        }

        private string Delinuxize(string path) {
            return path.Replace("/home/producao/PrevisaoPLD", "L:").Replace("/home/compass/sacompass/previsaopld", "Z:").Replace("/", "\\");            
            //return path;
        }

        //will clean ther history...
        public void SaveChanges() {
            if (System.IO.File.Exists(System.IO.Path.Combine(this.Folder, this.CommandName))) {
                System.IO.File.WriteAllText(System.IO.Path.Combine(this.Folder, this.CommandName), ToCommFile());
            }
        }

        public bool IgnoreQueue { get; set; }

        public string SDate { get; set; }

        public string EDate { get; set; }

        public bool HasError { get; set; }

        public Cluster Cluster { get; set; }

        public static bool operator ==(CommItem a, object b) {

            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            else
                return a.Equals(b);
        }
        public static bool operator !=(CommItem a, object b) {

            return !(a == b);
        }

        public override bool Equals(object obj) {

            if (obj == null || !(obj is CommItem)) {
                return false;
            }
            if (this.CommandName == null || ((CommItem)obj).CommandName == null) {
                return false;
            }

            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            return (7 * CommandName.GetHashCode() + 31) / 5;
        }
    }
}
