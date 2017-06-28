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
 
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Initialize a form, copy it into a variable in ForecourtCommunication (to reference the form) and then run it
            Form1 form = new Form1(); 

            ////Create an instance of Forecourt Communication, start the Simulator server and create a thread to wait until the client connects to the Forecourt server
            //ForecourtCommunication fcc = new ForecourtCommunication(form);
            //fcc.StartPipeServer();

            //Thread FCThread = new Thread(() => fcc.CreateClient());
            //FCThread.IsBackground = true;
            //FCThread.Start();

            Application.Run(form);
        }

        
    }
}
