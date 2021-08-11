using LinuxQueue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinuxQueueGUI {
    public partial class FormConfig : Form {
        public FormConfig() {
            InitializeComponent();
        }

        Dictionary<Cluster, ClusterConfig> configDic = new Dictionary<Cluster, ClusterConfig>();

        private void FormConfig_Load(object sender, EventArgs e) {

            QueueController.ReadConfig();

            flowLayoutPanel1.Controls.AddRange(
            LinuxQueue.QueueController.Clusters.Where(x => x.Host != "")
                .Select(x => {
                    var cfg = new ClusterConfig(x);
                    configDic.Add(x, cfg);
                    return cfg;
                }).ToArray()
                );

            QueueController.Clusters.Insert(0, new Cluster() { Alias = "Auto", Host = "" });
        }

        private void button1_Click(object sender, EventArgs e) {
            foreach (var c in flowLayoutPanel1.Controls) {
                if (c is ClusterConfig) {
                    ((ClusterConfig)c).ApplyChanges();
                }
            }

            LinuxQueue.QueueController.WriteConfig();

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e) {

            var logFile = Path.Combine(LinuxQueue.QueueFolders.rootPath, "chkClusters.log");


            Tools.OpenText(logFile);
        }

        private async void button3_Click(object sender, EventArgs e) {

            this.Cursor = Cursors.WaitCursor;
            var comm = new CommItem();

            comm.Cluster = null;
            comm.CommandName = "CheckCluster_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            comm.WorkingDirectory = LinuxQueue.QueueFolders.rootPath;
            comm.Command = Path.Combine(LinuxQueue.QueueFolders.rootPath, "checkNode_sudo.sh");
            comm.IgnoreQueue = true;

            var ctl = new QueueController();
            ctl.Enqueue(comm);

            ctl.WaitCompletition(comm, 5000);

            var log = File.ReadAllText(comm.LogFile);
            this.Cursor = Cursors.Default;

            MessageBox.Show(log);

            await Task.Yield();

        }
    }
}
