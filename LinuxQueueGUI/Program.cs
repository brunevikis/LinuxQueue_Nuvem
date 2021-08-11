using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LinuxQueueGUI {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            try {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormMain());
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        public static Tuple<string, string> GetResultadosExPath(string path) {
            string anchorKeyD = @"SOFTWARE\Classes\directory\shell\decompToolsShellX";
            string ctxMenuD = @"SOFTWARE\Classes\directory\ContextMenus\decompToolsShellX";

            try {
                var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(anchorKeyD);

                var k2 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ctxMenuD);
                k2 = k2.OpenSubKey("shell");

                var k2_1 = k2.OpenSubKey("cmd1");
                var p = k2_1.OpenSubKey("command").GetValue("");

                var fcmd = p.ToString().Replace("%1", path);

                var tm = fcmd.Split(new string[] { " resultado " }, StringSplitOptions.None);

                var ret = new Tuple<string, string>(tm[0], fcmd.Substring(tm[0].Length));

                return ret;
            } catch {

                return null;
            }
        }
    }
}
