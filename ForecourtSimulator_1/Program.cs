using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForecourtSimulator_1
{
    static class Program
    {
        #region Private Objects


        #endregion

        [STAThread]
        static void Main()
        {
            ForecourtCommunication.StartPipeServer();
            Thread FCThread = new Thread(() => ForecourtCommunication.CreateClient());
            FCThread.IsBackground = true;
            FCThread.Start();

            //ForecourtCommunication.StartPipeServer();
            //ForecourtCommunication.CreateClient(); 

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        
    }
}
