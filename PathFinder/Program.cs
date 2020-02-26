using System;
using System.Windows.Forms;

namespace PathFinder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm mainForm = new MainForm();
            GameManager gameManager = new GameManager();

            mainForm.XYcellClick += gameManager.Handle_XYcellClick;
            mainForm.InterfaceModeChanged += gameManager.Handle_InterfaceModeChanged;
            mainForm.ShowNumbersChanged += gameManager.Handle_ShowNumbersChanged;

            gameManager.Command_MarkInterfaceCell += mainForm.PaintTheCell;
            gameManager.Command_PrintInterfaceCell += mainForm.PrintOnCell;

            Application.Run(mainForm);
        }
    }
}