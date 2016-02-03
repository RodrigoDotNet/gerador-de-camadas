using System;
using System.Security.Principal;
using System.Windows.Forms;
using DataDrain.ORM.Generator.Formularios;

namespace DataDrain.ORM.Generator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            Application.Run(new frmProvider());

        }
    }
}
