using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinuxQueueGUI
{
    public partial class PLD_Mensal : Form
    {
        public PLD_Mensal(object dados, string caso, string dir)
        {

            InitializeComponent();
            lb_name.Text = caso;
            lb_Dir.Text = dir;
            carrega_DGV(dados);

        }

        public void carrega_DGV(object dados)
        {
            var pld = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Resu_PLD_Mensal>>(dados.ToString());
            dgv_PLD.DataSource = pld.ToList();
        }

        
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        
    }
}
