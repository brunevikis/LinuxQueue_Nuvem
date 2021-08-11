using LinuxQueueCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LinuxQueueApi.Model
{
    public class Config
    {
        public string CaminhoDecompAuto { get; set; }

        public string ComandoDecompAuto { get; set; }

        public static List<CommItem> get()
        {


            var configFile = Path.Combine(LinuxQueueCore.QueueFolders.rootPath, "configAuto");
            var cont = System.IO.File.ReadAllText(configFile);

            var comm = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CommItem>>(cont);

            return comm;

            /*

            var configFile = System.IO.Path.Combine(LinuxQueueCore.QueueFolders.rootPath, "configAuto");

            var fileContent = System.IO.File.ReadAllLines(configFile);



            var prop = typeof(Config).GetProperties();
            var config = new Config();
            prop.Join(fileContent, p => p.Name, f => f.Split(':')[0], (p, f) => new { p, f }).ToList().ForEach(x =>
            {
                x.p.SetValue(config, x.f.Split(':')[1]);
            }
                );

            return config;
            */
        }

    }
}
