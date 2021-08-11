using LinuxQueue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinuxQueueGUI
{
    public partial class FormMain : Form
    {

        System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();

#if DEBUG

        public string apiUrl = @"http://10.206.194.196:5100/api/";
        public string apiUrl_Z = @"http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/";
#else
       // public string apiUrl = @"http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/";
        public string apiUrl = @"http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/";
#endif







        QueueController controller;
        string helpText = "";

        private Version GetRunningVersion()
        {

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {

                var curV = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;

                System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted += (object sender, System.Deployment.Application.CheckForUpdateCompletedEventArgs e) =>
                {
                    if (e.UpdateAvailable)
                        MessageBox.Show("Nova versão disponível (" + e.AvailableVersion.ToString() + "), reinicie o aplicativo para instalar.");
                };
                System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CheckForUpdateAsync();

                return curV;

            }
            else
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        }

        List<CommItem> Commands;
        List<CommItem> Commands_Queue;
        List<CommItem> Commands_Running;
        List<CommItem> Commands_Finished;


        public FormMain()
        {
            InitializeComponent();

            controller = new QueueController();

            //controller.RegisterClusters(System.Configuration.ConfigurationManager.AppSettings["clusters"]);


            QueueFolders.RegisterFolder(System.Configuration.ConfigurationManager.AppSettings["controllerFolder"]);
            QueueController.ReadConfig();
            QueueController.Clusters.Insert(0, new Cluster() { Alias = "Auto", Host = "" });

            cbxCluster.ValueMember = "Host";
            cbxCluster.DisplayMember = "Alias";

            cbxCluster.DataSource = QueueController.Clusters;


            controller.QueueChanged += controller_QueueChanged;

            Commands = new List<CommItem>();

            this.Text += " - " + GetRunningVersion().ToString();

            CommandMenu.menuFile = System.Configuration.ConfigurationManager.AppSettings["menufile"];


            dgvFinished.RowPostPaint += dgvFinished_RowPostPaint;
        }

        void controller_QueueChanged(object sender, CommChangesEventArgs e)
        {

            if (e.NewFolderType == QueueFolderEnum.Finished && e.User == currentUser)
            {
                notifyIcon1.ShowBalloonTip(30000, "Compass Executor", e.CommandName + " Finalizado", ToolTipIcon.Info);
            }

        }

        void dgvFinished_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {

            var comm = ((DataGridView)sender).Rows[e.RowIndex].DataBoundItem as CommItem;

            if (comm != null)
            {


                if (comm.HasError)
                {
                    ((DataGridView)sender).Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    ((DataGridView)sender).Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.White;
                }
                if (comm.ExitCode.HasValue && comm.ExitCode.Value == 2) // nao convergencia
                {
                    ((DataGridView)sender).Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Goldenrod;
                    ((DataGridView)sender).Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
                    ((DataGridView)sender).Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.White;
                }
                else if (comm.ExitCode.HasValue && comm.ExitCode.Value != 0)
                {
                    ((DataGridView)sender).Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Coral;
                    ((DataGridView)sender).Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.OrangeRed;
                }
            }

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openDetails();
        }

        private void openDetails()
        {
            if (selected != null)
            {
                userControl11.WorkingDirectory = selected.WorkingDirectory;
                userControl11.Command = selected.Command.Split(' ')[0];

                userControl11.EnviarEmail = selected.EnviarEmail;

                var n = selected.CommandName.Split('_');

                userControl11.Nome = string.Join("_", n.Take(n.Length - 1));
                userControl11.Argument = string.Join(" ", selected.Command.Split(' ').Skip(1));

                cbxCluster.SelectedIndex = 0;
            }
        }

        private async void outputToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (selected != null)
            {

                var fo = FormOutputs.OpenOutputs();

                await fo.OpenLog(selected);

            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;


            var comm = new CommItem()
            {
                Command = userControl11.Command,
                Cluster = (Cluster)cbxCluster.SelectedItem
            };

            if (!string.IsNullOrWhiteSpace(userControl11.Argument))
            {
                comm.Command += " " + userControl11.Argument;
            }


            comm.User = txtUsuario.Text;


            comm.EnviarEmail = userControl11.EnviarEmail;


            var wds = userControl11.WorkingDirectory.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < wds.Length; i++)
            {
                var wd = wds[i];

                var name_comp = wds.Length > 1 ? "_" + i.ToString("00") + "_" : "_";


                comm.CommandName = userControl11.Nome + name_comp + DateTime.Now.ToString("yyyyMMddHHmmss");
                comm.WorkingDirectory = wd;

                try
                {
                    var teste = (Newtonsoft.Json.JsonConvert.SerializeObject(comm));
                    var cont = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(comm));
                    cont.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                    System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                    try
                    {
                        response = await httpClient.PostAsync(apiUrl + "Command", cont);

                    }
                    catch
                    {
                        response = await httpClient.PostAsync(apiUrl_Z + "Command", cont);

                    }



                    using (response)
                    {

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception();
                        }
                    }
                }
                catch
                {
                    await controller.EnqueueAsync(comm);
                }
            }

            await RefreshListsAsync();

            this.Enabled = true;
            this.Cursor = DefaultCursor;
        }


        List<CommItem> selects = new List<CommItem>();
        CommItem selected
        {
            get { return selects.FirstOrDefault(); }
            set
            {
                selects.Clear();
                if (value != null) selects.Add(value);
            }
        }

        public object GetCurrent { get; private set; }

        private string currentUser;
        private void listViewFinished_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            // DataGridViewHelper.ApplyFilters(dgvFinished);
            // DataGridViewHelper.ApplyFilters(dgvQueue);
            // DataGridViewHelper.ApplyFilters(dgvRunning);

            currentUser = System.Environment.UserName;

            switch (currentUser)
            {
                case "CS320363":
                    currentUser = "Bruno_Araujo";
                    break;

                case "CS320326":
                    currentUser = "Alex_Freires";
                    break;

                case "CS320370":
                    currentUser = "Pedro_Modesto";
                    break;

                case "CS320365":
                    currentUser = "Natalia_Biondo";
                    break;

                case "CS320478":
                    currentUser = "Diana_Lima";
                    break;

                default:
                    break;
            }

            txtUsuario.Text = currentUser;
            //timer1_Tick(null, null);

            checkBox1.Checked = true;

            //ReadCommandMenu(this.cmdMenuRoot, null);

            userControl11.Initialize();

            cbxCluster.SelectedIndexChanged += cbxCluster_SelectedIndexChanged;
            cbxCluster.SelectedIndex = 0;

            await Task.Yield();
        }

        async Task SweepFoldersAsync(string Tipo = null)
        {

            List<CommItem> comms;

            try
            {
                System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                try
                {
                    response = await httpClient.GetAsync(apiUrl + "Command/" + Tipo);

                }
                catch
                {
                    response = await httpClient.GetAsync(apiUrl_Z + "Command/" + Tipo);

                }
                using (response)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var productJsonString = await response.Content.ReadAsStringAsync();
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CommItem>>(productJsonString);
                        comms = data;


                    }
                    
                    else throw new System.Net.Http.HttpRequestException();
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                comms = await controller.ReadCommsAsync();
            }






            Commands.Clear();
            if (String.IsNullOrEmpty(txtFiltro.Text))
            {
                Commands.AddRange(comms.Select(x => x.Delinux()));
            }
            else
            {

                Commands.AddRange(comms.Select(x => x.Delinux()).Where(x => x.WorkingDirectory != null && x.Command != null && (
                x.WorkingDirectory.ToUpperInvariant().Contains(txtFiltro.Text.ToUpperInvariant())
                || x.Command.ToUpperInvariant().Contains(txtFiltro.Text.ToUpperInvariant())
                )
                ).ToList());

            }



            comms = null;

            foreach (var sel in selects.ToList())
            {
                if (Commands.Contains(sel))
                {
                    selects.Remove(sel);
                    selects.Add(Commands.First(c => c.Equals(sel)));
                }
                else
                    selects.Remove(sel);
            }

            switch (Tipo)
            {
                case "queue":
                    Commands_Queue = Commands;
                    Commands.Clear();
                    break;
                case "running":
                    Commands_Running = Commands;
                    Commands.Clear();
                    break;
                case "finished":
                    Commands_Finished = Commands;
                    Commands.Clear();
                    break;
            }





            updateCommandButtons();

        }

        List<Disk> diskInfos = new List<Disk>();
        async Task GetDiskInfo()
        {
            diskInfos.Clear();
            try
            {
                System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                try
                {
                    response = await httpClient.GetAsync(apiUrl + "Disk/");

                }
                catch
                {
                    response = await httpClient.GetAsync(apiUrl_Z + "Disk/");

                }

                this.flowLayoutPanel1.Controls.Clear();
                using (response)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var productJsonString = await response.Content.ReadAsStringAsync();
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Disk>>(productJsonString);
                        diskInfos = data;
                    }
                    
                    else throw new System.Net.Http.HttpRequestException();
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                //var usedSpace = Tools.GetUsedSpace("L");
                //label7.Visible = usedSpace >= 95;

                //diskInfos.Add(new Disk() { Name = "L:\\", UsedPercentage = usedSpace });
            }

            foreach (var d in diskInfos)
            {
                this.flowLayoutPanel1.Controls.Add(new Label()
                {
                    AutoSize = true,
                    Text = d.Name + " - " + d.UsedPercentage.ToString("00") + "%",
                    ForeColor = d.UsedPercentage >= 95 ? Color.Red :
                    (d.UsedPercentage >= 85 ? Color.Orange : Color.Black),
                    Font = d.UsedPercentage >= 85 ? new Font(Font, FontStyle.Bold) : Font
                });
                this.flowLayoutPanel1.Controls.Add(new ProgressBar()
                {
                    Enabled = false,
                    MarqueeAnimationSpeed = 0,
                    Minimum = 0,
                    Maximum = 100,
                    Step = 1,
                    Value = (int)d.UsedPercentage,
                    Size = new System.Drawing.Size(327, 23),
                });
            }

            label7.Visible = diskInfos.Any(d => d.UsedPercentage >= 95);
        }


        bool updating = false;
        private async void timer1_Tick(object sender, EventArgs e)
        {

            await RefreshListsAsync();
        }

        async Task RefreshListsAsync()
        {
            if (!updating)
            {

                updating = true;
                checkBox1.ForeColor = Color.Yellow;

                try
                {
                    await SweepFoldersAsync("queue");
                    await SweepFoldersAsync("running");
                    await SweepFoldersAsync("finished");
                    await SweepFoldersAsync();
                    await UpdateListViewsAsync();


                    var frmOut = FormOutputs.GetOpenedOutputs();
                    if (frmOut != null)
                    {

                        frmOut.OutputLogs.ToList()
                            .ForEach(async c =>
                            {
                                var y = Commands.FirstOrDefault(x => x == c.Comm);
                                if (y != null) c.UpdadeFolderType(y.FolderType);

                                await c.LoadLogAsync();
                            });
                    }

                    checkBox1.ForeColor = Color.Green;


                    await GetDiskInfo();
                    // var usedSpace = Tools.GetUsedSpace("L");



                    // label7.Visible = usedSpace >= 95;

                }
                catch
                {
                    checkBox1.ForeColor = Color.Red;
                }
                finally
                {
                    updating = false;
                }
            }

        }

        private async Task UpdateListViewsAsync()
        {
            var q = Task.Run(() => Commands_Queue.Where(x => x.FolderType == QueueFolderEnum.Queue).OrderBy(x => x.Order).ToArray());
            var r = Task.Run(() => Commands_Running.Where(x => x.FolderType == QueueFolderEnum.Running).OrderBy(x => x.SDate).ToArray());
            var f = Task.Run(() => Commands_Finished.Where(x => x.FolderType == QueueFolderEnum.Finished && x.User != "hide").OrderByDescending(x => x.EDate).ThenByDescending(x => x.SDate).ToArray());



            var scrF = new Tuple<int, int, int>(dgvFinished.FirstDisplayedScrollingColumnHiddenWidth, dgvFinished.FirstDisplayedScrollingColumnIndex, dgvFinished.FirstDisplayedScrollingRowIndex);
            var scrR = new Tuple<int, int, int>(dgvRunning.FirstDisplayedScrollingColumnHiddenWidth, dgvRunning.FirstDisplayedScrollingColumnIndex, dgvRunning.FirstDisplayedScrollingRowIndex);
            //var po = dgvFinished.FirstDisplayedScrollingColumnHiddenWidth


            dgvRunning.DataSource = await r;
            dgvQueue.DataSource = await q;
            dgvFinished.DataSource = await f;

            updateSelected();

            dgvRunning.FirstDisplayedScrollingColumnIndex = scrR.Item2;
            if (scrR.Item3 > 0 && scrR.Item3 < dgvRunning.RowCount) dgvRunning.FirstDisplayedScrollingRowIndex = scrR.Item3;



            dgvFinished.FirstDisplayedScrollingColumnIndex = scrF.Item2;
            if (scrF.Item3 > 0 && scrF.Item3 < dgvFinished.RowCount) dgvFinished.FirstDisplayedScrollingRowIndex = scrF.Item3;

        }


        private async void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {

                await RefreshListsAsync();
                timer1.Start();
            }
            else
            {
                timer1.Stop();
                checkBox1.ForeColor = Color.Black;
            }

            await Task.Yield();
        }

        private async void cancelarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {

                btnCancelar.Enabled = false;
                if (selected.FolderType == QueueFolderEnum.Running)
                {

                    if (MessageBox.Show("Tem certeza que deseja cancelar o processo em andamento?\r\n" +
                "\r\nPID = " + selected.Pid +
                "\r\nComando = " + selected.Command
                , "Compass", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != System.Windows.Forms.DialogResult.Yes)
                    {
                        return;
                    }
                }

                foreach (var sel in selects)
                {

                    try
                    {
                        System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                        try
                        {
                            response = await httpClient.DeleteAsync(apiUrl + "Command/" + sel.CommandName);

                        }
                        catch
                        {
                            response = await httpClient.DeleteAsync(apiUrl_Z + "Command/" + sel.CommandName);

                        }

                        using (response)
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                
                            }
                        }
                    }
                    catch
                    {
                        await controller.CancelAsync(sel, txtUsuario.Text);
                    }
                }

                await RefreshListsAsync();
            }
        }

        private async void btnOrdUp_Click(object sender, EventArgs e)
        {

            if (selected != null && selected.FolderType == QueueFolderEnum.Queue)
            {
                try
                {
                    selected.Order -= 10;
                    if (selected.Order <= 0) selected.Order = 1;


                    var cont = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(selected));
                    cont.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                    System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                    try
                    {
                        response = await httpClient.PutAsync(apiUrl + "Command/", cont);

                    }
                    catch
                    {
                        response = await httpClient.PutAsync(apiUrl_Z + "Command/", cont);

                    }

                    using (response)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            response.EnsureSuccessStatusCode();
                        }

                    }
                }
                catch
                {

                    selected.SaveChanges();
                    //controller.SetHigherOrder(selected);
                }

                await RefreshListsAsync();
            }
        }

        private async void btnOrdDwn_Click(object sender, EventArgs e)
        {
            if (selected != null && selected.FolderType == QueueFolderEnum.Queue)
            {
                try
                {
                    selected.Order += 10;

                    var cont = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(selected));
                    cont.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                    System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                    try
                    {
                        response = await httpClient.PutAsync(apiUrl + "Command/", cont);

                    }
                    catch
                    {
                        response = await httpClient.PutAsync(apiUrl_Z + "Command/", cont);

                    }

                    using (response)
                    {
                        // response.EnsureSuccessStatusCode();

                        if (response.IsSuccessStatusCode)
                        {
                            response.EnsureSuccessStatusCode();
                        }
                       
                    }
                }
                catch
                {
                    selected.SaveChanges();
                    //controller.SetLowerOrder(selected);
                }

                await RefreshListsAsync();
            }

        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //switch (MessageBox.Show("Minimize to tray?", "Compass", MessageBoxButtons.YesNoCancel)) {
            //    case DialogResult.Cancel:
            //        e.Cancel = true;
            //        break;
            //    case DialogResult.Yes:
            //        e.Cancel = true;
            //        this.ShowInTaskbar = false;
            //        this.Visible = false;
            //        this.WindowState = FormWindowState.Minimized;
            //        break;
            //}
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }



        private async void ignoraFilaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected != null &&
                selected.FolderType == QueueFolderEnum.Queue
                )
            {

                var frm = new FormIgnoraFila(selected);


                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    try
                    {
                        foreach (var sel in selects)
                        {
                            if (sel.FolderType == QueueFolderEnum.Queue) await controller.IgnoreQueueAsync(sel);
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }

                    await RefreshListsAsync();
                }
            }
        }

        private void updateSelected()
        {
            dgvQueue.ClearSelection();
            dgvRunning.ClearSelection();
            dgvFinished.ClearSelection();

            if (selected != null)
            {
                foreach (DataGridViewRow r in dgvQueue.Rows)
                {
                    if (selects.Contains(r.DataBoundItem))
                    {
                        r.Selected = true;
                        // dgvQueue.CurrentCell = r.Cells[0];
                    }
                }


                foreach (DataGridViewRow r in dgvRunning.Rows)
                {
                    if (selects.Contains(r.DataBoundItem))
                    {
                        r.Selected = true;
                        dgvRunning.CurrentCell = r.Cells[0];
                    }
                }


                foreach (DataGridViewRow r in dgvFinished.Rows)
                {
                    if (selects.Contains(r.DataBoundItem))
                    {
                        r.Selected = true;
                        dgvFinished.CurrentCell = r.Cells[0];
                    }

                }
            }


        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var fro = FormOutputs.OpenOutputs();

            foreach (DataGridViewRow item in dgvRunning.Rows)
            {

                await fro.OpenLog(
                    ((CommItem)item.DataBoundItem)
                );

            }
        }



        private void dgvFinished_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void btnRefreshCommList_Click(object sender, EventArgs e)
        {
            //CommandMenus.Clear();
            //CommandMenus = null;
            //ReadCommandMenu(this.cmdMenuRoot, null);
        }

        private void dgvQueue_RowHeaderMouseClick(object sender, DataGridViewCellEventArgs e)
        {

            select(sender as DataGridView);
        }

        private void select(DataGridView sender)
        {
            if (sender.SelectedRows.Count > 0)
            {

                if (sender != dgvQueue) dgvQueue.ClearSelection();// foreach (DataGridViewRow r in dgvQueue.Rows) r.Selected = false;
                if (sender != dgvRunning) dgvRunning.ClearSelection();// foreach (DataGridViewRow r in dgvRunning.Rows) r.Selected = false;
                if (sender != dgvFinished) dgvFinished.ClearSelection();// foreach (DataGridViewRow r in dgvFinished.Rows) r.Selected = false;


                selected = (sender.SelectedRows[0].DataBoundItem as CommItem);
                for (int i = 1; i < sender.SelectedRows.Count; i++)
                {
                    selects.Add((sender.SelectedRows[i].DataBoundItem as CommItem));
                }

            }
            else selected = null;

            updateCommandButtons();

        }

        private void updateCommandButtons()
        {

            if (selected != null)
            {


                if (selected.FolderType == QueueFolderEnum.Queue)
                    btnIgnorarFila.Enabled = btnOrdUp.Enabled = btnOrdDwn.Enabled = true;
                else
                    btnIgnorarFila.Enabled = btnOrdUp.Enabled = btnOrdDwn.Enabled = false;

                if (selected.FolderType == QueueFolderEnum.Queue || selected.FolderType == QueueFolderEnum.Running)
                    btnCancelar.Enabled = true;
                else
                    btnCancelar.Enabled = false;

                if (selected.FolderType == QueueFolderEnum.Finished)
                    button5.Enabled = bt_PLDM.Enabled = true;

                else
                    button5.Enabled = bt_PLDM.Enabled = false;

                btnResultado.Enabled = btnAbrirPasta.Enabled = btnDetalhes.Enabled = btnOutput.Enabled = true;


            }
            else
            {
                button5.Enabled = bt_PLDM.Enabled = btnResultado.Enabled = btnAbrirPasta.Enabled = btnIgnorarFila.Enabled = btnOrdUp.Enabled = btnOrdDwn.Enabled = btnCancelar.Enabled = btnDetalhes.Enabled = btnOutput.Enabled = false;
            }

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void dgvFinished_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            openDetails();

        }


        private void btnAbrirPasta_Click(object sender, EventArgs e)
        {

            if (selected != null)
            {
                if (
                System.IO.Directory.Exists(selected.WorkingDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", selected.WorkingDirectory);
                }
                else
                {
                    MessageBox.Show("Pasta não acessível.");
                }
            }
        }

        private async void btnResultado_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {
                if (
                System.IO.Directory.Exists(selected.WorkingDirectory))
                {
                    //encontrar relato.* ou pmo.dat e abrir

                    var files = System.IO.Directory.GetFiles(selected.WorkingDirectory, "relato.*")
                        .Concat(System.IO.Directory.GetFiles(selected.WorkingDirectory, "pmo.dat"))
                        .Concat(System.IO.Directory.GetFiles(selected.WorkingDirectory, "*.log")).ToArray();

                    if (files.Length > 0)
                    {

                        Tools.OpenText(files[0]);



                        await Task.Yield();
                    }
                }
            }
        }



        private async void cbxCluster_SelectedIndexChanged(object sender, EventArgs e)
        {

            await ChangeCluster();

        }

        private async Task ChangeCluster()
        {


            //ReadCommandMenu(this.cmdMenuRoot, null);

            //await Task.Factory.StartNew(() => {

            //    //CommandMenu.menuFile = System.Configuration.ConfigurationManager.AppSettings["menufile" + c];
            //    //    QueueFolders.rootPath = System.Configuration.ConfigurationManager.AppSettings["controllerFolder" + c];
            //    ReadCommandMenu(this.cmdMenuRoot, null);

            //});

            await Task.Yield();

            //    await RefreshListsAsync();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {
                if (
                System.IO.Directory.Exists(selected.WorkingDirectory))
                {

                    var p = Program.GetResultadosExPath(selected.WorkingDirectory);

                    if (p != null)
                    {
                        System.Diagnostics.Process.Start(p.Item1, p.Item2);
                    }

                }
                else
                {
                    MessageBox.Show("Pasta não acessível.");
                }
            }
        }


        private async void button7_Click(object sender, EventArgs e)
        {


            var oc = button7.BackColor;
            button7.BackColor = Color.OrangeRed;

            Clipboard.SetText(
                await controller.ListAllFinnished(), TextDataFormat.Text
            );

            button7.BackColor = oc;

        }

        private void btnCommHelp_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(helpText))
            {
                MessageBox.Show(helpText);
            }
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConfig frm = new FormConfig();

            frm.ShowDialog();

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            httpClient.Dispose();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void automáticoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConfigAuto frm = new FormConfigAuto();
            frm.ShowDialog();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void userControl11_Load(object sender, EventArgs e)
        {

        }

        private void dgvQueue_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtUsuario_TextChanged(object sender, EventArgs e)
        {

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private async void bt_PLDM_Click(object sender, EventArgs e)
        {
            if (selected != null)
            {

                try
                {
                    string caso = selected.CommandName.ToString();
                    string dir = selected.WorkingDirectory.ToString();
                    System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                    try
                    {
                        response = await httpClient.GetAsync(apiUrl + "Command/PLD_Mensal-" + caso);

                    }
                    catch
                    {
                        response = await httpClient.GetAsync(apiUrl_Z + "Command/PLD_Mensal-" + caso);

                    }


                    using (response)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var productJsonString = await response.Content.ReadAsStringAsync();
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject(productJsonString);

                            PLD_Mensal form_pld = new PLD_Mensal(data, caso, dir);

                            form_pld.Show();
                        }
                        
                        else throw new System.Net.Http.HttpRequestException();
                    }

                }
                catch (Exception err)
                {
                    MessageBox.Show("Erro na Requisão do PLD");
                }
            }
        }

        private void automáticoENCADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConfigEncadAuto frm = new FormConfigEncadAuto();
            frm.ShowDialog();
        }
    }
}
