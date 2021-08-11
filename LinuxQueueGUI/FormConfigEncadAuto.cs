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

namespace LinuxQueueGUI
{
    public partial class FormConfigEncadAuto : Form
    {
#if DEBUG
        public string apiUrl = @"http://10.206.194.196:5100/api/";
        public string apiUrl_Z = @"http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/";
#else
        public string apiUrl = @"http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/";
#endif

        System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
        public FormConfigEncadAuto()
        {
            InitializeComponent();
        }
        private async void FormConfigEncad_Load(object sender, EventArgs e)
        {
            userControl12.Initialize();

            try
            {
                System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                try
                {
                    response = await httpClient.GetAsync(apiUrl + "ConfigEncad");

                }
                catch
                {
                    response = await httpClient.GetAsync(apiUrl_Z + "ConfigEncad");

                }

                using (response)
                {

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }
                    else
                    {


                        var productJsonString = await response.Content.ReadAsStringAsync();
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CommItem>>(productJsonString);

                        userControl12.WorkingDirectory = string.Join("|", data.Select(x => x.Delinux()).Select(x => x.WorkingDirectory));
                        userControl12.Command = data.First().Command;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error!!");
            }
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            var commList = new List<CommItem>();

            foreach (var wd in userControl12.WorkingDirectory.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var comm = new CommItem()
                {
                    Command = userControl12.Command,
                    CommandName = "auto" + userControl12.Command,

                };

                if (!string.IsNullOrWhiteSpace(userControl12.Argument))
                {
                    comm.Command += " " + userControl12.Argument;
                }
                comm.Command = CommItem.Linuxize(comm.Command);
                comm.User = "AutoRun";
                comm.WorkingDirectory = CommItem.Linuxize(wd);


                commList.Add(comm);
            }


            try
            {
                var cont = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(commList));
                cont.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
                try
                {
                    response = await httpClient.PutAsync(apiUrl + "ConfigEncad", cont);

                }
                catch
                {
                    response = await httpClient.PutAsync(apiUrl_Z + "ConfigEncad", cont);

                }


                using (response)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        MessageBox.Show("Alterado");
                    }


                }
            }
            catch
            {
                MessageBox.Show("Error!!");
            }

            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void userControl11_Load(object sender, EventArgs e)
        {

        }

        private void FormConfigEncadAuto_Load(object sender, EventArgs e)
        {

        }
    }
}
