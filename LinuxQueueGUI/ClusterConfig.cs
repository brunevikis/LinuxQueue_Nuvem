using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinuxQueueGUI {
    public partial class ClusterConfig : UserControl {

        private LinuxQueue.Cluster databind;

        public string Alias { get { return textBox1.Text.Trim(); } set { textBox1.Text = value; } }
        public string Host { get { return textBox2.Text.Trim(); } set { textBox2.Text = value; } }
        public bool Active { get { return checkBox1.Checked; } set { checkBox1.Checked = value; } }
        public int QueueLength { get { return (int)numericUpDown1.Value; } set { numericUpDown1.Value = value; } }


        public bool Status {
            set {
                if (value) {
                    label1.BackColor = Color.Green;
                    label1.Text = "Cluster OK";
                } else {
                    label1.BackColor = Color.Red;
                    label1.Text = "Cluster Not OK";
                }
            }
        }


        public ClusterConfig() {
            InitializeComponent();
        }
        public ClusterConfig(LinuxQueue.Cluster cluster)
            : this() {

            databind = cluster;

            Alias = cluster.Alias;
            Host = cluster.Host;
            Active = cluster.Enabled;
            QueueLength = cluster.QueueLength;
        }

        public void ApplyChanges() {
            databind.Enabled = Active;
            databind.QueueLength = QueueLength;            
        }
    }
}
