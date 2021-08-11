using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LinuxQueueGUI {

    public class CommandMenu {

        public static string menuFile = "";


        public string Name { get; set; }
        public string Command { get; set; }
        public List<CommandMenu> SubComands { get; set; }
        public List<string> RunOn { get; set; }
        public string help;
        public string Help {
            get { return help; }
            set {
                help = value.Replace("\\r\\n", "\r\n");
            }
        }


        public void Save() {

            XmlSerializer ser = new XmlSerializer(this.GetType());

            using (var sw = File.OpenWrite("commandMenu.xml")) {

                ser.Serialize(sw, this);

            }

        }

        public static CommandMenu Open() {
            XmlSerializer ser = new XmlSerializer(typeof(CommandMenu));

            var file = menuFile;// ConfigurationManager.AppSettings["menufile"];

            using (var sr = File.OpenRead(file)) {

                var r = ser.Deserialize(sr);

                return r as CommandMenu;
            }
        }
    }
}
