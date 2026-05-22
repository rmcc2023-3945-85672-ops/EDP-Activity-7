using System;
using System.Windows.Forms;

namespace LibrarySystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Forms.Form1());
        }
    }
}
// An edit happened here
