using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FEBuilderGBA_Installer
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //ArchSevenZip.Extract("c:\\temp\\rentatsu.7z", "c:\\temp");

            Application.Run(new Form1());
        }
    }
}
