using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace LinuxQueueCore
{
    public class CommItem //: ListViewItem 
    {
        public static CommItem Create(string file, string folder)
        {
            var result = new CommItem()
            {
                Folder = folder,
                CommandName = System.IO.Path.GetFileName(file)
            };

            result.GetDetails();

            return result;
        }

        public void Update(string currentfolder = null)
        {
            this.Folder = currentfolder ?? this.Folder;

            GetDetails();
        }

        public CommItem() : base() { }

        public CommItem(string file, string folder/*, string cluster*/)
            : this()
        {

            this.Folder = folder;
            this.CommandName = System.IO.Path.GetFileName(file);
            //this.Cluster = cluster;
            //base.Name = this.CommandName;
            GetDetails();
        }

        public void Linuxize()
        {
            this.WorkingDirectory = Linuxize(this.WorkingDirectory);
            this.Command = Linuxize(this.Command);
        }

        protected void GetDetails()
        {
            var content = System.IO.File.ReadAllText(System.IO.Path.Combine(this.Folder, this.CommandName));

            var lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("dir=", StringComparison.OrdinalIgnoreCase))
                {
                    //WorkingDirectory = Delinuxize(line.Replace("dir=", "").Trim());
                    WorkingDirectory = line.Replace("dir=", "").Trim();
                }
                else if (line.StartsWith("cluster=", StringComparison.OrdinalIgnoreCase))
                {
                    Cluster = QueueController.GetCluster(line.Replace("cluster=", "").Trim());
                }
                else if (line.StartsWith("cmd=", StringComparison.OrdinalIgnoreCase))
                {
                    //Command = Delinuxize(line.Replace("cmd=", "").Trim());
                    Command = line.Replace("cmd=", "").Trim();
                }
                else if (line.StartsWith("mail=", StringComparison.OrdinalIgnoreCase))
                {
                    EnviarEmail = bool.TrueString == line.Replace("mail=", "").Trim() ? true : false;
                }
                else if (line.StartsWith("ign=", StringComparison.OrdinalIgnoreCase))
                {
                    IgnoreQueue = bool.TrueString == line.Replace("ign=", "").Trim() ? true : false;
                }
                else if (line.StartsWith("usr=", StringComparison.OrdinalIgnoreCase))
                {
                    User = line.Replace("usr=", "").Trim();
                }
                else if (line.StartsWith("PID=", StringComparison.OrdinalIgnoreCase))
                {
                    Pid = line.Replace("PID=", "").Trim();
                }
                else if (line.StartsWith("ord=", StringComparison.OrdinalIgnoreCase))
                {
                    //int ord;
                    if (int.TryParse(line.Replace("ord=", "").Trim(), out int ord))
                        Order = ord;
                    else
                        Order = 99;
                }
                else if (line.StartsWith("STIME=", StringComparison.OrdinalIgnoreCase))
                {
                    SDate = line.Replace("STIME=", "").Trim();// x;
                }
                else if (line.StartsWith("ETIME=", StringComparison.OrdinalIgnoreCase))
                {
                    EDate = line.Replace("ETIME=", "").Trim();// x;
                }
                else if (line.StartsWith("pldSE=", StringComparison.OrdinalIgnoreCase))
                {
                    pldSE = line.Replace("pldSE=", "").Trim();// x;
                }
                else if (line.StartsWith("pldS=", StringComparison.OrdinalIgnoreCase))
                {
                    pldS = line.Replace("pldS=", "").Trim();// x;
                }
                else if (line.StartsWith("pldN=", StringComparison.OrdinalIgnoreCase))
                {
                    pldN = line.Replace("pldN=", "").Trim();// x;
                }
                else if (line.StartsWith("pldNE=", StringComparison.OrdinalIgnoreCase))
                {
                    pldNE = line.Replace("pldNE=", "").Trim();// x;
                }
                else if (line.StartsWith("KTIME=", StringComparison.OrdinalIgnoreCase))
                {
                    HasError = true;
                }
                else if (line.StartsWith("EXITCODE=", StringComparison.OrdinalIgnoreCase))
                {
                    //int exc;
                    if (int.TryParse(line.Replace("EXITCODE=", "").Trim(), out int exc))
                        ExitCode = exc;
                }
            }
        }

        public string CommandName { get; set; }
        public string WorkingDirectory { get; set; }
        public string Command { get; set; }
        public string Pid { get; set; }
        public bool EnviarEmail { get; set; }

        public int Order { get; set; }

        public string ToCommFile()
        {
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


            content.Append("mail=");
            content.Append(EnviarEmail.ToString());
            content.Append("\n");

            return content.ToString();
        }

        public int? ExitCode { get; set; }

        string folder = "";
        public string Folder { get { return folder; } set { folder = value; folderType = QueueFolders.FolderToType(Folder); } }

        QueueFolderEnum folderType = QueueFolderEnum.Unknown;
        public QueueFolderEnum FolderType
        {
            get
            {
                return folderType;
            }
            set
            {
                folderType = value;
                folder = QueueFolders.TypeToFolder(folderType);
            }
        }
        public string LogFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CommandName))
                {
                    return null;
                }
                else
                {
                    return System.IO.Path.Combine(QueueFolders.runlogFolder, CommandName) + ".run";
                }

            }
        }

        public string User { get; set; }

        private string Linuxize(string path)
        {
            return path.Replace("\\", "/").Replace("Z:", "/home/compass/sacompass/previsaopld").Replace("L:", "/home/producao/PrevisaoPLD");
        }

        private string Delinuxize(string path)
        {
            return path.Replace("/home/producao/PrevisaoPLD", "L:").Replace("/home/compass/sacompass/previsaopld", "Z:").Replace("/", "\\");
            //return path;
        }

        //will clean ther history...
        public void SaveChanges()
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(this.Folder, this.CommandName)))
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(this.Folder, this.CommandName), ToCommFile());
            }
        }

        public bool IgnoreQueue { get; set; }

        public string SDate { get; set; }

        public string EDate { get; set; }

        public string pldSE { get; set; }

        public string pldS { get; set; }

        public string pldNE { get; set; }

        public string pldN { get; set; }

        string ttime;
        public string TTime
        {
            get
            {
                if (String.IsNullOrWhiteSpace(ttime) && !String.IsNullOrWhiteSpace(SDate) && !String.IsNullOrWhiteSpace(EDate))
                {

                    if (DateTime.TryParse(SDate, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out DateTime dts)
                        &&
                        DateTime.TryParse(EDate, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out DateTime dte)
                        )
                    {
                        ttime = (dte - dts).ToString();
                    }
                }

                return ttime;
            }
            set { ttime = value; }
        }

        public bool HasError { get; set; }

        public Cluster Cluster { get; set; }

        public static bool operator ==(CommItem a, object b)
        {

            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            else
                return a.Equals(b);
        }
        public static bool operator !=(CommItem a, object b)
        {

            return !(a == b);
        }

        public override bool Equals(object obj)
        {

            if (obj == null || !(obj is CommItem))
            {
                return false;
            }
            if (this.CommandName == null || ((CommItem)obj).CommandName == null)
            {
                return false;
            }

            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return (7 * CommandName.GetHashCode() + 31) / 5;
        }
    }

    public enum QueueFolderEnum
    {
        Queue,
        Running,
        Finished,
        Unknown
    }
    public static class QueueFolders
    {

        //static QueueFolders ff = null;

        public static void RegisterFolder()
        {
            string rootCtlPath;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                rootCtlPath = @"/home/compass/sacompass/previsaopld/cpas_ctl_common";
            }
            else
            {
                rootCtlPath = @"Z:\cpas_ctl_common\";
            }

            RegisterFolder(rootCtlPath);
        }


        public static void RegisterFolder(string rootPath)
        {
            //ff= new QueueFolders() {
            QueueFolders.rootPath = rootPath;
            //};
        }

        //public static QueueFolders Instance() {
        //    return ff;
        //}
        //L:\Teste_Fila_Dotnet\queuectl\
        public static string rootPath = "";
        public static string pathQueueNFS = "/home/compass/queuectl";
        // public static string pathQueueNFS = "L:\\Teste_Fila_Dotnet\\queuectl\\";
         public static string queueFolder { get { return System.IO.Path.Combine("/home/compass/queuectl", "queue"); } }
        
        public static string runningFolder { get { return System.IO.Path.Combine("/home/compass/queuectl", "running"); } }
        public static string finishedFolder { get { return System.IO.Path.Combine("/home/compass/queuectl", "finished"); } }

        
        public static string runlogFolder { get { return System.IO.Path.Combine(rootPath, "run_log"); } }

        public static QueueFolderEnum FolderToType(string folder)
        {
            if (folder.Equals(queueFolder, StringComparison.OrdinalIgnoreCase))
            {
                return QueueFolderEnum.Queue;
            }
            else if (folder.Equals(runningFolder, StringComparison.OrdinalIgnoreCase))
            {
                return QueueFolderEnum.Running;
            }
            else if (folder.Equals(finishedFolder, StringComparison.OrdinalIgnoreCase))
            {
                return QueueFolderEnum.Finished;
            }
            else
                return QueueFolderEnum.Unknown;
        }

        internal static string TypeToFolder(QueueFolderEnum folderType)
        {
            switch (folderType)
            {
                case QueueFolderEnum.Queue:
                    return queueFolder;
                case QueueFolderEnum.Running:
                    return runningFolder;
                case QueueFolderEnum.Finished:
                    return finishedFolder;
                default:
                    return "";
            }
        }
    }

    public class Command_Auth
    {
        public string Cluster { get; set; }
        public string Command { get; set; }

       
    }

    public class Cluster
    {

        public string Alias { get; set; }
        public string Host { get; set; }
        public bool Enabled { get; set; }
        public int QueueLength { get; set; }

        public override bool Equals(object obj)
        {

            if (obj != null && obj is Cluster)
            {
                return this.GetHashCode() == ((Cluster)obj).GetHashCode();

            }
            else return false;

        }

        public override int GetHashCode()
        {
            return ("cluster:" + Host).GetHashCode();
        }

        public override string ToString()
        {
            return Alias;
        }

        public static Cluster Default = new Cluster() { Alias = "Auto", Host = "" };

    }

    public class QueueController
    {
        public static List<Cluster> Clusters = new List<Cluster>();
        public static List<Command_Auth> Commands_Auth = new List<Command_Auth>();

        public static Cluster GetCluster(string addr)
        {
            if (addr == "null")
            {
                return null;
            }
            else
            {
                return Clusters.FirstOrDefault(x => x.Host == addr);
            }
        }

        public QueueController()
        {
            CachedCommands = new List<CommItem>();
        }

        protected List<CommItem> CachedCommands = null;

        public int CurrentOrder
        {
            get
            {
                if (CachedCommands != null && CachedCommands.Any(x => x.FolderType == QueueFolderEnum.Queue))
                {
                    return CachedCommands.Where(x => x.FolderType == QueueFolderEnum.Queue).Max(x => x.Order);
                }
                else
                    return 1;
            }
        }

        public void Enqueue(CommItem comm)
        {

            if (comm.IgnoreQueue) comm.Order = 1;
            else comm.Order = CurrentOrder + 3;

            var txt = comm.ToCommFile();

            System.IO.File.WriteAllText(
                System.IO.Path.Combine(QueueFolders.queueFolder, comm.CommandName),
                txt
            );

            comm.Folder = QueueFolders.queueFolder;
        }

        public bool Cancel(CommItem comm, string requestor)
        {
            if (
                    comm.FolderType == QueueFolderEnum.Queue &&
                    System.IO.File.Exists(System.IO.Path.Combine(QueueFolders.queueFolder, comm.CommandName))
                    )
            {
                System.IO.File.Delete(
                    System.IO.Path.Combine(QueueFolders.queueFolder, comm.CommandName)
                    );
            }
            else if (
                comm.FolderType == QueueFolderEnum.Running &&
                System.IO.File.Exists(System.IO.Path.Combine(QueueFolders.runningFolder, comm.CommandName))
                )
            {

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    var killCmd = @"/home/compass/sacompass/previsaopld/cpas_ctl_common/killer.sh";

                    Console.WriteLine("killing: " + comm.Pid);

                    var p = System.Diagnostics.Process.Start(killCmd, comm.Pid);

                    p.WaitForExit();

                }
                else
                {
                    CreateKillCommand(comm, requestor);
                }
            }
            return true;
        }

        protected void CreateKillCommand(CommItem commToKill, string requestor)
        {

            if (string.IsNullOrWhiteSpace(commToKill.Pid))
            {
                throw new Exception("Process ID não identificado");
            }

            var comm = new CommItem()
            {
                WorkingDirectory = ".",
                Command = "Z:\\cpas_ctl_common\\killer.sh " + commToKill.Pid,
                CommandName = "Killer" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                User = requestor,
                IgnoreQueue = true,
                Cluster = commToKill.Cluster
            };

            Enqueue(comm);
        }


        public List<Command_Auth> ReadAuth()
        {
            var tempAuth = new List<Command_Auth>();

            // var configPath = System.IO.Path.Combine(QueueFolders.rootPath, "config");
            var configPath = System.IO.Path.Combine("L:\\Teste_Fila_Dotnet", "commandAuth");

            foreach (var cl in System.IO.File.ReadAllLines(configPath).Skip(1))
            {

                var clArr = cl.Split(':');
                if (clArr.Length >= 2)
                {
                    var command_Auth = new Command_Auth()
                    {
                        Cluster = clArr[0].Trim(),
                        Command = clArr[1].Trim()
                    };
                    if (!clArr[0].Trim().StartsWith("#"))
                    {
                        tempAuth.Add(command_Auth);

                    }
                   

                    
                }
            }

            return tempAuth;
        }
        public List<Cluster> ReadClusters()
        {
            var tempCluster = new List<Cluster>();

            var configPath = System.IO.Path.Combine(QueueFolders.rootPath, "config");
            //var configPath = System.IO.Path.Combine("L:\\Teste_Fila_Dotnet", "config");

            foreach (var cl in System.IO.File.ReadAllLines(configPath).Skip(1))
            {

                var clArr = cl.Split(';');
                
                var cluster = new Cluster()
                {
                    Host = clArr[1].Trim(),
                    QueueLength = int.Parse(clArr[2].Trim())
                };
                if (clArr[0].Trim().StartsWith("#"))
                {
                    cluster.Enabled = false;
                    cluster.Alias = clArr[0].Substring(1).Trim();
                }
                else
                {
                    cluster.Enabled = true;
                    cluster.Alias = clArr[0].Trim();
                }
                tempCluster.Add(cluster);

            }

            return tempCluster;
        }

        public List<CommItem> ReadComms(int days = -4)
        {

            var tempCommands = new List<CommItem>();
            try
            {

                var finishedFiles = System.IO.Directory.GetFiles(QueueFolders.finishedFolder);
                //filter 48 hours executions...
                var dAgo = DateTime.Today.AddDays(days).ToString("yyyyMMddHHmmss");

                finishedFiles
                    .ToList().ForEach(f =>
                    {
                        var fdt = f.Split('_').Reverse().First();
                        if (fdt.CompareTo(dAgo) > 0)
                        {
                            var c = CommItem.Create(f, QueueFolders.finishedFolder);
                            tempCommands.Add(c);
                        }
                    });

                var queueFiles = System.IO.Directory.GetFiles(QueueFolders.queueFolder);
                queueFiles
                    .ToList().ForEach(f =>
                    {
                        var c = CommItem.Create(f, QueueFolders.queueFolder);
                        tempCommands.Add(c);
                    });

                var runningFiles = System.IO.Directory.GetFiles(QueueFolders.runningFolder);
                runningFiles
                         .ToList().ForEach(f =>
                         {
                             var c = CommItem.Create(f, QueueFolders.runningFolder);
                             tempCommands.Add(c);
                         });
            }
            catch { }

            CachedCommands.Clear();
            CachedCommands.AddRange(tempCommands);

            tempCommands.Clear();
            tempCommands = null;

            return CachedCommands;
        }

        public static void ReadConfig()
        {
            Clusters.Clear();

            var configPath = System.IO.Path.Combine(QueueFolders.rootPath, "config");
            //var configPath = System.IO.Path.Combine("L:\\Teste_Fila_Dotnet", "config");

            foreach (var cl in System.IO.File.ReadAllLines(configPath).Skip(1))
            {

                var clArr = cl.Split(';');
                var cluster = new Cluster()
                {
                    Host = clArr[1].Trim(),
                    QueueLength = int.Parse(clArr[2].Trim())
                };
                if (clArr[0].Trim().StartsWith("#"))
                {
                    cluster.Enabled = false;
                    cluster.Alias = clArr[0].Substring(1).Trim();
                }
                else
                {
                    cluster.Enabled = true;
                    cluster.Alias = clArr[0].Trim();
                }
                Clusters.Add(cluster);

                Clusters.Insert(0, Cluster.Default);
            }
        }

        public static void WriteConfig()
        {
            var configPath = System.IO.Path.Combine(QueueFolders.rootPath, "config");

            var stringBuider = new StringBuilder("#ALIAS;HOST_ADDR;MAX_PROC\n");


            foreach (var c in Clusters.Where(x => x.Host != ""))
            {

                stringBuider.Append(
                    (!c.Enabled ? "#" : "") + c.Alias + ";" +
                    c.Host + ";" +
                    c.QueueLength.ToString() + "\n");
            }

            System.IO.File.WriteAllText(configPath, stringBuider.ToString());

        }
    }
}
