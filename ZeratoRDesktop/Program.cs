using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace ZeratoRDesktop
{
    static class Program
    {
        public static string VERSION = "0.0.1";
        public static bool FROM_STARTUP = false;

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            FROM_STARTUP = args != null && args.Any(arg => arg.Equals("startup", StringComparison.CurrentCultureIgnoreCase));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show("ZeratoR Desktop est déjà lancé dans votre barre des tâches.");
                Application.Exit();
                return;
            }
            try
            {
                WebClient webClient = new WebClient();
                String lastVersion = webClient.DownloadString("http://zerator-desktop.mog-creations.com/version");
                if (lastVersion != VERSION)
                {
                    DialogResult result = MessageBox.Show("Voulez-vous télécharger la nouvelle version de l'application?",
                        "Une nouvelle version est disponible!", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("http://zerator-desktop.mog-creations.com/");
                        Application.Exit();
                        return;
                    }
                }
            }
            catch (System.Net.WebException e) { }
            Application.Run(new Form1());
        }
    }
}
