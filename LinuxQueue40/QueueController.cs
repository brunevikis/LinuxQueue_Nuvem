using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;


namespace LinuxQueue40
{


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

    }

    public class QueueController
    {

        public static List<Cluster> Clusters = new List<Cluster>();

        public static Cluster GetCluster(string addr)
        {

            if (addr == "null") return null;
            else return Clusters.FirstOrDefault(x => x.Host == addr);
        }

        public QueueController()
        {
            CachedCommands = new List<CommItem>();
            Clusters.Add(new Cluster() { Alias = "Auto", Host = "" });
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

                CreateKillCommand(comm, requestor);
            }
            return true;
        }

        protected void CreateKillCommand(CommItem commToKill, string requestor)
        {

            if (string.IsNullOrWhiteSpace(commToKill.Pid))
            {
                throw new Exception("Process ID não identificado");
            }

            var comm = new CommItem();

            comm.WorkingDirectory = ".";
            comm.Command = "Z:\\cpas_ctl_common\\killer.sh " + commToKill.Pid;

            comm.CommandName = "Killer" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            comm.User = requestor;

            comm.IgnoreQueue = true;

            comm.Cluster = commToKill.Cluster;

            Enqueue(comm);
        }

        public void SetHigherOrder(CommItem comm)
        {

            var comms = CachedCommands.Where(x => x.FolderType == QueueFolderEnum.Queue);

            if (comms.Count() < 2)
            {
                return;
            }

            var nextComm = comms.Where(x => x.Order < comm.Order && x != comm).OrderBy(x => x.Order).LastOrDefault();

            if (nextComm == null)
            {
                return;
            }

            var ord = nextComm.Order;
            nextComm.Order = comm.Order;
            comm.Order = ord;

            nextComm.SaveChanges();
            comm.SaveChanges();

        }

        public void SetLowerOrder(CommItem comm)
        {
            var comms = CachedCommands.Where(x => x.FolderType == QueueFolderEnum.Queue);

            if (comms.Count() < 2)
            {
                return;
            }

            var nextComm = comms.Where(x => x.Order > comm.Order && x != comm).OrderBy(x => x.Order).FirstOrDefault();

            if (nextComm == null)
            {
                return;
            }

            var ord = nextComm.Order;
            nextComm.Order = comm.Order;
            comm.Order = ord;

            nextComm.SaveChanges();
            comm.SaveChanges();
        }

        public void IgnoreQueue(CommItem comm)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(QueueFolders.queueFolder, comm.CommandName)))
            {
                comm.IgnoreQueue = true;
                comm.SaveChanges();
            }
        }

        public List<CommItem> ReadComms()
        {

            var tempCommands = new List<CommItem>();
            try
            {

                var finishedFiles = System.IO.Directory.GetFiles(QueueFolders.finishedFolder);
                //filter 48 hours executions...
                var dAgo = DateTime.Today.AddDays(-1).ToString("yyyyMMddHHmmss");
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

        public List<string> ListComms()
        {
            var tempCommands = new List<string>();
            try
            {

                var finishedFiles = System.IO.Directory.GetFiles(QueueFolders.finishedFolder);
                //filter 48 hours executions...
                var dAgo = DateTime.Today.AddDays(-1).ToString("yyyyMMddHHmmss");
                finishedFiles
                    .ToList().ForEach(f =>
                    {
                        var fdt = f.Split('_').Reverse().First();
                        if (fdt.CompareTo(dAgo) > 0)
                        {                            
                            tempCommands.Add(f);
                        }
                    });

                var queueFiles = System.IO.Directory.GetFiles(QueueFolders.queueFolder);
                queueFiles
                    .ToList().ForEach(f =>
                    {                        
                        tempCommands.Add(f);
                    });

                var runningFiles = System.IO.Directory.GetFiles(QueueFolders.runningFolder);
                runningFiles
                         .ToList().ForEach(f =>
                         {                             
                             tempCommands.Add(f);
                         });
            }
            catch { }
            return tempCommands;
        }

        public string ListAllFinnished()
        {
            var finishedFiles = System.IO.Directory.GetFiles(QueueFolders.finishedFolder);
            //filter 48 hours executions...
            var dAgo = DateTime.Today.AddDays(-30).ToString("yyyyMMddHHmmss");
            var tf = finishedFiles
                .Where(f =>
                {
                    var fdt = f.Split('_').Reverse().First();
                    return fdt.CompareTo(dAgo) > 0;
                })
                    .Select(f =>
                        CommItem.Create(f, QueueFolders.finishedFolder));

            var r = new StringBuilder("");
            tf.ToList().ForEach(x =>
            {
                r.AppendLine(string.Join("\t", new string[] {
                    x.Cluster != null ? x.Cluster.Alias : "",
                    x.Folder,
                    x.CommandName,
                    x.WorkingDirectory,
                    x.Command,
                    x.User,
                    x.SDate,
                    x.EDate,
                    x.ExitCode.HasValue? x.ExitCode.ToString() : "",
                }));
            }
                );


            return r.ToString();
        }

        public bool? WaitCompletition(CommItem comm, int milisecondsInverval = 5000, int timeout = 120000)
        {

            var it = (int)(timeout / milisecondsInverval);
            var cit = 0;
            do
            {
                System.Threading.Thread.Sleep(milisecondsInverval);

                var f = System.IO.Directory.GetFiles(
                    QueueFolders.finishedFolder,
                    comm.CommandName);

                if (f.Length > 0)
                {
                    comm.Update(QueueFolders.finishedFolder);
                    return true;
                }
                else if (cit++ > it)
                {
                    return false;
                }
            } while (true);
        }

        public static void ReadConfig()
        {
            Clusters.Clear();

            var configPath = System.IO.Path.Combine(QueueFolders.rootPath, "config");

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
