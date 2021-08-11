using LinuxQueue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinuxQueueGUI
{
    public partial class FormOutputs : Form
    {

        public OutputCollection OutputLogs { get; private set; }

        public FormOutputs()
        {
            InitializeComponent();
            OutputLogs = new OutputCollection(this.tabControl1);
        }

        public async Task OpenLog(CommItem comm)
        {

            if (!OutputLogs.Contains(comm.CommandName))
            {
                try
                {
                    await OutputLogs.AddAsync(comm);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }


        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await OutputLogs.AtualizaTudoAsync();
        }

        public static FormOutputs OpenOutputs()
        {
            FormOutputs frm = GetOpenedOutputs();

            if (frm == null)
            {
                frm = new FormOutputs();
            }

            frm.Show();
            frm.Focus();

            return frm;
        }

        public static FormOutputs GetOpenedOutputs()
        {

            FormOutputs frm = null;

            foreach (var ofrm in Application.OpenForms)
            {
                if (ofrm is FormOutputs)
                {
                    frm = (FormOutputs)ofrm;
                    break;
                }
            }

            return frm;
        }

        private void FormOutputs_Load(object sender, EventArgs e)
        {

        }
    }

    public class OutputCollection : IEnumerable<Output>
    {

        TabControl tab;
        //public CommItem Comm { get; set; }


        public OutputCollection(TabControl tab)
        {
            this.tab = tab;

            tab.DrawItem += tab_DrawItem;
        }

        void tab_DrawItem(object sender, DrawItemEventArgs e)
        {

            Output page = (Output)tab.TabPages[e.Index];

            switch (page.FolderType)
            {
                case QueueFolderEnum.Queue:
                    e.Graphics.FillRectangle(new SolidBrush(Color.Goldenrod), e.Bounds);
                    break;
                case QueueFolderEnum.Running:
                    e.Graphics.FillRectangle(new SolidBrush(Color.DarkSlateBlue), e.Bounds);
                    break;
                case QueueFolderEnum.Finished:
                    e.Graphics.FillRectangle(new SolidBrush(Color.DarkOliveGreen), e.Bounds);
                    break;
                default:
                    break;
            }

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, tab.Font, paddedBounds, Color.White);

        }

        public async Task AddAsync(CommItem comm)
        {

            var tp = new Output(comm);

            tab.TabPages.Add(tp);

            tp.UpdadeFolderType(comm.FolderType);

            await ((Output)tab.TabPages[comm.CommandName]).LoadLogAsync();

            tab.SelectedTab = tab.TabPages[comm.CommandName];
            tab.SelectedTab.Focus();
        }

        public void Clear()
        {
            tab.TabPages.Clear();
        }

        public bool Contains(string item)
        {
            return tab.TabPages.ContainsKey(item);
        }

        public bool Remove(string item)
        {
            tab.TabPages.RemoveByKey(item);
            return true;
        }

        //public void AtualizaTudo()
        //{
        //    foreach (Output item in tab.TabPages)
        //    {
        //        item.LoadLog().Wait();
        //    }
        //}

        public async Task AtualizaTudoAsync()
        {
            foreach (Output item in tab.TabPages)
            {
                await item.LoadLogAsync();
            }
        }

        public IEnumerator<Output> GetEnumerator()
        {

            return tab.TabPages.Cast<Output>().GetEnumerator();

            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {

            return tab.TabPages.Cast<Output>().GetEnumerator();
            throw new NotImplementedException();
        }
    }
    public class Output : TabPage
    {


#if DEBUG
        public string apiUrl = @"http://10.206.194.196:5100/api/";
        public string apiUrl_Z = @"http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/";
#else
        public string apiUrl = @"http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/";
#endif

        static Font font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        TextBox txtCtrl;

        public CommItem Comm { get; set; }
        public QueueFolderEnum FolderType { get; set; }

        public Output(CommItem comm)
            : base(comm.CommandName)
        {

            this.Comm = comm;
            //this.FolderType = FolderType;
            this.Name = comm.CommandName;
            txtCtrl = new TextBox() { Name = "txtOut_" + comm.CommandName };

            txtCtrl.Dock = DockStyle.Fill;
            txtCtrl.Multiline = true;
            txtCtrl.ScrollBars = ScrollBars.Both;
            //txtCtrl.BackColor = Color.DarkOliveGreen;
            txtCtrl.ForeColor = Color.White;
            txtCtrl.Font = font;

            this.Controls.Add(txtCtrl);

            // UpdadeFolderType(Comm.FolderType);
        }
        public async Task LoadLogAsync()
        {

            try
            {

                using (var client = new HttpClient())
                {
                    System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                    try
                    {
                        response = await client.GetAsync(apiUrl + "Command/" + Comm.CommandName);

                    }
                    catch
                    {
                        response = await client.GetAsync(apiUrl_Z + "Command/" + Comm.CommandName);

                    }
                    using (response)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var productJsonString = await response.Content.ReadAsStringAsync();

                            txtCtrl.Text = productJsonString;
                        }
                        else
                        {

                            var runlog = Comm.LogFile;
                            var txt = "";
                            if (System.IO.File.Exists(runlog))
                            {
                                txt = System.IO.File.ReadAllText(runlog);
                                txt = txt.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                            }
                            else
                            {
                                txt = "Log not found\r\n" + runlog;
                            }

                            txtCtrl.Text = txt;

                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                var runlog = Comm.LogFile;
                var txt = "";
                if (System.IO.File.Exists(runlog))
                {
                    txt = System.IO.File.ReadAllText(runlog);
                    txt = txt.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                }
                else
                {
                    txt = "Log not found\r\n" + runlog;
                }

                txtCtrl.Text = txt;
            }

            if (txtCtrl.Text.Length > 1)
            {
                txtCtrl.Select(txtCtrl.Text.Length - 1, 0);
                txtCtrl.ScrollToCaret();
            }
            txtCtrl.Refresh();

        }

        public void UpdadeFolderType(QueueFolderEnum folderType)
        {
            this.FolderType = folderType;
            switch (this.FolderType)
            {
                case QueueFolderEnum.Queue:
                    txtCtrl.BackColor = Color.Goldenrod;
                    break;
                case QueueFolderEnum.Running:
                    txtCtrl.BackColor = Color.DarkSlateBlue;
                    break;
                case QueueFolderEnum.Finished:
                    txtCtrl.BackColor = Color.DarkOliveGreen;
                    break;
                default:
                    break;
            }

            this.Parent.Invalidate();

        }
    }


}

