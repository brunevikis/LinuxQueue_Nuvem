using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinuxQueueGUI
{
    public partial class UserControl1 : UserControl
    {
        public List<CommandMenu> CommandMenus = null;
        private string helpText;

        public string Command { get { return textBox2.Text; } set { textBox2.Text = value; } }

        public string WorkingDirectory { get { return textBox1.Text; } set { textBox1.Text = value; } }

        public string Argument { get { return textBox4.Text; } set { textBox4.Text = value; } }

        public string Nome { get { return textBox3.Text; } set { textBox3.Text = value; } }

        public bool EnviarEmail { get { return chkEmail.Checked; } set { chkEmail.Checked = value; } }

        public UserControl1()
        {
            InitializeComponent();


        }

        public void Initialize()
        {
            ReadCommandMenu(this.cmdMenuRoot, null);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button4.ForeColor = Color.Black;
        }

        private void ReadCommandMenu(ToolStripMenuItem root, List<CommandMenu> comands)
        {

            if (CommandMenus == null)
            {
                var cmdMenu = CommandMenu.Open();
                CommandMenus = cmdMenu.SubComands;
            }

            if (comands == null)
            {
                comands = CommandMenus;
                this.Invoke((Action)(() => root.DropDownItems.Clear()));
            }

            foreach (var m in comands
                //.Where(x => (cluster.Alias.Equals("Auto") || x.RunOn.Count == 0)
                //|| (x.RunOn.Contains(cluster.Alias)))
                )
            {
                var mi = new System.Windows.Forms.ToolStripMenuItem()
                {
                    Text = m.Name
                };

                if (!string.IsNullOrWhiteSpace(m.Command))
                {
                    mi.Click += (object sender, EventArgs e) =>
                    {
                        textBox2.Text = m.Command;
                        textBox3.Text = m.Name.Replace(" ", "");
                        helpText = m.Help;
                    };
                }

                this.Invoke((Action)(() => root.DropDownItems.Add(mi)));

                if (m.SubComands != null && m.SubComands.Count > 0)
                {
                    ReadCommandMenu(mi, m.SubComands);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
            {
                textBox1.Text = "";

                foreach (var file in files)
                {
                    if (System.IO.Directory.Exists(file))
                    {
                        textBox1.Text += file + "|";
                    }
                    else
                    {
                        textBox1.Text += System.IO.Path.GetDirectoryName(file) + "|";
                    }
                }


            }
        }

        private void btnCommHelp_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(helpText))
            {
                MessageBox.Show(helpText);
            }
        }

        private void btnRefreshCommList_Click(object sender, EventArgs e)
        {
            CommandMenus.Clear();
            CommandMenus = null;
            ReadCommandMenu(this.cmdMenuRoot, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(textBox1.Text))
            {
                System.Diagnostics.Process.Start("explorer.exe", textBox1.Text);
            }
            else
            {
                (sender as Button).ForeColor = Color.Red;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            FormFolderList frm = new FormFolderList(textBox1.Text);

            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = frm.Folders;
            }
        }

        private void TesteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void CmdMenuRoot_Click(object sender, EventArgs e)
        {

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
