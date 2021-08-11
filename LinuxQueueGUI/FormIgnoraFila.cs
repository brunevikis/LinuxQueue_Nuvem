using LinuxQueue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinuxQueueGUI {
    public partial class FormIgnoraFila : Form {

        public CommItem Comm { get; set; }
        
        public FormIgnoraFila(CommItem comm) {


            Comm = comm;


            InitializeComponent();

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cbxCluster.ValueMember = "Host";
            cbxCluster.DisplayMember = "Alias";

            cbxCluster.DataSource = LinuxQueue.QueueController.Clusters;

            cbxCluster.SelectedItem = Comm.Cluster;

        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }


        private void btnOk_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            Comm.Cluster = (Cluster)cbxCluster.SelectedItem;
        }

        private void btnCancela_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }


    }
}
