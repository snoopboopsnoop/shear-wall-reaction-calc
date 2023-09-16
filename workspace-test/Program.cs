using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workspace_test
{
    internal static class Program
    {

        private static Form1 form;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.ApplicationExit += OnApplicationExit;
            Application.Run(form);
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            form.GetWorkspace().CloseWord();
        }
        

    }
}
