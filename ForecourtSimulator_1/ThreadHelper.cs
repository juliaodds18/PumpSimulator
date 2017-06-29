using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForecourtSimulator_1
{
    public static class ThreadHelper
    {
        delegate void SetTextCallback(Form f, Control ctrl, string text);
        delegate void SetButtonCallback(Form f, Control ctrl, bool value);
        /// <summary>
        /// Set text property of various controls
        /// </summary>
        /// <param name="form">The calling form</param>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        public static void SetText(Form form, Control ctrl, string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                form.Invoke(d, new object[] { form, ctrl, text });
            }
            else
            {
                ctrl.Text = text;
            }
        }

        public static void SetButton(Form form, Control ctrl, bool value)
        {
            if(ctrl.InvokeRequired)
            {
                SetButtonCallback d = new SetButtonCallback(SetButton);
                form.Invoke(d, new object[] { form, ctrl, value });
            }
            else
            {
                if (ctrl is Button)
                {
                    ((Button)ctrl).Enabled = value;
                }
            }
        }

    }
}
