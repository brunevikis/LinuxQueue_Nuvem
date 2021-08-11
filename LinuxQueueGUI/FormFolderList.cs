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
    public partial class FormFolderList : Form {



        public string Folders {
            get {
                string ret = string.Join("|", listBox1.Items.Cast<string>());
                return ret;
            }

            set {
                var arr = value.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                listBox1.Items.AddRange(arr);
                
            }
        }

        public FormFolderList(string folders) {

            
            InitializeComponent();
            
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Folders = folders;            

        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void listBox1_KeyPress(object sender, KeyPressEventArgs e) {
            
        }

        private void button2_Click(object sender, EventArgs e) {
            if (listBox1.SelectedItems != null && listBox1.SelectedItems.Count > 0) {


                for (int i = listBox1.SelectedItems.Count - 1; i >= 0; i--) {

                    listBox1.Items.Remove(listBox1.SelectedItems[i]);

                }                

                              
            }
        }

        private void panel1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true) {
                e.Effect = DragDropEffects.All;
            }
        }

        private void panel1_DragDrop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0) {                

                foreach (var file in files) {
                    if (System.IO.Directory.Exists(file)) {
                        listBox1.Items.Add(file);
                    } else {
                        listBox1.Items.Add(System.IO.Path.GetDirectoryName(file));                        
                    }
                }


            }
        }
    }
}
